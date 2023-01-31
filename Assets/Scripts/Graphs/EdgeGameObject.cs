using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Graphs;
using TMPro;

public class EdgeGameObject : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private MeshCollider _meshCollider;
    [SerializeField] private Color _baseColor;
    [SerializeField] private Color _colorWithKey;
    [SerializeField] private GraphRenderer _graphRenderer;
    [SerializeField] private TextMeshPro _lockText;
    [SerializeField] private GameObject _lockPanel;
    [Range(0.01f, 0.5f)]
    [SerializeField] private float _thickness;

    public VertexGameObject from;
    public VertexGameObject to;
    public Edge edge;
    public bool isSelected; 

    private void Awake()
    {
        SetColor();
        _graphRenderer = FindObjectOfType<GraphRenderer>();
    }

    private void Update()
    {
        if(from == null || to == null)
        {
            //Debug.LogError("[EDGE] one of the vertex is null");
            return;
        }
        var f = from.transform.position;
        var t = to.transform.position;
        var r = 0.95f * from.transform.localScale.x / 2;
        var actualFrom = f + (t - f).normalized * r;
        var actualTo = t + (f - t).normalized * r;
        _lineRenderer.SetPosition(0, actualFrom);
        _lineRenderer.SetPosition(1, actualTo);
        _lineRenderer.startWidth = _lineRenderer.endWidth = _thickness;

        Mesh mesh = new Mesh();
        _lineRenderer.BakeMesh(mesh, true);
        if (new List<Vector3>(mesh.vertices).Distinct().ToList().Count > 2)
        {
            _meshCollider.sharedMesh = mesh;
        }

        _lockPanel.transform.position = Vector3.Lerp(from.transform.position, to.transform.position, 0.5f);

        if (edge.Key != null)
        {
            _lockPanel.SetActive(true);
        }
        else
        {
            _lockPanel.SetActive(false);
        }

        _lockText.text = edge.Key?.ToString();
    }

    public void SetRestrictionInternal(int keyNumber, RestrictionType type)
    {
        if (type == RestrictionType.Lock)
        {
            Debug.Log($"added key number {keyNumber} to an edge");
            edge.Key = keyNumber;
            SetColor();
        }
        else
        {
            Debug.LogError("[EDGE] restriction type was not 'lock'");
        }
    }

    public Color SetColor(float darker = 0f)
    {
        var c = _baseColor;
        if (edge?.Key != null) c = _colorWithKey;
        return _lineRenderer.startColor = _lineRenderer.endColor = c.Lighter(darker);
    }

    private void OnMouseEnter()
    {
        //if(_graphRenderer.selectedVertex != null)
        //    return;

        isSelected = true;
        SetColor(0.2f);
        Debug.Log("edge enter");
    }

    private void OnMouseExit()
    {
        isSelected = false;
        SetColor();
        Debug.Log("edge exit");
    }
}
