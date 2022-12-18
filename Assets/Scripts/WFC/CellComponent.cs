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
    public void Fill(int tileCount, int roomCount)
    {
        _maxPossibilities = tileCount;
        for (int i = 0; i < tileCount; i++)
        {
            _superposition.Add(i);
        }
        GetComponent<Image>().color = Color.red;
        GetComponent<Image>().sprite = null;
        GetComponentInChildren<TextMeshProUGUI>().text = new StringBuilder().AppendJoin(' ',
            Enumerable.Range(-roomCount+1, roomCount).Select((i) => $"{-i}")
            ).ToString();
    }
    public void Remove(IEnumerable<int> indexes, IEnumerable<int> rooms)
    {
        foreach (var tile in indexes)
        {
            _superposition.Remove(tile);
        }
        var textComponent = GetComponentInChildren<TextMeshProUGUI>();
        textComponent.text = new StringBuilder().AppendJoin(' ', 
            textComponent.text
            .Split(' ')
            .Except(rooms.Select((i) => $"{i}"))
        ).ToString();


        GetComponent<Image>().color = Color.Lerp(Color.blue, Color.red, (_superposition.Count-1f)/(_maxPossibilities-1));

        if (_superposition.Count == 1)
        {
            GetComponent<Image>().sprite = SpriteAtlas.Atlas[_superposition.First()];
            GetComponent<Image>().color = Color.white;
        }
    }

}