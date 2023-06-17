using UnityEngine;

public class AttackTrail : MonoBehaviour
{
	public Transform target;

	public Transform pivot;

	public int distance;

	private void Update()
	{
		if ((bool)target && (bool)pivot)
		{
			Vector3 position = target.position;
			base.transform.position = target.position + (target.position - pivot.position).normalized * distance;
			base.transform.rotation = Quaternion.LookRotation(base.transform.position - position);
		}
	}

	public void DelayedDestroy(float time)
	{
		Invoke("DestroyNow", time);
	}

	private void DestroyNow()
	{
		Object.Destroy(base.gameObject);
	}
}
