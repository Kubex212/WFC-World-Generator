using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiles;
using System.Linq;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEditor;
using System.IO;
using Graphs;
using Newtonsoft.Json;
using System.IO.Compression;
using System;
using UnityEngine.SceneManagement;
using System.Reflection;
using SFB;

public class TileCollectionRenderer : MonoBehaviour
{
    [SerializeField] private GameObject _tilePrefab;
    public GameObject tilesPanel;

    private TileCollection _tileCollection = new TileCollection();
    public Tile EdgeTile { get => _tileCollection.edgeTile; set => _tileCollection.edgeTile = value; }
    public bool Diagonal { get => _tileCollection.diagonal; set => _tileCollection.diagonal = value; }
    public Dictionary<Tile, TileComponent> tileObjects = new Dictionary<Tile, TileComponent>();

    private List<NeighborSlotComponent> _neighborSlots;
    private SelectedSlotComponent _selectionSlot;

    [SerializeField] private Button _addTileButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _backButton;


    void OnEnable()
    {
        _neighborSlots = FindObjectsOfType<NeighborSlotComponent>().OrderBy((v) => v.direction).ToList();
        _selectionSlot = FindObjectOfType<SelectedSlotComponent>();
    }
    private void Start()
    {
        _addTileButton.onClick.AddListener(AddButton);
        _saveButton.onClick.AddListener(Save);
        _loadButton.onClick.AddListener(Load);
        _backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));


        var dhTiles = FindObjectOfType<DataHolder>().Tiles;

        if(dhTiles != null)
        {
            _tileCollection = dhTiles;
            Tile.Load(_tileCollection.tiles.Count);
            var tempPath = Application.temporaryCachePath;
            var imgPath = Path.Combine(tempPath, "runtime/");
            SpawnTiles(imgPath);
            _selectionSlot.Selected = null;
        }
        else
        {
            var path = Path.Combine(Application.dataPath, "Scripts", "Neighbors", "tileset2.tset");
            Load(path);
            dhTiles = _tileCollection;
        }
    }
    private void OnRectTransformDimensionsChange()
    {
        foreach (TileComponent obj in tileObjects.Values)
        {
            obj.ResetPosition();
        }
    }

    public void AddButton()
    {
        var imageFiles = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);

        foreach (var imageFile in imageFiles)
        {
            if (!File.Exists(imageFile))
            {
                Debug.LogError("Could not find file");
                return;
            }
            var newTile = _tileCollection.AddTile();
            AddTileObject(newTile, imageFile);
        }
    }
    private void AddTileObject(Tile tile, string imageFile)
    {
        var tileGO = Instantiate(original: _tilePrefab, parent: transform);
        tileGO.GetComponent<TileComponent>().tile = tile;
        tileGO.GetComponent<TileComponent>().LoadImage(imageFile);
        tileObjects[tile] = tileGO.GetComponent<TileComponent>();
    }
    private void Save()
    {
        var tempPath = Application.temporaryCachePath;
        var imgPath = Path.Combine(tempPath, "tileset/");
        Directory.CreateDirectory(imgPath);

        var zipPaths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);
        //var zipPath = EditorUtility.SaveFilePanel(
        //  "Save tileset",
        //  "",
        //  "tileset" + ".tset",
        //  "tset");
        var zipPath = zipPaths[0];
        if (zipPath.Length == 0)
        {
            Debug.LogError("failed to choose info path");
            return;
        }

        var jsonPath = Path.Combine(imgPath, Path.ChangeExtension(Path.GetFileName(zipPath), ".json"));

        for (int i = 0; i < _tileCollection.tiles.Count; i++)
        {
            var tile = _tileCollection.tiles[i];
            var correspondingTileGO = tileObjects[tile];
            //string pic = Path.GetFileName(correspondingTileGO.imagePath);

            File.Copy(correspondingTileGO.imagePath, Path.Combine(imgPath, tile.ToString()), true);
        }

        var json = _tileCollection.Serialize();
        File.WriteAllText(jsonPath, json);

        if (File.Exists(zipPath))
            File.Delete(zipPath);
        ZipFile.CreateFromDirectory(imgPath, zipPath);

        Directory.Delete(imgPath, true);
    }

    private void Load()
    {
        Load(null);
    }

    private void Load(string zipPath)
    {
        Clear();
        TempCleanup();

        if (zipPath == null)
        {
            zipPath = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false).FirstOrDefault();
        }
        if (zipPath == null)
        {
            return;
        }
        var tempPath = Application.temporaryCachePath;
        var imgPath = Path.Combine(tempPath, "runtime/");
        Directory.CreateDirectory(imgPath);
        ZipFile.ExtractToDirectory(zipPath, imgPath);

        var jsonPath = Path.Combine(imgPath, Path.ChangeExtension(Path.GetFileName(zipPath), ".json"));
        var json = File.ReadAllText(jsonPath);
        _tileCollection.Deserialize(json);

        FindObjectOfType<DataHolder>().Tiles = _tileCollection;
        Tile.Load(_tileCollection.tiles.Count);
        SpawnTiles(imgPath);

        _selectionSlot.Selected = null;
    }

    private void SpawnTiles(string pictures)
    {
        foreach (var tile in _tileCollection.tiles)
        {
            AddTileObject(tile, Path.Combine(pictures,tile.ToString()));
        }
    }

    private void Clear()
    {
        foreach (var obj in tileObjects.Values)
        {
            Destroy(obj.gameObject);
        }
        tileObjects.Clear();
        foreach(var slot in _neighborSlots)
        {
            slot.ShowNeighbors(null);
        }
        _selectionSlot.Selected = null;
    }
    private void OnDestroy()
    {
        CreateAtlas();
        //TempCleanup();
    }
    private void CreateAtlas()
    {
        SpriteAtlas.Atlas = tileObjects
            .OrderBy((pair)=>pair.Key.Index)
            .Select((t) => t.Value.GetComponent<Image>().sprite)
            .ToArray();

    }
    private void TempCleanup()
    {
        var tempPath = Application.temporaryCachePath;
        var path = Path.Combine(tempPath, "runtime/");
        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }
}
