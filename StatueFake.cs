using UnityEngine;

public class StatueFake : MonoBehaviour
{
	private Animator anim;

	private AudioSource aud;

	private ParticleSystem part;

	public GameObject[] toDeactivate;

	public GameObject enemyObject;

	public bool spawn;

	public GameObject[] toActivate;

	public bool quickSpawn;

	[HideInInspector]
	public bool beingActivated;

	private void Start()
	{
		anim = GetComponentInChildren<Animator>();
		aud = GetComponentInChildren<AudioSource>();
		part = GetComponentInChildren<ParticleSystem>();
		StatueIntroChecker instance = MonoSingleton<StatueIntroChecker>.Instance;
		if (instance != null)
		{
			if (instance.beenSeen)
			{
				quickSpawn = true;
			}
			else if (!quickSpawn)
			{
				instance.beenSeen = true;
			}
		}
		if (quickSpawn)
		{
			anim.speed = 1.5f;
		}
		if (beingActivated)
		{
			Activate();
		}
	}

	public void Activate()
	{
		beingActivated = true;
		if (anim == null)
		{
			anim = GetComponentInChildren<Animator>();
		}
		if (quickSpawn)
		{
			anim.Play("Awaken", -1, 0.33f);
		}
		else
		{
			Invoke("SlowStart", 3f);
		}
	}

	public void Crack()
	{
		aud.Play();
		part.Play();
	}

	public void Done()
	{
		GameObject[] array = toDeactivate;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		array = toActivate;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
		if (spawn)
		{
			Object.Instantiate(enemyObject, base.transform.position + base.transform.forward * 4f, base.transform.rotation);
		}
		Object.Destroy(this);
	}

	private void SlowStart()
	{
		anim.SetTrigger("Awaken");
	}
}
