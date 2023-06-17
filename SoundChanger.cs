using UnityEngine;

public class SoundChanger : MonoBehaviour
{
	public AudioSource target;

	public AudioClip newSound;

	public bool keepProgress;

	private void Start()
	{
		float time = 0f;
		if (keepProgress)
		{
			time = target.time;
		}
		target.clip = newSound;
		target.Play();
		if (keepProgress)
		{
			target.time = time;
		}
	}
}
