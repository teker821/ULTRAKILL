using UnityEngine;

public class CheatsControllerProxy : MonoBehaviour
{
	private CheatsController actualInstance;

	private void OnEnable()
	{
		actualInstance = MonoSingleton<CheatsController>.Instance;
	}

	private void Update()
	{
		actualInstance.Update();
	}
}
