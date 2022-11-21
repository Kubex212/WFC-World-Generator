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
    [SerializeField] private GameObject _tempEdgePrefab;
    [SerializeField] private Button _addVertexButton; 

    public VertexGameObject selectedVertex = null;

    private TempEdgeGameObject _tempEdge = null;
    private List<VertexGameObject> _vertices = new List<VertexGameObject>();
    private List<EdgeGameObject> _edges = new List<EdgeGameObject>();
    private UndirectedGraph Graph { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        _addVertexButton.onClick.AddListener(OnAdd);

        Graph = new UndirectedGraph();
        var v0 = Graph.AddVertex("v0");
        var v1 = Graph.AddVertex("v1");
        Graph.AddEdge(v0, v1);

        foreach (var v in Graph.Vertices)
        {
            if (true) // nie pamietam co tu mialo byc - pewnie nic
            {
                var vertex = Instantiate(_vertexPrefab);
                vertex.GetComponent<VertexGameObject>()._vertex = v;
            }
        }

        _vertices = new List<VertexGameObject>(FindObjectsOfType<VertexGameObject>());
        SpawnEdges();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            selectedVertex = _vertices.Where(v => v._isSelected).FirstOrDefault();
            if(selectedVertex != null)
            {
                Debug.Log("started creating an edge...");
                _tempEdge = Instantiate(original: _tempEdgePrefab).GetComponent<TempEdgeGameObject>();
                _tempEdge._from = selectedVertex.gameObject;
            }
        }
        else if(Input.GetMouseButtonUp(1))
        {
            Destroy(_tempEdge.gameObject);
            var targetVertex = _vertices.Where(v => v._isSelected).FirstOrDefault();
            if(selectedVertex != null && targetVertex != null && selectedVertex != targetVertex)
            {
                // add edge
                Debug.Log("add edge");
                AddEdgeGO(selectedVertex._vertex, targetVertex._vertex);
            }
            else
            {
                Debug.Log("released");
                selectedVertex = null;
            }
        }
        else if (Input.GetMouseButtonUp(2))
        {
            SpawnEdges();
        }

        if(Input.GetKeyDown(KeyCode.Delete) && selectedVertex != null)
        {
            if (!Graph.DeleteVertex(selectedVertex._vertex)) Debug.LogError("[GRAPH] delete vertex returned false");
        }

    }

    public void SetVertexRestriction(VertexGameObject vertex, RestrictionType type)
    {
        if (type == RestrictionType.Start)
        {
            foreach (var v in _vertices)
            {
                v._vertex.IsStart = false;
            }

            vertex._vertex.IsStart = true;
            vertex.SetRestrictionInternal(type);
        }
        else if (type == RestrictionType.End)
        {
            foreach (var v in _vertices)
            {
                v._vertex.IsExit = false;
            }

            vertex._vertex.IsExit = true;
            vertex.SetRestrictionInternal(type);
        }
        else if (type == RestrictionType.Key)
        {
            // add key

            vertex.SetRestrictionInternal(type);
        }
    }

    public void SetEdgeRestriction(EdgeGameObject edge, RestrictionType type)
    {
        if(type == RestrictionType.Lock)
        {
            edge.SetRestrictionInternal(Graph.LowestAvailableKey, type);
        }
    }

    public void OnAdd()
    {
        var newVertex = Graph.AddVertex();
        var vertexGO = Instantiate(original: _vertexPrefab);
        vertexGO.GetComponent<VertexGameObject>()._vertex = newVertex;
        _vertices = new List<VertexGameObject>(FindObjectsOfType<VertexGameObject>());
    }

    private void AddEdgeGO(Vertex v1, Vertex v2)
    {
        Graph.AddEdge(v1, v2);
        SpawnEdges();
    }

    private void SpawnEdges()
    {
        foreach(var edge in _edges)
        {
            if(edge != null) Destroy(edge.gameObject);
        }

        _edges.Clear();

        // iteration over 'Edges'
        foreach(var pair in Graph.Edges)
        {
            var sourceVertex = pair.Key;
            var sourceVertexGO = _vertices.Where(v => v._vertex == sourceVertex).First().gameObject;
            var neighbors = pair.Value;

            foreach(var edgePair in neighbors)
            {
                var edgeObj = edgePair.Value;
                var spawnedEdgeObjs = _edges.Select(e => e.edge);
                if(spawnedEdgeObjs.Contains(edgeObj))
                {
                    continue;
                }

                // get target vertex
                var targetVertex = edgePair.Key;
                var targetVertexGO = _vertices.Where(v => v._vertex == targetVertex).First().gameObject;

                // spawn edge and set its properties
                var edgeGO = Instantiate(original: _edgePrefab);
                var edgeGOScript = edgeGO.GetComponent<EdgeGameObject>();
                edgeGOScript.from = sourceVertexGO;
                edgeGOScript.to   = targetVertexGO;
                edgeGOScript.edge = edgeObj;
                edgeGOScript.SetColor();

                // add new edge to '_edges' field so in the next iterations we know wheter such edge has already been spawned
                _edges.Add(edgeGOScript);
            }
        }

        _edges = FindObjectsOfType<EdgeGameObject>().ToList(); // this is probably not necessary
    }
}
