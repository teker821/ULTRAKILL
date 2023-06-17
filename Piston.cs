using UnityEngine;

public class Piston : MonoBehaviour
{
	public bool off;

	private RaycastHit rhit;

	public LayerMask enviroMask;

	public Vector3 minPos;

	public Vector3 maxPos;

	public Vector3 targetPos;

	private Collider dzone;

	private Collider basedzone;

	public float timer;

	public float returnTime;

	public float attackTime;

	private ParticleSystem part;

	private ParticleSystem[] steamParts;

	private AudioSource partAud;

	private AudioSource aud;

	private void Awake()
	{
		if (minPos == Vector3.zero)
		{
			maxPos = base.transform.localPosition;
			minPos = new Vector3(0f, -7f, 0f);
		}
		base.transform.localPosition = minPos;
		dzone = GetComponentInChildren<DeathZone>().GetComponent<Collider>();
		dzone.enabled = false;
		basedzone = base.transform.parent.Find("Base").GetComponentInChildren<DeathZone>().GetComponent<Collider>();
		basedzone.enabled = false;
		targetPos = Vector3.one;
		part = GetComponentInChildren<ParticleSystem>();
		partAud = part.GetComponent<AudioSource>();
		aud = GetComponent<AudioSource>();
		steamParts = base.transform.parent.GetChild(0).GetComponentsInChildren<ParticleSystem>();
	}

	private void Update()
	{
		if (off)
		{
			return;
		}
		timer -= Time.deltaTime * 2f;
		if (timer <= 0f)
		{
			if (base.transform.localPosition == minPos)
			{
				targetPos = maxPos;
				timer = returnTime;
				dzone.enabled = true;
				basedzone.enabled = false;
			}
			else
			{
				targetPos = minPos;
				timer = attackTime;
			}
		}
		if (timer <= 1f && !aud.isPlaying && base.transform.localPosition == minPos)
		{
			aud.Play();
			ParticleSystem[] array = steamParts;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play();
			}
		}
		if (base.transform.localPosition != targetPos && targetPos != Vector3.one)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, targetPos, Time.deltaTime * 75f);
		}
		if (base.transform.localPosition == maxPos && dzone.enabled)
		{
			dzone.enabled = false;
			part.Play();
			partAud.Play();
			if (aud.isPlaying)
			{
				aud.Stop();
				ParticleSystem[] array = steamParts;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Stop();
				}
			}
		}
		if (base.transform.localPosition == minPos)
		{
			basedzone.enabled = true;
		}
	}
}
