using UnityEngine;

public class CameraShake : MonoBehaviour
{
	public float amount;

	private void Start()
	{
		MonoSingleton<CameraController>.Instance.CameraShake(amount);
		Object.Destroy(this);
	}
}
