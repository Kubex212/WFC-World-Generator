using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeGameObject : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;

    public GameObject _from;
    public GameObject _to;

    private void Update()
    {
        if(_from == null || _to == null)
        {
            Debug.LogError("[EDGE] one of the vertex is null");
            return;
        }
        _lineRenderer.SetPosition(0, _from.transform.position);
        _lineRenderer.SetPosition(1, _to.transform.position);
    }
}
