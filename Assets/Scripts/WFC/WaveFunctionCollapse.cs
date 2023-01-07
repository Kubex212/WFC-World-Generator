using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using UnityEngine.SocialPlatforms;
using Unity.VisualScripting;

public class WaveFunctionCollapse
{
    public List<(int tile, int? room, int index)> OriginTiles { get; private set; } = new(); 

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


        for (int i = 0; i < tileset.tiles.Count; i++)
        {
            if (tileset.tiles[i].Walkable)
            {
                for(int j = 0; j < graph.Vertices.Count; j++)
                {
                    OriginTiles.Add((i, j, OriginTiles.Count));
                }
            }
            else
            {
                OriginTiles.Add((i, null, OriginTiles.Count));
            }
        }
        SpriteAtlas.Atlas = OriginTiles.Select((tr) => SpriteAtlas.Atlas[tr.tile]).ToArray();

        for (int x = 0; x < _board.GetLength(0); x++)
            for (int y = 0; y < _board.GetLength(1); y++)
            {
                _board[x, y] = new HashSet<int>(Enumerable.Range(0, OriginTiles.Count));
            }

        for (int i = 0; i < OriginTiles.Count; i++)
        {
            _neighborhoods.Add(new List<(Vector2Int, int)>());
            for(int dir = 0; dir<8; dir++)
            {
                var coord = DirectionEnum((Tiles.Direction)dir);
                if (!tileset.diagonal && dir % 2 == 1)
                    for (int index = 0; index < OriginTiles.Count; index++)
                        _neighborhoods[i].Add((coord, index));
                else
                    foreach (var tile in tileset.tiles[OriginTiles[i].tile].Neighbors[dir])
                        foreach (var originTile in OriginTiles.Where(t => t.tile == tile.Index))
                            _neighborhoods[i].Add((coord, originTile.index));
            }
        }

    }
    public Modification EnforceEdgeRules(int edgeTile)
    {
        var modified = new Modification();

        foreach (var cell in EdgeCells)
        {
            modified.Tiles.Add(cell, Collapse(cell, OriginTiles.First(t => t.tile == edgeTile).index));
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
        public bool isPath = false;
    }
    public Modification SeedRooms(Graphs.UndirectedGraph graph)
    {
        var roomLocations = new Dictionary<int, Vector2Int>();
        var map = new Dictionary<Vector2Int, PathingNode>();

        var unwalkableTiles = OriginTiles.Where(t => !t.room.HasValue).Select(t => OriginTiles.IndexOf(t)).ToList();
        var walkableTiles = OriginTiles.Where(t => t.room.HasValue).Select(t => OriginTiles.IndexOf(t)).ToList();

        var modified = new Modification();

        for (int i = 0; i<graph.Vertices.Count; i++)
        {
            do
                roomLocations[i] = new Vector2Int(
                    _randomEngine.Next(_board.GetLength(0) - (_borderWidth + 1) * 2) + _borderWidth + 1,
                    _randomEngine.Next(_board.GetLength(1) - (_borderWidth + 1) * 2) + _borderWidth + 1);
            while (map.TryGetValue(roomLocations[i], out _));

            map[roomLocations[i]] = new PathingNode()
            {
                room = i,
                length = 0,
                isPath = true
            };
            foreach (var n in GetNeighbors(roomLocations[i]))
            {
                map[n] = new PathingNode()
                {
                    room = i,
                    length = 1,
                    surrounding = true,
                    parent = map[roomLocations[i]]
                };
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
                    var visited = map.TryGetValue(neighbor, out var neighborNode);

                    // we can enter if
                    if (_board[neighbor.x, neighbor.y].Any(t => OriginTiles[t].room == i) &&
                        (!visited ||
                        (neighborNode.room != i && !neighborNode.isPath && !neighborNode.surrounding)))
                    {
                        map[neighbor] = new PathingNode()
                        {
                            parent = map[v],
                            length = map[v].length + 1,
                            room = i
                        };
                        queue.Enqueue(neighbor);
                    }
                    else if (visited &&
                        targets.Contains(neighborNode.room.Value) &&
                        (neighborNode.isPath || neighborNode.surrounding))
                    {
                        map[v].room = neighborNode.room;
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
            // collapse to room i
            foreach (var coord in pathCoords)
            {
                if (!modified.Tiles.ContainsKey(coord.Key))
                    modified.Tiles.Add(coord.Key, new());
                modified.Tiles[coord.Key].AddRange(Collapse(coord.Key, walkableTiles.Where(t => OriginTiles[t].room == i)));
            }
            // propagate
            Propagate(modified);
        } // end paths

        //var allPathCoords = map.Where(p => p.Value.isPath).Select(p => p.Key).ToList();


        //foreach(var coords in allPathCoords)
        //{
        //    _board[coords.x, coords.y].SymmetricExceptWith(unwalkableTiles);
        //    _rooms[coords.x, coords.y] = map[coords].room;
        //}

        //Propagate(modified);

        return modified;

        IEnumerable<Vector2Int> GetNeighbors(Vector2Int v)
        {
            Vector2Int of;
            of = new Vector2Int(-1, 0);
            if (InBounds(v + of))
                yield return v + of;
            of = new Vector2Int(0, -1);
            if (InBounds(v + of))
                yield return v + of;
            of = new Vector2Int(1, 0);
            if (InBounds(v + of))
                yield return v + of;
            of = new Vector2Int(0, 1);
            if (InBounds(v + of))
                yield return v + of;
        }
        IEnumerable<Vector2Int> GetRandomizedNeighbors(Vector2Int v) => GetNeighbors(v).OrderBy((k) => _randomEngine.Next());

        //TODO: - customizowac dlugosc granicy
    }

    public Modification Next()
    {
        var cell = Observe();

        if(state!=AlgorithmState.Running)
            return null;

        var collapsedStates = Collapse(cell);

        var modified = new Modification(new()
        {
            [cell] = collapsedStates 
        });

        Propagate(modified);
        _history.Push(modified);

        //foreach (var coords in modified.Tiles.Keys.Where(t => !_rooms[t.x, t.y].HasValue && _board[t.x, t.y].All(p => OriginTiles[p].room.HasValue)))
        //{
        //    foreach (var n in GetAdjacentCells(coords).Where(v => InBounds(v)))
        //    {
        //        if (_rooms[n.x, n.y].HasValue)
        //        {
        //            _rooms[coords.x, coords.y] = modified.Rooms[coords] = _rooms[n.x, n.y].Value;
        //            break;
        //        }
        //    }
        //}
        //FloodFillRoom(modified);

        return modified;
    }

    //private void FloodFillRoom(Modification modified)
    //{
    //    var q = new Queue<(Vector2Int cell, int room)>();
    //    foreach (var cell in modified.Rooms.Keys) 
    //        q.Enqueue((cell, modified.Rooms[cell]));
                
    //    while (q.Count > 0)
    //    {
    //        var point = q.Dequeue();
    //        var neighbors = GetAdjacentCells(point.cell).Where(v => InBounds(v));
    //        foreach (var cell in neighbors) // loop through all adjacent cells
    //        {
    //            // if room doesnt have value yet and consists of walkables only, visit
    //            if (!_rooms[cell.x, cell.y].HasValue && _board[cell.x, cell.y].All(p => _originTiles[p].room.HasValue)) 
    //            {
    //                _rooms[cell.x, cell.y] = point.room; // exclude tile if not suported by anything anymore
    //                modified.Rooms[cell] = point.room;
    //                q.Enqueue((cell, point.room));
    //            }
    //        }
    //    }
    //}

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
    private List<int> Collapse(Vector2Int cell, IEnumerable<int> tiles)
    {
        var superposition = _board[cell.x, cell.y];

        var statesToDelete = superposition.Except(tiles).ToList();
        var range = tiles.Where(t => superposition.Contains(t));

        superposition.Clear();
        superposition.AddRange(range);

        return statesToDelete;
    }
    private List<int> Collapse(Vector2Int cell, int? tile = null)
    {
        var superposition = _board[cell.x, cell.y];

        var statesToDelete = superposition.ToList();
        superposition.Clear();

        var ranges = new List<int>();
        ranges.Add(OriginTiles[statesToDelete[0]].room.HasValue ? 1 : 1);

        for(int i = 1; i < statesToDelete.Count; i++)
        {
            ranges.Add(ranges[i - 1] + (OriginTiles[statesToDelete[i]].room.HasValue ? 1 : 1));   // 1000 1001 2001 3001
        }

        var pickedIndex = _randomEngine.Next(ranges.Last());

        var pickedPosition = tile == null ? ranges.IndexOf(ranges.First(i => i > pickedIndex)) : statesToDelete.IndexOf(tile.Value);

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
        foreach (var cell in modified.Tiles.Keys) // begin by excluding all tiles modified
            foreach (var tile in modified.Tiles[cell])
                s.Push((cell, tile));
        while (s.Count > 0)
        {
            var point = s.Pop();
            var neighbors = GetInfluencedTiles(point);
            foreach ((Vector2Int cell, int tile) n in neighbors) // loop through all possible neighboring tiles
            {
                if (_board[n.cell.x, n.cell.y].Contains(n.tile)) // if tile not excluded yet, visit
                {
                    var nneighbors = GetInfluencedTiles(n);
                    if (nneighbors.Any( // if tile is still supported by anything else, skip
                        ((Vector2Int cell, int tile) p)
                            => p.cell == point.cell &&
                            _board[p.cell.x, p.cell.y].Contains(p.tile)
                            )
                        )
                        continue;

                    _board[n.cell.x, n.cell.y].Remove(n.tile); // exclude tile if not suported by anything anymore
                    if (!modified.Tiles.ContainsKey(n.cell))
                        modified.Tiles.Add(n.cell, new());
                    modified.Tiles[n.cell].Add(n.tile);
                    s.Push(n);
                }
            }
        }
        _queue.Notify(modified.Tiles.First().Key);
    }
    private bool InBounds(Vector2Int cell)
    {
        return cell.x >= 0 &&
            cell.y >= 0 &&
            cell.x < _board.GetLength(0) &&
            cell.y < _board.GetLength(1);
    }
    
    private IEnumerable<Vector2Int> GetAdjacentCells(Vector2Int v)
    {
        yield return v + new Vector2Int(0, 1);
        yield return v + new Vector2Int(-1, 0);
        yield return v + new Vector2Int(1, 0);
        yield return v + new Vector2Int(0, -1);
    }

    private List<(Vector2Int, int)> GetInfluencedTiles((Vector2Int c, int t) point)
    {
        return _neighborhoods[point.t].Where(
            ((Vector2Int c, int t) p) => _tileset.diagonal || p.c.sqrMagnitude == 1)
            .Select(
            ((Vector2Int c, int t) p) => (p.c + point.c, p.t))
            .Where(
            ((Vector2Int c, int t) p) => InBounds(p.c))
            .ToList();
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
                yield return new Vector2Int(x, _board.GetLength(1)-1);
            }
            for(int y = 1; y<_board.GetLength(1)-1; y++)
            {
                yield return new Vector2Int(0, y);
                yield return new Vector2Int(_board.GetLength(0) - 1, y);
            }
        }
    }

    public class Modification
    { 
        //public Dictionary<Vector2Int, int> Rooms { get; set; }
        public Dictionary<Vector2Int, List<int>> Tiles { get; set; }

        public Modification(Dictionary<Vector2Int, List<int>> tiles, Dictionary<Vector2Int, int> rooms = null)
        {
            Tiles = tiles;
            //Rooms = rooms ?? new();
        }
        public Modification()
        {
            Tiles = new();
            //Rooms = new();
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
