using System.Collections;
using System.Collections.Generic;
using Tiles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NeighborTileGameObject : MonoBehaviour, IPointerClickHandler
{
    public Tile tile;
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            var selectedSlot = FindObjectOfType<SelectedSlotGameObject>();
            var parentSlot = GetComponentInParent<NeighborSlotGameObject>();
            selectedSlot.Selected.RemoveNeighbor(tile, parentSlot.direction);
            tile.RemoveNeighbor(selectedSlot.Selected, parentSlot.direction.Opposite());
            parentSlot.ShowNeighbors(selectedSlot.Selected);
        }
    }
}
