using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tiles
{
    public class Tile
    {
        public List<List<Tile>> Neighbors { get; private set; } = new(Enumerable.Repeat<List<Tile>>(null, 8));
        public bool Walkable;
        public int Lock, Key;
        public Tile()
        {
            for (int i = 0; i < 8; i++)
            {
                Neighbors[i] = new List<Tile>();
            }
        }
        public void AddNeighbor(Tile neighbor, Direction direction)
        {
            Neighbors[(int)(direction)].Add(neighbor);
        }
        public void RemoveNeighbor(Tile neighbor, Direction direction)
        {
            Neighbors[(int)(direction)].Remove(neighbor);
        }
    }
    public enum Direction
    {
        North = 0, NorthEast = 1, East = 2, SouthEast = 3,
        South = 4, SouthWest = 5, West = 6, NorthWest = 7
    }
}
