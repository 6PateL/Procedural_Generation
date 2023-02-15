using System;
using System.Threading;
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

    public const int mapChunkSize = 241;
    [Range(0,6)] [SerializeField] private int EditorPreviewLOD;
    
    [Range(0f, 200f)] [SerializeField] private float noiseScale;

    [SerializeField] private int octaves;
    [Range(0f,1f)] [SerializeField] private float persistance;
    [SerializeField] private float lacunarity;

    [Range(0,100)] [SerializeField] private int seed; 
    [SerializeField] private Vector2 offset;

    [SerializeField] private float meshHeightMultiplier;
    [SerializeField] private AnimationCurve meshHeightCurve; 

    [SerializeField] private TerrarianType[] regions;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>(); 


    public bool AutoUpdate;

    public void DrawMapInEditor() //MapGeneratorEditor  
    {
        MapData mapData = GenerateMapData(Vector2.zero); 
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        
        if(_drawMode == DrawMode.NoiseMap){mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));}
        else if(_drawMode == DrawMode.ColorMap){mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap,mapChunkSize,mapChunkSize));}
        else if (_drawMode == DrawMode.Mesh) {
            mapDisplay.DrawMesh(MeshGenerator.GenerateTerrarianMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, EditorPreviewLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap,mapChunkSize,mapChunkSize));
        }
    }

    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
           MapDataThread(centre, callback); 
        };
        new Thread(threadStart).Start(); 
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();  
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrarianMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);

        lock (meshDataThreadInfoQueue)
        {
          meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback,meshData));
        }
    }
    
    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter); 
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter); 
            }
        }
    }

    MapData GenerateMapData(Vector2 centre) //Generate map data used map chunk size 
    {
        float[,] noiseMap = Noise.GenerateNoise(mapChunkSize,mapChunkSize,seed,noiseScale,octaves,persistance,lacunarity,centre + offset);
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
        return new MapData(noiseMap, colorMap);
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
    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback; //value was immutable(used only when realized) 
        public readonly T parameter; //value was immutable(used only when realized) 

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.parameter = parameter;
            this.callback = callback; 
        }
    }
}

public struct MapData
{
    public readonly float[,] heightMap; //value was immutable(used only when realized) 
    public readonly Color[] colorMap; //value was immutable(used only when realized)  

    public MapData(float[,] heightMap, Color[] colorMap)

    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
