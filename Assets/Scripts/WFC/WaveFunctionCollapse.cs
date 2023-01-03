using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using UnityEngine.SocialPlatforms;

public class WaveFunctionCollapse
{
    private List<List<(Vector2Int, int)>> _neighborhoods = new List<List<(Vector2Int, int)>>();
    private HashSet<int>[,] _board;
    private int?[,] _rooms;
    private Stack<Modification> _history = new Stack<Modification>();
    private EntropyQueue _queue;
    
    private System.Random _randomEngine;
    private AlgorithmState state = AlgorithmState.Running;
    private Tiles.TileCollection _tileset;
    private int _borderWidth = 2;

    public WaveFunctionCollapse(int width, int height, Tiles.TileCollection tileset, Graphs.UndirectedGraph graph, int randomSeed, int borderWidth)
    {
        _board = new HashSet<int>[width, height];
        _rooms = new int?[width, height];
        _tileset = tileset;
        _borderWidth = borderWidth;


        _randomEngine = new System.Random(randomSeed);
        _queue = new EntropyQueue(_board, _randomEngine);

        for (int x = 0; x < _board.GetLength(0); x++)
            for (int y = 0; y < _board.GetLength(1); y++)
            {
                _board[x, y] = new HashSet<int>(Enumerable.Range(0, tileset.tiles.Count));
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
    public Modification EnforceEdgeRules(int edgeTile)
    {
        var modified = new Modification();
        foreach (var cell in EdgeCells)
        {
            modified.Add(cell, Collapse(cell, edgeTile));
        }
        Propagate(modified);

        return modified;
    }

    private class PathingNode
    {
        public PathingNode parent = null;
        public int? room = null;
        public int length;
        public bool surrounding = false;
        public bool blocked = false;
        public bool isPath = false;
    }
    public Modification SeedRooms(Graphs.UndirectedGraph graph)
    {
        var roomLocations = new Dictionary<int, Vector2Int>();
        var map = new Dictionary<Vector2Int, PathingNode>();
        Vector2Int min = new(int.MaxValue, int.MaxValue), max = new(int.MinValue, int.MinValue);

        for (int i = 0; i<graph.Vertices.Count; i++)
        {
            do
                roomLocations[i] = new Vector2Int(_randomEngine.Next(_board.GetLength(0)), _randomEngine.Next(_board.GetLength(1)));
            while (map.TryGetValue(roomLocations[i], out _));

            map[roomLocations[i]] = new PathingNode()
            {
                room = i,
                length = 0,
                isPath = true
            };
            setBoundaries(roomLocations[i]);
            foreach (var n in GetNeighbors(roomLocations[i]))
            {
                map[n] = new PathingNode()
                {
                    room = i,
                    length = 1,
                    surrounding = true,
                    parent = map[roomLocations[i]]
                };
                setBoundaries(n);
            }
        }
        for (int i = 0; i < graph.Vertices.Count; i++)
        {
            var targets = new HashSet<int>(
                graph.Edges[graph.Vertices[i]].Keys
                .Select((v) => graph.Vertices.IndexOf(v))
                .Where((a) => a > i)
            );
            var surroundings = GetNeighbors(roomLocations[i]);
            var queue = new Queue<Vector2Int>(surroundings);
            var paths = new List<PathingNode>();
            while (queue.Count > 0)
            {
                var v = queue.Dequeue();
                foreach (var neighbor in GetRandomizedNeighbors(v))
                {
                    setBoundaries(neighbor);
                    // we can enter if
                    if (!map.TryGetValue(neighbor, out var neighborNode) || // it was never visited OR
                        !neighborNode.surrounding && // it is not surrounding the center
                        !neighborNode.blocked && // it is not next to an existing path
                        !neighborNode.isPath && // it is not an existing path
                        neighborNode.room.Value != i) // we didn't visit it in current BFS
                    {
                        map[neighbor] = new PathingNode()
                        {
                            parent = map[v],
                            length = map[v].length + 1,
                            room = i
                        };
                        queue.Enqueue(neighbor);
                    }
                    else if (!neighborNode.blocked && targets.Contains(neighborNode.room.Value) && (neighborNode.isPath || neighborNode.surrounding))
                    {
                        paths.Add(map[v]);
                        targets.Remove(neighborNode.room.Value);
                        break;
                    }
                }
                
            } // end BFS
            if (targets.Count > 0)
            {
                // could not find all the paths
                return null;
            }

            foreach (var path in paths)
            {
                PathingNode v = path;
                while(v.length>path.length/2)
                {
                    v.isPath = true;
                    v.parent.room = v.room;
                    v = v.parent;
                }
                while(v.length != 0)
                {
                    v.isPath = true;
                    v = v.parent;
                }
            }

            var pathCoords = map.Where(p => p.Value.room == i && !p.Value.surrounding && p.Value.isPath).ToList();

            foreach (var pair in pathCoords)
            {
                var coords = pair.Key;
                var node = pair.Value;

                foreach(var neighbor in GetNeighbors(coords))
                {
                    if(!map.TryGetValue(neighbor, out var n) || !n.isPath)
                        map[neighbor] = new PathingNode() { blocked = true };
                }
            }
        } // end paths

        var allPathCoords = map.Where(p => p.Value.isPath).Select(p => p.Key - min).ToList();

        var unwalkableTiles = _tileset.tiles.Where(t => !t.Walkable).Select(t => t.Index).ToList();
        var modified = new Modification(allPathCoords.ToDictionary(p => p, p => unwalkableTiles));

        foreach(var coords in allPathCoords)
        {
            _board[coords.x, coords.y].SymmetricExceptWith(unwalkableTiles);
            _rooms[coords.x, coords.y] = map[coords + min].room;
        }

        Propagate(modified);
        // TO DO: ¿eby nie wychodzi³o
        return modified;

        IEnumerable<Vector2Int> GetNeighbors(Vector2Int v)
        {
            Vector2Int of;
            of = new Vector2Int(-1, 0);
            if (checkBoundaries(v + of))
                yield return v + of;
            of = new Vector2Int(0, -1);
            if (checkBoundaries(v + of))
                yield return v + of;
            of = new Vector2Int(1, 0);
            if (checkBoundaries(v + of))
                yield return v + of;
            of = new Vector2Int(0, 1);
            if (checkBoundaries(v + of))
                yield return v + of;
        }
        IEnumerable<Vector2Int> GetRandomizedNeighbors(Vector2Int v) => GetNeighbors(v).OrderBy((k) => _randomEngine.Next());

        //TODO: - customizowac dlugosc granicy
        bool checkBoundaries(Vector2Int v)
        {
            var locmin = min;
            var locmax = max;
            locmin.x = Math.Min(min.x, v.x);
            locmin.y = Math.Min(min.y, v.y);
            locmax.x = Math.Max(max.x, v.x);
            locmax.y = Math.Max(max.y, v.y);
            return locmax.x - locmin.x < _board.GetLength(0) - _borderWidth && locmax.y - locmin.y < _board.GetLength(1) - _borderWidth;
        }
        void setBoundaries(Vector2Int v)
        {
            min.x = Math.Min(min.x, v.x);
            min.y = Math.Min(min.y, v.y);
            max.x = Math.Max(max.x, v.x);
            max.y = Math.Max(max.y, v.y);
        }
    }

    public Modification Next()
    {
        var cell = Observe();

        if(state!=AlgorithmState.Running)
            return null;

        var collapsedStates = Collapse(cell);

        var modified = new Modification
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
    private List<int> Collapse(Vector2Int cell, int? tile = null, int? room = null)
    {
        var superposition = _board[cell.x, cell.y];

        var statesToDelete = superposition.ToList();
        superposition.Clear();

        var pickedPosition = tile == null ? _randomEngine.Next(statesToDelete.Count) : statesToDelete.IndexOf(tile.Value);

        if(pickedPosition != -1)
        {
            superposition.Add(statesToDelete[pickedPosition]);
            statesToDelete.RemoveAt(pickedPosition);
        }

        return statesToDelete;
    }
    private void Propagate(Modification modified)
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
                        modified.Add(n.cell, new());
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

    public class Modification : Dictionary<Vector2Int, List<int>> 
    { 
        public Modification(Dictionary<Vector2Int, List<int>> dic) : base(dic)
        { }
        public Modification() : base() { }
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
