using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempEdgeGameObject : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Color _color;
    [Range(0.01f, 0.5f)]
    [SerializeField] private float _thickness;

    public GameObject _from;

    private void Start()
    {
        _lineRenderer.startColor = _lineRenderer.endColor = _color;
    }

    private void Update()
    {
        if (_from == null)
        {
            Debug.LogError("[TEMPEDGE] 'from' vertex is null");
            return;
        }
        _lineRenderer.SetPosition(0, _from.transform.position);
        _lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        _lineRenderer.startWidth = _lineRenderer.endWidth = _thickness;
    }
}