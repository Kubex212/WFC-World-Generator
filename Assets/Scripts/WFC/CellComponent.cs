using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CellComponent : MonoBehaviour
{
    private HashSet<int> _superposition = new HashSet<int>();
    private int _maxPossibilities;
    public void Fill(int tileCount)
    {
        _maxPossibilities = tileCount;
        for (int i = 0; i < tileCount; i++)
        {
            _superposition.Add(i);
        }
        GetComponent<Image>().color = Color.black;
    }
    public void Remove(IEnumerable<int> indexes)
    {
        foreach (var tile in indexes)
        {
            _superposition.Remove(tile);
        }

        GetComponent<Image>().color = Color.white.Darker((_superposition.Count-1f)/(_maxPossibilities-1));

        if (_superposition.Count == 1)
        {
            GetComponent<Image>().sprite = SpriteAtlas.Atlas[_superposition.First()];
        }
    }
}
