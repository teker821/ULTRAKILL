using System.Collections.Generic;
using UnityEngine;

public class ProjectileParryZone : MonoBehaviour
{
	private List<GameObject> projs = new List<GameObject>();

	public Material origMat;

	public Material newMat;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 14 && other.gameObject.GetComponentInChildren<Projectile>() != null)
		{
			projs.Add(other.gameObject);
			MeshRenderer component = other.GetComponent<MeshRenderer>();
			if (component != null && component.sharedMaterial == origMat)
			{
				component.material = newMat;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == 14 && projs.Contains(other.gameObject))
		{
			projs.Remove(other.gameObject);
			MeshRenderer component = other.GetComponent<MeshRenderer>();
			if (component != null && component.sharedMaterial == origMat)
			{
				component.material = origMat;
			}
		}
	}

	public Projectile CheckParryZone()
	{
		Projectile projectile = null;
		float num = 100f;
		List<GameObject> list = new List<GameObject>();
		if (projs.Count > 0)
		{
			foreach (GameObject proj in projs)
			{
				if (proj != null && proj.activeInHierarchy && Vector3.Distance(base.transform.parent.position, proj.transform.position) < num)
				{
					projectile = proj.GetComponentInChildren<Projectile>();
					if (projectile != null && !projectile.undeflectable)
					{
						num = Vector3.Distance(base.transform.parent.position, proj.transform.position);
					}
					else
					{
						list.Add(proj);
					}
				}
				else if (proj == null || !proj.activeInHierarchy)
				{
					list.Add(proj);
				}
			}
		}
		if (list.Count > 0)
		{
			foreach (GameObject item in list)
			{
				projs.Remove(item);
			}
		}
		if (projectile != null)
		{
			return projectile;
		}
		return null;
	}
}
