using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileSlot : MonoBehaviour
{
    public bool _isSelected = false;

    private void OnMouseEnter()
    {
        _isSelected = true;
        Debug.Log("slot enter");
    }

    private void OnMouseExit()
    {
        _isSelected = false;
        Debug.Log("slot exit");
    }
    //TODO: przyda³oby siê dla sensownego debugowania
    public override string ToString()
    {
        return base.ToString();
    }
}
