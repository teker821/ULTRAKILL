using System;
using UnityEngine;

public class FishConstraints : MonoBehaviour
{
	[SerializeField]
	private Collider[] restrictToColliderBounds;

	[NonSerialized]
	public Bounds area;

	private void Awake()
	{
		Collider[] components;
		if (restrictToColliderBounds == null || restrictToColliderBounds.Length == 0)
		{
			components = GetComponents<BoxCollider>();
			restrictToColliderBounds = components;
		}
		if (restrictToColliderBounds == null || restrictToColliderBounds.Length == 0)
		{
			return;
		}
		components = restrictToColliderBounds;
		foreach (Collider collider in components)
		{
			if (!(collider == null))
			{
				_ = area;
				if (area.size == Vector3.zero)
				{
					area = collider.bounds;
				}
				else
				{
					area.Encapsulate(collider.bounds);
				}
			}
		}
	}

	private void OnDrawGizmos()
	{
		Bounds bounds = default(Bounds);
		if (restrictToColliderBounds != null)
		{
			Collider[] array = restrictToColliderBounds;
			foreach (Collider collider in array)
			{
				if (!(collider == null))
				{
					if (bounds.size == Vector3.zero)
					{
						bounds = collider.bounds;
					}
					else
					{
						bounds.Encapsulate(collider.bounds);
					}
				}
			}
		}
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(bounds.center, bounds.size);
	}
}
