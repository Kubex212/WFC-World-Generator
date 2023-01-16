using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Tiles;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.IO;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Newtonsoft.Json;

public class WFCComponent : MonoBehaviour
{
    [SerializeField] private Button _goBackButton;
    [SerializeField] private Button _goForwardButton;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _exportButton;
    [SerializeField] private Button _returnToMainMenuButton;

    [SerializeField] private GameObject _cellPrefab;

    [SerializeField] private int _randomSeed;

    private CellComponent[,] _board;
    [SerializeField] private int _width = 10, _height = 10;

    private WaveFunctionCollapse _algorithm = null;

    // Start is called before the first frame update
    void Start()
    {
        _goBackButton.onClick.AddListener(Back);
        _goForwardButton.onClick.AddListener(Next);
        _retryButton.onClick.AddListener(Init);
        _exportButton.onClick.AddListener(Export);
        _returnToMainMenuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        _board = new CellComponent[_width, _height];
        var tileCollection = FindObjectOfType<DataHolder>().Tiles;
        int size = Math.Max(_width, _height);
        float pixSize = Math.Min(GetComponent<RectTransform>().rect.width, GetComponent<RectTransform>().rect.height);
        float childSize = _cellPrefab.GetComponent<RectTransform>().rect.width;
        var scale = pixSize * Vector3.one / size / childSize;
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _board[x, y] = Instantiate(original: _cellPrefab, parent: transform).GetComponent<CellComponent>();
                _board[x, y].GetComponent<RectTransform>().localScale = scale;
                _board[x, y].GetComponent<RectTransform>().localPosition = GetPos(x, y, size, pixSize);
            }
        }
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.RightArrow))
            Next();
        else if (Input.GetKey(KeyCode.LeftArrow))
            Back();

        if (_algorithm.State == WaveFunctionCollapse.AlgorithmState.Finished)
            _exportButton.interactable = true;
        else
            _exportButton.interactable = false;

        if (Input.GetKeyDown(KeyCode.G))
            Serialize();
    }

    private void Init()
    {
        var data = FindObjectOfType<DataHolder>();
        var tileCollection = data.Tiles;
        var graph = data.Graph;

        var totalPossibilities = tileCollection.tiles.Count + tileCollection.tiles.Count(t => t.Walkable) * (graph.Vertices.Count + graph.EdgeList.Count - 1);



        WaveFunctionCollapse.Modification modified = null;

        for (int i = 0; i < 10; i++)
        {
            _algorithm = new WaveFunctionCollapse(_width, _height, tileCollection, graph, _randomSeed++, 2);


            if (tileCollection.edgeTile != null)
                _algorithm.EnforceEdgeRules(tileCollection.edgeTile.Index);
            modified = _algorithm.SeedRooms(graph);
            if (_algorithm.State != WaveFunctionCollapse.AlgorithmState.Paradox)
                break;
        }
        Func<int, string> roomNameFunc = _algorithm.RoomNameFunc;
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _board[x, y].Fill(totalPossibilities, roomNameFunc);
            }
        }

        UpdateVisuals(modified);

        _randomSeed++;
    }

    private void Back()
    {
        var state = _algorithm.State;
        var (undone,done) = _algorithm.Undo();
        if (state == WaveFunctionCollapse.AlgorithmState.Paradox)
            SetParadoxVisuals(false);
        UndoVisuals(undone);
        UpdateVisuals(done);
    }

    private void Next()
    {
        var modified = _algorithm.Next();
        UpdateVisuals(modified);
        while (_algorithm.State == WaveFunctionCollapse.AlgorithmState.Paradox)
        {
            var (undone, done) = _algorithm.Undo();
            SetParadoxVisuals(false);
            UndoVisuals(undone);
            UpdateVisuals(done);
        }
    }
    private void SetParadoxVisuals(bool value)
    {
        foreach (var cell in _board)
        {
            cell.Paradox = value;
        }
    }

    private void UpdateVisuals(WaveFunctionCollapse.Modification modified)
    {
        if(_algorithm.State == WaveFunctionCollapse.AlgorithmState.Paradox)
        {
            SetParadoxVisuals(true);
            return;
        }
        if (modified != null)
        {
            foreach (var key in modified.Tiles.Keys)
            {
                _board[key.x, key.y].Remove(modified.Tiles[key]);
            }
        }

    }

    private void UndoVisuals(WaveFunctionCollapse.Modification modified)
    {
        if (modified != null)
        {
            foreach (var key in modified.Tiles.Keys)
            {
                _board[key.x, key.y].Add(modified.Tiles[key]);
            }
        }
    }

    private Vector3 GetPos(int x, int y, int size, float pixSize)
    {
        var rect = GetComponent<RectTransform>().rect;
        return new Vector3(x * pixSize, -(y * pixSize), 0)/size
            + new Vector3(rect.center.x-pixSize/2, rect.center.y+pixSize/2);
    }

    private void Export()
    {
        var path = EditorUtility.SaveFilePanel(
          "Save the result board.",
          "",
          "board" + ".csv",
          "csv");

        if (path.Length == 0)
        {
            return;
        }

        var csv = ToCsv();
        File.WriteAllText(path, csv);
    }

    private string ToCsv()
    {
        var sb = new StringBuilder();

        var tiles = FindObjectOfType<DataHolder>().Tiles.tiles;

        if (_board.GetLength(0) == 0 || _board.GetLength(1) == 0)
            return null;

        for(int row = 0; row < _width; row++)
        {
            for(int col = 0; col < _height - 1; col++)
            {
                var t = _board[row, col]._superposition.Single();
                sb.Append($"{t},{tiles[t].Walkable} ");
            }
            var tt = _board[row, _height-1]._superposition.Single();
            sb.Append($"{tt},{tiles[tt].Walkable}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private void Serialize()
    {
        var path = EditorUtility.SaveFilePanel(
          "Save the result board.",
          "",
          "board" + ".json",
          "json");

        if (path.Length == 0)
        {
            return;
        }

        var tiles = FindObjectOfType<DataHolder>().Tiles.tiles;

        var data = FindObjectOfType<DataHolder>();
        var edges = data.Graph.EdgeList;
        var vertices = data.Graph.Vertices;
        var eo = new ExportObject2()
        {
            Width = _width,
            Height = _height,
            StartX = _algorithm.startRoomLocation.HasValue ? _algorithm.startRoomLocation.Value.x : -1,
            StartY = _algorithm.startRoomLocation.HasValue ? _algorithm.startRoomLocation.Value.y : -1,
            EndX = _algorithm.endRoomLocation.HasValue ? _algorithm.endRoomLocation.Value.x : -1,
            EndY = _algorithm.endRoomLocation.HasValue ? _algorithm.endRoomLocation.Value.y : -1,
            TileInfo = new TileInfo[_width, _height],
            RoomCenters = new (int, int, int?)[vertices.Count],
            Corridors = new (int, int)?[edges.Count]
        };
        foreach (var cell in _algorithm.roomLocations.Keys)
        {
            var room = _algorithm.roomLocations[cell];
            eo.RoomCenters[cell] = (room.x, room.y, vertices[cell].Key);
        }

        if (_board.GetLength(0) == 0 || _board.GetLength(1) == 0)
            return;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Debug.Log($"{x}, {y}");
                var t = _board[x, y]._superposition.Single();
                var tex = _board[x, y].GetComponent<Image>().sprite.texture;
                eo.TileInfo[x, y] = new TileInfo()
                {
                    x = tex.width,
                    y = tex.height,
                    bytes = ImageConversion.EncodeToPNG(tex),
                    Walkable = _algorithm.OriginTiles[t].room.HasValue
                };
                if (t >= _algorithm.StandardTileCount)
                    eo.Corridors[_algorithm.OriginTiles[t].room.Value] = (x, y);
            }
        }

        string text = JsonConvert.SerializeObject(eo);
        File.WriteAllText(path, text);
    }

    public class ExportObject
    {
        public List<Tile> Tiles { get; set; }
        public Tile EdgeTile { get; set; }
        public bool Diagonal { get; set; }
    }

    [Serializable]
    public class ExportObject2
    {
        public int Width { get; set; }
        public int Height {get; set; }
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int EndX { get; set; }
        public int EndY { get; set; }
        public (int, int, int?)[] RoomCenters { get; set; }
        public (int, int)?[] Corridors { get; set; }
        public TileInfo[,] TileInfo { get; set; }
    }

    [Serializable]
    public class TileInfo
    {
        [SerializeField]
        public int x;
        [SerializeField]
        public int y;
        [SerializeField]
        public byte[] bytes;

        public int? Key;
        public bool Walkable { get; set; }
    }
}
