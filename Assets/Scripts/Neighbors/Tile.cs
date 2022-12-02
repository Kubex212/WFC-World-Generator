using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tiles
{
    public class Tile
    {
        public int Index = _nextIndex++;
        public List<List<Tile>> Neighbors { get; private set; } = new List<List<Tile>>();
        public bool Walkable;
        public int Lock, Key;
        public override string ToString()
        {
            return $"{Index}";
        }
        public void AddNeighbor(Tile neighbor, Direction direction)
        {
            if (!Neighbors[(int)(direction)].Contains(neighbor))
                Neighbors[(int)(direction)].Add(neighbor);
        }
        public void RemoveNeighbor(Tile neighbor, Direction direction)
        {
            Neighbors[(int)(direction)].Remove(neighbor);
        }
        public static void Load(int amount) => _nextIndex = amount;
        private static int _nextIndex = 0;
    }
    public enum Direction
    {
        North = 0, NorthEast = 1, East = 2, SouthEast = 3,
        South = 4, SouthWest = 5, West = 6, NorthWest = 7
    }
    public static class DirectionExtension
    {
        public static Direction Opposite(this Direction d)
        {
            return (Direction)(((int)d+4)%8);
        }
    }
}
