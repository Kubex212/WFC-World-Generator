using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Graphs;

public class GraphRenderer : MonoBehaviour
{
    [SerializeField] private GameObject vertexPrefab;
    private UndirectedGraph Graph { get; set; }
    private List<Vertex> Vertices { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Vertices = new List<Vertex>();
        Vertices.Add(new Vertex("v1"));
        Vertices.Add(new Vertex("v2"));
        Graph = new UndirectedGraph(Vertices);
        Graph.AddEdge(Graph.Vertices.ToList()[0], Graph.Vertices.ToList()[1]);
    }

    // Update is called once per frame
    void Update()
    {
        foreach(var v in Graph.Vertices)
        {
            if(true)
            {
                if (vertexPrefab != null) Instantiate(vertexPrefab);
            }
        }
    }
}
