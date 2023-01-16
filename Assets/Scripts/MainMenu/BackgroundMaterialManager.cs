using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMaterialManager : MonoBehaviour
{
    [SerializeField] private float _dragSpeed = 0.001f;
    private Vector4 _position = new(0.5f,0.5f);
    void Update()
    {
        Vector4 v = Input.mousePosition;
        (v.x, v.y) = (v.x/Screen.width, v.y/Screen.height);
        _position = (v + _position) / 2;
        GetComponent<CanvasRenderer>().GetMaterial()?.SetVector("_CursorPosition", _position);
    }
}
