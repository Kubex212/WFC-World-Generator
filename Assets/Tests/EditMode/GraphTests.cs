using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Graphs;
public class GraphTests
{
    [Test]
    public void GraphIsUndirectional()
    {
        var graph = GetCompleteGraph(10);

        foreach(var v1 in graph.Vertices)
        {
            foreach (var v2 in graph.Vertices)
            {
                if(v1 != v2)
                {
                    Assert.AreEqual(graph.Edges[v1][v2], graph.Edges[v2][v1]);
                }
            }

        }
    }

    [Test]
    public void GraphTestConnectivity()
    {
        var connectedGraph1 = GetCompleteGraph(10);
        var connectedGraph2 = GetPathGraph();

        var unconnectedGraph = GetUnconnectedGraph();

        Assert.IsTrue(connectedGraph1.IsConnected);
        Assert.IsTrue(connectedGraph2.IsConnected);
        Assert.IsFalse(unconnectedGraph.IsConnected);
    }

    [Test]
    public void GraphAddingSameEdgeShouldFail()
    {
        var graph = GetEmptyGraph(2);

        var result = graph.AddEdge(graph.Vertices[0], graph.Vertices[1]);
        Assert.IsTrue(result != null);

        //Assert.Throws<System.ArgumentException>(() => graph.AddEdge(graph.Vertices[0], graph.Vertices[1]));
        result = graph.AddEdge(graph.Vertices[0], graph.Vertices[1]);
        Assert.IsTrue(result == null);

    }


    private UndirectedGraph GetCompleteGraph(int vertexCount)
    {
        var graph = new UndirectedGraph();

        for (int i = 0; i < vertexCount; i++)
        {
            graph.AddVertex();
        }

        for (int i = 0; i < vertexCount; i++)
        {
            for(int j = i + 1; j < vertexCount; j++)
            {
                graph.AddEdge(graph.Vertices[i], graph.Vertices[j]);
            }
        }

        return graph;
    }

    private UndirectedGraph GetUnconnectedGraph()
    {
        var graph = new UndirectedGraph();

        var v0 = graph.AddVertex();
        var v1 = graph.AddVertex();
        var v2 = graph.AddVertex();

        graph.AddEdge(v0, v1);
        graph.AddEdge(v1, v2);

        var v3 = graph.AddVertex();

        return graph;
    }

    private UndirectedGraph GetPathGraph()
    {
        var graph = new UndirectedGraph();

        var v0 = graph.AddVertex();
        var v1 = graph.AddVertex();
        var v2 = graph.AddVertex();
        var v3 = graph.AddVertex();
        var v4 = graph.AddVertex();

        graph.AddEdge(v0, v1);
        graph.AddEdge(v1, v2);
        graph.AddEdge(v2, v3);
        graph.AddEdge(v3, v4);


        return graph;
    }

    private UndirectedGraph GetEmptyGraph(int vertexCount)
    {
        var graph = new UndirectedGraph();
        for(int i = 0; i < vertexCount; i++)
        {
            graph.AddVertex();
        }
        return graph;
    }
}
