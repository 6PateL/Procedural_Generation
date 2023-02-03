using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generation : MonoBehaviour
{
    private void Start()
    {
        Texture2D texture = new Texture2D(512,512);
        GetComponent<Renderer>().material.mainTexture = texture;
        
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float c = ((x ^ y) % 256f) / 255f;
                Color color = new Color(c, c, c);
                texture.SetPixel(x, y, color);
            }
            texture.Apply();
        }
    }
}
