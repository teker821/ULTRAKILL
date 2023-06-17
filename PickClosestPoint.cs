using UnityEngine;

public class PickClosestPoint : MonoBehaviour
{
	public Transform target;

	public Transform[] points;

	public Transform customComparisonPoint;

	[SerializeField]
	private bool pickOnEnable = true;

	[SerializeField]
	private bool parentTargetToClosestPoint = true;

	[SerializeField]
	private bool mimicRotation = true;

	[SerializeField]
	private bool mimicPosition = true;

	[SerializeField]
	private bool mimicScale = true;

	[SerializeField]
	private bool closestToPlayer = true;

	private void OnEnable()
	{
		if (pickOnEnable)
		{
			Pick();
		}
	}

	private void Pick()
	{
		Transform transform = null;
		float num = float.MaxValue;
		Transform transform2 = (closestToPlayer ? MonoSingleton<PlayerTracker>.Instance.GetPlayer() : customComparisonPoint);
		Transform[] array = points;
		foreach (Transform transform3 in array)
		{
			float num2 = Vector3.Distance(transform3.position, transform2.position);
			if (num2 < num)
			{
				num = num2;
				transform = transform3;
			}
		}
		if (transform == null)
		{
			Debug.LogWarning("Unable to find closest point");
			return;
		}
		if (parentTargetToClosestPoint)
		{
			target.SetParent(transform);
			target.localRotation = Quaternion.identity;
			target.localScale = Vector3.one;
			target.localPosition = Vector3.zero;
			return;
		}
		if (mimicRotation)
		{
			target.rotation = transform.rotation;
		}
		if (mimicScale)
		{
			target.localScale = transform.localScale;
		}
		if (mimicPosition)
		{
			target.position = transform.position;
		}
	}
}
