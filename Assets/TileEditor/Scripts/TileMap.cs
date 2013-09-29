using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TileMap : MonoBehaviour
{
	static Queue<PathTile> queue = new Queue<PathTile>();
	static List<PathTile> closed = new List<PathTile>();
	static Dictionary<PathTile, PathTile> source = new Dictionary<PathTile, PathTile>();

	public const int maxColumns = 10000;

	public float tileSize = 1;
	public Transform tilePrefab;
	public TileSet tileSet;
	public bool connectDiagonals;
	public bool cutCorners;

	public List<int> hashes = new List<int>(100000);
	public List<Transform> prefabs = new List<Transform>(100000);
	public List<int> directions = new List<int>(100000);
	public List<Transform> instances = new List<Transform>(100000);

	void Start()
	{
		UpdateConnections();
	}

	public int GetHash(int x, int z)
	{
		return (x + TileMap.maxColumns / 2) + (z + TileMap.maxColumns / 2) * TileMap.maxColumns;
	}
	
	public int GetIndex(int x, int z)
	{
		return hashes.IndexOf(GetHash(x, z));
	}
	
	public Vector3 GetPosition(int index)
	{
		index = hashes[index];
		return new Vector3(((index % maxColumns) - (maxColumns / 2)) * tileSize, 0, ((index / maxColumns) - (maxColumns / 2)) * tileSize);
	}
	public void GetPosition(int index, out int x, out int z)
	{
		index = hashes[index];
		x = (index % maxColumns) - (maxColumns / 2);
		z = (index / maxColumns) - (maxColumns / 2);
	}

	public void UpdateConnections()
	{
		//Build connections
		PathTile r, l, f, b;
		for (int i = 0; i < instances.Count; i++)
		{
			var tile = instances[i].GetComponent<PathTile>();
			if (tile != null)
			{
				int x, z;
				GetPosition(i, out x, out z);
				tile.connections.Clear();
				r = Connect(tile, x, z, x + 1, z);
				l = Connect(tile, x, z, x - 1, z);
				f = Connect(tile, x, z, x, z + 1);
				b = Connect(tile, x, z, x, z - 1);
				if (connectDiagonals)
				{
					if (cutCorners)
					{
						Connect(tile, x, z, x + 1, z + 1);
						Connect(tile, x, z, x - 1, z - 1);
						Connect(tile, x, z, x - 1, z + 1);
						Connect(tile, x, z, x + 1, z - 1);
					}
					else
					{
						if (r != null && f != null)
							Connect(tile, x, z, x + 1, z + 1);
						if (l != null && b != null)
							Connect(tile, x, z, x - 1, z - 1);
						if (l != null && f != null)
							Connect(tile, x, z, x - 1, z + 1);
						if (r != null && b != null)
							Connect(tile, x, z, x + 1, z - 1);
					}
				}
			}
		}
	}

	PathTile Connect(PathTile tile, int x, int z, int toX, int toZ)
	{
		var index = GetIndex(toX, toZ);
		if (index >= 0)
		{
			var other = instances[index].GetComponent<PathTile>();
			if (other != null)
			{
				tile.connections.Add(other);
				return other;
			}
		}
		return null;
	}

	PathTile GetPathTile(int x, int z)
	{
		var index = GetIndex(x, z);
		if (index >= 0)
			return instances[index].GetComponent<PathTile>();
		else
			return null;
	}
	public PathTile GetPathTile(Vector3 position)
	{
		var x = Mathf.RoundToInt(position.x / tileSize);
		var z = Mathf.RoundToInt(position.z / tileSize);
		return GetPathTile(x, z);
	}
	
	public bool FindPath(PathTile start, PathTile end, List<PathTile> path, Predicate<PathTile> isWalkable)
	{
		if (!isWalkable(end))
			return false;
		closed.Clear();
		source.Clear();
		queue.Clear();
		closed.Add(start);
		source.Add(start, null);
		if (isWalkable(start))
			queue.Enqueue(start);
		while (queue.Count > 0)
		{
			var tile = queue.Dequeue();
			if (tile == end)
			{
				path.Clear();
				while (tile != null)
				{
					path.Add(tile);
					tile = source[tile];
				}
				path.Reverse();
				return true;
			}
			else
			{
				foreach (var connection in tile.connections)
				{
					if (!closed.Contains(connection) && isWalkable(connection))
					{
						closed.Add(connection);
						source.Add(connection, tile);
						queue.Enqueue(connection);
					}
				}
			}
		}
		return false;
	}
	public bool FindPath(PathTile start, PathTile end, List<PathTile> path)
	{
		return FindPath(start, end, path, tile => true);
	}
	public bool FindPath(Vector3 start, Vector3 end, List<PathTile> path, Predicate<PathTile> isWalkable)
	{
		var startTile = GetPathTile(start);
		var endTile = GetPathTile(end);
		return startTile != null && endTile != null && FindPath(startTile, endTile, path, isWalkable);
	}
	public bool FindPath(Vector3 start, Vector3 end, List<PathTile> path)
	{
		return FindPath(start, end, path, tile => true);
	}
}
