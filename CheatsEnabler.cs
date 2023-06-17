using UnityEngine;

public class CheatsEnabler : MonoBehaviour
{
	private void Start()
	{
		MonoSingleton<CheatsController>.Instance.ActivateCheats();
	}
}
