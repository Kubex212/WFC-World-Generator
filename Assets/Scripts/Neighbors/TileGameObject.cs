using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tiles;
using UnityEngine;
using UnityEngine.UI;

public class TileGameObject : MonoBehaviour
{
    [SerializeField] private string _imagePath = "";
    [SerializeField] private SelectedSlotGameObject _selectedTileSlot;

    public Tile tile;

    public bool IsSelected { get; private set; }

    private Vector3 _offset;
    private Vector3 _originalPosition;
    public void LoadImage(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        ImageConversion.LoadImage(GetComponent<Image>().sprite.texture, data);
    }

    void Start()
    {
        _originalPosition = transform.position;
    }

    private void OnMouseEnter()
    {
        IsSelected = true;
    }

    private void OnMouseExit()
    {
        IsSelected = false;
    }

    private void OnMouseUp()
    {
        var neighborSlots = FindObjectsOfType<TileSlot>();
        var slot = neighborSlots.Where(v => v._isSelected).FirstOrDefault();

        var neighborSlot = (NeighborSlotGameObject)slot;
        if (neighborSlot != null)
        {
            _selectedTileSlot.selected.AddNeighbor(tile, neighborSlot.direction);
            tile.AddNeighbor(_selectedTileSlot.selected, neighborSlot.direction);
            Debug.Log($"dodano {tile} jako s¹siada w kierunku {neighborSlot.direction} od kafelka {_selectedTileSlot.selected}");
        }
        else if (slot == _selectedTileSlot)
        {
            _selectedTileSlot.selected = tile;
            Debug.Log($"wybrano {tile} do edycji s¹siedztwa");
        }

        GetComponent<Image>().color = new Color(1, 1, 1, 1);
        transform.position = _originalPosition;
    }

    private void OnMouseDrag()
    {
        gameObject.transform.position = GetMousePos().ZeroZ() + _offset;
    }

    private void OnMouseDown()
    {
        var offset = gameObject.transform.position - GetMousePos();
        _offset = new Vector3(offset.x, offset.y, 0);

        GetComponent<Image>().color = new Color(1, 1, 1, .5f);
    }
    private Vector3 GetMousePos()
    {
        var mousePos = Input.mousePosition.ZeroZ();
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
