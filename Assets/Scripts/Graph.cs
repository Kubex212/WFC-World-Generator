using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graphs
{
    public class UndirectedGraph
    {
        public HashSet<Vertex> Vertices { get; private set; }

        public UndirectedGraph(ICollection<Vertex> vertices)
        {
            Vertices = vertices.ToHashSet();
        }
        public bool AddVertex(Vertex v)
        {
            return Vertices.Add(v);
        }

        public bool AddEdge(Vertex v1, Vertex v2)
        {
            if (v1 == v2) return false;
            return v1.AddNeighbor(v2) && v2.AddNeighbor(v1);
        }

        public bool RemoveEdge(Vertex v1, Vertex v2)
        {
            return v1.RemoveNeighbor(v2) && v2.RemoveNeighbor(v1);
        }
    }

    public struct Vertex
    {
        public readonly string Name { get; }
        public HashSet<Vertex> Neighbors { get; private set; }
        public bool AddNeighbor(Vertex v) { return Neighbors.Add(v); }
        public bool RemoveNeighbor(Vertex v) { return Neighbors.Remove(v); }
        public Vertex(string name)
        {
            Name = name;
            Neighbors = new HashSet<Vertex>();
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
}
