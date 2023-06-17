using UnityEngine;

public class ChangeLayer : MonoBehaviour
{
	public GameObject target;

	public int layer;

	public float delay;

	private void Start()
	{
		Invoke("Change", delay);
	}

	private void Change()
	{
		if (target != null)
		{
			target.layer = layer;
		}
		else
		{
			base.gameObject.layer = layer;
		}
		Object.Destroy(this);
	}
}
