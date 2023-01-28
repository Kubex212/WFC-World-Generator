using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class CellHighlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _linePrefab;
    [SerializeField] private CellType _target;

    private GameObject _myTile = null;
    private List<GameObject> _lines = null;
    private bool Hovered {
        get => _hovered;
        set
        {
            _hovered = value;
            if (_lines == null && _hovered)
            {
                _lines = new();
                var cells = FindObjectsOfType<CellComponent>().Where(c => (c.Type & _target) != 0);
                foreach (var cell in cells)
                {
                    var line = Instantiate(_linePrefab);
                    //var width =  GetComponent<RectTransform>().rect.width / FindObjectOfType<Canvas>().scaleFactor;
                    line.GetComponent<LineRenderer>().SetPosition(0, transform.position.ZeroZ() + new Vector3(0,0,-1));
                    line.GetComponent<LineRenderer>().SetPosition(1, cell.transform.position.ZeroZ() + new Vector3(0, 0, -1));
                    _lines.Add(line);
                }
            }
            else if (_lines != null)
                foreach (var line in _lines)
                    line.SetActive(_hovered);

        }
    }

    public void OnPointerEnter(PointerEventData eventData) => Hovered = true;

    public void OnPointerExit(PointerEventData eventData) => Hovered = false;

    private bool _hovered = false;
}
