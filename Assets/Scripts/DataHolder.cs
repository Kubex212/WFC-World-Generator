using System.Collections;
using System.Collections.Generic;
using Tiles;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataHolder : MonoBehaviour
{
    public List<Tile> tiles;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Q))
            SceneManager.LoadScene("Neighborhoods");
        if (Input.GetKey(KeyCode.W))
            SceneManager.LoadScene("WaveFunctionCollapse");
    }
}
