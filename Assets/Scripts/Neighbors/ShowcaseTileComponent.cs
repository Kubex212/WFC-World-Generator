using System.Collections;
using System.Collections.Generic;
using Tiles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShowcaseTileComponent : MonoBehaviour, IPointerClickHandler
{
    public Tile tile;
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            var selectedSlot = FindObjectOfType<SelectedSlotComponent>();
            var parentSlot = GetComponentInParent<NeighborSlotComponent>();
            selectedSlot.Selected.RemoveNeighbor(tile, parentSlot.direction);
            tile.RemoveNeighbor(selectedSlot.Selected, parentSlot.direction.Opposite());
            parentSlot.ShowNeighbors(selectedSlot.Selected);
        }
    }
}
