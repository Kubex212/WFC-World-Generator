using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiles;
using UnityEngine.UI;
using TMPro;

public class SelectedSlotComponent : TileSlot
{
    [SerializeField] private Toggle _walkableToggle;
    [SerializeField] private Toggle _edgeTileToggle;
    [SerializeField] private Toggle _doorTileToggle;
    [SerializeField] private Toggle _diagonalityToggle;
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
            _walkableToggle.interactable = value != null && tcr.EdgeTile!=value && !value.IsDoor;
            _edgeTileToggle.isOn = value != null ? tcr.EdgeTile==value : false;
            _edgeTileToggle.interactable = value != null && !value.Walkable && !value.IsDoor;
            _doorTileToggle.isOn = value != null ? value.IsDoor : false;
            _doorTileToggle.interactable = value != null && tcr.EdgeTile != value;
            _doorTileToggle.GetComponentInChildren<TextMeshProUGUI>().text = value != null ? value.Walkable ? "Klucz" : "Drzwi" : "Klucz/Drzwi";
            _diagonalityToggle.isOn = tcr.Diagonal;
            foreach (var nslot in FindObjectsOfType<NeighborSlotComponent>(true))
                nslot.gameObject.SetActive(
                    value != null && !value.IsDoor &&
                    (tcr.Diagonal || (int)nslot.direction % 2 == 0)
                    );
            _selected = value;
        }
    }

    private void Awake()
    {
        _defaultColor = GetComponent<Image>().color;
        _walkableToggle.onValueChanged.AddListener((v) =>
        {
            if (_selected != null)
            {
                _selected.Walkable = v;
                Selected = Selected;
            }
        });
        _edgeTileToggle.onValueChanged.AddListener((v) =>
        {
            if (_selected != null)
            {
                var tcr = FindObjectOfType<TileCollectionRenderer>();
                if (v)
                    tcr.EdgeTile = _selected;
                else if (tcr.EdgeTile == _selected)
                    tcr.EdgeTile = null;

                Selected = Selected;
            }
        });
        _doorTileToggle.onValueChanged.AddListener((v) =>
        {
            if(_selected != null)
            {
                _selected.IsDoor = v;
                Selected = Selected;
            }
        });
        _diagonalityToggle.onValueChanged.AddListener((v) =>
        {
            var tcr = FindObjectOfType<TileCollectionRenderer>();
            tcr.Diagonal = v;
            Selected = Selected;
        });
    }

    private Tile _selected;
}
