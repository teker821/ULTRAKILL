using System;
using UnityEngine;

namespace CustomRay;

public class RaycastResult : IComparable<RaycastResult>
{
	public float distance;

	public Transform transform;

	public RaycastHit rrhit;

	public RaycastResult(RaycastHit hit)
	{
		distance = hit.distance;
		transform = hit.transform;
		rrhit = hit;
	}

	public int CompareTo(RaycastResult other)
	{
		return distance.CompareTo(other.distance);
	}
}
