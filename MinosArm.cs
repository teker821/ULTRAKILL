using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.Events;

public class MinosArm : MonoBehaviour
{
	private bool introOver;

	private float attackCooldown = 1.5f;

	private bool inAction;

	private Animator anim;

	private int previousSlam;

	private int maxSlams = 2;

	private int currentSlams;

	public Transform hand;

	public GameObject slamWave;

	public ObjectSpawner rubbleSpawner;

	private bool shaking;

	public GameObject shakeEffect;

	public GameObject impactSound;

	public GameObject hurtSound;

	public GameObject bigHurtSound;

	private Statue stat;

	private float originalHealth;

	private float speedState;

	public UnityEvent encounterStart;

	public UnityEvent encounterEnd;

	private int difficulty = -1;

	private float originalAnimSpeed = 1f;

	private EnemyIdentifier eid;

	private void Start()
	{
		stat = GetComponent<Statue>();
		originalHealth = stat.health;
		SetSpeed();
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	private void SetSpeed()
	{
		if (!anim)
		{
			anim = GetComponent<Animator>();
		}
		if (!eid)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (difficulty < 0)
		{
			if (eid.difficultyOverride >= 0)
			{
				difficulty = eid.difficultyOverride;
			}
			else
			{
				difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
			}
		}
		if (difficulty >= 3)
		{
			maxSlams = 3;
		}
		if (difficulty == 1)
		{
			originalAnimSpeed = 0.85f;
		}
		else if (difficulty == 0)
		{
			originalAnimSpeed = 0.65f;
		}
		else
		{
			originalAnimSpeed = 1f;
		}
		originalAnimSpeed *= eid.totalSpeedModifier;
		anim.speed = originalAnimSpeed * (1f + speedState / 4f);
	}

	private void Update()
	{
		if (introOver)
		{
			if (BlindEnemies.Blind)
			{
				return;
			}
			if (attackCooldown > 0f)
			{
				attackCooldown = Mathf.MoveTowards(attackCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
			}
			else if (!inAction)
			{
				bool flag = false;
				while (!flag)
				{
					int num = Random.Range(0, 3);
					if (num != previousSlam)
					{
						previousSlam = num;
						flag = true;
						switch (num)
						{
						case 0:
							SlamDown();
							break;
						case 1:
							SlamLeft();
							break;
						case 2:
							SlamRight();
							break;
						}
						currentSlams++;
						if (currentSlams >= maxSlams)
						{
							currentSlams = 0;
							attackCooldown = 5f - speedState;
						}
					}
				}
			}
		}
		if (shaking)
		{
			MonoSingleton<CameraController>.Instance.CameraShake(0.25f);
		}
		if (speedState == 1f && stat.health < originalHealth * 0.4f)
		{
			speedState = 2f;
			anim.speed = originalAnimSpeed * 1.5f;
			Flinch();
		}
		else if (speedState == 0f && stat.health < originalHealth * 0.75f)
		{
			speedState = 1f;
			anim.speed = originalAnimSpeed * 1.25f;
			Flinch();
		}
	}

	private void SlamLeft()
	{
		anim.SetTrigger("SlamLeft");
		inAction = true;
	}

	private void SlamRight()
	{
		anim.SetTrigger("SlamRight");
		inAction = true;
	}

	private void SlamDown()
	{
		anim.SetTrigger("SlamDown");
		inAction = true;
	}

	public void Slam(int type)
	{
		Vector3 direction = Vector3.down;
		switch (type)
		{
		case 1:
			direction = base.transform.right;
			break;
		case 2:
			direction = base.transform.right * -1f;
			break;
		}
		Vector3 position = hand.position;
		if (Physics.Raycast(hand.position, direction, out var hitInfo, 100f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			position = hitInfo.point;
		}
		GameObject gameObject = Object.Instantiate(slamWave, position, Quaternion.identity);
		GoreZone componentInParent = GetComponentInParent<GoreZone>();
		if ((bool)componentInParent)
		{
			gameObject.transform.SetParent(componentInParent.transform, worldPositionStays: true);
		}
		else
		{
			gameObject.transform.SetParent(base.transform, worldPositionStays: true);
		}
		float num = 3f;
		if (type > 0)
		{
			num = 7f;
		}
		gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y * num, gameObject.transform.localScale.z);
		PhysicalShockwave component = gameObject.GetComponent<PhysicalShockwave>();
		if ((bool)component)
		{
			if (difficulty == 1)
			{
				component.speed = 60f;
			}
			else if (difficulty == 0)
			{
				component.speed = 30f;
			}
			else
			{
				component.speed = 90f;
			}
			component.speed *= eid.totalSpeedModifier;
			component.damage = Mathf.RoundToInt(35f * eid.totalDamageModifier);
			component.maxSize = 350f;
		}
		Debug.Log("SlamType: " + type);
		if (type > 0)
		{
			gameObject.transform.Rotate(base.transform.forward * 90f);
		}
		BigImpact();
	}

	public void BigImpact(float shakeAmount = 2f)
	{
		MonoSingleton<CameraController>.Instance.CameraShake(2f);
		rubbleSpawner.SpawnObject(0);
		if (shakeAmount != 2f)
		{
			Object.Instantiate(impactSound, base.transform.position, base.transform.rotation);
		}
	}

	private void Flinch()
	{
		anim.SetTrigger("Flinch");
		inAction = true;
		currentSlams = 0;
		maxSlams++;
		attackCooldown = 0f;
		if (!introOver)
		{
			if (shaking)
			{
				StopShaking();
			}
			StartEncounter();
			IntroEnd();
		}
		Object.Instantiate(hurtSound, base.transform.position, Quaternion.identity);
	}

	public void Retreat()
	{
		anim.SetBool("Retreat", value: true);
		inAction = true;
		StartShaking();
		Object.Instantiate(bigHurtSound, base.transform.position, Quaternion.identity);
	}

	public void EndEncounter()
	{
		StopShaking();
		encounterEnd.Invoke();
		GetComponentInChildren<DoubleRender>()?.RemoveEffect();
	}

	public void IntroEnd()
	{
		introOver = true;
	}

	public void StopAction()
	{
		inAction = false;
	}

	public void StartShaking()
	{
		shaking = true;
		rubbleSpawner.SpawnObject(0);
		shakeEffect.SetActive(value: true);
	}

	public void StopShaking()
	{
		shaking = false;
		shakeEffect.SetActive(value: false);
		BigImpact(1f);
	}

	public void StartEncounter()
	{
		encounterStart.Invoke();
	}
}
