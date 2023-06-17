using UnityEngine;

public class OnCheatsEnable : MonoBehaviour
{
	public bool includeMajorAssists;

	public UltrakillEvent onCheatsEnable;

	private void Update()
	{
		if (MonoSingleton<CheatsController>.Instance.cheatsEnabled || (includeMajorAssists && MonoSingleton<StatsManager>.Instance.majorUsed))
		{
			onCheatsEnable.Invoke();
			base.enabled = false;
		}
	}
}
