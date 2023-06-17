using ULTRAKILL.Cheats;
using UnityEngine;

public class GabrielSecond : MonoBehaviour
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

	private float forwardSpeedMinimum;

	private float forwardSpeedMaximum;

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

	[Header("Swords")]
	public Transform rightHand;

	public Transform leftHand;

	private TrailRenderer rightHandTrail;

	private TrailRenderer leftHandTrail;

	[SerializeField]
	private SwingCheck2 generalSwingCheck;

	private SwingCheck2 rightSwingCheck;

	private SwingCheck2 leftSwingCheck;

	private MeshRenderer rightHandGlow;

	private MeshRenderer leftHandGlow;

	[SerializeField]
	private AudioSource swingSound;

	[SerializeField]
	private AudioSource kickSwingSound;

	[SerializeField]
	private Renderer[] swordRenderers;

	[SerializeField]
	private GameObject fakeCombinedSwords;

	[SerializeField]
	private Projectile combinedSwordsThrown;

	private Projectile currentCombinedSwordsThrown;

	[HideInInspector]
	public bool swordsCombined;

	private float combinedSwordsCooldown;

	[HideInInspector]
	public bool lightSwords;

	[Space(20f)]
	public TrailRenderer kickTrail;

	public GameObject dashEffect;

	private bool dashing;

	private float forcedDashTime;

	private Vector3 dashTarget;

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

	public bool ceilingHitChallenge;

	[SerializeField]
	private GameObject ceilingHitEffect;

	private float ceilingHitCooldown;

	[Header("Events")]
	public UltrakillEvent onFirstPhaseEnd;

	public UltrakillEvent onSecondPhaseStart;

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
			anim = GetComponent<Animator>();
			mach = GetComponent<Machine>();
			rb = GetComponent<Rigidbody>();
			eid = GetComponent<EnemyIdentifier>();
			smr = GetComponentInChildren<SkinnedMeshRenderer>();
			voice = GetComponent<GabrielVoice>();
			col = GetComponent<Collider>();
			origBody = smr.sharedMaterials[0];
			origWing = smr.sharedMaterials[1];
			origWing.SetFloat("_OpacScale", 1f);
			rightHandTrail = rightHand.GetComponentInChildren<TrailRenderer>();
			rightSwingCheck = rightHand.GetComponentInChildren<SwingCheck2>();
			rightHandGlow = rightHand.GetComponentInChildren<MeshRenderer>(includeInactive: true);
			leftHandTrail = leftHand.GetComponentInChildren<TrailRenderer>();
			leftSwingCheck = leftHand.GetComponentInChildren<SwingCheck2>();
			leftHandGlow = leftHand.GetComponentInChildren<MeshRenderer>(includeInactive: true);
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
			if (enraged)
			{
				EnrageNow();
			}
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
		if ((bool)leftSwingCheck)
		{
			DamageStopLeft(0);
		}
		if ((bool)rightSwingCheck)
		{
			DamageStopRight(0);
		}
		StopAction();
		ResetAnimSpeed();
		overrideRotation = false;
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
			if (secondPhase && difficulty >= 3 && !currentSwords)
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
				else if ((secondPhase || !currentCombinedSwordsThrown) && Physics.Raycast(base.transform.position, target.position - base.transform.position, Vector3.Distance(base.transform.position, target.position), LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
				{
					Teleport();
				}
				else if ((bool)currentCombinedSwordsThrown && !secondPhase && (combinedSwordsCooldown > 0f || Vector3.Distance(base.transform.position, combinedSwordsThrown.transform.position) < Vector3.Distance(base.transform.position, target.transform.position)))
				{
					combinedSwordsCooldown = Mathf.MoveTowards(combinedSwordsCooldown, 0f, Time.deltaTime);
				}
				else
				{
					bool flag = false;
					bool flag2 = false;
					if (Vector3.Distance(base.transform.position, target.position) > 20f)
					{
						flag2 = true;
					}
					else if (Vector3.Distance(base.transform.position, target.position) < 5f)
					{
						flag = true;
					}
					float[] array = new float[4];
					int num = -1;
					if (previousMove != 0 && !flag)
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
					if (previousMove != 3 && !flag2)
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
						CombineSwords();
						break;
					case 1:
						FastComboDash();
						break;
					case 2:
						BasicCombo();
						break;
					case 3:
						ThrowCombo();
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
			voice.secondPhase = true;
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
			rb.AddForce(Vector3.up * (juggleHp - mach.health) * 10f, ForceMode.VelocityChange);
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
				if (!MonoSingleton<NewMovement>.Instance.playerCollider.Raycast(new Ray(base.transform.position, base.transform.forward), out var hitInfo2, forwardSpeed * Time.fixedDeltaTime))
				{
					rb.velocity = base.transform.forward * forwardSpeed;
				}
				else
				{
					if (hitInfo2.distance > 1f)
					{
						base.transform.position += base.transform.forward * (hitInfo2.distance - 1f);
					}
					rb.velocity = Vector3.zero;
				}
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
			RaycastHit hitInfo3;
			if (ceilingHitCooldown > 0f)
			{
				ceilingHitCooldown = Mathf.MoveTowards(ceilingHitCooldown, 0f, Time.fixedDeltaTime);
			}
			else if (rb.velocity.y > 1f && Physics.Raycast(base.transform.position, Vector3.up, out hitInfo3, 3f + rb.velocity.y * Time.fixedDeltaTime, LayerMaskDefaults.Get(LMD.Environment)))
			{
				Debug.Log("Bonk");
				ceilingHitCooldown = 0.5f;
				base.transform.position = hitInfo3.point - Vector3.up * 3f;
				mach.GetHurt(base.gameObject, Vector3.zero, Mathf.Min(rb.velocity.y, 5f), 0f);
				rb.velocity = new Vector3(0f, 0f - rb.velocity.y, 0f);
				anim.Play("Juggle", 0, 0f);
				juggleHp = mach.health;
				voice.Hurt();
				Object.Instantiate(ceilingHitEffect, hitInfo3.point - Vector3.up, Quaternion.LookRotation(Vector3.down));
				MonoSingleton<CameraController>.Instance.CameraShake(0.5f);
				if (ceilingHitChallenge)
				{
					MonoSingleton<ChallengeManager>.Instance.ChallengeDone();
				}
			}
			if (juggleFalling && Physics.SphereCast(base.transform.position, 1.25f, Vector3.down, out hitInfo3, 3.6f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
			{
				JuggleStop();
			}
			if (rb.velocity.y < 0f)
			{
				juggleFalling = true;
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
				FastCombo();
			}
		}
		else
		{
			col.enabled = true;
		}
	}

	private void BasicCombo()
	{
		if (!juggled)
		{
			CheckIfSwordsCombined();
			forwardSpeedMinimum = 125f;
			forwardSpeedMaximum = 175f;
			inAction = true;
			anim.Play("BasicCombo");
		}
	}

	private void FastComboDash()
	{
		if (!juggled)
		{
			CheckIfSwordsCombined();
			if (difficulty >= 2)
			{
				forwardSpeed = 100f;
			}
			else
			{
				forwardSpeed = 40f;
			}
			forwardSpeed *= eid.totalSpeedModifier;
			anim.Play("FastComboDash");
			inAction = true;
		}
	}

	private void FastCombo()
	{
		if (!juggled)
		{
			forwardSpeedMinimum = 75f;
			forwardSpeedMaximum = 125f;
			inAction = true;
			anim.Play("FastCombo");
			LookAtTarget();
		}
	}

	private void ThrowCombo()
	{
		if (!juggled)
		{
			CheckIfSwordsCombined();
			forwardSpeedMinimum = 125f;
			forwardSpeedMaximum = 175f;
			inAction = true;
			anim.Play("ThrowCombo");
			LookAtTarget();
		}
	}

	private void CombineSwords()
	{
		if (!juggled)
		{
			if (swordsCombined)
			{
				UnGattai();
			}
			inAction = true;
			anim.Play("SwordsCombine");
		}
	}

	private void Gattai()
	{
		if (swordsCombined)
		{
			UnGattai();
		}
		swordsCombined = true;
		Renderer[] array = swordRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		fakeCombinedSwords.SetActive(value: true);
	}

	private void CombinedSwordAttack()
	{
		if (!juggled)
		{
			anim.Play("SwordsCombinedThrow");
		}
	}

	public void UnGattai(bool destroySwords = true)
	{
		swordsCombined = false;
		Renderer[] array = swordRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
		fakeCombinedSwords.SetActive(value: false);
		if (destroySwords && (bool)currentCombinedSwordsThrown)
		{
			Object.Destroy(currentCombinedSwordsThrown.gameObject);
		}
		if (lightSwords)
		{
			lightSwords = false;
			if (!leftSwingCheck.damaging)
			{
				leftHandGlow.enabled = false;
			}
			if (!rightSwingCheck.damaging)
			{
				rightHandGlow.enabled = false;
			}
		}
	}

	private void CheckIfSwordsCombined()
	{
		if (swordsCombined)
		{
			if (secondPhase || currentCombinedSwordsThrown.friendly)
			{
				CreateLightSwords();
			}
			else
			{
				UnGattai();
			}
		}
	}

	private void CreateLightSwords()
	{
		lightSwords = true;
		leftHandGlow.enabled = true;
		rightHandGlow.enabled = true;
	}

	private void ThrowSwords()
	{
		if (!juggled)
		{
			Object.Instantiate(kickSwingSound, base.transform);
			fakeCombinedSwords.SetActive(value: false);
			currentCombinedSwordsThrown = Object.Instantiate(combinedSwordsThrown, fakeCombinedSwords.transform.position, base.transform.rotation, base.transform.parent);
			currentCombinedSwordsThrown.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
			if (difficulty <= 2)
			{
				combinedSwordsCooldown = 2f;
			}
			else if (difficulty >= 3)
			{
				combinedSwordsCooldown = 1f;
			}
			currentCombinedSwordsThrown.speed *= eid.totalSpeedModifier;
			currentCombinedSwordsThrown.damage *= eid.totalDamageModifier;
			if (currentCombinedSwordsThrown.TryGetComponent<GabrielCombinedSwordsThrown>(out var component))
			{
				component.gabe = this;
			}
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
		}
	}

	public void Teleport(bool closeRange = false, bool longrange = false, bool firstTime = true, bool horizontal = false, bool vertical = false)
	{
		if (firstTime)
		{
			teleportAttempts = 0;
		}
		outOfSightTime = 0f;
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
		GameObject gameObject = Object.Instantiate(decoy, position, base.transform.GetChild(0).rotation);
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

	private void StartDash()
	{
		inAction = true;
		overrideRotation = true;
		dashTarget = target.position;
		overrideTarget = dashTarget;
		dashing = true;
		Object.Instantiate(dashEffect, base.transform.position, base.transform.rotation);
	}

	private void Parryable()
	{
		if (!juggled)
		{
			mach.parryable = true;
			AttackFlash();
		}
	}

	private void AttackFlash(int unparryable = 0)
	{
		if (!juggled)
		{
			Object.Instantiate((unparryable == 0) ? MonoSingleton<DefaultReferenceManager>.Instance.parryableFlash : MonoSingleton<DefaultReferenceManager>.Instance.unparryableFlash, head).transform.localScale *= 3f;
		}
	}

	private void JuggleStart()
	{
		DamageStopLeft(0);
		DamageStopRight(0);
		MonoSingleton<TimeController>.Instance.SlowDown(0.25f);
		voice.BigHurt();
		inAction = true;
		CancelInvoke();
		dashing = false;
		rb.velocity = Vector3.zero;
		rb.AddForce(Vector3.up * 35f, ForceMode.VelocityChange);
		rb.useGravity = true;
		origWing.SetFloat("_OpacScale", 0f);
		base.transform.LookAt(new Vector3(target.position.x, base.transform.position.y, target.position.z));
		overrideRotation = false;
		stopRotation = true;
		juggled = true;
		juggleHp = mach.health;
		juggleEndHp = mach.health - 7.5f;
		juggleLength = 5f;
		juggleFalling = false;
		Object.Instantiate(juggleEffect, base.transform.position, base.transform.rotation);
		eid.totalDamageTakenMultiplier = 0.5f;
		if ((bool)currentEnrageEffect)
		{
			MeshRenderer componentInChildren = currentEnrageEffect.GetComponentInChildren<MeshRenderer>();
			if ((bool)componentInChildren)
			{
				componentInChildren.material.color = new Color(0.5f, 0f, 0f, 0.5f);
			}
			Light componentInChildren2 = currentEnrageEffect.GetComponentInChildren<Light>();
			if ((bool)componentInChildren2)
			{
				componentInChildren2.enabled = false;
			}
		}
		if (swordsCombined)
		{
			UnGattai();
		}
		particles.SetActive(value: false);
		particlesEnraged.SetActive(value: false);
		ResetAnimSpeed();
		anim.Play("Juggle", 0, 0f);
		onFirstPhaseEnd?.Invoke();
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
		if ((enrage || mach.health <= phaseChangeHealth) && (bool)currentEnrageEffect)
		{
			EnrageAnimation();
			return;
		}
		inAction = false;
		attackCooldown = 1f;
		Teleport();
	}

	private void EnrageAnimation()
	{
		anim.Play("Enrage", 0, 0f);
		Invoke("ForceUnEnrage", 3f * anim.speed);
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
		FadeOut fadeOut = currentEnrageEffect.AddComponent<FadeOut>();
		fadeOut.activateOnEnable = true;
		fadeOut.speed = 0.1f;
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

	private void ForceUnEnrage()
	{
		UnEnrage();
		anim.Play("Idle");
		StopAction();
	}

	public void UnEnrage()
	{
		CancelInvoke("ForceUnEnrage");
		Material[] materials = smr.materials;
		materials[0] = origBody;
		materials[1] = origWing;
		smr.materials = materials;
		eid.totalDamageTakenMultiplier = 1f;
		enraged = false;
		if (particlesEnraged.activeSelf)
		{
			particlesEnraged.SetActive(value: false);
			particles.SetActive(value: true);
		}
		if (difficulty >= 3)
		{
			SpawnSummonedSwords();
		}
		if ((bool)currentEnrageEffect)
		{
			Object.Destroy(currentEnrageEffect);
		}
		onSecondPhaseStart?.Invoke();
		attackCooldown = 0f;
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

	public void DamageStartLeft(int damage)
	{
		if (!juggled)
		{
			leftHandTrail.emitting = true;
			leftHandGlow.gameObject.SetActive(value: true);
			SetDamage(damage);
			leftSwingCheck.DamageStart();
			generalSwingCheck.DamageStart();
			Object.Instantiate(swingSound, base.transform);
			DecideMovementSpeed(forwardSpeedMinimum, forwardSpeedMaximum);
			goForward = true;
		}
	}

	public void DamageStopLeft(int keepMoving)
	{
		leftHandTrail.emitting = false;
		leftSwingCheck.DamageStop();
		if (!lightSwords)
		{
			leftHandGlow.gameObject.SetActive(value: false);
		}
		if (keepMoving == 0)
		{
			goForward = false;
		}
		if (!rightSwingCheck || !rightSwingCheck.damaging)
		{
			mach.parryable = false;
			generalSwingCheck.DamageStop();
		}
	}

	public void DamageStartRight(int damage)
	{
		if (!juggled)
		{
			rightHandTrail.emitting = true;
			rightHandGlow.gameObject.SetActive(value: true);
			SetDamage(damage);
			rightSwingCheck.DamageStart();
			generalSwingCheck.DamageStart();
			Object.Instantiate(swingSound, base.transform);
			DecideMovementSpeed(forwardSpeedMinimum, forwardSpeedMaximum);
			goForward = true;
		}
	}

	public void DamageStopRight(int keepMoving)
	{
		rightHandTrail.emitting = false;
		rightSwingCheck.DamageStop();
		if (!lightSwords)
		{
			rightHandGlow.gameObject.SetActive(value: false);
		}
		if (keepMoving == 0)
		{
			goForward = false;
		}
		if (!leftSwingCheck || !leftSwingCheck.damaging)
		{
			mach.parryable = false;
			generalSwingCheck.DamageStop();
		}
	}

	public void DamageStartKick(int damage)
	{
		if (!juggled)
		{
			kickTrail.emitting = true;
			SetDamage(damage);
			generalSwingCheck.DamageStart();
			Object.Instantiate(kickSwingSound, base.transform);
			DecideMovementSpeed(forwardSpeedMinimum, forwardSpeedMaximum);
			goForward = true;
		}
	}

	public void DamageStopKick(int keepMoving)
	{
		if ((bool)kickTrail)
		{
			kickTrail.emitting = false;
		}
		if (keepMoving == 0)
		{
			goForward = false;
		}
		if ((!leftSwingCheck || !leftSwingCheck.damaging) && (!rightSwingCheck || !rightSwingCheck.damaging))
		{
			mach.parryable = false;
			generalSwingCheck.DamageStop();
		}
	}

	public void DamageStartBoth(int damage)
	{
		DamageStartLeft(damage);
		DamageStartRight(damage);
	}

	public void DamageStopBoth(int keepMoving)
	{
		DamageStopLeft(keepMoving);
		DamageStopRight(keepMoving);
		DamageStopKick(keepMoving);
	}

	public void SetForwardSpeed(int newSpeed)
	{
		forwardSpeedMinimum = newSpeed;
		forwardSpeedMaximum = newSpeed + 50;
		DecideMovementSpeed(forwardSpeedMinimum, forwardSpeedMaximum);
	}

	public void EnrageTeleport(int teleportType = 0)
	{
		if (secondPhase && !currentCombinedSwordsThrown)
		{
			if (teleportType >= 10)
			{
				if (difficulty < 3)
				{
					return;
				}
				teleportType -= 10;
			}
			if (teleportType <= 0)
			{
				teleportType = 2;
			}
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
			Invoke("ResetAnimSpeed", 0.25f);
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

	private void SetDamage(int damage)
	{
		leftSwingCheck.damage = damage;
		rightSwingCheck.damage = damage;
		generalSwingCheck.damage = damage;
	}

	private void PlayerBeenHit()
	{
		leftSwingCheck.DamageStop();
		rightSwingCheck.DamageStop();
		generalSwingCheck.DamageStop();
		goForward = false;
	}

	private void DecideMovementSpeed(float normal, float longDistance)
	{
		if (difficulty <= 1)
		{
			forwardSpeed = normal * anim.speed;
		}
		forwardSpeed = ((Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position + MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity() * 0.25f, base.transform.position) > 20f) ? (longDistance * anim.speed * (currentSwords ? 0.85f : 1f)) : (normal * anim.speed));
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
