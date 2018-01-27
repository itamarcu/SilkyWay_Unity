using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWall : MonoBehaviour
{
	[Range(0f, 1f)]
	public float positionAlongWay;
	public float speedInGridUnitsPerSeconds;
	public float distanceToMoveInGridUnits;

	private void Update()
	{
		throw new System.NotImplementedException();
	}
}
