using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _speed;
    [Range(0,100)] [SerializeField] private float _mouseSentivity;
    
    [SerializeField] private Transform _player; 
    
    private void Update()
    {
        float vertical = Input.GetAxis("Vertical") * _speed * Time.deltaTime;
        float horizontal = Input.GetAxis("Horizontal") * _speed * Time.deltaTime;
        float mouseX = Input.GetAxis("Mouse X");

        Vector3 offset = new Vector3(horizontal, 0f, vertical);
        transform.Translate(offset);
        _player.Rotate(0f,mouseX * _mouseSentivity,0f);
    }
}
