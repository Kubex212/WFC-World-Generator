using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Graphs;
using UnityEngine.UI;

public class GraphRenderer : MonoBehaviour
{
    [SerializeField] private GameObject _vertexPrefab;
    [SerializeField] private GameObject _edgePrefab;
    [SerializeField] private Button _addVertexButton; 

    private List<VertexGameObject> _vertexList = new List<VertexGameObject>();
    private VertexGameObject _selectedVertex = null;
    private List<LineRenderer> _lines = new List<LineRenderer>();
    private UndirectedGraph Graph { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        _addVertexButton.onClick.AddListener(OnAdd);

        Graph = new UndirectedGraph();
        var v0 = Graph.AddVertex("v0");
        var v1 = Graph.AddVertex("v1");
        Graph.AddEdge(v0, v1, new Edge());

        foreach (var v in Graph.Vertices)
        {
            if (true)
            {
                var vertex = Instantiate(_vertexPrefab);
                vertex.GetComponent<VertexGameObject>()._vertex = v;
            }
        }

        _vertexList = new List<VertexGameObject>(FindObjectsOfType<VertexGameObject>());
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            _selectedVertex = _vertexList.Where(v => v._isSelected).FirstOrDefault();
            if(_selectedVertex != null)
            {
                Debug.Log("waiting to release...");
            }
        }
        else if(Input.GetMouseButtonUp(1))
        {
            var targetVertex = _vertexList.Where(v => v._isSelected).FirstOrDefault();
            if(_selectedVertex != null && targetVertex != null && _selectedVertex != targetVertex)
            {
                // add edge
                Debug.Log("add edge");
                Graph.AddEdge(_selectedVertex._vertex, targetVertex._vertex, new Edge());
            }
            else
            {
                Debug.Log("released");
                _selectedVertex = null;
            }
        }
        else if (Input.GetMouseButtonUp(2))
        {
            DrawGraph();
        }

    }

    public void OnAdd()
    {
        Graph.AddVertex();
        _vertexList = new List<VertexGameObject>(FindObjectsOfType<VertexGameObject>());
    }

    private void DrawGraph()
    {
        foreach(var pair in Graph.Edges)
        {
            var sourceVertex = pair.Key;
            var sourceVertexGO = _vertexList.Where(v => v._vertex == sourceVertex).First().gameObject;
            var neighbors = pair.Value;

            foreach(var edge in neighbors)
            {
                var targetVertex = edge.Key;
                var targetVertexGO = _vertexList.Where(v => v._vertex == targetVertex).First().gameObject;
                var edgeGO = Instantiate(original: _edgePrefab);
                var edgeGOScript = edgeGO.GetComponent<EdgeGameObject>();
                edgeGOScript._from = sourceVertexGO;
                edgeGOScript._to   = targetVertexGO;
            }
        }
    }
}
