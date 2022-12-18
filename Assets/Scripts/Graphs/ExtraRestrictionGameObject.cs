using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExtraRestrictionGameObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private Image _image;
    [SerializeField] private GraphRenderer _graphRenderer;
    [SerializeField] public RestrictionType restrictionType;
    
    public Target Target { get => restrictionType == RestrictionType.Lock ? Target.Edge : Target.Vertex; }
    public bool IsSelected { get; private set; }
    public bool IsDragged
    {
        get
        {
            return _isDragged;
        }
        private set
        {
            if (value == _isDragged) return;
            _isDragged = value;
            _offset = gameObject.transform.position - Input.mousePosition.ZeroZ();
            _image.color = new Color(1, 1, 1, value ? 0.5f : 1);
            if (_isDragged)
            {
                //transform.SetParent(FindObjectOfType<Canvas>().transform);
            }
            else
            {
                transform.position = _originalPosition;
            }
        }
    }

    private Vector3 _offset;
    private Vector3 _originalPosition;
    private bool _isDragged;

    void Start()
    {
        _originalPosition = transform.position;
        //_spriteRenderer.size = new Vector2(1f, 1f);
    }

    void Update()
    {
        if(IsDragged)
            gameObject.transform.position = Input.mousePosition.ZeroZ() + _offset;
    }

    private void OnMouseEnter()
    {
        IsSelected = true;
        Debug.Log(IsSelected);
    }

    private void OnMouseExit()
    {
        IsSelected = false;
    }

    private void OnMouseUp()
    {
        if(Target == Target.Vertex)
        {
            var vertices = FindObjectsOfType<VertexGameObject>();
            var selectedVertex = vertices.Where(v => v.isSelected).FirstOrDefault();
            if(selectedVertex != null)
            {
                _graphRenderer.SetVertexRestriction(selectedVertex, restrictionType);
                Debug.Log($"ustalono ograniczenie typu {restrictionType}");
            }
        }
        else
        {
            var edges = FindObjectsOfType<EdgeGameObject>();
            var selectedEdge = edges.Where(e => e.isSelected).FirstOrDefault();
            if(selectedEdge != null)
            {
                _graphRenderer.SetEdgeRestriction(selectedEdge, restrictionType);
            }
        }

        _image.color = new Color(1, 1, 1, 1);
        transform.position = _originalPosition;

        IsDragged = false;
    }

    private void OnMouseDrag()
    {
        gameObject.transform.position = GetMousePos().ZeroZ() + _offset;
    }

    private void OnMouseDown()
    {
        var offset = gameObject.transform.position - GetMousePos();
        _offset = new Vector3(offset.x, offset.y, 0);

        IsDragged = true;
        //_spriteRenderer.color = new Color(1, 1, 1, .5f);
    }

    public void OnPointerUp(PointerEventData _)
    {
        OnMouseUp();
    }
    public void OnPointerDown(PointerEventData _)
    {
        OnMouseDown();
    }

    public void OnPointerEnter(PointerEventData _)
    {
        OnMouseEnter();
    }

    public void OnPointerExit(PointerEventData _)
    {
        OnMouseExit();
    }

    private Vector3 GetMousePos()
    {
        var mousePos = Input.mousePosition.ZeroZ();
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}

public enum RestrictionType
{
    Key,
    Lock,
    Start,
    End,
    None
}

public enum Target
{
    Vertex,
    Edge
}

