using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWall : MonoBehaviour
{
    public Vector2 pathVector;
    [Range(0f, 1f)] public float positionAlongLoop;
    [Range(0f, 5f)] public float speedInGridUnitsPerSecond;
    [Range(-2.5f, 2.5f)] public float rotationsPerSecond;

    private Vector2 birthPlace;

    private void Awake()
    {
        birthPlace = transform.position;
    }

    private void Update()
    {
        Slide();
        Rotate();
    }

    private void Slide()
    {
        positionAlongLoop = (positionAlongLoop + speedInGridUnitsPerSecond / pathVector.magnitude * Time.deltaTime) % 1;
        Vector2 delta = (2 * positionAlongLoop - (int)(positionAlongLoop >= 0.5f)) * pathVector;
        transform.position = birthPlace + delta;
    }

    private void Rotate()
    {
        transform.rotation.z += 360f * rotationsPerSecond * Time.deltaTime;
        transform.rotation.z %= 360;
    }
}