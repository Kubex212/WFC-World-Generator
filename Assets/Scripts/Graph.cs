using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graphs
{
    public class UndirectedGraph
    {
        public Dictionary<Vertex, Dictionary<Vertex, Edge>> Edges { get; private set; } = new Dictionary<Vertex, Dictionary<Vertex, Edge>>();
        public List<Vertex> Vertices { get => Edges.Keys.ToList(); }

        public UndirectedGraph()
        {

        }
        public Vertex AddVertex(string name = null)
        {
            var vertex = new Vertex(name ?? $"v{Vertices.Count}");
            Edges[vertex] = new Dictionary<Vertex, Edge>();
            return vertex;
        }

        public bool AddEdge(Vertex v1, Vertex v2, Edge edge)
        {
            if (v1 == v2) return false;
            try
            {
                Edges[v1].Add(v2, edge);
                Edges[v2].Add(v1, edge);
            }
            catch (System.ArgumentException)
            {
                return false;
            }
            return true;
        }
    }

    public struct Vertex
    {
        public readonly string Name { get; }

        public Vertex(string name)
        {
            Name = name;
        }

        public static bool operator ==(Vertex left, Vertex right)
        {
            return left.Name == right.Name;
        }

        public static bool operator !=(Vertex left, Vertex right)
        {
            return left.Name != right.Name;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class Edge
    {
        public object Key { get; private set; }
    }
}
