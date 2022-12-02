using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiles;
using UnityEngine.UI;

public class SelectedSlotGameObject : TileSlot
{

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
        }
    }
    private Tile _selected;
}
