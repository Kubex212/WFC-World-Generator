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
using SFB;
using System.IO.Compression;

public class WFCComponent : MonoBehaviour
{
    [SerializeField] private Button _goBackButton;
    [SerializeField] private Button _goForwardButton;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _jsonExportButton;
    [SerializeField] private Button _csvExportButton;
    [SerializeField] private Button _returnToMainMenuButton;
    [SerializeField] private ErrorListComponent _errors;

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
        _jsonExportButton.onClick.AddListener(Serialize);
        _csvExportButton.onClick.AddListener(ExportCsv);
        _returnToMainMenuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        _board = new CellComponent[_width, _height];
        var tileCollection = FindObjectOfType<DataHolder>().Tiles;
        int size = Math.Max(_width, _height);
        Vector2 pixSize = new(GetComponent<RectTransform>().rect.width, GetComponent<RectTransform>().rect.height);
        float childSize = _cellPrefab.GetComponent<RectTransform>().rect.width;
        var scale = Mathf.Min(pixSize.x,pixSize.y) * Vector3.one / size / childSize;
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _board[x, y] = Instantiate(original: _cellPrefab, parent: transform).GetComponent<CellComponent>();
                _board[x, y].GetComponent<RectTransform>().localScale = scale;
                _board[x, y].GetComponent<RectTransform>().localPosition = GetPos(x, y, size);
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

        _csvExportButton.interactable = _jsonExportButton.interactable =
            _algorithm.State == WaveFunctionCollapse.AlgorithmState.Finished;

        if (_algorithm.State == WaveFunctionCollapse.AlgorithmState.Paradox)
        {
            _errors.tipToShow = "Wyst¹pi³ paradoks. Spróbuj cofn¹æ ruch lub zresetuj planszê.";
            _errors.gameObject.SetActive(true);
        }
        else
        {
            _errors.gameObject.SetActive(false);
        }

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

        if (_algorithm.State == WaveFunctionCollapse.AlgorithmState.Running)
        {
            _board[_algorithm.startRoomLocation.Value.x, _algorithm.startRoomLocation.Value.y].Type = CellType.Start;
            _board[_algorithm.endRoomLocation.Value.x, _algorithm.endRoomLocation.Value.y].Type = CellType.End;
        }
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
        var iter = 0;
        while (_algorithm.State == WaveFunctionCollapse.AlgorithmState.Paradox)
        {
            var (undone, done) = _algorithm.Undo();
            SetParadoxVisuals(false);
            UndoVisuals(undone);
            UpdateVisuals(done);
            if (iter++ > 100)
                break;
        }
        if (_algorithm.State == WaveFunctionCollapse.AlgorithmState.Paradox)
            SetParadoxVisuals(true);
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

    private Vector3 GetPos(int x, int y, int size)
    {
        var rect = GetComponent<RectTransform>().rect;
        var tileSize = Mathf.Min(rect.width, rect.height);
        return new Vector3(x * tileSize, -(y * tileSize), 0)/size
            + new Vector3(-_width, _height) * tileSize/2/size
            + new Vector3(rect.center.x, rect.center.y);
    }

    private void Export()
    {
        var path = StandaloneFileBrowser.SaveFilePanel("Zapisz plik", "", "plansza", "json");

        if (path.Length == 0)
        {
            return;
        }

        var csv = ToCsv();
        File.WriteAllText(path, csv);
    }

    private void ExportCsv()
    {
        var savePath = StandaloneFileBrowser.SaveFilePanel("Zapisz plik", "", "plansza", "zip");
        var dh = FindObjectOfType<DataHolder>();

        using (var fs = new FileStream(savePath, FileMode.OpenOrCreate))
        {
            var boardSb = new StringBuilder();

            for (int row = 0; row < _height; row++)
            {
                for (int col = 0; col < _width; col++)
                {
                    var t = _board[row, col].superposition.First();
                    t = _algorithm.OriginTiles[t].tile;
                    var room = _algorithm.RoomNameFunc(t) == "" ? "x" : _algorithm.RoomNameFunc(t);
                    boardSb.Append($"{t}|{room}");

                    if(col != _width - 1)
                    {
                        boardSb.Append(",");
                    }
                }
                boardSb.AppendLine();
            }

            var tilesetSb = new StringBuilder();
            var ti = dh.Tiles.tiles;

            for (int row = 0; row < ti.Count; row++)
            {
                tilesetSb.Append($"{row}.png,{ti[row].Walkable},{dh.Tiles.edgeTile == ti[row]}");
                tilesetSb.AppendLine();
            }

            var roomsSb = new StringBuilder();
            var g = dh.Graph;

            for (int row = 0; row < g.Vertices.Count; row++)
            {
                var first = g.Vertices[row].IsStart ? "START" : g.Vertices[row].IsExit ? "EXIT" : "";
                roomsSb.Append($"{first},{_algorithm.roomLocations[row].x},{_algorithm.roomLocations[row].y}");
                roomsSb.AppendLine();
            }

            var doorsSb = new StringBuilder();

            var doorInfo = new List<(int x, int y, int from, int to, string key)>();
            //first iterate over board
            for(int x = 0; x < _board.GetLength(0); x++)
                for(int y = 0; y < _board.GetLength(1); y++)
                {
                    var t = _board[x, y].superposition.First();
                    var roomStr = _algorithm.RoomNameFunc(t);
                    if (roomStr.Contains(':'))
                    {
                        var from = roomStr.Split(':')[0];
                        var to = roomStr.Split(':')[1].Split('(')[0];
                        string key = null;
                        if(roomStr.Contains('('))
                        {
                            key = roomStr.Split('(')[1].Split(')')[0];
                        }
                        doorInfo.Add((x, y, int.Parse(from), int.Parse(to),  key));
                    }
                }

            for (int row = 0; row < doorInfo.Count; row++)
            {
                var i = doorInfo[row];
                doorsSb.Append($"{i.key ?? ""},{i.x},{i.y}, {i.from}, {i.to}");
                doorsSb.AppendLine();
            }

            using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Update))
            {
                do
                {
                    archive.Entries[0].Delete();
                } while (archive.Entries.Count > 0);

                var entry = archive.CreateEntry("plansza.csv");
                using (StreamWriter writer = new StreamWriter(entry.Open()))
                {
                    writer.Write(boardSb.ToString());
                }

                entry = archive.CreateEntry("kafelki.csv");
                using (StreamWriter writer = new StreamWriter(entry.Open()))
                {
                    writer.Write(tilesetSb.ToString());
                }

                entry = archive.CreateEntry("pokoje.csv");
                using (StreamWriter writer = new StreamWriter(entry.Open()))
                {
                    writer.Write(roomsSb.ToString());
                }

                entry = archive.CreateEntry("drzwi.csv");
                using (StreamWriter writer = new StreamWriter(entry.Open()))
                {
                    writer.Write(doorsSb.ToString());
                }

                for(int i = 0; i < ti.Count; i++)
                {
                    var t = SpriteAtlas.Atlas[i].texture;
                    var bytes = t.EncodeToPNG();
                    entry = archive.CreateEntry($"{i}.png");
                    using (var writer = new BinaryWriter(entry.Open()))
                    {
                        writer.Write(bytes);
                    }
                }
            }
        }
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
                var t = _board[row, col].superposition.Single();
                sb.Append($"{t},{tiles[t].Walkable} ");
            }
            var tt = _board[row, _height-1].superposition.Single();
            sb.Append($"{tt},{tiles[tt].Walkable}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private void Serialize()
    {

        var path = StandaloneFileBrowser.SaveFilePanel("Zapisz plik", "", "plansza", "json");

        if (path.Length == 0)
        {
            return;
        }

        var tiles = FindObjectOfType<DataHolder>().Tiles.tiles;

        var data = FindObjectOfType<DataHolder>();
        var edges = data.Graph.EdgeList;
        var vertices = data.Graph.Vertices;
        var eo = new Board()
        {
            Width = _width,
            Height = _height,
            TileInfo = new TileId[_height, _width],
            RoomCenters = new RoomCenter[vertices.Count],
            Corridors = new Corridor[edges.Count],
            Tiles = new Tile[tiles.Count],
        };
        foreach (var cell in _algorithm.roomLocations.Keys)
        {
            var room = _algorithm.roomLocations[cell];
            eo.RoomCenters[cell] = new RoomCenter()
            {
                X = room.x,
                Y = room.y,
                Key = vertices[cell].Key,
                Props = vertices[cell].IsStart ? "START" : vertices[cell].IsExit ? "EXIT" : null
            };

        } // (room.x, room.y, vertices[cell].Key);

        var doorInfo = new List<(int x, int y, int from, int to, int? key)>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Debug.Log($"{x}, {y}");
                var t = _board[x, y].superposition.Single();
                var tex = _board[x, y].GetComponent<Image>().sprite.texture;
                var roomStr = _algorithm.RoomNameFunc(t);
                if (roomStr.Contains(':'))
                {
                    var from = roomStr.Split(':')[0];
                    var to = roomStr.Split(':')[1].Split('(')[0];
                    string key = null;
                    if (roomStr.Contains('('))
                    {
                        key = roomStr.Split('(')[1].Split(')')[0];
                    }

                    doorInfo.Add((x, y, int.Parse(from), int.Parse(to), key == null ? null : int.Parse(key)));
                }
                t = _algorithm.OriginTiles[t].tile;
                var roomExists = int.TryParse(roomStr, out int room);
                eo.TileInfo[x, y] = new TileId()
                {
                    Id = _algorithm.OriginTiles[t].tile,
                    Room = roomExists ? room : null
                };
            }
        }

        for(int i = 0; i < doorInfo.Count; i++)
        {
            eo.Corridors[i] = new Corridor()
            {
                X = doorInfo[i].x,
                Y = doorInfo[i].y,
                From = doorInfo[i].from,
                To = doorInfo[i].to,
                Key = doorInfo[i].key,
            };
        }

        for (int i = 0; i < tiles.Count; i++)
        {
            var tex = SpriteAtlas.Atlas[i].texture;
            eo.Tiles[i] = new Tile()
            {
                Bytes = tex.EncodeToPNG(),
                Width = tex.width,
                Height = tex.height,
                Walkable = tiles[i].Walkable
            };
        }

        string text = JsonConvert.SerializeObject(eo);
        File.WriteAllText(path, text);
    }

    //public class ExportObject
    //{
    //    public List<Tile> Tiles { get; set; }
    //    public Tile EdgeTile { get; set; }
    //    public bool Diagonal { get; set; }
    //}

    //[Serializable]
    //public class ExportObject2
    //{
    //    public int Width { get; set; }
    //    public int Height {get; set; }
    //    public int StartX { get; set; }
    //    public int StartY { get; set; }
    //    public int EndX { get; set; }
    //    public int EndY { get; set; }
    //    public (int, int, int?)[] RoomCenters { get; set; }
    //    public (int, int)?[] Corridors { get; set; }
    //    public TileInfo[,] TileInfo { get; set; }
    //}

    //[Serializable]
    //public class TileInfo
    //{
    //    [SerializeField]
    //    public int x;
    //    [SerializeField]
    //    public int y;
    //    [SerializeField]
    //    public byte[] bytes;

    //    public int? Key;
    //    public bool Walkable { get; set; }
    //}

    public class RoomCenter
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int? Key { get; set; }
        public string Props { get; set; }
    }

    public class Corridor
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public int? Key { get; set; }
    }

    public class TileId
    {
        public int Id { get; set; }
        public int? Room { get; set; }
    }

    public class Tile
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Bytes { get; set; }
        public bool Walkable { get; set; }
    }

    public class Board
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public RoomCenter[] RoomCenters { get; set; }
        public Corridor[] Corridors { get; set; }
        public TileId[,] TileInfo { get; set; }
        public Tile[] Tiles { get; set; }
    }
}
