using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MapSettings : ScriptableObject
{
	public Vector2 buildRangeX = new Vector2(-20f, 20f);
	public Vector2 buildRangeY = new Vector2(-20f, 20f);
	public Vector2 buildRangeZ = new Vector2(-20f, 20f);

	public List<Vector3> spawnPositions = new List<Vector3>();
	public List<Vector3> podiumPositions = new List<Vector3>();
}
