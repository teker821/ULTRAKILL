using UnityEngine;

public class InvertCorrection : MonoBehaviour
{
	public bool invert;

	public bool checkX = true;

	public bool checkY = true;

	public bool checkZ = true;

	private void Update()
	{
		Vector3 lossyScale = base.transform.lossyScale;
		Vector3 localScale = base.transform.localScale;
		if (invert)
		{
			if (checkX && lossyScale.x > 0f)
			{
				localScale.x *= -1f;
			}
			if (checkY && lossyScale.y > 0f)
			{
				localScale.y *= -1f;
			}
			if (checkZ && lossyScale.z > 0f)
			{
				localScale.z *= -1f;
			}
		}
		else
		{
			if (checkX && lossyScale.x < 0f)
			{
				localScale.x *= -1f;
			}
			if (checkY && lossyScale.y < 0f)
			{
				localScale.y *= -1f;
			}
			if (checkZ && lossyScale.z < 0f)
			{
				localScale.z *= -1f;
			}
		}
		base.transform.localScale = localScale;
	}
}
