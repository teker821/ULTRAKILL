using UnityEngine;

public class DestroyOnDisable : MonoBehaviour
{
	[HideInInspector]
	public bool beenActivated;

	private void Start()
	{
		if (!beenActivated)
		{
			beenActivated = true;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDisable()
	{
		if (base.gameObject.activeSelf)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
