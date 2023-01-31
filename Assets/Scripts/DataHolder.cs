using System.Collections;
using System.Collections.Generic;
using Tiles;
using Graphs;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataHolder : MonoBehaviour
{
    public TileCollection Tiles { get; set; } = null;
    public UndirectedGraph Graph { get; set; } = null;

    public Dictionary<string, (float X, float Y)> VertexPositions { get; set; } = null;
    public int BoardWidth { get; set; } = 30;
    public int BoardHeight { get; set; } = 25;

    public static DataHolder Instance { get; private set; } = null;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            return;
        }
        Destroy(this.gameObject);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
            SceneManager.LoadScene("MainMenu");

    }
}
