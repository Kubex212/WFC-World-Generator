using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Unity.VisualScripting;

public class WaveFunctionCollapse
{
    public List<(int tile, int? room, int index)> OriginTiles { get; private set; } = new();
    public int StandardTileCount { get; private set; }
    public AlgorithmState State { get; private set; } = AlgorithmState.Running;

    public Vector2Int? startRoomLocation = null;
    public Vector2Int? endRoomLocation = null;
    public List<(int room, int lx, int ly)> locksInfo = new(); // lx -> lock_x, ly -> lock_y

    public Dictionary<int, Vector2Int> roomLocations = null;
    private List<List<(Vector2Int, int)>> _neighborhoods = new List<List<(Vector2Int, int)>>();
    private List<(int from, int to, int? key)> _edgeInfo = new();
    private HashSet<int>[,] _board;
    private HashSet<int>[,] _reachability;
    private Stack<(Modification modification, List<Vector2Int> collapsedCells, int collapsedTile)> _history = new();
    private EntropyQueue _queue;
    private Modification _modified;
    
    private System.Random _randomEngine;
    private Tiles.TileCollection _tileset;
    private Graphs.UndirectedGraph _graph;
    private int _borderWidth = 2;

    public WaveFunctionCollapse(int width, int height, Tiles.TileCollection tileset, Graphs.UndirectedGraph graph, int randomSeed, int borderWidth)
    {
        _board = new HashSet<int>[width, height];
        _reachability = new HashSet<int>[width, height];
        _tileset = tileset;
        _graph = graph;
        _borderWidth = borderWidth;


        _randomEngine = new System.Random(randomSeed);
        _queue = new EntropyQueue(_board, _randomEngine, EntropySort);

        var vertexList = _graph.Vertices;
        var e = _graph.Edges;

        for(int i = 0; i < vertexList.Count; i++)
        {
            for(int j = i + 1; j < vertexList.Count; j++)
            {
                if(graph.CheckEdge(i, j)) 
                    _edgeInfo.Add((i, j, e[vertexList[i]][vertexList[j]].Key));
            }
        }

        for (int i = 0; i < tileset.tiles.Count; i++)
        {
            if (!tileset.tiles[i].Walkable)
            {
                OriginTiles.Add((i, null, OriginTiles.Count));
            }
        }

        for (int i = 0; i < tileset.tiles.Count; i++)
        {
            if (tileset.tiles[i].Walkable)
            {
                for (int j = 0; j < vertexList.Count; j++)
                {
                    OriginTiles.Add((i, j, OriginTiles.Count));
                }
            }
        }

        StandardTileCount = OriginTiles.Count;

        for(int i = 0; i < tileset.tiles.Count; i++)
        {
            if (tileset.tiles[i].Walkable)
            {
                for (int j = 0; j < _edgeInfo.Count; j++)
                {
                    OriginTiles.Add((i, j, OriginTiles.Count));
                }
            }
        }

        SpriteAtlas.Atlas = OriginTiles.Select((tr) => SpriteAtlas.Atlas[tr.tile]).ToArray();

        for (int x = 0; x < _board.GetLength(0); x++)
            for (int y = 0; y < _board.GetLength(1); y++)
            {
                _board[x, y] = new HashSet<int>(Enumerable.Range(0, OriginTiles.Count));
                _reachability[x, y] = new HashSet<int>();
            }

        for (int i = 0; i < StandardTileCount; i++)
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
                        foreach (var originTile in OriginTiles.Take(StandardTileCount).Where(t => t.tile == tile.Index))
                            if (originTile.room.HasValue && OriginTiles[i].room.HasValue)
                            {
                                if (originTile.room == OriginTiles[i].room)
                                    _neighborhoods[i].Add((coord, originTile.index));
                                else
                                {

                                }
                            }
                            else
                                _neighborhoods[i].Add((coord, originTile.index));
            }
        }

        for (int i = StandardTileCount; i < OriginTiles.Count; i++)
        {
            _neighborhoods.Add(new List<(Vector2Int, int)>());
            var edge = _edgeInfo[OriginTiles[i].room.Value];

            for (int dir = 0; dir < 8; dir++)
            {
                var coord = DirectionEnum((Tiles.Direction)dir);
                var oppositeCoord = DirectionEnum((Tiles.Direction)((dir + 4) % 8));
                if (!tileset.diagonal && dir % 2 == 1)
                    for (int index = 0; index < OriginTiles.Count; index++)
                        _neighborhoods[i].Add((coord, index));
                else
                    foreach (var tile in tileset.tiles[OriginTiles[i].tile].Neighbors[dir])
                        foreach (var originTile in OriginTiles.Take(StandardTileCount).Where(t => t.tile == tile.Index))
                            if (originTile.room.HasValue)
                            {

                                if (originTile.room.Value == edge.to ||
                                    originTile.room.Value == edge.from)
                                {
                                    _neighborhoods[i].Add((coord, originTile.index));
                                    _neighborhoods[originTile.index].Add((oppositeCoord, i));
                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                _neighborhoods[i].Add((coord, originTile.index));
                                _neighborhoods[originTile.index].Add((oppositeCoord, i));
                            }
            }
        }
    }
    public string RoomNameFunc(int tile)
    {
        var ind = OriginTiles[tile].room;
        var (from, to, key) = tile >= StandardTileCount ? _edgeInfo[ind.Value] : default;
        return tile < StandardTileCount ?
            ind?.ToString() ?? "" :
            $"{from}:{to}{(key.HasValue?$"\n({key.Value})":"")}";
    }
    public void EnforceEdgeRules(int edgeTile)
    {
        _modified = new Modification();

        foreach (var cell in EdgeCells)
        {
            _modified.Tiles.Add(cell,
                LimitSuperposition(cell, OriginTiles
                    .Where(t => t.tile == edgeTile)
                    .Select(t => t.index)));
        }
        Propagate();
    }

    private class PathingNode
    {
        public PathingNode parent = null;
        public int? room = null;
        public int length;
        public bool surrounding = false;
        public bool isPath = false;
        public int? door = null;
    }
    public Modification SeedRooms(Graphs.UndirectedGraph graph)
    {
        roomLocations = new Dictionary<int, Vector2Int>();
        var map = new Dictionary<Vector2Int, PathingNode>();

        var unwalkableTiles = OriginTiles.Take(StandardTileCount).Where(t => !t.room.HasValue).Select(t => OriginTiles.IndexOf(t)).ToList();
        var walkableTiles = OriginTiles.Take(StandardTileCount).Where(t => t.room.HasValue).Select(t => OriginTiles.IndexOf(t)).ToList();

        var vertices = graph.Vertices;
        var ordering = Enumerable.Range(0, vertices.Count)
            .OrderBy((i) => -graph.Edges[vertices[i]].Count)
            .ToList();


        var grid = new List<(Vector2Int min, Vector2Int max)>();
        {
            int gridSize = (int)Math.Ceiling(Math.Sqrt(vertices.Count));
            gridSize = gridSize % 2 == 0 ? gridSize + 1 : gridSize;
            for (int x = 0; x < gridSize; x++)
                for (int y = 0; y < gridSize; y++)
                {
                    grid.Add((new(
                        x * (_board.GetLength(0) - (_borderWidth + 1) * 2) / gridSize + _borderWidth + 1,
                        y * (_board.GetLength(1) - (_borderWidth + 1) * 2) / gridSize + _borderWidth + 1
                        ),new(
                        (x+1) * (_board.GetLength(0) - (_borderWidth + 1) * 2) / gridSize + _borderWidth + 1,
                        (y+1) * (_board.GetLength(1) - (_borderWidth + 1) * 2) / gridSize + _borderWidth + 1
                        )));
                }
            var center = new Vector2Int(_board.GetLength(0), _board.GetLength(1))/2;
            grid = grid.OrderBy((v) => ((v.min + v.max) / 2 - center).sqrMagnitude).ToList();
        }

        for (int i = 0; i < vertices.Count; i++)
        {

            var currentRoom = ordering[i];
            do
                roomLocations[currentRoom] = new Vector2Int(
                    _randomEngine.Next(grid[i].min.x, grid[i].max.x),
                    _randomEngine.Next(grid[i].min.y, grid[i].max.y));
            while (map.TryGetValue(roomLocations[currentRoom], out _));

            if (graph.Vertices[currentRoom].IsStart)
            {
                startRoomLocation = roomLocations[currentRoom];
            }
            else if(graph.Vertices[currentRoom].IsExit)
            {
                endRoomLocation = roomLocations[currentRoom];
            }

            map[roomLocations[currentRoom]] = new PathingNode()
            {
                room = currentRoom,
                length = 0,
                isPath = true
            };
            foreach (var n in GetNeighbors(roomLocations[currentRoom]))
            {
                map[n] = new PathingNode()
                {
                    room = currentRoom,
                    length = 1,
                    surrounding = true,
                    parent = map[roomLocations[currentRoom]]
                };
            }
        }
        var sumModified = _modified;
        for (int i = 0; i < vertices.Count; i++)
        {
            var currentRoom = ordering[i];
            var targets = new HashSet<int>(
                graph.Edges[vertices[currentRoom]].Keys
                .Select((v) => vertices.IndexOf(v))
                .Where((a) => ordering.IndexOf(a) > i) // a is calculated after currentRoom
            );
            var surroundings = GetNeighbors(roomLocations[currentRoom]);
            var queue = new Queue<Vector2Int>(surroundings);
            var paths = new List<PathingNode>();
            while (queue.Count > 0 && targets.Count > 0)
            {
                var v = queue.Dequeue();
                foreach (var neighbor in GetRandomizedNeighbors(v))
                {
                    var visited = map.TryGetValue(neighbor, out var neighborNode);

                    // we can enter if
                    if (_board[neighbor.x, neighbor.y].Any(t => OriginTiles[t].room == currentRoom) &&
                        (!visited ||
                        (neighborNode.room != currentRoom && !neighborNode.isPath && !neighborNode.surrounding)))
                    {
                        map[neighbor] = new PathingNode()
                        {
                            parent = map[v],
                            length = map[v].length + 1,
                            room = currentRoom
                        };
                        queue.Enqueue(neighbor);
                    }
                    else if (visited &&
                        targets.Contains(neighborNode.room.Value) &&
                        (neighborNode.isPath || neighborNode.surrounding))
                    {
                        map[v].room = neighborNode.room;
                        map[v].isPath = true;
                        neighborNode.isPath = true;
                        paths.Add(map[v]);
                        targets.Remove(neighborNode.room.Value);
                        break;
                    }
                }
                
            } // end BFS
            if (targets.Count > 0)
            {
                // could not find all the paths
                State = AlgorithmState.Paradox;
                return null;
            }

            foreach (var path in paths)
            {
                PathingNode v = path;
                while(v.length > (path.length + 2 + 1)/2)
                {
                    v.isPath = true;
                    v.parent.room = v.room;
                    v = v.parent;
                }
                v.room = v.parent.room;
                v.door = _edgeInfo.FindIndex(ei => (ei.from == v.room && ei.to == path.room) || 
                                                   (ei.to == v.room && ei.from == path.room));
                v.isPath = true;
                v = v.parent;

                while (v.length != 0)
                {
                    v.isPath = true;
                    v = v.parent;
                }
            }
            
            var pathCoords = map.Where(p => p.Value.room == currentRoom && p.Value.isPath).ToList();
            // collapse to room i
            _modified = new Modification();

            foreach (var coord in pathCoords.Where(c => c.Value.door == null))
            {
                _modified.Tiles[coord.Key] = new(LimitSuperposition(coord.Key, walkableTiles.Where(t => OriginTiles[t].room == currentRoom)));
            }
            var tilesToRemove = new List<int>();
            foreach (var coord in pathCoords.Where(c => c.Value.door != null))
            {
                var doorTiles = OriginTiles.Skip(StandardTileCount).Where(t => t.room == coord.Value.door).Select(t => t.index);
                tilesToRemove.AddRange(doorTiles);
                _modified.Tiles[coord.Key] = new(LimitSuperposition(coord.Key, doorTiles));
            }
            var tilesToLeave = Enumerable.Range(0, OriginTiles.Count).Except(tilesToRemove);
            for(int x = 0; x < _board.GetLength(0); x++)
                for (int y = 0; y < _board.GetLength(1); y++)
                    if(_board[x, y].Count > 1)
                    {
                        var modif = LimitSuperposition(new(x, y), tilesToLeave);
                        if (modif.Count == 0)
                            continue;
                        if (!_modified.Tiles.ContainsKey(new(x, y)))
                            _modified.Tiles.Add(new(x, y), new());
                        _modified.Tiles[new (x,y)].AddRange(modif);
                    }
        // propagate
            Propagate();
            sumModified += _modified;
            if (State == AlgorithmState.Paradox)
                return _modified = sumModified;
        } // end paths


        FloodFillRoom();
        Propagate();
        return _modified = sumModified;

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

    public (Modification undone, Modification done) Undo()
    {
        if(_history.Count < 1)
            return default;

        var move = _history.Pop();

        for(int i = 0; i<move.collapsedCells.Count; i++)
        {
            _queue.Enqueue(move.collapsedCells[i]);
        }

        _queue.Notify();


        foreach (var pair in move.modification.Tiles)
        {
            var v = pair.Key;
            var sp = pair.Value;
            _board[v.x, v.y].UnionWith(sp);
        }

        State = AlgorithmState.Running;
        var tilesToLeave = Enumerable.Range(0, _neighborhoods.Count).Where(t => t != move.collapsedTile);
        _modified = new();
        _modified.Tiles[move.collapsedCells[^1]] = LimitSuperposition(move.collapsedCells[^1], tilesToLeave);
        Propagate();
        if (_history.Count > 0)
        {
            var h = _history.Pop();
            h.modification += _modified;
            _history.Push(h);
        }


        return (move.modification, _modified);
    }

    public Modification Next()
    {
        if (State != AlgorithmState.Running)
            return default;

        _modified = new Modification();
        var cells = Observe();

        if (State != AlgorithmState.Running)
        {
            if(cells.Count>0)
                _history.Push((_modified, cells, -1));
            return default;
        }

        var tile = Collapse(cells[cells.Count-1]);


        Propagate();
        FloodFillRoom();
        Propagate();
        // TODO: auto undo if paradox
        _history.Push((_modified, cells, tile));

        return _modified;
    }

    private void FloodFillRoom()
    {
        for (int x = 0; x < _board.GetLength(0); x++)
            for (int y = 0; y < _board.GetLength(1); y++)
                _reachability[x, y].Clear();
        var q = new Queue<(Vector2Int cell, int room)>();
        for (int i = 0; i < roomLocations.Count; i++)
        {
            var (x, y) = (roomLocations[i].x, roomLocations[i].y);
            q.Enqueue((new(x, y), i));
            _reachability[x, y].Add(i);
        }
        while (q.Count > 0)
        {
            var point = q.Dequeue();
            var neighbors = GetAdjacentCells(point.cell).Where(v => InBounds(v));
            foreach (var cell in neighbors) // loop through all adjacent cells
            {
                // if can turn into chosen room, add
                if (!_reachability[cell.x,cell.y].Contains(point.room) &&
                    _board[cell.x, cell.y].Any(p => p<StandardTileCount && point.room == OriginTiles[p].room))
                {
                    _reachability[cell.x, cell.y].Add(point.room);
                    q.Enqueue((cell, point.room));
                }
            }
        }
        RemoveUnreachable();
    }
    private void RemoveUnreachable()
    {
        var allTiles = Enumerable.Range(0, _neighborhoods.Count);
        for (int x = 0; x < _board.GetLength(0); x++)
            for (int y = 0; y < _board.GetLength(1); y++)
            {
                var m = LimitSuperposition(new(x, y), allTiles.Where(t =>
                    !OriginTiles[t].room.HasValue || t >= StandardTileCount ||
                    _reachability[x, y].Contains(OriginTiles[t].room.Value))
                    );
                if(m.Count>0)
                {
                    if (!_modified.Tiles.ContainsKey(new(x, y)))
                        _modified.Tiles[new(x, y)] = m;
                    else
                        _modified.Tiles[new(x, y)].AddRange(m);
                }
            }

    }
    private List<Vector2Int> Observe()
    {
        var cells = new List<Vector2Int>();
        HashSet<int> superposition;
        do
        {
            if (_queue.Count == 0)
            {
                State = AlgorithmState.Finished;
                return cells;
            }

            var cell = _queue.Dequeue();
            cells.Add(cell);
            superposition = _board[cell.x, cell.y];
        }
        while (superposition.Count == 1);
        if (superposition.Count == 0)
        {
            State = AlgorithmState.Paradox;
            return cells;
        }
        return cells;
    }
    private List<int> LimitSuperposition(Vector2Int cell, IEnumerable<int> tilesToLeave)
    {
        var superposition = _board[cell.x, cell.y];

        var statesToDelete = superposition.Except(tilesToLeave).ToList();

        superposition.IntersectWith(tilesToLeave);

        return statesToDelete;
    }
    private int Collapse(Vector2Int cell)
    {
        var superposition = _board[cell.x, cell.y];

        var statesToDelete = superposition.ToList();
        superposition.Clear();

        var ranges = new List<int>
        {
            OriginTiles[statesToDelete[0]].room.HasValue ? 1 : 1
        };

        for(int i = 1; i < statesToDelete.Count; i++)
        {
            ranges.Add(ranges[i - 1] + (OriginTiles[statesToDelete[i]].room.HasValue ? 1 : 1));   // 1000 1001 2001 3001
        }

        var pickedIndex = _randomEngine.Next(ranges.Last());
        var picked = statesToDelete[ranges.FindIndex(i => i > pickedIndex)];

        // zamiast _currentRoom++ powinno byc sprawdzanie po krawêdziach
        
        //if (OriginTiles[picked].room.HasValue)
        //{
        //    var roomNeighbors = GetAdjacentCells(cell)
        //        .Where(v => InBounds(v) &&
        //        _board[v.x, v.y].Count == 1 &&
        //        statesToDelete.Contains(_board[v.x,v.y].First()) &&
        //        OriginTiles[_board[v.x, v.y].First()].room == _currentRoom).ToList();

        //    if (roomNeighbors.Any())
        //    {
        //        roomNeighbors = roomNeighbors.OrderBy(v => _randomEngine.Next()).ToList();
        //        var pickedSeq = _board[roomNeighbors.First().x, roomNeighbors.First().y];
        //        picked = pickedSeq.First();
        //    }
            
        //}

        superposition.Add(picked); 
        statesToDelete.Remove(picked);

        if (!_modified.Tiles.ContainsKey(cell))
            _modified.Tiles[cell] = statesToDelete;
        else
            _modified.Tiles[cell].AddRange(statesToDelete);
        return picked;
    }
    private void Propagate()
    {
        var s = new Stack<(Vector2Int cell, int tile)>();
        foreach (var cell in _modified.Tiles.Keys) // begin by excluding all tiles modified
            foreach (var tile in _modified.Tiles[cell])
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
                    if (!_modified.Tiles.ContainsKey(n.cell))
                        _modified.Tiles.Add(n.cell, new());
                    _modified.Tiles[n.cell].Add(n.tile);
                    if (!_board[n.cell.x, n.cell.y].Any())
                    {
                        State = AlgorithmState.Paradox;
                        return;
                    }
                    s.Push(n);
                }
            }
        }
        _queue.Notify();
    }
    private bool InBounds(Vector2Int cell)
    {
        return cell.x >= 0 &&
            cell.y >= 0 &&
            cell.x < _board.GetLength(0) &&
            cell.y < _board.GetLength(1);
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

        public Modification(Dictionary<Vector2Int, List<int>> tiles)
        {
            Tiles = tiles;
            //Rooms = rooms ?? new();
        }
        public Modification()
        {
            Tiles = new();
            //Rooms = new();
        }
        public static Modification operator+(Modification l, Modification r)
        {
            var newL = l.Tiles.Select(pair =>
            {
                var exists = r.Tiles.TryGetValue(pair.Key, out var tiles);
                List<int> newList = new(pair.Value);
                if(exists)
                    newList.AddRange(tiles);
                var newPair = new KeyValuePair<Vector2Int, List<int>>(pair.Key, newList);
                return newPair;
            });
            var newR = r.Tiles
                .Where(pair => !l.Tiles.ContainsKey(pair.Key))
                .Select(pair => new KeyValuePair<Vector2Int, List<int>>(pair.Key, new(pair.Value)));
            return new Modification(new(newL.Concat(newR)));
        }

    }

    private IEnumerable<Vector2Int> GetAdjacentCells(Vector2Int v)
    {
        yield return v + new Vector2Int(0, 1);
        yield return v + new Vector2Int(-1, 0);
        yield return v + new Vector2Int(1, 0);
        yield return v + new Vector2Int(0, -1);
    }
    private double EntropySort(Vector2Int cell)
    {
        var walkableCollapsedNeighbors = GetAdjacentCells(cell)
            .Where(v => InBounds(v) &&
            _board[v.x, v.y].Count == 1 &&
            OriginTiles[_board[v.x, v.y].First()].room.HasValue);

        return (4 - walkableCollapsedNeighbors.Count())
            *(OriginTiles.Count+1) + _board[cell.x, cell.y].Count + 1.0*(cell.y*_board.GetLength(0) + cell.x).GetHashCode()/int.MaxValue;
    }
    private class EntropyQueue
    {
        private bool _boardModified = true;
        private List<Vector2Int> _list = new List<Vector2Int>();
        private Func<Vector2Int, double> _sortingMethod;
        public int Count { get => _list.Count; }
        public EntropyQueue(HashSet<int>[,] board, System.Random rng, Func<Vector2Int, double> sortingMethod)
        {
            for(int x = 0; x<board.GetLength(0); x++)
                for(int y = 0; y< board.GetLength(1); y++)
                    _list.Add(new Vector2Int(x, y));
            _list = _list.ToList();
            _sortingMethod = sortingMethod;
        }
        public Vector2Int Dequeue()
        {
            var v = Peek();
            _list.RemoveAt(_list.Count-1);
            return v;
        }

        public Vector2Int Peek()
        {
            if (_boardModified)
            {
                _list = _list.OrderBy(v =>
                    -_sortingMethod(v)
                    ).ToList();
                _boardModified = false;
            }
            return _list[_list.Count - 1];
        }

        public void Enqueue(Vector2Int cell)
        {
            _list.Add(cell);
        }

        public void Notify()
        { 
            _boardModified = true;
        }
    }
    public enum AlgorithmState
    {
        Finished, Running, Paradox
    }
}
