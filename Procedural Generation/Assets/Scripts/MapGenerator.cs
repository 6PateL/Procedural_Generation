using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;
    [Range(0f, 200f)] [SerializeField] private float noiseScale;
    
    public bool AutoUpdate; 

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoise(mapWidth,mapHeight,noiseScale);

        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        mapDisplay.DrawNoiseMap(noiseMap);
    }
}
