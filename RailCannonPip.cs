using UnityEngine;

public class RailCannonPip : MonoBehaviour
{
	private Vector3 origPos;

	private Vector3 targetPos;

	private Vector3 tempPos;

	public Vector3 pushAmount;

	public float chargeLevel;

	private Railcannon rc;

	private Quaternion origRot;

	private Quaternion tempRot;

	private AudioSource[] auds;

	private bool playIdle;

	private bool playClick;

	private void Start()
	{
		rc = GetComponentInParent<Railcannon>();
		auds = GetComponents<AudioSource>();
		origPos = base.transform.localPosition;
		tempPos = origPos;
		targetPos = origPos + pushAmount;
		origRot = base.transform.localRotation;
		tempRot = origRot;
		if (auds != null && (bool)rc.wid && rc.wid.delay != 0f)
		{
			AudioSource[] array = auds;
			foreach (AudioSource audioSource in array)
			{
				audioSource.volume -= rc.wid.delay * 2f;
				if (audioSource.volume < 0f)
				{
					audioSource.volume = 0f;
				}
			}
		}
		CheckSounds();
	}

	private void OnEnable()
	{
		CheckSounds();
	}

	private void LateUpdate()
	{
		if (MonoSingleton<WeaponCharges>.Instance.raicharge >= chargeLevel)
		{
			base.transform.localPosition = tempPos;
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, origPos, Vector3.Distance(base.transform.localPosition, origPos) * 50f * Time.deltaTime);
			tempPos = base.transform.localPosition;
			base.transform.localRotation = tempRot;
			base.transform.Rotate(Vector3.up, Time.deltaTime * -2400f, Space.Self);
			tempRot = base.transform.localRotation;
			if (!playClick && !playIdle)
			{
				return;
			}
			if (auds != null)
			{
				AudioSource[] array = auds;
				foreach (AudioSource audioSource in array)
				{
					if ((audioSource.loop && playIdle) || (!audioSource.loop && playClick))
					{
						audioSource.Play();
					}
				}
			}
			playClick = false;
			playIdle = false;
			return;
		}
		base.transform.localPosition = tempPos;
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, targetPos, Vector3.Distance(base.transform.localPosition, targetPos) * 50f * Time.deltaTime);
		tempPos = base.transform.localPosition;
		base.transform.localRotation = origRot;
		tempRot = origRot;
		if (playClick && playIdle)
		{
			return;
		}
		playClick = true;
		playIdle = true;
		if (auds != null)
		{
			AudioSource[] array = auds;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Stop();
			}
		}
	}

	private void CheckSounds()
	{
		if (rc == null)
		{
			rc = GetComponentInParent<Railcannon>();
		}
		if (auds == null)
		{
			auds = GetComponents<AudioSource>();
		}
		if (MonoSingleton<WeaponCharges>.Instance.raicharge > chargeLevel)
		{
			playClick = false;
		}
		else
		{
			playClick = true;
		}
		playIdle = true;
	}
}
