using UnityEngine;

public class DragBehind : MonoBehaviour
{
	private Vector3 previousPosition;

	private Quaternion currentRotation;

	private Quaternion nextRotation;

	private Quaternion previousRotation;

	public bool active;

	public bool notAnimated;

	public float dragAmount;

	private Quaternion defaultRotation;

	private void Awake()
	{
		previousPosition = base.transform.position;
		previousRotation = base.transform.rotation;
		defaultRotation = base.transform.localRotation;
	}

	private void LateUpdate()
	{
		if (active)
		{
			currentRotation = base.transform.rotation;
			Quaternion rotation = Quaternion.LookRotation(previousPosition - base.transform.position, base.transform.right);
			base.transform.rotation = rotation;
			base.transform.up = base.transform.forward;
			nextRotation = Quaternion.Lerp(currentRotation, base.transform.rotation, Vector3.Distance(base.transform.position, previousPosition) / 5f);
			if (notAnimated)
			{
				base.transform.rotation = Quaternion.Lerp(Quaternion.RotateTowards(previousRotation, nextRotation, Time.deltaTime * 1000f), base.transform.parent.rotation * defaultRotation, dragAmount);
			}
			else
			{
				base.transform.rotation = Quaternion.Lerp(Quaternion.RotateTowards(previousRotation, nextRotation, Time.deltaTime * 1000f), currentRotation, dragAmount);
			}
		}
		previousPosition = Vector3.MoveTowards(previousPosition, base.transform.position, Time.deltaTime * (Vector3.Distance(previousPosition, base.transform.position) * 10f));
		previousRotation = base.transform.rotation;
	}
}
