using System;
using UnityEngine;

[Serializable]
public class SavedQuaternion
{
	public float x;

	public float y;

	public float z;

	public float w;

	public SavedQuaternion(Quaternion quaternion)
	{
		x = quaternion.x;
		y = quaternion.y;
		z = quaternion.z;
		w = quaternion.w;
	}

	public Quaternion ToQuaternion()
	{
		return new Quaternion(x, y, z, w);
	}
}
