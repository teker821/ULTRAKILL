using UnityEngine;

public class CheckpointsDisabler : MonoBehaviour
{
	private bool activated;

	private void Start()
	{
		if (!activated)
		{
			activated = true;
			MonoSingleton<CheckPointsController>.Instance.DisableCheckpoints();
		}
	}

	private void OnEnable()
	{
		if (!activated)
		{
			activated = true;
			MonoSingleton<CheckPointsController>.Instance.DisableCheckpoints();
		}
	}

	private void OnDisable()
	{
		if (activated && base.gameObject.scene.isLoaded)
		{
			activated = false;
			MonoSingleton<CheckPointsController>.Instance.EnableCheckpoints();
		}
	}

	private void OnDestroy()
	{
		if (activated && base.gameObject.scene.isLoaded)
		{
			activated = false;
			MonoSingleton<CheckPointsController>.Instance.EnableCheckpoints();
		}
	}
}
