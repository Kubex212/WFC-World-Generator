using System;
using System.Collections;
using System.Collections.Generic;
using Tiles;
using UnityEngine;
using UnityEngine.UI;

public class NeighborSlotComponent : TileSlot
{
    [SerializeField] GameObject _showcaseTilePrefab;
    public Direction direction;

    public void ShowNeighbors(Tile t)
    {
        Clear();
        if (t == null)
            return;
        var tiles = t.Neighbors[(int)direction];
        int size = SizeFromCount(tiles.Count);
        for (int i = 0; i<tiles.Count; i++)
        {
            var go = Instantiate(_showcaseTilePrefab, transform);
            go.GetComponent<Image>().sprite =
                FindObjectOfType<TileCollectionRenderer>().tileObjects[tiles[i]]
                .GetComponent<Image>().sprite;
            go.GetComponent<RectTransform>().localScale = Vector3.one * GetScale(size);
            go.GetComponent<RectTransform>().localPosition = GetPos(i, size);
            go.GetComponent<ShowcaseTileComponent>().tile = tiles[i];
        }
    }
    private void Clear()
    {
        while (transform.childCount > 0)
        {
            var c = transform.GetChild(0);
            c.SetParent(null);
            Destroy(c.gameObject);
        }
    }
    private static int SizeFromCount(int count)
    {
        return Mathf.CeilToInt(Mathf.Sqrt(count));
    }
    private float GetScale(int size)
    {
        var tileSpacing = TileComponent._tileSpacing;
        return 1f / (size * (1 + tileSpacing) + tileSpacing);
    }
    private Vector3 GetPos(int index, int size)
    {
        var rect = GetComponent<RectTransform>().rect;
        var spacing = TileComponent._tileSpacing*rect.width;
        var d = rect.width + spacing;
        int y = index / size;
        int x = index - y * size;
        return new Vector3(spacing + x * d, -(spacing + y * d), 0)*GetScale(size) + new Vector3(rect.xMin, rect.yMax);
    }
}
