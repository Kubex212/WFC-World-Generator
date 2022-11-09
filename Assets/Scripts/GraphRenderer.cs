using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Graphs;
using UnityEngine.UI;

public class GraphRenderer : MonoBehaviour
{
    [SerializeField] private GameObject _vertexPrefab;
    [SerializeField] private Button _addVertexButton; 
    private UndirectedGraph Graph { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        _addVertexButton.onClick.AddListener(OnAdd);

        Graph = new UndirectedGraph();
        var v0 = Graph.AddVertex("v0");
        var v1 = Graph.AddVertex("v1");

        foreach (var v in Graph.Vertices)
        {
            if (true)
            {
                if (_vertexPrefab != null) Instantiate(_vertexPrefab);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnAdd()
    {
        Graph.AddVertex();
    }
}
