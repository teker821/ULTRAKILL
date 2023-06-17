using UnityEngine;

public class SoundPitchDip : MonoBehaviour
{
	private AudioSource aud;

	private bool dipping;

	private float origPitch;

	private float target;

	public float speed;

	public bool onEnable;

	private void OnEnable()
	{
		if (onEnable)
		{
			Dip(0f);
		}
	}

	public void Dip(float pitch)
	{
		if (!aud)
		{
			aud = GetComponent<AudioSource>();
			if ((bool)aud)
			{
				origPitch = aud.pitch;
				aud.pitch = pitch;
				target = origPitch;
				dipping = true;
			}
		}
	}

	public void DipToZero()
	{
		if (!aud)
		{
			aud = GetComponent<AudioSource>();
			if ((bool)aud)
			{
				origPitch = aud.pitch;
				target = 0f;
				dipping = true;
			}
		}
	}

	private void Update()
	{
		if (dipping)
		{
			aud.pitch = Mathf.MoveTowards(aud.pitch, target, Time.deltaTime * speed);
			if (aud.pitch == target)
			{
				dipping = false;
			}
		}
	}
}
