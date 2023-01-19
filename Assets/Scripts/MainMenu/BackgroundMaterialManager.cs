using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BackgroundMaterialManager : MonoBehaviour
{
    void Update()
    {
        Vector4 v = Input.mousePosition;
        (v.x, v.y) = (v.x / Screen.width, v.y / Screen.height);
        GetComponent<CanvasRenderer>().GetMaterial()?.SetVector("_CursorPosition", v);
    }
}
