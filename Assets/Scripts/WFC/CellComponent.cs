using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellComponent : MonoBehaviour
{
    public HashSet<int> _superposition = new HashSet<int>();
    private int _maxPossibilities;
    private Func<int, string> _roomNameFunc;
    public bool Paradox
    {
        get => _paradox;
        set
        {
            _paradox = value;
            SetVisuals();
        }
    }

    public CellType Type 
    {
        get => _cellType;
        set
        {
            _cellType = value;
            //SetVisuals();
        }
    }

    public void Fill(int tileCount, Func<int, string> roomNameFunc)
    {
        _roomNameFunc = roomNameFunc;
        _maxPossibilities = tileCount;
        for (int i = 0; i < tileCount; i++)
        {
            _superposition.Add(i);
        }
        _paradox = false;
        Type = CellType.None;
        SetVisuals();
    }
    public void Remove(IEnumerable<int> indexes)
    {
        _superposition.ExceptWith(indexes);

        SetVisuals();
    }

    public void Add(IEnumerable<int> indexes)
    {
        _superposition.UnionWith(indexes);

        SetVisuals();
    }

    private void SetVisuals()
    {
        
        if (_superposition.Count == 0 || _paradox)
        {
            _paradox = true;
            GetComponent<Image>().sprite = null;
            SetRoom(null);
            GetComponent<Image>().color = Color.yellow;
        }
        else if (_superposition.Count == 1)
        {
            var sp = GetComponent<Image>().sprite = SpriteAtlas.Atlas[_superposition.First()];
            SetRoom(_superposition.First());
            var c = GetComponent<Image>().color;
            GetComponent<Image>().color = Color.white;
        }
        else
        {
            GetComponent<Image>().sprite = null;
            SetRoom(null);
            var value = (_superposition.Count - 1f) / (_maxPossibilities - 1);
            GetComponent<Image>().color = Color.Lerp(ColorPalette.Gray, value > 0.5f ? ColorPalette.Red : ColorPalette.Blue, Mathf.Abs(value-0.5f)*2);
        }
    }

    public void SetRoom(int? room)
    {
        GetComponentInChildren<TextMeshProUGUI>().text = room.HasValue ? _roomNameFunc(room.Value) : "?";
    }
    private bool _paradox = false;
    private CellType _cellType = CellType.None;
}

public enum CellType
{
    None,
    Start,
    End,
    Center
}