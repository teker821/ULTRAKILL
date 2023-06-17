using UnityEngine;

public class SpiderLeg : MonoBehaviour
{
	public Vector3 target;

	private void Start()
	{
		target = base.transform.position;
	}

	private void Update()
	{
		if (base.transform.position != target)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, target, Time.deltaTime * 10f);
		}
	}
}
