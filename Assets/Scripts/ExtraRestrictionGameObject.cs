using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExtraRestrictionGameObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private GraphRenderer _graphRenderer;
    [SerializeField] public RestrictionType restrictionType;
    
    public Target Target { get => restrictionType == RestrictionType.Lock ? Target.Edge : Target.Vertex; }
    public bool IsSelected { get; private set; }

    private Vector3 _offset;
    private Vector3 _originalPosition;

    void Start()
    {
        _originalPosition = transform.position;
        _spriteRenderer.size = new Vector2(1f, 1f);
    }

    void Update()
    {

    }

    private void OnMouseEnter()
    {
        IsSelected = true;
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

        _spriteRenderer.color = new Color(1, 1, 1, 1);
        transform.position = _originalPosition;
    }

    private void OnMouseDrag()
    {
        gameObject.transform.position = GetMousePos().ZeroZ() + _offset;
    }

    private void OnMouseDown()
    {
        var offset = gameObject.transform.position - GetMousePos();
        _offset = new Vector3(offset.x, offset.y, 0);

        _spriteRenderer.color = new Color(1, 1, 1, .5f);
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

