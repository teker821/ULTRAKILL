using System.Collections.Generic;
using UnityEngine;

public class WallCheck : MonoBehaviour
{
	public bool onWall;

	public Vector3 poc;

	public List<Collider> cols = new List<Collider>();

	private List<Collider> colsToDelete = new List<Collider>();

	private List<Collider> enemyCols = new List<Collider>();

	private void Update()
	{
		if (onWall && cols.Count == 0)
		{
			onWall = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((other.gameObject.layer == 8 || other.gameObject.layer == 24 || other.gameObject.layer == 11) && !other.isTrigger)
		{
			onWall = true;
			cols.Add(other);
		}
		else if (other.gameObject.layer == 12)
		{
			enemyCols.Add(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == 8 || other.gameObject.layer == 17 || other.gameObject.layer == 24 || other.gameObject.layer == 11)
		{
			if (cols.Contains(other))
			{
				cols.Remove(other);
			}
		}
		else if (other.gameObject.layer == 12 && enemyCols.Contains(other))
		{
			enemyCols.Remove(other);
		}
	}

	public bool CheckForCols()
	{
		bool result = false;
		poc = Vector3.zero;
		float num = 100f;
		if (cols.Count > 1)
		{
			foreach (Collider col in cols)
			{
				if (col != null && col.enabled && col.gameObject.activeInHierarchy && (col.gameObject.layer == 8 || col.gameObject.layer == 24) && !col.isTrigger)
				{
					Vector3 a = ColliderUtility.FindClosestPoint(col, base.transform.position, ignoreVerticalTriangles: true);
					if (Vector3.Distance(a, base.transform.position) < num && Vector3.Distance(a, base.transform.position) < 5f)
					{
						num = Vector3.Distance(a, base.transform.position);
						poc = a;
						result = true;
					}
					else if (Vector3.Distance(a, base.transform.position) >= 5f)
					{
						colsToDelete.Add(col);
					}
				}
				else
				{
					colsToDelete.Add(col);
				}
			}
		}
		else if (cols.Count == 1 && cols[0] != null && cols[0].enabled && cols[0].gameObject.activeInHierarchy && Vector3.Distance(ColliderUtility.FindClosestPoint(cols[0], base.transform.position, ignoreVerticalTriangles: true), base.transform.position) < 5f)
		{
			poc = ColliderUtility.FindClosestPoint(cols[0], base.transform.position, ignoreVerticalTriangles: true);
			result = true;
		}
		else if (cols.Count == 1 && (cols[0] == null || Vector3.Distance(ColliderUtility.FindClosestPoint(cols[0], base.transform.position, ignoreVerticalTriangles: true), base.transform.position) < 5f))
		{
			colsToDelete.Add(cols[0]);
		}
		if (colsToDelete.Count > 0)
		{
			foreach (Collider item in colsToDelete)
			{
				if (cols.Contains(item))
				{
					cols.Remove(item);
				}
				else if (enemyCols.Contains(item))
				{
					enemyCols.Remove(item);
				}
			}
		}
		colsToDelete.Clear();
		return result;
	}

	public bool CheckForEnemyCols()
	{
		if ((bool)MonoSingleton<NewMovement>.Instance.ridingRocket)
		{
			return false;
		}
		if (enemyCols.Count > 0)
		{
			bool result = false;
			foreach (Collider enemyCol in enemyCols)
			{
				if (enemyCol != null && enemyCol.enabled && enemyCol.gameObject.activeInHierarchy && Vector3.Distance(base.transform.position, enemyCol.transform.position) < 40f)
				{
					result = true;
				}
				else
				{
					colsToDelete.Add(enemyCol);
				}
			}
			if (colsToDelete.Count > 0)
			{
				foreach (Collider item in colsToDelete)
				{
					if (cols.Contains(item))
					{
						cols.Remove(item);
					}
					else if (enemyCols.Contains(item))
					{
						enemyCols.Remove(item);
					}
				}
				colsToDelete.Clear();
			}
			return result;
		}
		return false;
	}
}
