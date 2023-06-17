using ULTRAKILL.Cheats;
using UnityEngine;

public class Mandalore : MonoBehaviour
{
	private AudioSource aud;

	private EnemyIdentifier eid;

	public AudioClip voiceFull;

	public AudioClip voiceFuller;

	private float cooldown = 2f;

	private float fullerChance;

	private int shotsLeft;

	private float maxHp;

	private int phase = 1;

	public GameObject fullAutoProjectile;

	public GameObject fullerAutoProjectile;

	public MandaloreVoice[] voices;

	private bool taunt = true;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
		eid = GetComponent<EnemyIdentifier>();
		maxHp = GetComponent<Drone>().health;
		voices = GetComponentsInChildren<MandaloreVoice>();
		if (taunt)
		{
			int num = Random.Range(0, voices[0].taunts.Length);
			MandaloreVoice[] array = voices;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Taunt(num);
			}
			switch (num)
			{
			case 0:
				MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("<color=#FFC49E>You cannot imagine what you'll face here</color>");
				MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("<color=#9EE6FF>I'm gonna shoot em with a gun</color>", 2.5f, base.gameObject);
				break;
			case 1:
				MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("<color=#9EE6FF>Why are we in the past</color>");
				break;
			case 2:
				MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("<color=#9EE6FF>I'm going to fucking poison you</color>");
				MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("<color=#FFC49E>What</color>", 2f, base.gameObject);
				break;
			case 3:
				MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("<color=#FFC49E>Hold still</color>", 0.6f, base.gameObject);
				break;
			}
		}
	}

	private void Update()
	{
		if (eid.dead)
		{
			if (!voices[0].dying)
			{
				MandaloreVoice[] array = voices;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Death();
				}
				GetComponent<Rigidbody>().mass = 5f;
				MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("<color=#9EE6FF>Oh great, now we lost the fight, fantastic</color>");
			}
			return;
		}
		if (cooldown > 0f)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		else if (!voices[0].talking && !voices[1].talking && !BlindEnemies.Blind)
		{
			if (Random.Range(0f, 1f) > fullerChance)
			{
				if (fullerChance < 0.5f)
				{
					fullerChance = 0.5f;
				}
				fullerChance += 0.2f;
				if (phase == 1)
				{
					cooldown = 4f;
				}
				else if (phase == 2)
				{
					cooldown = 3.25f;
				}
				else
				{
					cooldown = 2.5f;
				}
				aud.PlayOneShot(voiceFull);
				MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Full auto");
				Invoke("FullBurst", 1f);
				shotsLeft = 4;
			}
			else
			{
				if (fullerChance > 0.5f)
				{
					fullerChance = 0.5f;
				}
				fullerChance -= 0.2f;
				if (eid.health > maxHp / 3f * 2f)
				{
					cooldown = 4f;
				}
				else if (eid.health > maxHp / 3f)
				{
					cooldown = 3f;
				}
				else
				{
					cooldown = 2f;
				}
				aud.PlayOneShot(voiceFuller);
				MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Fuller auto");
				Invoke("FullerBurst", 1f);
				shotsLeft = 40;
			}
		}
		if (aud.isPlaying || voices[0].talking || voices[1].talking)
		{
			return;
		}
		if (phase < 4 && eid.health < maxHp / 4f)
		{
			phase = 4;
			MandaloreVoice[] array = voices;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].FinalPhase();
			}
			MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("<color=#9EE6FF>Use the salt!</color>");
			MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("<color=#FFC49E>I'm reaching!</color>", 1.5f, base.gameObject);
			Invoke("Sandify", 2.5f / eid.totalSpeedModifier);
		}
		else if (phase < 3 && eid.health < maxHp / 2f)
		{
			phase = 3;
			MandaloreVoice[] array = voices;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ThirdPhase();
			}
			Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.enrageEffect, base.transform);
			EnemySimplifier[] componentsInChildren = GetComponentsInChildren<EnemySimplifier>();
			if (componentsInChildren.Length != 0)
			{
				EnemySimplifier[] array2 = componentsInChildren;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].enraged = true;
				}
			}
			MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("<color=#FFC49E>Feel my maximum speed!</color>");
			MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("<color=#9EE6FF>Slow down</color>", 3.25f, base.gameObject);
		}
		else if (phase < 2 && eid.health < maxHp / 4f * 3f)
		{
			phase = 2;
			MandaloreVoice[] array = voices;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SecondPhase();
			}
			MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("<color=#FFC49E>Through the magic of the Druids, I increase my speed!</color>");
			MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("<color=#9EE6FF>Just fucking hit em</color>", 2.5f, base.gameObject);
		}
	}

	public void FullBurst()
	{
		if (!eid.dead)
		{
			GameObject obj = Object.Instantiate(fullAutoProjectile, base.transform.position, base.transform.rotation);
			Projectile componentInChildren = obj.GetComponentInChildren<Projectile>();
			componentInChildren.speed = 250f * eid.totalSpeedModifier;
			componentInChildren.safeEnemyType = EnemyType.Drone;
			componentInChildren.precheckForCollisions = true;
			componentInChildren.damage *= eid.totalDamageModifier;
			obj.GetComponent<ProjectileSpread>().projectileAmount = 6;
			shotsLeft--;
			if (shotsLeft > 0)
			{
				Invoke("FullBurst", 0.2f / eid.totalSpeedModifier);
			}
		}
	}

	public void FullerBurst()
	{
		if (!eid.dead)
		{
			Projectile componentInChildren = Object.Instantiate(fullerAutoProjectile, base.transform.position, Random.rotation).GetComponentInChildren<Projectile>();
			componentInChildren.speed = 2.5f * eid.totalSpeedModifier;
			componentInChildren.turnSpeed = 150f;
			componentInChildren.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
			componentInChildren.safeEnemyType = EnemyType.Drone;
			componentInChildren.damage *= eid.totalDamageModifier;
			shotsLeft--;
			if (shotsLeft > 0)
			{
				Invoke("FullerBurst", 0.02f / eid.totalSpeedModifier);
			}
		}
	}

	public void Sandify()
	{
		eid.Sandify();
	}

	public void DontTaunt()
	{
		taunt = false;
	}
}
