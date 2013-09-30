using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
	public float walkSpeed;
	
	TileMap tileMap;
	List<PathTile> path = new List<PathTile>();
	LineRenderer lineRenderer;

	void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		tileMap = FindObjectOfType(typeof(TileMap)) as TileMap;
		enabled = tileMap != null;


	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			var plane = new Plane(Vector3.up, Vector3.zero);
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			float hit;
			if (plane.Raycast(ray, out hit))
			{
				var target = ray.GetPoint(hit);
				if (tileMap.FindPath(transform.position, target, path))
				{
					lineRenderer.SetVertexCount(path.Count);
					for (int i = 0; i < path.Count; i++)
						lineRenderer.SetPosition(i, path[i].transform.position);

					StopAllCoroutines();
					StartCoroutine(WalkPath());
				}
			}
		}
	}

	IEnumerator WalkPath()
	{
		var index = 0;
		while (index < path.Count)
		{
			yield return StartCoroutine(WalkTo(path[index].transform.position));
			index++;
		}
	}

	IEnumerator WalkTo(Vector3 position)
	{
		while (Vector3.Distance(transform.position, position) > 0.01f)
		{
			transform.position = Vector3.MoveTowards(transform.position, position, walkSpeed * Time.deltaTime);
			yield return 0;
		}
		transform.position = position;
	}

	/*void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		for (int i = 0; i < path.Count; i++)
		{
			Gizmos.DrawSphere(path[i].transform.position, 0.05f);
			if (i > 0)
				Gizmos.DrawLine(path[i - 1].transform.position, path[i].transform.position);
		}
	}*/
}
