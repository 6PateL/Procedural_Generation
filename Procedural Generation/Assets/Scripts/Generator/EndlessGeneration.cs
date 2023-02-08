using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

public class EndlessGeneration : MonoBehaviour
{
  private const float maxViewDistance = 300;
  [SerializeField] private Transform viewer;

  public static Vector2 viewerPosition;
  private int chunkSize;
  private int chunkVisibleInViewDistance;

  Dictionary<Vector2, TerrainChunk> _terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
  List<TerrainChunk> _terrainChunksVisibleLastUpdate = new List<TerrainChunk>(); 

  private void Start()
  {
    chunkSize = MapGenerator.mapChunkSize - 1;
    chunkVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
  }

  private void Update()
  {
    viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
    UpdateVisibleChunks();
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
          _terrainChunkDictionary.Add(viewedChunkCoordinate, new TerrainChunk(viewedChunkCoordinate, chunkSize));
        }
      }
    }
  }
  
  public class TerrainChunk
  {
    GameObject meshObject; 
    Vector2 position;
    Bounds bounds; 
    
    public TerrainChunk(Vector2 coordinate, int size)
    {
      position = coordinate * size;
      bounds = new Bounds(position, Vector2.one * size); 
      Vector3 positionV3 = new Vector3(position.x,0,position.y);
      
      meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
      meshObject.transform.position = positionV3;
      meshObject.transform.localScale = Vector3.one * size / 10f;
      
      SetVisible(false);
    }

    public void UpdateTerrainChunk()
    {
      float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
      bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;
      SetVisible(visible);
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
}
