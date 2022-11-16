using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Graphs;

public class VertexGameObject : MonoBehaviour
{
    [SerializeField] private CircleCollider2D _circleCollider;
    [SerializeField] private Color _color = Color.white;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] GameObject _addButton;

    public bool _isSelected = false;
    public Vertex _vertex;

    private Vector3 _offset;
    private AddButton _addButtonScript;


    void Start()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        _color = Color.white;
        _addButtonScript = _addButton.GetComponent<AddButton>();
    }

    void Update()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if(_isSelected)
        {
            _addButton.SetActive(true);
            spriteRenderer.color = _color.Darker(0.2f);
        }
        else
        {
            _addButton.SetActive(false);
            spriteRenderer.color = _color;
        }
    }

    private void OnMouseEnter()
    {
        if (_addButtonScript.IsActive)
        {
            return;
        }

        _isSelected = true;
        _addButton.SetActive(true);
        _spriteRenderer.color = _color.Darker(0.2f);
    }

    private void OnMouseExit()
    {
        if (_addButtonScript.IsActive)
        {
            return;
        }

        _isSelected = false;
        _addButton.SetActive(false);
        _spriteRenderer.color = _color;
    }


    private void OnMouseDrag()
    {
        Debug.Log(_addButtonScript.IsActive);
        if(true/*!_addButtonScript.IsActive*/) gameObject.transform.position = ZeroZ(GetMousePos()) + _offset;
    }

    private Vector3 GetMousePos()
    {
        var mousePos = Input.mousePosition;
        mousePos.z = 0;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    private void OnMouseDown()
    {
        var offset = gameObject.transform.position - GetMousePos();
        _offset = new Vector3(offset.x, offset.y, 0);
    }

    private Vector3 ZeroZ(Vector3 v)
    {
        return new Vector3(v.x, v.y, 0);
    }
}
