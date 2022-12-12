using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class WaveFunctionCollapse
{
    private List<List<(Vector2Int, int)>> _neighborhoods = new List<List<(Vector2Int, int)>>();
    private HashSet<int>[,] _board;
    //private HashSet<int>[,] _rooms;
    //private HashSet<int> _availableRooms;
    private Stack<Dictionary<Vector2Int, List<int>>> _history = new Stack<Dictionary<Vector2Int, List<int>>>();
    private EntropyQueue _queue;
    
    private System.Random _randomEngine;
    private AlgorithmState state = AlgorithmState.Running;

    public WaveFunctionCollapse(int width, int height, Tiles.TileCollection tileset, Graphs.UndirectedGraph graph, int randomSeed)
    {
        _board = new HashSet<int>[width, height];
        //_rooms = new HashSet<int>[width, height];
        //_availableRooms = new HashSet<int>(Enumerable.Range(0, graph.Vertices.Count));


        _randomEngine = new System.Random(randomSeed);
        _queue = new EntropyQueue(_board, _randomEngine);

        for (int x = 0; x < _board.GetLength(0); x++)
            for (int y = 0; y < _board.GetLength(1); y++)
            {
                _board[x, y] = new HashSet<int>(Enumerable.Range(0, tileset.tiles.Count));
                //_rooms[x, y] = new HashSet<int>(Enumerable.Range(0, graph.Vertices.Count));
            }


        for (int i = 0; i<tileset.tiles.Count; i++)
        {
            _neighborhoods.Add(new List<(Vector2Int, int)>());
            for(int dir = 0; dir<8; dir++)
            {
                var coord = DirectionEnum((Tiles.Direction)dir);
                if (!tileset.diagonal && dir % 2 == 1)
                    for (int index = 0; index<tileset.tiles.Count; index++)
                        _neighborhoods[i].Add((coord, index));
                else
                    foreach (var tile in tileset.tiles[i].Neighbors[dir])
                        _neighborhoods[i].Add((coord, tile.Index));

            }
        }
    }
    public Dictionary<Vector2Int, List<int>> EnforceEdgeRules(int edgeTile)
    {
        var modified = new Dictionary<Vector2Int, List<int>>();
        foreach (var cell in EdgeCells)
        {
            modified.Add(cell, Collapse(cell, edgeTile));
        }
        Propagate(modified);

        return modified;
    }

    public Dictionary<Vector2Int, List<int>> Next()
    {
        var cell = Observe();

        if(state!=AlgorithmState.Running)
            return null;

        var collapsedStates = Collapse(cell);

        var modified = new Dictionary<Vector2Int, List<int>>
        {
            { cell, collapsedStates }
        };

        Propagate(modified);
        _history.Push(modified);
        return modified;
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
    private List<int> Collapse(Vector2Int cell, int? tile = null)
    {
        var superposition = _board[cell.x, cell.y];

        var states = superposition.ToList();
        superposition.Clear();

        var pickedState = tile ?? _randomEngine.Next(states.Count);
        superposition.Add(states[pickedState]);

        states.RemoveAt(pickedState);
        return states;
    }
    private void Propagate(Dictionary<Vector2Int, List<int>> modified)
    {
        var s = new Stack<(Vector2Int cell, int tile)>();
        foreach (var cell in modified.Keys) // begin by excluding all tiles modified
            foreach (var tile in modified[cell])
                s.Push((cell, tile));
        while (s.Count > 0)
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
                            => p.cell == point.cell &&
                            _board[p.cell.x, p.cell.y].Contains(p.tile)
                            )
                        )
                        continue;

                    _board[n.cell.x, n.cell.y].Remove(n.tile); // exclude tile if not suported by anything anymore
                    if (!modified.ContainsKey(n.cell))
                        modified.Add(n.cell, new List<int>());
                    modified[n.cell].Add(n.tile);
                    s.Push(n);
                }
            }
        }
        _queue.Notify(modified.First().Key);
    }
    private bool InBounds(Vector2Int cell)
    {
        return cell.x >= 0 &&
            cell.y >= 0 &&
            cell.x < _board.GetLength(0) &&
            cell.y < _board.GetLength(1);
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
    private IEnumerable<Vector2Int> EdgeCells
    {
        get
        {
            for (int x = 0; x < _board.GetLength(0); x++)
            {
                yield return new Vector2Int(x, 0);
                yield return new Vector2Int(x, _board.GetLength(0)-1);
            }
            for(int y = 1; y<_board.GetLength(1)-1; y++)
            {
                yield return new Vector2Int(0, y);
                yield return new Vector2Int(_board.GetLength(1) - 1, y);
            }
        }
    }

    private class EntropyQueue
    {
        private HashSet<int>[,] _board;
        private bool _boardModified = false;
        private Vector2Int _lastModified;
        private List<Vector2Int> _list = new List<Vector2Int>();
        public int Count { get => _list.Count; }
        public EntropyQueue(HashSet<int>[,] board, System.Random rng)
        {
            _board = board;
            for(int x = 0; x<board.GetLength(0); x++)
                for(int y = 0; y< board.GetLength(1); y++)
                    _list.Add(new Vector2Int(x, y));
            _list = _list.OrderBy(x => rng.Next()).ToList();
        }
        public Vector2Int Dequeue()
        {
            if (_boardModified)
            {
                _list.Sort((v1, v2)
                    => (_board[v2.x, v2.y].Count - _board[v1.x, v1.y].Count) * _board.Length
                    + (v2 - _lastModified).sqrMagnitude - (v1 - _lastModified).sqrMagnitude
                    );
                _boardModified = false;
            }
            var v = _list[_list.Count - 1];
            _list.RemoveAt(_list.Count-1);
            return v;
        }
        public void Notify(Vector2Int lastModified)
        { 
            _boardModified = true;
            _lastModified = lastModified;
        }
    }
    public enum AlgorithmState
    {
        Finished, Running, Paradox
    }
}
