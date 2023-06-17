using UnityEngine;

public static class VectorUtility
{
	public static Vector3 SmoothStep(Vector3 from, Vector3 to, float t)
	{
		return new Vector3(Mathf.SmoothStep(from.x, to.x, t), Mathf.SmoothStep(from.y, to.y, t), Mathf.SmoothStep(from.z, to.z, t));
	}
}
