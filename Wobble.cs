using UnityEngine;

public class Wobble : MonoBehaviour
{
	public Vector3[] rotations;

	private int targetRotation;

	public float speed;

	private void Update()
	{
		Quaternion b = Quaternion.Euler(rotations[rotations.Length - 1]);
		if (targetRotation > 0)
		{
			b = Quaternion.Euler(rotations[targetRotation - 1]);
		}
		base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.Euler(rotations[targetRotation]), (Mathf.Min(speed, Quaternion.Angle(base.transform.rotation, Quaternion.Euler(rotations[targetRotation])), Quaternion.Angle(base.transform.rotation, b)) + 0.1f) * Time.deltaTime);
		if (Quaternion.Angle(base.transform.rotation, Quaternion.Euler(rotations[targetRotation])) < 0.1f)
		{
			if (targetRotation + 1 < rotations.Length)
			{
				targetRotation++;
			}
			else
			{
				targetRotation = 0;
			}
		}
	}
}
