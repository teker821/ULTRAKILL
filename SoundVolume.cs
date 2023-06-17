using UnityEngine;

public class SoundVolume : MonoBehaviour
{
	private AudioSource aud;

	public float targetVolume;

	public float speed;

	public bool notOnEnable;

	private bool activated;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
		if (!notOnEnable)
		{
			activated = true;
		}
	}

	private void Update()
	{
		if ((bool)aud && activated)
		{
			aud.volume = Mathf.MoveTowards(aud.volume, targetVolume, speed * Time.deltaTime);
		}
	}

	public void Activate()
	{
		activated = true;
	}

	public void Deactivate()
	{
		activated = false;
	}

	public void ChangeVolume(float newVolume)
	{
		targetVolume = newVolume;
	}

	public void ChangeSpeed(float newSpeed)
	{
		speed = newSpeed;
	}
}
