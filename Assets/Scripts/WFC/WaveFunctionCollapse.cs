using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class WaveFunctionCollapse
{
    private List<List<(Vector2Int, int)>> _neighborhoods = new List<List<(Vector2Int, int)>>();
    private HashSet<int>[,] _board;
    private Dictionary<Vector2Int, List<int>> _modified = new Dictionary<Vector2Int, List<int>>();
    private Stack<Dictionary<Vector2Int, List<int>>> _history = new Stack<Dictionary<Vector2Int, List<int>>>();
    private EntropyQueue _queue;

    private const int randomSeed = 1234567890;
    private System.Random _randomEngine = new System.Random(randomSeed);
    private AlgorithmState state = AlgorithmState.Running;

    public WaveFunctionCollapse(int width, int height, List<Tiles.Tile> tiles)
    {
        _board = new HashSet<int>[width, height];
        _queue = new EntropyQueue(_board);

        for (int x = 0; x < _board.GetLength(0); x++)
            for (int y = 0; y < _board.GetLength(1); y++)
            {
                _board[x, y] = new HashSet<int>();
                for (int i = 0; i < tiles.Count; i++)
                    _board[x, y].Add(i);

            }
        for(int i = 0; i<tiles.Count; i++)
        {
            _neighborhoods.Add(new List<(Vector2Int, int)>());
            for(int dir = 0; dir<8; dir++)
            {
                var coord = DirectionEnum((Tiles.Direction)dir);
                foreach (var tile in tiles[i].Neighbors[dir])
                    _neighborhoods[i].Add((coord, tile.Index));
            }
        }
    }

    public AlgorithmState Next()
    {
        var cell = Observe();

        if(state!=AlgorithmState.Running)
                return state;

        var superposition = _board[cell.x, cell.y];

        var states = superposition.ToList();
        superposition.Clear();

        var pickedState = _randomEngine.Next(states.Count);
        superposition.Add(states[pickedState]);

        states.RemoveAt(pickedState);
        _modified.Add(cell, states);

        var s = new Stack<(Vector2Int cell, int tile)>();
        foreach (var tile in states) // begin by excluding all tiles not chosen during collapse
            s.Push((cell, tile));

        while(s.Count > 0)
        {
            var point = s.Pop();
            var neighbors = GetNeighbors(point);
            foreach ((Vector2Int cell, int tile) n in neighbors) // loop through all possible neighboring tiles
            {
                if (_board[n.cell.x, n.cell.y].Contains(n.tile)) // if tile not excluded yet, visit
                {
                    var nneighbors = GetNeighbors(n);
                    if (nneighbors.Any( // if tile is still supported by anything else, skip
                        ((Vector2Int cell, int tile) p)
                            => p.cell==point.cell &&
                            _board[p.cell.x,p.cell.y].Contains(p.tile)
                            )
                        ) 
                        continue;

                    _board[n.cell.x,n.cell.y].Remove(n.tile); // exclude tile if not suported by anything anymore
                    if (!_modified.ContainsKey(n.cell))
                        _modified.Add(n.cell, new List<int>());
                    _modified[n.cell].Add(n.tile);
                    s.Push(n);
                }
            }
        }
        _history.Push(_modified);
        _modified = new Dictionary<Vector2Int, List<int>>();
        _queue.Notify();
        return state;
    }
    private bool InBounds(Vector2Int cell)
    {
        return cell.x>=0 &&
            cell.y>=0 &&
            cell.x<_board.GetLength(0) &&
            cell.y<_board.GetLength(1);
    }
    private List<(Vector2Int, int)> GetNeighbors((Vector2Int c, int t) point)
    {
        return _neighborhoods[point.t].Select(
            ((Vector2Int c, int t) p)
            => (p.c + point.c, p.t)
            ).Where(
            ((Vector2Int c, int t) p)
            => InBounds(p.c)
            ).ToList();
    }

    private Vector2Int Observe()
    {
        Vector2Int cell;
        HashSet<int> superposition;
        do
        {
            if (_queue.Count == 0)
            {
                state = AlgorithmState.Finished;
                return new Vector2Int(-1, -1);
            }

            cell = _queue.Dequeue();
            superposition = _board[cell.x, cell.y];
        }
        while (superposition.Count == 1);
        if (superposition.Count == 0)
        {
            state = AlgorithmState.Paradox;
            return new Vector2Int(-1, -1);
        }
        return cell;
    }
    private Vector2Int DirectionEnum(Tiles.Direction dir)
    {
        return dir switch
        {
            Tiles.Direction.North => new(0, -1),
            Tiles.Direction.NorthEast => new(1, -1),
            Tiles.Direction.East => new(1, 0),
            Tiles.Direction.SouthEast => new(1, 1),
            Tiles.Direction.South => new(0, 1),
            Tiles.Direction.SouthWest => new(-1, 1),
            Tiles.Direction.West => new(-1, 0),
            Tiles.Direction.NorthWest => new(-1, -1),
            _ => new(0, 0),
        };
    }

    private class EntropyQueue
    {
        private HashSet<int>[,] _board;
        private bool _boardModified = false;
        private List<Vector2Int> _list = new List<Vector2Int>();
        public int Count { get => _list.Count; }
        public EntropyQueue(HashSet<int>[,] board)
        {
            _board = board;
            for(int x = 0; x<board.GetLength(0); x++)
                for(int y = 0; y< board.GetLength(1); y++)
                    _list.Add(new Vector2Int(x, y));
        }
        public Vector2Int Dequeue()
        {
            if (_boardModified)
            {
                _list.Sort((v1, v2) => _board[v2.x, v2.y].Count - _board[v1.x, v1.y].Count);
                _boardModified = false;
            }
            var v = _list[_list.Count - 1];
            _list.RemoveAt(_list.Count-1);
            return v;
        }
        public void Notify() { _boardModified = true; }
    }
    public enum AlgorithmState
    {
        Finished, Running, Paradox
    }
}
