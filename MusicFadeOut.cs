using UnityEngine;

public class MusicFadeOut : MonoBehaviour
{
	public bool forceOff;

	public bool oneTime = true;

	private bool colliderless = true;

	private void Start()
	{
		if (TryGetComponent<Collider>(out var _))
		{
			colliderless = false;
			return;
		}
		MonoSingleton<MusicManager>.Instance.off = true;
		if (forceOff)
		{
			MonoSingleton<MusicManager>.Instance.forcedOff = true;
		}
		if (oneTime)
		{
			Object.Destroy(this);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			MonoSingleton<MusicManager>.Instance.off = true;
			if (forceOff)
			{
				MonoSingleton<MusicManager>.Instance.forcedOff = true;
			}
			if (oneTime)
			{
				Object.Destroy(this);
			}
		}
	}
}
