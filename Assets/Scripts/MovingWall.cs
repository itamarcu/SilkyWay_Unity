using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWall : MonoBehaviour
{
    private const float GRID_UNITS_IN_UNITY_UNITS = 0.7f;
    public Vector2 pathVectorInGridUnits;
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
        positionAlongLoop = (positionAlongLoop +
                             (1 / 2f * speedInGridUnitsPerSecond) / pathVectorInGridUnits.magnitude * Time.deltaTime) %
                            1;
        Vector2 delta = (0.5f - 0.5f * Mathf.Cos(positionAlongLoop * 2 * Mathf.PI)) * pathVectorInGridUnits;
        transform.position = birthPlace + delta * MovingWall.GRID_UNITS_IN_UNITY_UNITS;
    }

    private void Rotate()
    {
        transform.Rotate(Vector3.forward, 360f * rotationsPerSecond * Time.deltaTime);
    }
}