using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWall : MonoBehaviour
{
    public Vector2 pathVector;
    [Range(0f, 1f)] public float positionAlongLoop;
    public float speedInGridUnitsPerSeconds;

    private Vector2 birthPlace;

    private void Awake()
    {
        birthPlace = transform.position;
    }

    private void Update()
    {
        positionAlongLoop = (positionAlongLoop + speedInGridUnitsPerSeconds / pathVector.magnitude * Time.deltaTime) %
                            1;
        Vector2 delta;
        if (positionAlongLoop < 0.5f)
        {
            delta = 2 * positionAlongLoop * pathVector;
        }
        else
        {
            delta = (2 * positionAlongLoop - 1) * pathVector;
        }

        transform.position = birthPlace + delta;
    }
}