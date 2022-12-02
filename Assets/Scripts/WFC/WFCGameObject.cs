using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WFCGameObject : MonoBehaviour
{
    [SerializeField] private Button _goBackButton;
    [SerializeField] private Button _goForwardButton;
    [SerializeField] private Button _retryButton;

    private WaveFunctionCollapse _algorithm;

    // Start is called before the first frame update
    void Start()
    {
        _goForwardButton.onClick.AddListener(Next);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Next()
    {
        _algorithm.Next();
        // ...
    }
}