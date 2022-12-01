using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class TileSlot : MonoBehaviour
{

    public bool IsHovered
    {
        get => _isHovered;
        set
        {
            if (_isHovered != value)
            {
                _isHovered = value;
                //Debug.Log(value ? "selected" : "deselected");
            }
        }
    }

    //TODO: przyda³oby siê dla sensownego debugowania
    public override string ToString()
    {
        return base.ToString();
    }
    private void Update()
    {
        var mouse = GetComponent<RectTransform>().InverseTransformPoint(Input.mousePosition);
        IsHovered = GetComponent<RectTransform>().rect.Contains(mouse);
    }
    private bool _isHovered = false;
}
