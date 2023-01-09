using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Tiles;
using UnityEngine;
using UnityEngine.UI;

public class WFCComponent : MonoBehaviour
{
    [SerializeField] private Button _goBackButton;
    [SerializeField] private Button _goForwardButton;
    [SerializeField] private Button _retryButton;

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
        if (Input.GetKey(KeyCode.Return))
            Next();
    }

    private void Init()
    {
        var data = FindObjectOfType<DataHolder>();
        var tileCollection = data.Tiles;
        var graph = data.Graph;

        var totalPossibilities = tileCollection.tiles.Count + tileCollection.tiles.Count(t => t.Walkable) * (graph.Vertices.Count + graph.EdgeList.Count - 1);

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _board[x, y].Fill(totalPossibilities);
            }
        }
        _algorithm = new WaveFunctionCollapse(_width, _height, tileCollection, graph, _randomSeed, 2);

        WaveFunctionCollapse.Modification modified = null;
        if (tileCollection.edgeTile != null)
            modified = _algorithm.EnforceEdgeRules(tileCollection.edgeTile.Index);
        UpdateVisuals(modified);

        modified = null;
        for (int i = 0; i < 100; i++)
        {
            modified = _algorithm.SeedRooms(graph);
            if (modified != null)
                break;
        }

        if (modified == null)
            throw new ArgumentException("Could not seed map with specified input graph");
        UpdateVisuals(modified);

        _randomSeed++;
    }

    private void Back()
    {
        var modified = _algorithm.Undo();

        UndoVisuals(modified);
    }

    private void Next()
    {
        var modified = _algorithm.Next();

        UpdateVisuals(modified);
    }

    private void UpdateVisuals(WaveFunctionCollapse.Modification modified)
    {
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
}
