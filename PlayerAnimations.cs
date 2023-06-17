using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
	public bool onGround;

	public AudioClip[] footsteps;

	private AudioSource aud;

	private Animator anim;

	public bool enemy = true;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
		if (!enemy)
		{
			anim = GetComponent<Animator>();
		}
	}

	private void Update()
	{
		if ((bool)anim)
		{
			anim.speed = Mathf.Min(MonoSingleton<NewMovement>.Instance.rb.velocity.magnitude, 15f) / 15f;
		}
	}

	public void Footstep()
	{
		if (aud == null)
		{
			aud = GetComponent<AudioSource>();
		}
		else
		{
			if (!onGround || !aud)
			{
				return;
			}
			if (!MonoSingleton<NewMovement>.Instance || !MonoSingleton<NewMovement>.Instance.groundProperties || !MonoSingleton<NewMovement>.Instance.groundProperties.overrideFootsteps)
			{
				if (footsteps.Length != 0)
				{
					aud.pitch = Random.Range(0.9f, 1.1f);
					aud.clip = footsteps[Random.Range(0, footsteps.Length)];
					aud.Play();
				}
			}
			else if (MonoSingleton<NewMovement>.Instance.groundProperties.newFootstepSound != null)
			{
				aud.clip = MonoSingleton<NewMovement>.Instance.groundProperties.newFootstepSound;
				aud.pitch = Random.Range(0.9f, 1.1f);
				aud.Play();
			}
		}
	}
}
