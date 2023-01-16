using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Graphs;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using UnityEngine.Events;
using Tiles;
using UnityEngine.SceneManagement;

public class GraphRenderer : MonoBehaviour
{
    [SerializeField] private GameObject _vertexPrefab;
    [SerializeField] private GameObject _edgePrefab;
    [SerializeField] private GameObject _tempEdgePrefab;
    [SerializeField] private ErrorListComponent _errors;
    [SerializeField] private Button _addVertexButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _backButton;

    public UnityEvent<VertexGameObject> onDeleteVertex;
    public UnityEvent<EdgeGameObject> onDeleteEdge;
    public VertexGameObject selectedVertex = null;

    private TempEdgeGameObject _tempEdge = null;
    private List<VertexGameObject> _vertices = new List<VertexGameObject>();
    private List<EdgeGameObject> _edges = new List<EdgeGameObject>();
    private UndirectedGraph Graph { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        _addVertexButton.onClick.AddListener(OnAdd);
        _saveButton.onClick.AddListener(Save);
        _loadButton.onClick.AddListener(Load);
        _backButton.onClick.AddListener(Back);

        var dataHolder = FindObjectOfType<DataHolder>();

        if (dataHolder.Graph == null)
        {
            Graph = new UndirectedGraph();
            var v0 = Graph.AddVertex("v0");
            var v1 = Graph.AddVertex("v1");
            var v2 = Graph.AddVertex("v2");
            var v3 = Graph.AddVertex("v3");
            Graph.AddEdge(v1, v0);
            Graph.AddEdge(v1, v2);
            Graph.AddEdge(v1, v3);

            var positions = new Dictionary<string, (float X, float Y)>()
            {
                [v0.ToString()] = (-3f, -0.5f),
                [v1.ToString()] = (-0.5f, -0.5f),
                [v2.ToString()] = (2f, -0.5f),
                [v3.ToString()] = (-0.5f, -3f)
            };
            dataHolder.Graph = Graph;
            SpawnVertices(positions);
        }
        else
        {
            Graph = dataHolder.Graph;
            SpawnVertices(FindObjectOfType<DataHolder>().VertexPositions);
        }

        SetVertexRestriction(_vertices.Single(v => v.vertex.Name == "v0"), RestrictionType.Start);
        SetVertexRestriction(_vertices.Single(v => v.vertex.Name == "v2"), RestrictionType.End);

        SpawnEdges();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            selectedVertex = _vertices.Where(v => v != null && v.isSelected).FirstOrDefault();
            if(selectedVertex != null)
            {
                Debug.Log("started creating an edge...");
                _tempEdge = Instantiate(original: _tempEdgePrefab).GetComponent<TempEdgeGameObject>();
                _tempEdge._from = selectedVertex.gameObject;
            }
        }
        else if(Input.GetMouseButtonUp(1))
        {
            if(_tempEdge?.gameObject != null) Destroy(_tempEdge.gameObject);
            var targetVertex = _vertices.Where(v => v.isSelected).FirstOrDefault();
            if(selectedVertex != null && targetVertex != null && selectedVertex != targetVertex)
            {
                // add edge
                Debug.Log("add edge");
                AddEdgeGO(selectedVertex.vertex, targetVertex.vertex);
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

        var hoveredVertex = _vertices.Where(v => v.isSelected).FirstOrDefault();
        var hoveredEdge = _edges.Where(e => e.isSelected).FirstOrDefault();
        if (Input.GetKeyDown(KeyCode.Delete) && hoveredVertex != null)
        {
            DestroyVertex(hoveredVertex);
        }
        else if(Input.GetKeyDown(KeyCode.Delete) && hoveredEdge != null)
        {
            DestroyEdge(hoveredEdge);
        }

        if(Graph.ValidationErrors.Any())
        {
            _errors.tipToShow = string.Join("\n", Graph.ValidationErrors.ToArray());
            _errors.gameObject.SetActive(true);
        }
        else if(Graph.IsValid && _errors.gameObject.activeInHierarchy)
        {
            _errors.gameObject.SetActive(false);
        }

        _edges = FindObjectsOfType<EdgeGameObject>().ToList();
    }

    public void SetVertexRestriction(VertexGameObject vertexGO, RestrictionType type)
    {
        if (type == RestrictionType.Start)
        {
            if (vertexGO.vertex.IsStart) return;

            foreach (var v in _vertices)
            {
                v.vertex.IsStart = false;
                v.SetRestrictionInternal(RestrictionType.None);
            }

            vertexGO.vertex.IsStart = true;
            vertexGO.SetRestrictionInternal(type);
        }
        else if (type == RestrictionType.End)
        {
            if (vertexGO.vertex.IsExit) return;

            foreach (var v in _vertices)
            {
                v.vertex.IsExit = false;
            }

            vertexGO.vertex.IsExit = true;
            vertexGO.SetRestrictionInternal(type);
        }
        else if (type == RestrictionType.Key)
        {
            if (vertexGO.vertex.Key != null) return;
            // add key
            vertexGO.vertex.SetKey(Graph.LowestVertexAvailableKey);
            vertexGO.SetRestrictionInternal(type);
        }
    }

    public void SetEdgeRestriction(EdgeGameObject edge, RestrictionType type)
    {
        if(type == RestrictionType.Lock && edge.edge.Key == null)
        {
            edge.SetRestrictionInternal(Graph.LowestEdgeAvailableKey, type);
        }
    }

    private void OnAdd()
    {
        var newVertex = Graph.AddVertex();
        var vertexGO = Instantiate(original: _vertexPrefab);
        vertexGO.GetComponent<VertexGameObject>().vertex = newVertex;
        _vertices = new List<VertexGameObject>(FindObjectsOfType<VertexGameObject>());
    }

    private void Save()
    {
        var positions = new Dictionary<string, (float X, float Y)>();

        for(int i = 0; i < Graph.Vertices.Count; i++)
        {
            var vertex = Graph.Vertices[i];
            var correspondingVertexGO = _vertices.Where(v => v.vertex.Name == vertex.Name).FirstOrDefault();
            if(correspondingVertexGO == null)
            {
                Debug.LogError("[SAVE] could not find a corresponding vertex game object");
            }
            (float x, float y) pos = (correspondingVertexGO.transform.position.x, correspondingVertexGO.transform.position.y);
            positions.Add(vertex.ToString(), pos);
            //Graph.Vertices[i] = new Vertex(vertex.Name) { IsExit = vertex.IsExit, IsStart = vertex.IsStart, Key = vertex.Key, Position = pos };
        }

        var json = Graph.Serialize(positions);

        var path = EditorUtility.SaveFilePanel(
          "Save",
          "",
          "g" + ".json",
          "json");

        if (path.Length != 0)
        {
            File.WriteAllText(path, json);
        }

    }

    private void Load()
    {
        Clear();

        string path = EditorUtility.OpenFilePanel("Overwrite with png", "", "json");
        if (path.Length != 0)
        {
            var json = File.ReadAllText(path);
            Graph.Deserialize(json, out Dictionary<string, (float X, float Y)> positions);

            SpawnVertices(positions);
            SpawnEdges();

            FindObjectOfType<DataHolder>().Graph = Graph;
        }
    }

    private void Clear()
    {
        foreach(var v in _vertices)
        {
            Destroy(v.gameObject);
        }
        _vertices.Clear();

        foreach(var e in _edges)
        {
            Destroy(e.gameObject);
        }
        _edges.Clear();

        Graph = new UndirectedGraph();
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
            var sourceVertexGO = _vertices.Where(v => v.vertex == sourceVertex).First();
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
                var targetVertexGO = _vertices.Where(v => v.vertex.Name == targetVertex.Name).First();

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

    private void SpawnVertices(Dictionary<string, (float X, float Y)> positions = null)
    {
        foreach (var v in Graph.Vertices)
        {
            var vertex = Instantiate(_vertexPrefab);
            var vertexGO = vertex.GetComponent<VertexGameObject>();
            vertexGO.vertex = v;
            if(positions != null)
            {
                vertexGO.transform.position = new Vector3(positions[vertexGO.vertex.ToString()].X, positions[vertexGO.vertex.ToString()].Y, 0);
            }
            vertexGO.SetColor();
        }
        _vertices = new List<VertexGameObject>(FindObjectsOfType<VertexGameObject>());
    }

    private void DestroyVertex(VertexGameObject vertexToDestroy)
    {
        // delete its edges
        var edgesToDestroy = _edges.Where(e => e.from == vertexToDestroy || e.to == vertexToDestroy).ToList();
        foreach(var e in edgesToDestroy)
        {
            DestroyEdge(e);
        }

        // delete the vertex from the graph
        var deleted = Graph.DeleteVertex(vertexToDestroy.vertex);
        if(!deleted) Debug.LogError("[GRAPH] delete vertex returned false");

        // delete the vertex game object
        Destroy(vertexToDestroy.gameObject);
    }

    private void DestroyEdge(EdgeGameObject edgeToDestroy)
    {
        _edges.Remove(edgeToDestroy);
        Graph.DeleteEdge(edgeToDestroy.from.vertex, edgeToDestroy.to.vertex);
        Destroy(edgeToDestroy.gameObject);
    }

    private void Back()
    {
        FindObjectOfType<DataHolder>().VertexPositions = _vertices.ToDictionary(v => v.vertex.ToString(), v => (v.transform.position.x, v.transform.position.y));

        SceneManager.LoadScene("MainMenu");
    }
}
