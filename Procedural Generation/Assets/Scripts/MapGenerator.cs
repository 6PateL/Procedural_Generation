using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap
    };
    public DrawMode _drawMode;

    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;
    [Range(0f, 200f)] [SerializeField] private float noiseScale;

    [SerializeField] private int octaves;
    [Range(0f,1f)] [SerializeField] private float persistance;
    [SerializeField] private float lacunarity;

    [Range(0,100)] [SerializeField] private int seed; 
    [SerializeField] private Vector2 offset;

    [SerializeField] private TerrarianType[] regions;
    
    public bool AutoUpdate; 

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoise(mapWidth,mapHeight,seed,noiseScale,octaves,persistance,lacunarity,offset);
        Color[] colorMap = new Color[mapWidth * mapHeight]; 
        
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight < regions[i].height)
                    {
                        colorMap[y * mapWidth + x] = regions[i].color; 
                        break;
                    }
                }
            } 
        }
        
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        
        if(_drawMode == DrawMode.NoiseMap){mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));}
        else if(_drawMode == DrawMode.ColorMap){mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap,mapWidth,mapHeight));}
    }

    public void OnValidate()
    {
        if (mapWidth < 1) mapWidth = 1;
        if (mapHeight < 1) mapHeight = 1;
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }
    
    [System.Serializable]
    public struct TerrarianType
    {
        public string name;
        public float height;
        public Color color;
    }
}
