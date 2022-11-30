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
        public int LowestAvailableKey { get => Vertices.Where(v => v.Key != null).Select(v => v.Key.Value).DefaultIfEmpty().Max() + 1; }

        public List<Edge> EdgeList
        {
            get
            {
                var result = new List<Edge>();
                Edges.Values.ToList().ForEach(pair => result.AddRange(pair.Values));
                return result.Distinct().ToList();
            }
        }


        public UndirectedGraph()
        {

        }
        public Vertex AddVertex(string name = null)
        {
            var vertex = new Vertex(name ?? $"v{Vertices.Count}");
            Edges[vertex] = new Dictionary<Vertex, Edge>();
            return vertex;
        }

        public Edge AddEdge(Vertex v1, Vertex v2)
        {
            var newEdge = new Edge();
            if (v1 == v2) return null;
            try
            {
                Edges[v1].Add(v2, newEdge);
                Edges[v2].Add(v1, newEdge);
            }
            catch (System.ArgumentException) // tak moze byc?
            {
                return null;
            }
            return newEdge;
        }

        public bool DeleteVertex(Vertex v)
        {
            if(!Vertices.Contains(v)) return false;

            bool result = true;

            foreach(var pair in Edges)
                pair.Value.Remove(v);

            result &= Edges.Remove(v);

            return result;
        }
    }

    public struct Vertex
    {
        public readonly string Name { get; }
        public bool IsStart { get; set; }
        public bool IsExit { get; set; }
        public int? Key { get; set; }

        public Vertex(string name)
        {
            Name = name;
            IsStart = false;
            IsExit = false;
            Key = null;
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
        public int? Key { get; set; }
    }
}
