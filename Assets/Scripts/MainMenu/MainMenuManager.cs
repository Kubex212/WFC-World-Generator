using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button _graphsButton;
    [SerializeField] private Button _neighborhoodButton;
    [SerializeField] private Button _synthesisButton;
    [SerializeField] private Button _authorsButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private EventSystem _eventSystem;
    private static MainMenuManager _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
            return;
        }
        Destroy(this.gameObject);
    }

    void Start()
    {
        _graphsButton.onClick.AddListener(GoToGraphs);
        _neighborhoodButton.onClick.AddListener(GoToNeighborhood);
        _synthesisButton.onClick.AddListener(GoToSynthesis);
        _exitButton.onClick.AddListener(Application.Quit);
    }

    private void GoToGraphs()
    {
        SceneManager.LoadScene("Graphs");
    }
    private void GoToNeighborhood()
    {
        SceneManager.LoadScene("Neighborhoods", LoadSceneMode.Additive);
    }

    private void GoToSynthesis()
    {
        SceneManager.LoadScene("WaveFunctionCollapse", LoadSceneMode.Additive);
    }
}
