using UnityEngine;

public class SoundStopper : MonoBehaviour
{
	public AudioSource toStop;

	private void Start()
	{
		toStop.Stop();
	}
}
