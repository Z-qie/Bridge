using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Transform playerCamera;
    public Vector2 parallaxRate;

    private Vector2 backgroundStartPosition;
    private Vector2 cameraStartPosition;

    // Start is called before the first frame update
    void Start()
    {
        backgroundStartPosition = transform.position;
        cameraStartPosition = playerCamera.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector2(backgroundStartPosition.x + (playerCamera.position.x - cameraStartPosition.x) * parallaxRate.x, backgroundStartPosition.y + (playerCamera.position.y - cameraStartPosition.y) * parallaxRate.y); 
    }
}
