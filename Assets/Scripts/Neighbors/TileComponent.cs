using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tiles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    public string imagePath = "";
    public int Index { get => tile.Index; }

    public static float _tileSpacing = 0.5f;

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
                transform.SetParent(FindObjectOfType<TileCollectionRenderer>().tilesPanel.transform);
                ResetPosition();
            }
        }
    }

    private Vector3 _offset;
    public void LoadImage(string path)
    {
        imagePath = path;
        var tex = new Texture2D(2, 2);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        byte[] data = File.ReadAllBytes(path);
        ImageConversion.LoadImage(tex, data);
        GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0,0,tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }
    public void OnPointerUp(PointerEventData _)
    {
        var allSlots = FindObjectsOfType<TileSlot>();
        var neighborSlots = FindObjectsOfType<NeighborSlotComponent>();
        var selectedSlot = FindObjectOfType<SelectedSlotComponent>();
        var slot = allSlots.Where(v => v.IsHovered).FirstOrDefault();

        var neighborSlot = slot as NeighborSlotComponent;
        if (slot is NeighborSlotComponent)
        {
            selectedSlot.Selected.AddNeighbor(tile, neighborSlot.direction);
            tile.AddNeighbor(selectedSlot.Selected, neighborSlot.direction.Opposite());
            neighborSlot.ShowNeighbors(selectedSlot.Selected);
            if (tile == selectedSlot.Selected)
                neighborSlots.FirstOrDefault((s) => s.direction == neighborSlot.direction.Opposite())
                    .ShowNeighbors(tile);
            Debug.Log($"dodano {tile} jako s¹siada w kierunku {neighborSlot.direction} od kafelka {selectedSlot.Selected}");
        }
        else if (slot == selectedSlot)
        {
            selectedSlot.Selected = tile;
            foreach (var s in neighborSlots)
                s.ShowNeighbors(selectedSlot.Selected);
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
        var spacing = GetComponent<RectTransform>().rect.width * _tileSpacing;
        float w = rect.width - spacing;
        float d = GetComponent<RectTransform>().rect.width + spacing;
        int columnCount = (int)(w / d);
        int y = Index / columnCount;
        int x = Index - y * columnCount;
        transform.localPosition = new Vector3(spacing + x * d, -(spacing + y * d), 0) + new Vector3(rect.xMin, rect.yMax, 0);
    }

    private bool _isDragged = false;
}
