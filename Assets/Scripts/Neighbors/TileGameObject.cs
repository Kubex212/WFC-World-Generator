using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tiles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileGameObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private string _imagePath = "";
    public int Index { get; private set; }
    private static int _nextIndex = 0;

    private static float _tileSpacing = 15;

    public Tile tile;

    public bool IsHovered { get; private set; }
    public bool IsDragged
    {
        get
        {
            return _isDragged;
        }
        private set
        {
            if (value == _isDragged) return;
            _isDragged = value;
            _offset = gameObject.transform.position - Input.mousePosition.ZeroZ();
            GetComponent<Image>().color = new Color(1, 1, 1, value?0.5f:1);
            if (_isDragged)
            {
                transform.SetParent(FindObjectOfType<Canvas>().transform);
            }
            else
            {
                transform.SetParent(FindObjectOfType<TileCollectionRenderer>()._tilesPanel.transform);
                ResetPosition();
            }
        }
    }

    private Vector3 _offset;
    public void LoadImage(string path)
    {
        var tex = new Texture2D(2, 2);
        byte[] data = File.ReadAllBytes(path);
        ImageConversion.LoadImage(tex, data);
        GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0,0,tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }
    public void OnPointerUp(PointerEventData _)
    {
        var neighborSlots = FindObjectsOfType<TileSlot>();
        var selectedSlot = FindObjectOfType<SelectedSlotGameObject>();
        var slot = neighborSlots.Where(v => v.IsHovered).FirstOrDefault();

        var neighborSlot = slot as NeighborSlotGameObject;
        if (slot is NeighborSlotGameObject)
        {
            selectedSlot.Selected.AddNeighbor(tile, neighborSlot.direction);
            tile.AddNeighbor(selectedSlot.Selected, neighborSlot.direction);
            Debug.Log($"dodano {tile} jako s¹siada w kierunku {neighborSlot.direction} od kafelka {selectedSlot.Selected}");
        }
        else if (slot == selectedSlot)
        {
            selectedSlot.Selected = tile;
            Debug.Log($"wybrano {tile} do edycji s¹siedztwa");
        }

        IsDragged = false;
    }
    public void OnPointerDown(PointerEventData _)
    {
        IsDragged = IsHovered;
    }

    public void OnPointerEnter(PointerEventData _)
    {
        IsHovered = true;
    }

    public void OnPointerExit(PointerEventData _)
    {
        IsHovered = false;
    }

    void Start()
    {
        Index = _nextIndex++;
        ResetPosition();
    }
    void Update()
    {
        if (IsDragged)
            gameObject.transform.position = Input.mousePosition.ZeroZ() + _offset;
    }
    public void ResetPosition()
    {
        var rect = transform.parent.GetComponent<RectTransform>().rect;
        float w = rect.width - _tileSpacing;
        float d = GetComponent<RectTransform>().rect.width + _tileSpacing;
        int columnCount = (int)(w / d);
        int y = Index / columnCount;
        int x = Index - y * columnCount;
        transform.localPosition = new Vector3(_tileSpacing + x * d, -(_tileSpacing + y * d), 0) + new Vector3(rect.xMin, rect.yMax, 0);
    }

    private bool _isDragged = false;
}
