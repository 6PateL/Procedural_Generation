using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

public class EndlessGeneration : MonoBehaviour
{
  const float viewerMoveThresholdForChunkUpdate = 25f;

  private const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate; 

  public LODInfo[] detailLevels;
  public static float maxViewDistance;
  [SerializeField] private Transform viewer;

  public static Vector2 viewerPosition;
  private Vector2 viewerPositionOld; 
  private int chunkSize;
  private int chunkVisibleInViewDistance;

  Dictionary<Vector2, TerrainChunk> _terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
  List<TerrainChunk> _terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

  static MapGenerator _mapGenerator;

  [SerializeField] private Material mapMaterial; 

  private void Start()
  {
    _mapGenerator = FindObjectOfType<MapGenerator>();

    maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
    chunkSize = MapGenerator.mapChunkSize - 1;
    chunkVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
    
    UpdateVisibleChunks();
  }

  private void Update()
  {
    viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

    if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
    {
      viewerPosition = viewerPositionOld; 
      UpdateVisibleChunks();
    }
  }

  private void UpdateVisibleChunks()
  {
    for (int i = 0; i < _terrainChunksVisibleLastUpdate.Count; i++)
    {
      _terrainChunksVisibleLastUpdate[i].SetVisible(false);
    }
    _terrainChunksVisibleLastUpdate.Clear();
    
    int chunkCurrentCoordinateX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
    int chunkCurrentCoordinateY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

    for (int yOffset = -chunkVisibleInViewDistance; yOffset <= chunkVisibleInViewDistance; yOffset++)
    {
      for (int xOffset = -chunkVisibleInViewDistance; xOffset <= chunkVisibleInViewDistance; xOffset++)
      {
        Vector2 viewedChunkCoordinate = new Vector2(chunkCurrentCoordinateX + xOffset,chunkCurrentCoordinateY + yOffset);

        if (_terrainChunkDictionary.ContainsKey(viewedChunkCoordinate))
        {
          _terrainChunkDictionary[viewedChunkCoordinate].UpdateTerrainChunk();
          if(_terrainChunkDictionary[viewedChunkCoordinate].IsVisible()){_terrainChunksVisibleLastUpdate.Add(_terrainChunkDictionary[viewedChunkCoordinate]);}
        } else {
          _terrainChunkDictionary.Add(viewedChunkCoordinate, new TerrainChunk(viewedChunkCoordinate, chunkSize, detailLevels, transform, mapMaterial));
        }
      }
    }
  }
  
  class TerrainChunk
  {
    GameObject meshObject; 
    Vector2 position;
    Bounds bounds;

    MapData mapData;
    bool mapDataReceived; 
    
    MeshRenderer _meshRenderer;
    MeshFilter _meshFilter;
    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;
    int previousLODIndex = -1;

    public TerrainChunk(Vector2 coordinate, int size,LODInfo[] detailLevels, Transform parent, Material material)
    {
      this.detailLevels = detailLevels; 
      
      position = coordinate * size;
      bounds = new Bounds(position, Vector2.one * size); 
      Vector3 positionV3 = new Vector3(position.x,0,position.y);

      meshObject = new GameObject("Terrain chunk");
      _meshRenderer = meshObject.AddComponent<MeshRenderer>();
      _meshFilter = meshObject.AddComponent<MeshFilter>();
      _meshRenderer.material = material; 
      
      meshObject.transform.position = positionV3;
      meshObject.transform.parent = parent; 
      
      SetVisible(false);

      lodMeshes = new LODMesh[detailLevels.Length];
      for (int i = 0; i < detailLevels.Length; i++)
      {
        lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk); 
      }
      
      _mapGenerator.RequestMapData(position,OnMapDataReceived);
    }

    void OnMapDataReceived(MapData mapData)
    {
      this.mapData = mapData;
      mapDataReceived = true;

      Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
      _meshRenderer.material.mainTexture = texture; 
      
      UpdateTerrainChunk();
    }

    public void UpdateTerrainChunk()
    {
      if (mapDataReceived)
      {
        float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
       bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;
       SetVisible(visible);
 
       if (visible) {
         int lodIndex = 0;
         for (int i = 0; i < detailLevels.Length - 1; i++) {
           if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold) {
             lodIndex = i + 1;
           } else {
             break;
           }
         }
         if (lodIndex != previousLODIndex) {
           LODMesh lodMesh = lodMeshes[lodIndex];
           if (lodMesh.hasMesh) {
             previousLODIndex = lodIndex; 
             _meshFilter.mesh = lodMesh.mesh;
           }
           else if(!lodMesh.hasRequestedMesh) {
             lodMesh.RequestMesh(mapData);
           }
         }
       }
      }
    }
    public void SetVisible(bool visible)
    {
       meshObject.SetActive(visible);
    }
    public bool IsVisible()
    {
      return meshObject.activeSelf;
    }
  }

  class LODMesh
  {
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int lod;
    System.Action updateCallBack; 

    public LODMesh(int lod, System.Action updateCallBack)
    {
      this.lod = lod;
      this.updateCallBack = updateCallBack;
    }

    void OnMeshDataReceived(MeshData meshData)
    {
      mesh = meshData.CreateMesh();
      hasMesh = true;

      updateCallBack();
    }
    public void RequestMesh(MapData mapData)
    {
      hasRequestedMesh = true;
      _mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
    }
  }
  
  [System.Serializable]
  public struct LODInfo
  {
    public int lod;
    public float visibleDistanceThreshold;
  }
}
