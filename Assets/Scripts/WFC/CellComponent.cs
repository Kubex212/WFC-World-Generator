using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
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
        GetComponent<Image>().color = Color.red;
        GetComponent<Image>().sprite = null;
        GetComponentInChildren<TextMeshProUGUI>().text = "?";
    }
    public void Remove(IEnumerable<int> indexes)
    {
        _superposition.ExceptWith(indexes);

        SetVisuals();
    }

    public void Add(IEnumerable<int> indexes)
    {
        _superposition.UnionWith(indexes);

        SetVisuals();
    }

    private void SetVisuals()
    {
        if (_superposition.Count == 1)
        {
            var sp = GetComponent<Image>().sprite = SpriteAtlas.Atlas[_superposition.First()];
            SetRoom(_superposition.First() - SpriteAtlas.Atlas.ToList().IndexOf(sp));
            GetComponent<Image>().color = Color.white;
        }
        else
        {
            GetComponent<Image>().sprite = null;
            GetComponent<Image>().color = Color.Lerp(Color.blue, Color.red, (_superposition.Count - 1f) / (_maxPossibilities - 1));
        }
    }

    public void SetRoom(int? room)
    {
        GetComponentInChildren<TextMeshProUGUI>().text = room?.ToString(/* ?? */) ?? "?"; // ??
    }

}