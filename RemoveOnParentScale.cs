using UnityEngine;

public class RemoveOnParentScale : MonoBehaviour
{
	private void Update()
	{
		if (base.transform.parent.localScale == Vector3.zero)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
