using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh
    };
    public DrawMode _drawMode;

    private const int mapChunkSize = 241;
    [Range(0,6)] [SerializeField] private int levelOfDetail;
    
    [Range(0f, 200f)] [SerializeField] private float noiseScale;

    [SerializeField] private int octaves;
    [Range(0f,1f)] [SerializeField] private float persistance;
    [SerializeField] private float lacunarity;

    [Range(0,100)] [SerializeField] private int seed; 
    [SerializeField] private Vector2 offset;

    [SerializeField] private float meshHeightMultiplier;
    [SerializeField] private AnimationCurve meshHeightCurve; 

    [SerializeField] private TerrarianType[] regions;
    
    public bool AutoUpdate; 

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoise(mapChunkSize,mapChunkSize,seed,noiseScale,octaves,persistance,lacunarity,offset);
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize]; 
        
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight < regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color; 
                        break;
                    }
                }
            } 
        }
        
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        
        if(_drawMode == DrawMode.NoiseMap){mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));}
        else if(_drawMode == DrawMode.ColorMap){mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap,mapChunkSize,mapChunkSize));}
        else if (_drawMode == DrawMode.Mesh) {
            mapDisplay.DrawMesh(MeshGenerator.GenerateTerrarianMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap,mapChunkSize,mapChunkSize));
        }
    }

    public void OnValidate()
    {
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
