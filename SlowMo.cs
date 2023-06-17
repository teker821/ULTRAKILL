using UnityEngine;

public class SlowMo : MonoBehaviour
{
	public float amount;

	private void OnEnable()
	{
		MonoSingleton<TimeController>.Instance.SlowDown(amount);
		Object.Destroy(this);
	}
}
