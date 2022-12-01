using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Q: AddEdgeButton?
public class AddButton : MonoBehaviour
{
    [SerializeField] private Color _color = Color.green;
    private GameObject _parent;
    private LineRenderer _lineRenderer;
    public bool IsActive { get; set; }
    

    void Start()
    {
        _parent = transform.parent.gameObject;
    }

    void Update()
    {
        if(_parent.GetComponent<VertexGameObject>().isSelected)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }

    }

    private void OnMouseEnter()
    {
        IsActive = true;
        Debug.Log("onmouseenter");

        //var spriteRenderer = GetComponent<SpriteRenderer>();
        //spriteRenderer.color = _color.Darker(0.2f);
    }

    private void OnMouseExit()
    {
        IsActive = false;
        Debug.Log("onmouse leave");

        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = _color;

        Destroy(_lineRenderer);
    }

    private void OnMouseDown()
    {
        if (_lineRenderer == null)
        {
            _lineRenderer = _parent.AddComponent<LineRenderer>();
            _lineRenderer.SetPosition(0, _parent.transform.position);
            _lineRenderer.SetPosition(1, ZeroZ(GetMousePos()));
        }
        else Debug.LogError("_lineRendered should be null here");
    }

    private void OnMouseDrag()
    {
        if(_lineRenderer != null)
        {
            _lineRenderer.SetPosition(1, ZeroZ(GetMousePos()));
        }
    }

    private void OnMouseUp()
    {
        Destroy(_lineRenderer);
    }

    private Vector3 GetMousePos()
    {
        var mousePos = Input.mousePosition;
        mousePos.z = 0;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    // Q: obsolete with Utility.cs
    private Vector3 ZeroZ(Vector3 v)
    {
        return new Vector3(v.x, v.y, 0);
    }
}
