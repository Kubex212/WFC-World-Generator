using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiles;
using System.Linq;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEditor;
using System.IO;

public class TileCollectionRenderer : MonoBehaviour
{
    [SerializeField] private GameObject _tilePrefab;
    public GameObject _tilesPanel;

    public Dictionary<Tile, TileGameObject> tileObjects = new Dictionary<Tile, TileGameObject>();
    public List<Tile> tiles { get => tileObjects.Keys.ToList(); }
    private List<NeighborSlotGameObject> _neighborSlots;
    private SelectedSlotGameObject _selectionSlot;

    [SerializeField] private Button _addTileButton;
    void OnEnable()
    {
        _neighborSlots = FindObjectsOfType<NeighborSlotGameObject>().OrderBy((v) => v.direction).ToList();
        _selectionSlot = FindObjectOfType<SelectedSlotGameObject>();
    }
    private void Start()
    {
        _addTileButton.onClick.AddListener(OnAdd);
    }
    private void OnRectTransformDimensionsChange()
    {
        foreach (TileGameObject obj in tileObjects.Values)
        {
            obj.ResetPosition();
        }
    }

    public void OnAdd()
    {
        var newTile = new Tile();
        var imageFile = EditorUtility.OpenFilePanelWithFilters("Select new tile image.", "", new[]{ "Image files", "png,jpg,jpeg" });
        if (!File.Exists(imageFile))
        {
            Debug.LogError("Chosen file does not exist");
            return;
        }
        var tileGO = Instantiate(original: _tilePrefab, parent: transform);
        
        tileGO.GetComponent<TileGameObject>().tile = newTile;
        tileGO.GetComponent<TileGameObject>().LoadImage(imageFile);
        tileObjects[newTile] = tileGO.GetComponent<TileGameObject>();
    }

}
