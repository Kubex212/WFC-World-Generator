using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Graphs;
using TMPro;

public class VertexGameObject : MonoBehaviour
{
    [SerializeField] private CircleCollider2D _circleCollider;
    [SerializeField] private Color _color = Color.white;
    [SerializeField] private Color _baseColor = Color.white;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private TextMeshPro _keyText;

    public bool isSelected = false;
    public Vertex vertex;

    private Vector3 _offset;


    void Start()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        _color = Color.white;
        SetColor();
    }

    void Update()
    {
        var MousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var allHits = Physics2D.RaycastAll(MousePos, Vector2.zero);
        foreach (var hit in allHits)
        {
            Debug.Log(hit.collider.gameObject.name);
            if (hit.collider.gameObject == this.gameObject)
            {
                Debug.Log("entered");
                isSelected = true;
                SetColor(0.2f);
                return;
            }
        }
        isSelected = false;
        SetColor();

        _keyText.text = vertex.Key.ToString();

    }

    public void SetRestrictionInternal(RestrictionType type)
    {
        if (type == RestrictionType.Start)
        {
            _color = Color.green;

        }
        else if (type == RestrictionType.End)
        {
            _color = Color.red;
        }
        else if (type == RestrictionType.Key)
        {
            // add key
        }
        else if(type == RestrictionType.None)
        {
            _color = _baseColor;
        }
        SetColor();
    }

    public Color SetColor(float darker = 0f)
    {
        var c = _color;
        if(vertex.IsStart) c = Color.green;
        else if(vertex.IsExit) c = Color.red;
        return _spriteRenderer.color = c.Darker(darker);
    }

    private void OnMouseEnter()
    {
        
    }

    private void OnMouseOver()
    {
        Debug.Log("enterdded");
    }

    private void OnMouseExit()
    {
        
    }

    private void OnMouseDrag()
    {
        if(true/*!_addButtonScript.IsActive*/) gameObject.transform.position = ZeroZ(GetMousePos()) + _offset;
    }
    private void OnMouseDown()
    {
        var offset = gameObject.transform.position - GetMousePos();
        _offset = new Vector3(offset.x, offset.y, 0);
    }

    private Vector3 GetMousePos()
    {
        var mousePos = Input.mousePosition;
        mousePos.z = 0;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }


    private Vector3 ZeroZ(Vector3 v)
    {
        return new Vector3(v.x, v.y, 0);
    }
}
