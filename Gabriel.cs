using ULTRAKILL.Cheats;
using UnityEngine;

public class Gabriel : MonoBehaviour
{
	private Transform target;

	private Animator anim;

	private Machine mach;

	private Rigidbody rb;

	private EnemyIdentifier eid;

	private SkinnedMeshRenderer smr;

	private GabrielVoice voice;

	private Collider col;

	public GameObject particles;

	public GameObject particlesEnraged;

	private Material origBody;

	private Material origWing;

	public Material enrageBody;

	public Material enrageWing;

	private int difficulty;

	private bool valuesSet;

	private bool active = true;

	private bool inAction;

	private bool goingLeft;

	private bool goForward;

	private float forwardSpeed;

	private float startCooldown = 2f;

	private float attackCooldown;

	public bool enraged;

	private GameObject currentEnrageEffect;

	public bool secondPhase;

	public float phaseChangeHealth;

	private float outOfSightTime;

	private int teleportAttempts;

	private int teleportInterval = 6;

	public GameObject teleportSound;

	public GameObject decoy;

	private bool overrideRotation;

	private bool stopRotation;

	private Vector3 overrideTarget;

	private LayerMask environmentMask;

	public Transform rightHand;

	public Transform leftHand;

	private GameObject rightHandWeapon;

	private GameObject leftHandWeapon;

	private WeaponTrail rightHandTrail;

	private WeaponTrail leftHandTrail;

	private SwingCheck2 rightSwingCheck;

	private SwingCheck2 leftSwingCheck;

	public GameObject sword;

	public GameObject zweiHander;

	public GameObject axe;

	public GameObject spear;

	public GameObject glaive;

	private bool spearing;

	private int spearAttacks;

	private bool dashing;

	private float forcedDashTime;

	private Vector3 dashTarget;

	public GameObject dashEffect;

	private int throws;

	private GameObject thrownObject;

	private bool threwAxes;

	private float[] moveChanceBonuses = new float[4];

	private int previousMove = -1;

	private int burstLength = 2;

	private bool juggled;

	private float juggleHp;

	private float juggleEndHp;

	private float juggleLength;

	public GameObject juggleEffect;

	private bool juggleFalling;

	public GameObject summonedSwords;

	private GameObject currentSwords;

	private float summonedSwordsCooldown = 15f;

	public Transform head;

	private bool readyTaunt;

	private float defaultAnimSpeed = 1f;

	private bool bossVersion;

	[SerializeField]
	private GameObject genericOutro;

	private void Start()
	{
		SetValues();
	}

	private void SetValues()
	{
		if (!valuesSet)
		{
			valuesSet = true;
			target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
			if (!anim)
			{
				anim = GetComponent<Animator>();
			}
			mach = GetComponent<Machine>();
			rb = GetComponent<Rigidbody>();
			eid = GetComponent<EnemyIdentifier>();
			smr = GetComponentInChildren<SkinnedMeshRenderer>();
			voice = GetComponent<GabrielVoice>();
			col = GetComponent<Collider>();
			origBody = smr.sharedMaterials[0];
			origWing = smr.sharedMaterials[1];
			origWing.SetFloat("_OpacScale", 1f);
			if (enraged)
			{
				EnrageNow();
			}
			environmentMask = LayerMaskDefaults.Get(LMD.Environment);
			if (eid.difficultyOverride >= 0)
			{
				difficulty = eid.difficultyOverride;
			}
			else
			{
				difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
			}
			if (difficulty >= 3)
			{
				burstLength = 3;
			}
			UpdateSpeed();
			RandomizeDirection();
			bossVersion = TryGetComponent<BossHealthBar>(out var _);
		}
	}

	private void UpdateBuff()
	{
		SetValues();
		UpdateSpeed();
	}

	private void UpdateSpeed()
	{
		if (!anim)
		{
			anim = GetComponent<Animator>();
		}
		if (difficulty <= 1)
		{
			if (difficulty == 1)
			{
				anim.speed = 0.85f;
			}
			else
			{
				anim.speed = 0.75f;
			}
		}
		else
		{
			anim.speed = 1f;
		}
		anim.speed *= eid.totalSpeedModifier;
		defaultAnimSpeed = anim.speed;
	}

	private void OnDisable()
	{
		CancelInvoke();
		if ((bool)rightHandWeapon || (bool)leftHandWeapon)
		{
			DisableWeapon();
		}
		DamageStopLeft(0);
		DamageStopRight(0);
		StopAction();
		ResetAnimSpeed();
		overrideRotation = false;
		spearing = false;
		spearAttacks = 0;
		dashing = false;
		if ((bool)currentSwords)
		{
			currentSwords.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		if (juggled)
		{
			JuggleStop();
		}
		if ((bool)currentSwords)
		{
			currentSwords.SetActive(value: true);
		}
	}

	private void Update()
	{
		if (BlindEnemies.Blind)
		{
			return;
		}
		if (active)
		{
			if (startCooldown > 0f)
			{
				startCooldown = Mathf.MoveTowards(startCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
			}
			if ((secondPhase || enraged) && difficulty >= 3 && !currentSwords)
			{
				summonedSwordsCooldown = Mathf.MoveTowards(summonedSwordsCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
				if (summonedSwordsCooldown == 0f && !inAction && attackCooldown > 1f && attackCooldown < 2f)
				{
					summonedSwordsCooldown = 15f;
					SpawnSummonedSwords();
				}
			}
			if (!inAction && startCooldown <= 0f)
			{
				if (attackCooldown > 0f)
				{
					attackCooldown = Mathf.MoveTowards(attackCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
					if (readyTaunt && (bool)voice)
					{
						voice.Taunt();
						readyTaunt = false;
					}
				}
				else if (Physics.Raycast(base.transform.position, target.position - base.transform.position, Vector3.Distance(base.transform.position, target.position), LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
				{
					Teleport();
				}
				else
				{
					bool flag = false;
					bool flag2 = false;
					if (Vector3.Distance(base.transform.position, target.position) > 10f)
					{
						flag2 = true;
					}
					else if (Vector3.Distance(base.transform.position, target.position) < 5f)
					{
						flag = true;
					}
					float[] array = new float[4];
					int num = -1;
					if (previousMove != 0 && !flag && !threwAxes)
					{
						array[0] = Random.Range(0f, 1f) + moveChanceBonuses[0];
					}
					if (previousMove != 1 && !flag)
					{
						array[1] = Random.Range(0f, 1f) + moveChanceBonuses[1];
					}
					if (previousMove != 2 && !flag2)
					{
						array[2] = Random.Range(0f, 1f) + moveChanceBonuses[2];
					}
					if (previousMove != 3)
					{
						array[3] = Random.Range(0f, 1f) + moveChanceBonuses[3];
					}
					float num2 = 0f;
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i] > num2)
						{
							num2 = array[i];
							num = i;
						}
					}
					switch (num)
					{
					case 0:
						AxeThrow();
						break;
					case 1:
						SpearCombo();
						break;
					case 2:
						StingerCombo();
						break;
					case 3:
						ZweiDash();
						break;
					}
					ResetAnimSpeed();
					previousMove = num;
					for (int j = 0; j < array.Length; j++)
					{
						if (j != num)
						{
							moveChanceBonuses[j] += 0.25f;
						}
						else
						{
							moveChanceBonuses[j] = 0f;
						}
					}
					if (num != 0)
					{
						if (burstLength > 1)
						{
							burstLength--;
						}
						else
						{
							if (difficulty >= 3)
							{
								burstLength = 3;
							}
							else
							{
								burstLength = 2;
							}
							if (difficulty <= 3)
							{
								attackCooldown = 3f;
							}
							else
							{
								attackCooldown = 2f;
							}
							threwAxes = false;
							readyTaunt = true;
						}
					}
				}
			}
			bool flag3 = false;
			if (Vector3.Distance(base.transform.position, target.position) > 20f || base.transform.position.y > target.position.y + 15f || Physics.Raycast(base.transform.position, target.position - base.transform.position, Vector3.Distance(base.transform.position, target.position), environmentMask))
			{
				flag3 = true;
			}
			if (flag3 && startCooldown <= 0f)
			{
				outOfSightTime = Mathf.MoveTowards(outOfSightTime, 3f, Time.deltaTime * eid.totalSpeedModifier);
				if (outOfSightTime >= 3f && !inAction)
				{
					Teleport();
				}
			}
			else
			{
				outOfSightTime = Mathf.MoveTowards(outOfSightTime, 0f, Time.deltaTime * 2f * eid.totalSpeedModifier);
			}
			if (!stopRotation)
			{
				if (!overrideRotation)
				{
					Quaternion quaternion = Quaternion.LookRotation(target.position - base.transform.position, Vector3.up);
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, Time.deltaTime * (10f * Quaternion.Angle(quaternion, base.transform.rotation) + 2f) * eid.totalSpeedModifier);
				}
				else
				{
					Quaternion quaternion2 = Quaternion.LookRotation(overrideTarget - base.transform.position, Vector3.up);
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion2, Time.deltaTime * (2500f * Quaternion.Angle(quaternion2, base.transform.rotation) + 10f) * eid.totalSpeedModifier);
				}
			}
		}
		if (!secondPhase && mach.health <= phaseChangeHealth)
		{
			if (!juggled)
			{
				JuggleStart();
			}
			secondPhase = true;
		}
		if (!juggled)
		{
			return;
		}
		if (mach.health < juggleHp)
		{
			if (rb.velocity.y < 0f)
			{
				rb.velocity = Vector3.zero;
			}
			rb.AddForce(Vector3.up * (juggleHp - mach.health) * 5f, ForceMode.VelocityChange);
			anim.Play("Juggle", 0, 0f);
			juggleHp = mach.health;
			base.transform.LookAt(new Vector3(target.position.x, base.transform.position.y, target.position.z));
			voice.Hurt();
			if (mach.health < juggleEndHp || juggleLength <= 0f)
			{
				JuggleStop(enrage: true);
			}
		}
		juggleLength = Mathf.MoveTowards(juggleLength, 0f, Time.deltaTime * eid.totalSpeedModifier);
	}

	private void FixedUpdate()
	{
		if (!juggled)
		{
			if (!inAction)
			{
				Vector3 zero = Vector3.zero;
				float num = Vector3.Distance(base.transform.position, target.position);
				if (num > 10f)
				{
					zero += base.transform.forward * 7.5f;
				}
				else if (num > 5f)
				{
					zero += base.transform.forward * 7.5f * (num / 10f);
				}
				RaycastHit hitInfo;
				if (BlindEnemies.Blind)
				{
					zero = Vector3.zero;
				}
				else if (goingLeft)
				{
					if (!Physics.SphereCast(base.transform.position, 1.25f, base.transform.right * -1f, out hitInfo, 3f, environmentMask))
					{
						zero += base.transform.right * -5f;
					}
					else if (!Physics.SphereCast(base.transform.position, 1.25f, base.transform.right, out hitInfo, 3f, environmentMask))
					{
						goingLeft = false;
					}
					else
					{
						zero += base.transform.forward * 5f;
					}
				}
				else if (!Physics.SphereCast(base.transform.position, 1.25f, base.transform.right, out hitInfo, 3f, environmentMask))
				{
					zero += base.transform.right * 5f;
				}
				else if (!Physics.SphereCast(base.transform.position, 1.25f, base.transform.right * -1f, out hitInfo, 3f, environmentMask))
				{
					goingLeft = true;
				}
				else
				{
					zero += base.transform.forward * 5f;
				}
				rb.velocity = zero * eid.totalSpeedModifier;
			}
			else if (goForward)
			{
				rb.velocity = base.transform.forward * forwardSpeed;
			}
			else
			{
				rb.velocity = Vector3.zero;
			}
		}
		else
		{
			if (rb.velocity.y < 35f)
			{
				rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
			}
			else
			{
				rb.velocity = new Vector3(0f, 35f, 0f);
			}
			if (juggleFalling && Physics.SphereCast(base.transform.position, 1.25f, Vector3.down, out var _, 3.6f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
			{
				JuggleStop();
			}
			if (rb.velocity.y < 0f)
			{
				juggleFalling = true;
			}
		}
		if (spearing)
		{
			if (!goForward)
			{
				base.transform.position = target.position + Vector3.up * 15f;
			}
			else if (Physics.Raycast(base.transform.position, base.transform.forward, 2f, environmentMask))
			{
				spearing = false;
				DamageStopRight(0);
			}
		}
		if (dashing)
		{
			col.enabled = false;
			if (forcedDashTime > 0f)
			{
				forcedDashTime = Mathf.MoveTowards(forcedDashTime, 0f, Time.deltaTime * eid.totalSpeedModifier);
			}
			if (Vector3.Distance(base.transform.position, dashTarget) > 5f)
			{
				if (!Physics.SphereCast(base.transform.position, 0.75f, dashTarget - base.transform.position, out var _, Vector3.Distance(base.transform.position, dashTarget) - 0.75f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), QueryTriggerInteraction.Ignore))
				{
					rb.velocity = base.transform.forward * 100f * eid.totalSpeedModifier;
					return;
				}
				col.enabled = true;
				dashTarget = target.transform.position;
				Teleport(closeRange: false, longrange: true);
				forcedDashTime = 0.35f;
				LookAtTarget();
			}
			else if (forcedDashTime <= 0f)
			{
				dashing = false;
				ZweiCombo();
			}
		}
		else
		{
			col.enabled = true;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!juggleFalling || other.gameObject.layer != 0)
		{
			return;
		}
		DeathZone deathZone = ((!other.attachedRigidbody) ? other.GetComponent<DeathZone>() : other.attachedRigidbody.GetComponent<DeathZone>());
		if ((bool)deathZone)
		{
			if ((bool)voice)
			{
				voice.BigHurt();
			}
			base.transform.position = deathZone.respawnTarget;
			eid.DeliverDamage(head.gameObject, Vector3.zero, head.position, 15f, tryForExplode: false);
			juggleFalling = false;
			JuggleStop();
			MonoSingleton<ChallengeManager>.Instance.ChallengeDone();
		}
	}

	public void Teleport(bool closeRange = false, bool longrange = false, bool firstTime = true, bool horizontal = false, bool vertical = false)
	{
		if (firstTime)
		{
			teleportAttempts = 0;
		}
		outOfSightTime = 0f;
		spearing = false;
		Vector3 normalized = Random.onUnitSphere.normalized;
		if (normalized.y < 0f)
		{
			normalized.y *= -1f;
		}
		float num = Random.Range(8, 15);
		if (closeRange)
		{
			num = Random.Range(5, 8);
		}
		else if (longrange)
		{
			num = Random.Range(15, 20);
		}
		Vector3 vector = target.transform.position + Vector3.up;
		vector = ((!Physics.Raycast(target.transform.position + Vector3.up, normalized, out var hitInfo, num, environmentMask, QueryTriggerInteraction.Ignore)) ? (target.transform.position + Vector3.up + normalized * num) : (hitInfo.point - normalized * 3f));
		bool flag = false;
		bool flag2 = false;
		if (Physics.Raycast(vector, Vector3.up, out var hitInfo2, 8f, environmentMask, QueryTriggerInteraction.Ignore))
		{
			flag = true;
		}
		if (Physics.Raycast(vector, Vector3.down, out var hitInfo3, 8f, environmentMask, QueryTriggerInteraction.Ignore))
		{
			flag2 = true;
		}
		bool flag3 = false;
		Vector3 vector2 = base.transform.position;
		if (flag && flag2)
		{
			if (Vector3.Distance(hitInfo2.point, hitInfo3.point) > 7f)
			{
				vector2 = ((!horizontal) ? new Vector3(vector.x, (hitInfo3.point.y + hitInfo2.point.y) / 2f, vector.z) : new Vector3(vector.x, hitInfo3.point.y + 3.5f, vector.z));
				flag3 = true;
			}
			else
			{
				teleportAttempts++;
				if (teleportAttempts <= 10)
				{
					Teleport(closeRange, longrange, firstTime: false, horizontal, vertical);
				}
			}
		}
		else
		{
			flag3 = true;
			vector2 = (flag ? (hitInfo2.point + Vector3.down * Random.Range(5, 10)) : (flag2 ? ((!horizontal) ? (hitInfo3.point + Vector3.up * Random.Range(5, 10)) : new Vector3(hitInfo3.point.x, hitInfo3.point.y + 3.5f, hitInfo3.point.z)) : ((!horizontal) ? vector : new Vector3(vector.x, target.position.y, vector.z))));
		}
		if (flag3)
		{
			Collider[] array = Physics.OverlapCapsule(vector2 + base.transform.up * -2.25f, vector2 + base.transform.up * 1.25f, 1.25f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), QueryTriggerInteraction.Ignore);
			if (array != null && array.Length != 0)
			{
				teleportAttempts++;
				if (teleportAttempts <= 10)
				{
					Teleport(closeRange, longrange, firstTime: false, horizontal, vertical);
				}
				return;
			}
			int num2 = Mathf.RoundToInt(Vector3.Distance(base.transform.position, vector2) / 2.5f);
			for (int i = 0; i < num2; i++)
			{
				CreateDecoy(Vector3.Lerp(base.transform.position, vector2, (float)i / (float)num2), (float)i / (float)num2 + 0.1f);
			}
			base.transform.position = vector2;
			teleportAttempts = 0;
			Object.Instantiate(teleportSound, base.transform.position, Quaternion.identity);
			if (eid.hooked)
			{
				MonoSingleton<HookArm>.Instance.StopThrow(1f, sparks: true);
			}
		}
		if (goingLeft)
		{
			goingLeft = false;
		}
		else
		{
			goingLeft = true;
		}
	}

	public GameObject CreateDecoy(Vector3 position, float transparencyOverride = 1f, Animator animatorOverride = null)
	{
		if (!anim && !animatorOverride)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate(decoy, position, base.transform.GetChild(0).rotation, base.transform.parent);
		Animator componentInChildren = gameObject.GetComponentInChildren<Animator>();
		AnimatorStateInfo animatorStateInfo = (animatorOverride ? animatorOverride.GetCurrentAnimatorStateInfo(0) : anim.GetCurrentAnimatorStateInfo(0));
		componentInChildren.Play(animatorStateInfo.shortNameHash, 0, animatorStateInfo.normalizedTime);
		componentInChildren.speed = 0f;
		MindflayerDecoy[] componentsInChildren = gameObject.GetComponentsInChildren<MindflayerDecoy>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].fadeOverride = transparencyOverride;
		}
		return gameObject;
	}

	private void StingerCombo()
	{
		forwardSpeed = 100f * anim.speed;
		SpawnLeftHandWeapon(GabrielWeaponType.Sword);
		inAction = true;
		anim.Play("StingerCombo");
	}

	private void SpearCombo()
	{
		if (difficulty >= 2)
		{
			forwardSpeed = 150f;
		}
		else if (difficulty == 1)
		{
			forwardSpeed = 75f;
		}
		else
		{
			forwardSpeed = 60f;
		}
		forwardSpeed *= eid.totalSpeedModifier;
		if (enraged && secondPhase)
		{
			spearAttacks = 3;
		}
		else if (enraged || secondPhase)
		{
			spearAttacks = 2;
		}
		else
		{
			spearAttacks = 1;
		}
		SpawnRightHandWeapon(GabrielWeaponType.Spear);
		inAction = true;
		anim.Play("SpearReady");
	}

	private void ZweiDash()
	{
		if (difficulty >= 2)
		{
			forwardSpeed = 100f;
		}
		else
		{
			forwardSpeed = 40f;
		}
		forwardSpeed *= eid.totalSpeedModifier;
		anim.Play("ZweiDash");
		inAction = true;
		SpawnRightHandWeapon(GabrielWeaponType.Zweihander);
	}

	private void StartDash()
	{
		inAction = true;
		overrideRotation = true;
		dashTarget = target.position;
		overrideTarget = dashTarget;
		dashing = true;
		Object.Instantiate(dashEffect, base.transform.position, base.transform.rotation);
	}

	private void ZweiCombo()
	{
		forwardSpeed = 65f * anim.speed;
		inAction = true;
		anim.Play("ZweiCombo");
		LookAtTarget();
		if (secondPhase || enraged)
		{
			throws = 1;
		}
	}

	private void AxeThrow()
	{
		threwAxes = true;
		inAction = true;
		SpawnRightHandWeapon(GabrielWeaponType.Axe);
		SpawnLeftHandWeapon(GabrielWeaponType.Axe);
		anim.Play("AxeThrow");
	}

	private void SpearAttack()
	{
		if (juggled)
		{
			return;
		}
		if (spearAttacks > 0)
		{
			spearing = true;
			spearAttacks--;
			float num = Random.Range(0f, 1f);
			if (difficulty <= 1)
			{
				Invoke("SpearAttack", 2f / eid.totalSpeedModifier);
			}
			else if (difficulty == 2)
			{
				Invoke("SpearAttack", 1.5f / eid.totalSpeedModifier);
			}
			else
			{
				Invoke("SpearAttack", 0.75f / eid.totalSpeedModifier);
			}
			bool flag = false;
			Vector3 vector = target.position;
			if (!Physics.Raycast(target.position, Vector3.up, out var hitInfo, 17f, environmentMask, QueryTriggerInteraction.Ignore))
			{
				vector = target.position + Vector3.up * 15f;
				flag = true;
			}
			else if (!Physics.Raycast(target.position, Vector3.down, out hitInfo, 17f, environmentMask, QueryTriggerInteraction.Ignore))
			{
				vector = base.transform.position + Vector3.down * 15f;
				flag = true;
			}
			if (flag && ((difficulty <= 3 && !enraged) || num > 0.5f))
			{
				anim?.Play("SpearDown");
				int num2 = Mathf.RoundToInt(Vector3.Distance(base.transform.position, vector) / 2.5f);
				for (int i = 0; i < num2; i++)
				{
					CreateDecoy(Vector3.Lerp(base.transform.position, vector, (float)i / (float)num2), (float)i / (float)num2 + 0.1f);
				}
				base.transform.position = vector;
				teleportAttempts = 0;
				Object.Instantiate(teleportSound, base.transform.position, Quaternion.identity);
				if (eid.hooked)
				{
					MonoSingleton<HookArm>.Instance.StopThrow(1f, sparks: true);
				}
				LookAtTarget();
				if (difficulty <= 1)
				{
					Invoke("SpearFlash", 0.5f / eid.totalSpeedModifier);
					Invoke("SpearGo", 1f / eid.totalSpeedModifier);
				}
				else if (difficulty == 2)
				{
					Invoke("SpearFlash", 0.375f / eid.totalSpeedModifier);
					Invoke("SpearGo", 0.75f / eid.totalSpeedModifier);
				}
				else
				{
					Invoke("SpearFlash", 0.25f / eid.totalSpeedModifier);
					Invoke("SpearGo", 0.5f / eid.totalSpeedModifier);
				}
			}
			else
			{
				anim.Play("SpearStinger");
				Teleport(closeRange: false, longrange: true, firstTime: true, horizontal: true);
				LookAtTarget();
				SpearGo();
			}
		}
		else
		{
			SpearThrow();
		}
	}

	private void SpearFlash()
	{
		if (!juggled)
		{
			Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.unparryableFlash, head);
		}
	}

	private void SpearGo()
	{
		if (!juggled)
		{
			Object.Instantiate(dashEffect, base.transform.position, base.transform.rotation);
			DamageStartRight(25);
		}
	}

	private void JuggleStart()
	{
		if ((bool)leftHandWeapon)
		{
			DamageStopLeft(0);
		}
		if ((bool)rightHandWeapon)
		{
			DamageStopRight(0);
		}
		MonoSingleton<TimeController>.Instance.SlowDown(0.25f);
		voice.BigHurt();
		inAction = true;
		DisableWeapon();
		CancelInvoke();
		dashing = false;
		spearing = false;
		rb.velocity = Vector3.zero;
		rb.AddForce(Vector3.up * 35f, ForceMode.VelocityChange);
		rb.useGravity = true;
		origWing.SetFloat("_OpacScale", 0f);
		base.transform.LookAt(new Vector3(target.position.x, base.transform.position.y, target.position.z));
		overrideRotation = false;
		stopRotation = true;
		juggled = true;
		juggleHp = mach.health;
		juggleEndHp = mach.health - 15f;
		juggleLength = 5f;
		juggleFalling = false;
		Object.Instantiate(juggleEffect, base.transform.position, base.transform.rotation);
		particles.SetActive(value: false);
		particlesEnraged.SetActive(value: false);
		ResetAnimSpeed();
		anim.Play("Juggle");
	}

	private void JuggleStop(bool enrage = false)
	{
		rb.useGravity = false;
		if (difficulty != 0)
		{
			burstLength = difficulty;
		}
		else
		{
			burstLength = 1;
		}
		voice.PhaseChange();
		origWing.SetFloat("_OpacScale", 1f);
		stopRotation = false;
		juggled = false;
		if (enraged)
		{
			particlesEnraged.SetActive(value: true);
		}
		else
		{
			particles.SetActive(value: true);
		}
		anim.Play("Idle");
		spearing = false;
		if ((enrage || mach.health <= phaseChangeHealth) && !currentEnrageEffect)
		{
			Enrage();
			return;
		}
		inAction = false;
		attackCooldown = 1f;
		Teleport();
	}

	private void Enrage()
	{
		anim.Play("Enrage");
	}

	public void EnrageNow()
	{
		Material[] materials = smr.materials;
		materials[0] = enrageBody;
		materials[1] = enrageWing;
		smr.materials = materials;
		eid.UpdateBuffs(visualsOnly: true);
		if (!currentEnrageEffect)
		{
			currentEnrageEffect = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.enrageEffect, base.transform);
		}
		if (difficulty >= 3)
		{
			SpawnSummonedSwords();
		}
		if (particles.activeSelf)
		{
			particlesEnraged.SetActive(value: true);
			particles.SetActive(value: false);
		}
		burstLength = difficulty;
		if (burstLength == 0)
		{
			burstLength = 1;
		}
		attackCooldown = 0f;
		readyTaunt = false;
	}

	public void UnEnrage()
	{
		Material[] materials = smr.materials;
		materials[0] = origBody;
		materials[1] = origWing;
		smr.materials = materials;
		enraged = false;
		if (particlesEnraged.activeSelf)
		{
			particlesEnraged.SetActive(value: false);
			particles.SetActive(value: true);
		}
		if ((bool)currentEnrageEffect)
		{
			Object.Destroy(currentEnrageEffect);
		}
	}

	private void SpearThrow()
	{
		if (!juggled)
		{
			spearing = false;
			DamageStopRight(0);
			Teleport();
			FollowTarget();
			anim.Play("SpearThrow");
		}
	}

	private void ThrowWeapon(GameObject projectile)
	{
		if (juggled)
		{
			return;
		}
		if (rightHandWeapon != null)
		{
			rightHandWeapon.SetActive(value: false);
			if ((bool)rightHandTrail)
			{
				rightHandTrail.RemoveTrail();
			}
			Object.Destroy(rightHandWeapon);
			if ((bool)rightSwingCheck)
			{
				Object.Destroy(rightSwingCheck.gameObject);
			}
		}
		if (leftHandWeapon != null)
		{
			leftHandWeapon.SetActive(value: false);
			if ((bool)leftHandTrail)
			{
				leftHandTrail.RemoveTrail();
			}
			Object.Destroy(leftHandWeapon);
			if ((bool)leftSwingCheck)
			{
				Object.Destroy(leftSwingCheck.gameObject);
			}
		}
		if (throws > 0)
		{
			throws--;
			Invoke("CheckForThrown", 0.35f / eid.totalSpeedModifier);
		}
		thrownObject = Object.Instantiate(projectile, base.transform.position, base.transform.rotation);
		if (difficulty > 1 && eid.totalSpeedModifier == 1f && eid.totalDamageModifier == 1f)
		{
			return;
		}
		Projectile componentInChildren = thrownObject.GetComponentInChildren<Projectile>();
		if ((bool)componentInChildren)
		{
			if (difficulty <= 1)
			{
				componentInChildren.speed *= 0.5f;
			}
			componentInChildren.speed *= eid.totalSpeedModifier;
			componentInChildren.damage *= eid.totalDamageModifier;
		}
	}

	private void CheckForThrown()
	{
		if (juggled)
		{
			return;
		}
		if (thrownObject != null)
		{
			Vector3 position = thrownObject.transform.position;
			Collider[] array = Physics.OverlapCapsule(position + base.transform.up * -2.25f, position + base.transform.up * 1.25f, 1.25f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), QueryTriggerInteraction.Ignore);
			if (array != null && array.Length != 0)
			{
				throws = 0;
				return;
			}
			int num = Mathf.RoundToInt(Vector3.Distance(base.transform.position, position) / 2.5f);
			for (int i = 0; i < num; i++)
			{
				CreateDecoy(Vector3.Lerp(base.transform.position, position, (float)i / (float)num), (float)i / (float)num + 0.1f);
			}
			base.transform.position = position;
			teleportAttempts = 0;
			Object.Instantiate(teleportSound, base.transform.position, Quaternion.identity);
			thrownObject.gameObject.SetActive(value: false);
			Object.Destroy(thrownObject);
			base.transform.LookAt(target.position);
			anim.speed = 0f;
			SpearFlash();
			Invoke("ResetAnimSpeed", 0.25f / eid.totalSpeedModifier);
			anim.Play("ZweiCombo", -1, 0.5f);
		}
		else
		{
			throws = 0;
		}
	}

	public void EnableWeapon()
	{
		if (!juggled)
		{
			if ((bool)rightHandWeapon)
			{
				rightHandWeapon.SetActive(value: true);
			}
			if ((bool)leftHandWeapon)
			{
				leftHandWeapon.SetActive(value: true);
			}
		}
	}

	public void DisableWeapon()
	{
		if (juggled)
		{
			return;
		}
		if ((bool)rightHandWeapon)
		{
			if ((bool)rightHandTrail)
			{
				rightHandTrail.RemoveTrail();
			}
			Object.Destroy(rightHandWeapon);
			if ((bool)rightSwingCheck)
			{
				Object.Destroy(rightSwingCheck.gameObject);
			}
		}
		if ((bool)leftHandWeapon)
		{
			if ((bool)leftHandTrail)
			{
				leftHandTrail.RemoveTrail();
			}
			Object.Destroy(leftHandWeapon);
			if ((bool)leftSwingCheck)
			{
				Object.Destroy(leftSwingCheck.gameObject);
			}
		}
	}

	private void RandomizeDirection()
	{
		if (Random.Range(0f, 1f) > 0.5f)
		{
			goingLeft = true;
		}
		else
		{
			goingLeft = false;
		}
	}

	private void SpawnLeftHandWeapon(GabrielWeaponType weapon)
	{
		if (!juggled)
		{
			GameObject weaponGameObject = GetWeaponGameObject(weapon);
			if (weaponGameObject != null)
			{
				leftHandWeapon = Object.Instantiate(weaponGameObject, leftHand.position, leftHand.rotation);
				leftHandWeapon.transform.forward = leftHand.transform.up;
				leftHandWeapon.transform.SetParent(leftHand, worldPositionStays: true);
				leftHandTrail = leftHandWeapon.GetComponentInChildren<WeaponTrail>();
				leftHandWeapon.SetActive(value: false);
				leftSwingCheck = WeaponHitBox(weapon);
			}
		}
	}

	private void SpawnRightHandWeapon(GabrielWeaponType weapon)
	{
		if (!juggled)
		{
			GameObject weaponGameObject = GetWeaponGameObject(weapon);
			if (weaponGameObject != null)
			{
				rightHandWeapon = Object.Instantiate(weaponGameObject, rightHand.position, rightHand.rotation);
				rightHandWeapon.transform.forward = rightHand.transform.up;
				rightHandWeapon.transform.SetParent(rightHand, worldPositionStays: true);
				rightHandTrail = rightHandWeapon.GetComponentInChildren<WeaponTrail>();
				rightHandWeapon.SetActive(value: false);
				rightSwingCheck = WeaponHitBox(weapon);
			}
		}
	}

	private GameObject GetWeaponGameObject(GabrielWeaponType weapon)
	{
		return weapon switch
		{
			GabrielWeaponType.Sword => sword, 
			GabrielWeaponType.Zweihander => zweiHander, 
			GabrielWeaponType.Axe => axe, 
			GabrielWeaponType.Spear => spear, 
			GabrielWeaponType.Glaive => glaive, 
			_ => null, 
		};
	}

	private SwingCheck2 WeaponHitBox(GabrielWeaponType weapon)
	{
		return weapon switch
		{
			GabrielWeaponType.Sword => CreateHitBox(new Vector3(0f, 0f, 1.5f), new Vector3(4f, 5f, 3f)), 
			GabrielWeaponType.Zweihander => CreateHitBox(new Vector3(0f, 0f, 2.5f), new Vector3(8f, 5f, 5f)), 
			GabrielWeaponType.Spear => CreateHitBox(new Vector3(0f, 0f, 2.5f), new Vector3(3.5f, 3.5f, 5f), ignoreSlide: true), 
			_ => null, 
		};
	}

	private SwingCheck2 CreateHitBox(Vector3 position, Vector3 size, bool ignoreSlide = false)
	{
		GameObject obj = new GameObject();
		obj.transform.position = base.transform.position;
		obj.transform.rotation = base.transform.rotation;
		obj.transform.SetParent(base.transform, worldPositionStays: true);
		BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
		boxCollider.enabled = false;
		boxCollider.isTrigger = true;
		boxCollider.center = position;
		boxCollider.size = size;
		SwingCheck2 swingCheck = obj.AddComponent<SwingCheck2>();
		swingCheck.type = EnemyType.Gabriel;
		swingCheck.ignoreSlidingPlayer = ignoreSlide;
		return swingCheck;
	}

	public void DamageStartLeft(int damage)
	{
		if (!juggled)
		{
			leftHandTrail.AddTrail();
			leftSwingCheck.damage = damage;
			leftSwingCheck.DamageStart();
			goForward = true;
		}
	}

	public void DamageStopLeft(int keepMoving)
	{
		if ((bool)leftHandTrail)
		{
			leftHandTrail.RemoveTrail();
		}
		if ((bool)leftSwingCheck)
		{
			leftSwingCheck.DamageStop();
		}
		if (keepMoving == 0)
		{
			goForward = false;
		}
	}

	public void DamageStartRight(int damage)
	{
		if (!juggled)
		{
			rightHandTrail.AddTrail();
			rightSwingCheck.damage = damage;
			rightSwingCheck.DamageStart();
			goForward = true;
		}
	}

	public void DamageStopRight(int keepMoving)
	{
		if ((bool)rightHandTrail)
		{
			rightHandTrail.RemoveTrail();
		}
		if ((bool)rightSwingCheck)
		{
			rightSwingCheck.DamageStop();
		}
		if (keepMoving == 0)
		{
			goForward = false;
		}
	}

	public void SetForwardSpeed(int newSpeed)
	{
		forwardSpeed = (float)newSpeed * defaultAnimSpeed;
	}

	public void EnrageTeleport(int teleportType = 0)
	{
		if (enraged || secondPhase)
		{
			switch (teleportType)
			{
			case 1:
				Teleport(closeRange: true);
				break;
			case 2:
				Teleport();
				break;
			case 3:
				Teleport(closeRange: true, longrange: false, firstTime: true, horizontal: true);
				break;
			case 4:
				Teleport(closeRange: false, longrange: false, firstTime: true, horizontal: true);
				break;
			case 5:
				Teleport(closeRange: false, longrange: false, firstTime: true, horizontal: false, vertical: true);
				break;
			}
			anim.speed = 0f;
			Invoke("ResetAnimSpeed", 0.25f / eid.totalSpeedModifier);
		}
		base.transform.LookAt(target);
	}

	private void ResetAnimSpeed()
	{
		if ((bool)anim)
		{
			anim.speed = defaultAnimSpeed;
		}
	}

	public void LookAtTarget(int instant = 0)
	{
		overrideRotation = true;
		overrideTarget = base.transform.position + (target.transform.position - base.transform.position).normalized * 999f;
		base.transform.LookAt(base.transform.position + (target.transform.position - base.transform.position).normalized * 999f);
	}

	public void FollowTarget()
	{
		if (!juggled)
		{
			overrideRotation = false;
		}
	}

	public void StopAction()
	{
		if (!juggled)
		{
			FollowTarget();
			inAction = false;
		}
	}

	public void ResetWingMat()
	{
		origWing.SetFloat("_OpacScale", 1f);
	}

	public void Death()
	{
		if ((bool)currentSwords)
		{
			Object.Destroy(currentSwords);
		}
		if ((bool)currentEnrageEffect)
		{
			Object.Destroy(currentEnrageEffect);
		}
		if (!bossVersion)
		{
			Object.Instantiate(genericOutro, base.transform.position, Quaternion.LookRotation(new Vector3(base.transform.forward.x, 0f, base.transform.forward.z)));
			Object.Destroy(base.gameObject);
		}
	}

	private void SpawnSummonedSwords()
	{
		currentSwords = Object.Instantiate(summonedSwords, base.transform.position, Quaternion.identity);
		if (currentSwords.TryGetComponent<SummonedSwords>(out var component))
		{
			component.target = base.transform;
			component.speed *= eid.totalSpeedModifier;
		}
		if (eid.totalDamageModifier != 1f)
		{
			Projectile[] componentsInChildren = currentSwords.GetComponentsInChildren<Projectile>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].damage *= eid.totalDamageModifier;
			}
		}
	}
}
