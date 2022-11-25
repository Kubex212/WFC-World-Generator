using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

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
            var vertex = new Vertex(name);
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

        public bool DeleteEdge(Vertex v1, Vertex v2)
        {
            try
            {
                var edge = Edges[v1][v2];
                DeleteKey(edge.Key);
            }
            catch (KeyNotFoundException) { return false; }

            bool deleted = Edges[v1].Remove(v2);
            if(deleted != Edges[v2].Remove(v1))
            {
                throw new System.Exception("graph contained one-sided edge");
            }
            return deleted;
        }

        public bool DeleteKey(int? key)
        {
            if (key == null) return false;

            var edge = EdgeList.Where(e => e.Key == key).FirstOrDefault();
            var vertex = Vertices.Where(e => e.Key == key).FirstOrDefault();

            if(edge == null && vertex == null) return false;
            if(edge != null && vertex != default(Vertex))
            {
                vertex.SetKey(null);
                edge.Key = null;

                for (int i = 0; i < Vertices.Count; i++)
                {
                    if (Vertices[i].Key != null) Vertices[i].SetKey(Vertices[i].Key - 1);
                }

                for (int i = 0; i < EdgeList.Count; i++)
                {
                    if (EdgeList[i].Key != null) EdgeList[i].Key--;
                }
                return true;
            }
            else
            {
                //throw new System.Exception("a key existed only in vertex or only in edge");
                return false;
            }
        }

        public string Serialize(Dictionary<Vertex, (float X, float Y)> positions = null)
        {
            var exportObject = new ExportObject() { Edges = Edges, Positions = positions };
            return JsonConvert.SerializeObject(exportObject, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
        }

        public void Deserialize(string json, out Dictionary<Vertex, (float X, float Y)> positions)
        {
            var importedObject = JsonConvert.DeserializeObject<ExportObject>(json);
            Edges = importedObject.Edges;
            positions = importedObject.Positions;
        }
    }

    public class Vertex
    {
        public string Name { get; }
        public bool IsStart { get; set; }
        public bool IsExit { get; set; }
        public int? Key { get; set; }
        private static int Count { get; set; }

        public Vertex(string name)
        {
            Name = name ?? $"{Count++}";
            IsStart = false;
            IsExit = false;
            Key = null;
        }

        public void SetKey(int? key)
        {
            Key = key;
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

        public override string ToString()
        {
            return $"{Name}|{IsStart}|{IsExit}|{Key}";
        }

        public static implicit operator Vertex(string s)
        {
            var split = s.Split('|');
            var isStart = bool.Parse(split[1]);
            var isExit = bool.Parse(split[2]);
            return new Vertex(split[0]) { IsStart = isStart, IsExit = isExit };
        }
    }

    public class Edge
    {
        public int? Key { get; set; }

    }

    public class ExportObject
    {
        public Dictionary<Vertex, Dictionary<Vertex, Edge>> Edges { get; set; }
        public Dictionary<Vertex, (float X, float Y)> Positions { get; set; }
    }
}


