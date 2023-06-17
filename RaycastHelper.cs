using UnityEngine;

public class RaycastHelper
{
	private const float Duration = 15f;

	public static bool RaycastAndDebugDraw(Vector3 origin, Vector3 direction, float maxDistance, int layerMask)
	{
		RaycastHit hitInfo;
		bool flag = Physics.Raycast(origin, direction, out hitInfo, maxDistance, layerMask);
		if (Application.isEditor)
		{
			if (flag)
			{
				Debug.DrawRay(origin, direction.normalized * hitInfo.distance, Color.green, 15f);
				Debug.DrawRay(origin + direction.normalized * hitInfo.distance, direction.normalized * (maxDistance - hitInfo.distance), Color.red, 15f);
			}
			else
			{
				Debug.DrawRay(origin, direction.normalized * maxDistance, Color.green, 15f);
			}
		}
		return flag;
	}
}
