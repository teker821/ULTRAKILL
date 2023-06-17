using UnityEngine;

public class MusicDeactivator : MonoBehaviour
{
	public bool oneTime;

	private void OnEnable()
	{
		MonoSingleton<MusicManager>.Instance.StopMusic();
		if (oneTime)
		{
			Object.Destroy(this);
		}
	}
}
