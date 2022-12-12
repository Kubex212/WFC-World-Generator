using System.Collections;
using System.Collections.Generic;
using Tiles;
using Graphs;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataHolder : MonoBehaviour
{
    public TileCollection Tiles { get; set; }
    public UndirectedGraph Graph { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
            SceneManager.LoadScene("Graphs");
        if (Input.GetKey(KeyCode.W))
            SceneManager.LoadScene("Neighborhoods");
        if (Input.GetKey(KeyCode.E))
            SceneManager.LoadScene("WaveFunctionCollapse");
    }
}
