using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathTile : MonoBehaviour
{
	[HideInInspector] public List<PathTile> connections = new List<PathTile>();
}
