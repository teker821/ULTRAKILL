using UnityEngine;

public class BigDoor : MonoBehaviour
{
	public bool open;

	[HideInInspector]
	public bool gotPos;

	public Vector3 openRotation;

	[HideInInspector]
	public Quaternion targetRotation;

	[HideInInspector]
	public Quaternion origRotation;

	public float speed;

	private CameraController cc;

	public bool screenShake;

	private AudioSource aud;

	public AudioClip openSound;

	public AudioClip closeSound;

	private float origPitch;

	public Light openLight;

	public bool reverseDirection;

	private Door controller;

	public bool playerSpeedMultiplier;

	private void Awake()
	{
		if (!gotPos)
		{
			targetRotation.eulerAngles = base.transform.rotation.eulerAngles + openRotation;
			origRotation = base.transform.rotation;
			gotPos = true;
		}
		cc = MonoSingleton<CameraController>.Instance;
		aud = GetComponent<AudioSource>();
		origPitch = aud.pitch;
		controller = GetComponentInParent<Door>();
		if (open)
		{
			base.transform.rotation = targetRotation;
		}
	}

	private void Update()
	{
		if (open && base.transform.rotation != targetRotation)
		{
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, targetRotation, Time.deltaTime * (playerSpeedMultiplier ? Mathf.Max(speed, speed * (MonoSingleton<NewMovement>.Instance.rb.velocity.magnitude / 15f)) : speed));
			if (screenShake)
			{
				cc.CameraShake(0.05f);
			}
			if (base.transform.rotation == targetRotation)
			{
				aud.clip = closeSound;
				aud.loop = false;
				aud.pitch = Random.Range(origPitch - 0.1f, origPitch + 0.1f);
				aud.Play();
			}
		}
		else
		{
			if (open || !(base.transform.rotation != origRotation))
			{
				return;
			}
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, origRotation, Time.deltaTime * (playerSpeedMultiplier ? Mathf.Max(speed, speed * (MonoSingleton<NewMovement>.Instance.rb.velocity.magnitude / 15f)) : speed));
			if (screenShake)
			{
				cc.CameraShake(0.05f);
			}
			if (base.transform.rotation == origRotation)
			{
				aud.clip = closeSound;
				aud.loop = false;
				aud.pitch = Random.Range(origPitch - 0.1f, origPitch + 0.1f);
				aud.Play();
				if ((bool)controller && controller.doorType != 0)
				{
					controller.BigDoorClosed();
				}
				if (openLight != null)
				{
					openLight.enabled = false;
				}
			}
		}
	}

	public void Open()
	{
		if (!(base.transform.rotation != targetRotation))
		{
			return;
		}
		if (!aud)
		{
			aud = GetComponent<AudioSource>();
			origPitch = aud.pitch;
		}
		open = true;
		aud.clip = openSound;
		aud.loop = true;
		aud.pitch = Random.Range(origPitch - 0.1f, origPitch + 0.1f);
		aud.Play();
		if (Quaternion.Angle(base.transform.rotation, origRotation) < 20f)
		{
			if (reverseDirection)
			{
				targetRotation.eulerAngles = origRotation.eulerAngles - openRotation;
			}
			else
			{
				targetRotation.eulerAngles = origRotation.eulerAngles + openRotation;
			}
		}
	}

	public void Close()
	{
		if (base.transform.rotation != origRotation)
		{
			open = false;
			if ((bool)aud)
			{
				aud.clip = openSound;
				aud.loop = true;
				aud.pitch = Random.Range(origPitch / 2f - 0.1f, origPitch / 2f + 0.1f);
				aud.Play();
			}
		}
	}
}
