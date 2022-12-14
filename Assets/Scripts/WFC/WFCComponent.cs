using System;
using System.Collections;
using System.Collections.Generic;
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
        _goForwardButton.onClick.AddListener(Next);
        _retryButton.onClick.AddListener(Init);
        _board = new CellComponent[_width, _height];
        var tileCollection = FindObjectOfType<DataHolder>().Tiles;
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _board[x, y] = Instantiate(original: _cellPrefab, parent: transform).GetComponent<CellComponent>();
                int size = Math.Max(_width, _height);
                float pixSize = Math.Min(GetComponent<RectTransform>().rect.width, GetComponent<RectTransform>().rect.height);
                _board[x, y].GetComponent<RectTransform>().localScale = pixSize * Vector3.one / size;
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
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _board[x, y].Fill(tileCollection.tiles.Count);
            }
        }
        _algorithm = new WaveFunctionCollapse(_width, _height, tileCollection, graph, _randomSeed);

        Dictionary<Vector2Int, List<int>> modified = null;
        if (tileCollection.edgeTile != null)
            modified = _algorithm.EnforceEdgeRules(tileCollection.edgeTile.Index);
        UpdateVisuals(modified);
        _randomSeed++;
    }
    private void Next()
    {
        var modified = _algorithm.Next();

        UpdateVisuals(modified);
    }

    private void UpdateVisuals(Dictionary<Vector2Int, List<int>> modified)
    {
        if (modified != null)
            foreach (var key in modified.Keys)
            {
                _board[key.x, key.y].Remove(modified[key]);
            }
    }

    private Vector3 GetPos(int x, int y, int size, float pixSize)
    {
        var rect = GetComponent<RectTransform>().rect;
        return new Vector3(x * pixSize, -(y * pixSize), 0)/size
            + new Vector3(rect.center.x-pixSize/2, rect.center.y+pixSize/2);
    }
}
