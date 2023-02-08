using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator  
{
   public static MeshData GenerateTerrarianMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int leveOfDetail)
   {
      int width = heightMap.GetLength(0);
      int height = heightMap.GetLength(1);
      float topLeftX = (width - 1) / -2f;
      float topLeftZ = (height - 1) / 2f;

      int meshSimplificationIncrement = (leveOfDetail == 0) ? 1 : leveOfDetail * 2;
      int verticlesPerLine = (width - 1) / meshSimplificationIncrement + 1;  

      MeshData meshData = new MeshData(verticlesPerLine,verticlesPerLine);
      int vertexIndex = 0; 

      for (int y = 0; y < height; y += meshSimplificationIncrement)
      {
         for (int x = 0; x < width; x += meshSimplificationIncrement)
         {
            meshData.verticles[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x,y]) * heightMultiplier, topLeftZ - y);
            meshData._uvs[vertexIndex] = new Vector2(x/(float)width, y/(float)height); 

            if (x < width - 1 && y < height - 1)
            {
               meshData.AddTriangle(vertexIndex,vertexIndex + verticlesPerLine + 1, vertexIndex + verticlesPerLine);
               meshData.AddTriangle(vertexIndex + verticlesPerLine + 1,vertexIndex, vertexIndex + 1);
            }
            
            vertexIndex++;
         } 
      }

      return meshData;
   }
}

public class MeshData
{
   public Vector3[] verticles;
   public int[] triangles;
   public Vector2[] _uvs; 

   private int _triangleIndex; 

   public MeshData(int meshWidth, int meshHeight)
   {
      verticles = new Vector3[meshWidth * meshHeight];
      _uvs = new Vector2[meshWidth * meshHeight]; 
      triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
   }

   public void AddTriangle(int a, int b, int c)
   {
      triangles[_triangleIndex] = a;
      triangles[_triangleIndex + 1] = b;
      triangles[_triangleIndex + 2] = c;

      _triangleIndex += 3; 
   }

   public Mesh CreateMesh()
   {
      Mesh mesh = new Mesh();
      mesh.vertices = verticles;
      mesh.triangles = triangles;
      mesh.uv = _uvs;
      mesh.RecalculateNormals();

      return mesh;
   }
}
