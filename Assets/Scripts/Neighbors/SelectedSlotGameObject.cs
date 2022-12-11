using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiles;
using UnityEngine.UI;

public class SelectedSlotGameObject : TileSlot
{
    [SerializeField] private Toggle _walkableToggle;
    [SerializeField] private Toggle _edgeTileToggle;
    private Color _defaultColor;
    public Tile Selected
    {
        get => _selected;
        set
        {
            var tcr = FindObjectOfType<TileCollectionRenderer>();
            _selected = null;
            GetComponent<Image>().sprite = value == null ? null : tcr
                .tileObjects[value]
                .GetComponent<Image>()
                .sprite;
            GetComponent<Image>().color = value == null ? _defaultColor : Color.white;
            _walkableToggle.isOn = value != null ? value.Walkable : false;
            _walkableToggle.interactable = value != null;
            _edgeTileToggle.isOn = value != null ? tcr.EdgeTile==value : false;
            _edgeTileToggle.interactable = value != null;
            _selected = value;
        }
    }

    private void Start()
    {
        _defaultColor = GetComponent<Image>().color;
        _walkableToggle.onValueChanged.AddListener((v) => { if (_selected != null) _selected.Walkable = v; });
    }

    private Tile _selected;
}
