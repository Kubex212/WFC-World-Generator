using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiles;
using UnityEngine.UI;

public class SelectedSlotGameObject : TileSlot
{
    [SerializeField] private Toggle _walkableToggle;
    public Tile Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            GetComponent<Image>().sprite = value==null?null:
                FindObjectOfType<TileCollectionRenderer>()
                .tileObjects[value]
                .GetComponent<Image>()
                .sprite;
            _walkableToggle.isOn = value != null ? value.Walkable : false;
            _walkableToggle.interactable = value != null;
        }
    }

    private void Start()
    {
        _walkableToggle.onValueChanged.AddListener((v) => { if (_selected != null) _selected.Walkable = v; });
    }

    private Tile _selected;
}
