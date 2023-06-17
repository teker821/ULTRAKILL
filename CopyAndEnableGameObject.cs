using UnityEngine;

public class CopyAndEnableGameObject : MonoBehaviour
{
	public GameObject target;

	public bool onEnable;

	private void Start()
	{
		if (onEnable)
		{
			Activate();
		}
	}

	public void Activate()
	{
		GameObject gameObject = Object.Instantiate(target, target.transform.position, target.transform.rotation);
		if ((bool)target.transform.parent)
		{
			gameObject.transform.SetParent(target.transform.parent, worldPositionStays: true);
		}
		gameObject.SetActive(value: true);
	}
}
