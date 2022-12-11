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

public class TileCollectionRenderer : MonoBehaviour
{
    [SerializeField] private GameObject _tilePrefab;
    public GameObject tilesPanel;

    private TileCollection _tileCollection = new TileCollection();
    public Tile EdgeTile { get => _tileCollection.edgeTile; }
    public Dictionary<Tile, TileGameObject> tileObjects = new Dictionary<Tile, TileGameObject>();

    private List<NeighborSlotGameObject> _neighborSlots;
    private SelectedSlotGameObject _selectionSlot;

    [SerializeField] private Button _addTileButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _loadButton;

    [SerializeField] private Toggle _edgeTileToggle;

    void OnEnable()
    {
        _neighborSlots = FindObjectsOfType<NeighborSlotGameObject>().OrderBy((v) => v.direction).ToList();
        _selectionSlot = FindObjectOfType<SelectedSlotGameObject>();
    }
    private void Start()
    {
        _addTileButton.onClick.AddListener(AddButton);
        _saveButton.onClick.AddListener(Save);
        _loadButton.onClick.AddListener(Load);
        _edgeTileToggle.onValueChanged.AddListener((v)=> { if (_selectionSlot.Selected != null) _tileCollection.edgeTile = v ? _selectionSlot.Selected : null; });
        FindObjectOfType<DataHolder>().tiles = _tileCollection;
    }
    private void OnRectTransformDimensionsChange()
    {
        foreach (TileGameObject obj in tileObjects.Values)
        {
            obj.ResetPosition();
        }
    }

    public void AddButton()
    {
        var imageFile = EditorUtility.OpenFilePanelWithFilters("Select new tile image.", "", new[]{ "Image files", "png,jpg,jpeg" });
        if (!File.Exists(imageFile))
        {
            Debug.Log("Didn't choose a file");
            return;
        }
        var newTile = _tileCollection.AddTile();
        AddTileObject(newTile, imageFile);
    }
    private void AddTileObject(Tile tile, string imageFile)
    {
        var tileGO = Instantiate(original: _tilePrefab, parent: transform);
        tileGO.GetComponent<TileGameObject>().tile = tile;
        tileGO.GetComponent<TileGameObject>().LoadImage(imageFile);
        tileObjects[tile] = tileGO.GetComponent<TileGameObject>();
    }
    private void Save()
    {
        var tempPath = UnityEngine.Windows.Directory.temporaryFolder;
        var imgPath = Path.Combine(tempPath, "tileset/");
        Directory.CreateDirectory(imgPath);
        

        var zipPath = EditorUtility.SaveFilePanel(
          "Save tileset",
          "",
          "tileset" + ".tset",
          "tset");
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
            string pic = Path.GetFileName(correspondingTileGO.imagePath);

            File.Copy(correspondingTileGO.imagePath, Path.Combine(imgPath,tile.ToString()), true);
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
        Clear();
        TempCleanup();

        string zipPath = EditorUtility.OpenFilePanel("Load tileset", "", "tset");
        if (zipPath.Length == 0)
        {
            Debug.LogError("Could not open tileset");
            return;
        }
        var tempPath = UnityEngine.Windows.Directory.temporaryFolder;
        var imgPath = Path.Combine(tempPath, "runtime/");
        Directory.CreateDirectory(imgPath);
        ZipFile.ExtractToDirectory(zipPath, imgPath);

        var jsonPath = Path.Combine(imgPath, Path.ChangeExtension(Path.GetFileName(zipPath), ".json"));

        var json = File.ReadAllText(jsonPath);
        _tileCollection.Deserialize(json);
        FindObjectOfType<DataHolder>().tiles = _tileCollection;
        Tile.Load(_tileCollection.tiles.Count);
        SpawnTiles(imgPath);
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
        TempCleanup();
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
        var tempPath = UnityEngine.Windows.Directory.temporaryFolder;
        var path = Path.Combine(tempPath, "runtime/");
        if(Directory.Exists(path))
            Directory.Delete(path, true);
    }
}
