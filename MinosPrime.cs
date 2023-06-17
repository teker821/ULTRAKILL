using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;

public class MinosPrime : MonoBehaviour
{
	private NavMeshAgent nma;

	private Animator anim;

	private Machine mach;

	private EnemyIdentifier eid;

	private GroundCheckEnemy gce;

	private Rigidbody rb;

	private Collider col;

	private AudioSource aud;

	private float originalHp;

	private bool inAction;

	private float cooldown = 2f;

	private MPAttack lastAttack;

	private Transform target;

	private Vector3 playerPos;

	private bool tracking;

	private bool fullTracking;

	private bool aiming;

	private bool jumping;

	public GameObject explosion;

	public GameObject rubble;

	public GameObject bigRubble;

	public GameObject groundWave;

	public GameObject swoosh;

	public Transform aimingBone;

	private Transform head;

	public GameObject projectileCharge;

	public GameObject snakeProjectile;

	private bool hasProjectiled;

	public GameObject warningFlash;

	public GameObject parryableFlash;

	private bool gravityInAction;

	private bool hasRiderKicked;

	private bool previouslyRiderKicked;

	private int downSwingAmount;

	private bool ignoreRiderkickAngle;

	public GameObject attackTrail;

	public GameObject swingSnake;

	private List<GameObject> currentSwingSnakes = new List<GameObject>();

	private bool uppercutting;

	private bool hitSuccessful;

	private bool gotParried;

	public Transform[] swingLimbs;

	private bool swinging;

	private bool boxing;

	private int attacksSinceBoxing;

	private SwingCheck2 sc;

	private GoreZone gz;

	private int attackAmount;

	private bool enraged;

	public GameObject passiveEffect;

	private GameObject currentPassiveEffect;

	public GameObject flameEffect;

	public GameObject phaseChangeEffect;

	private int difficulty = -1;

	private MPAttack previousCombo = MPAttack.Jump;

	private bool activated = true;

	private bool ascending;

	private bool vibrating;

	private Vector3 origPos;

	public GameObject lightShaft;

	public GameObject outroExplosion;

	public UltrakillEvent onOutroEnd;

	private Vector3 spawnPoint;

	[Header("Voice clips")]
	public AudioClip[] riderKickVoice;

	public AudioClip[] dropkickVoice;

	public AudioClip[] dropAttackVoice;

	public AudioClip[] boxingVoice;

	public AudioClip[] comboVoice;

	public AudioClip[] overheadVoice;

	public AudioClip[] projectileVoice;

	public AudioClip[] uppercutVoice;

	public AudioClip phaseChangeVoice;

	public AudioClip[] hurtVoice;

	private bool bossVersion;

	private void Start()
	{
		nma = GetComponent<NavMeshAgent>();
		mach = GetComponent<Machine>();
		gce = GetComponentInChildren<GroundCheckEnemy>();
		rb = GetComponent<Rigidbody>();
		sc = GetComponentInChildren<SwingCheck2>();
		col = GetComponent<Collider>();
		aud = GetComponent<AudioSource>();
		SetSpeed();
		head = eid.weakPoint.transform;
		target = MonoSingleton<PlayerTracker>.Instance.GetPlayer();
		originalHp = mach.health;
		gz = GetComponentInParent<GoreZone>();
		spawnPoint = base.transform.position;
		bossVersion = TryGetComponent<BossHealthBar>(out var _);
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
		if (difficulty == 1)
		{
			anim.speed = 0.9f;
		}
		else if (difficulty == 0)
		{
			anim.speed = 0.8f;
		}
		else
		{
			anim.speed = 1f;
		}
		anim.speed *= eid.totalSpeedModifier;
	}

	private void OnDisable()
	{
		if ((bool)mach)
		{
			CancelInvoke();
			StopAction();
			DamageStop();
			uppercutting = false;
			ascending = false;
			tracking = false;
			fullTracking = false;
			aiming = false;
			jumping = false;
		}
	}

	private void OnEnable()
	{
		if (!activated)
		{
			OutroEnd();
		}
	}

	private void Update()
	{
		if (activated && !BlindEnemies.Blind)
		{
			playerPos = new Vector3(target.position.x, base.transform.position.y, target.position.z);
			if (!inAction)
			{
				cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
			}
			if (!enraged && mach.health < originalHp / 2f)
			{
				enraged = true;
				MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("WEAK");
				aud.clip = phaseChangeVoice;
				aud.pitch = 1f;
				aud.Play();
				currentPassiveEffect = Object.Instantiate(passiveEffect, base.transform.position + Vector3.up * 3.5f, Quaternion.identity);
				currentPassiveEffect.transform.SetParent(base.transform);
				EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
				foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in componentsInChildren)
				{
					Object.Instantiate(flameEffect, enemyIdentifierIdentifier.transform);
				}
				Object.Instantiate(phaseChangeEffect, mach.chest.transform.position, Quaternion.identity);
			}
		}
		else if (ascending)
		{
			rb.velocity = Vector3.MoveTowards(rb.velocity, Vector3.up * 3f, Time.deltaTime);
			MonoSingleton<CameraController>.Instance.CameraShake(0.1f);
		}
		else if (vibrating)
		{
			base.transform.position = new Vector3(origPos.x + Random.Range(-0.1f, 0.1f), origPos.y + Random.Range(-0.1f, 0.1f), origPos.z + Random.Range(-0.1f, 0.1f));
		}
	}

	private void FixedUpdate()
	{
		if (!activated)
		{
			return;
		}
		CustomPhysics();
		if (BlindEnemies.Blind)
		{
			return;
		}
		if (!inAction && gce.onGround && (bool)nma && nma.enabled && nma.isOnNavMesh && Vector3.Distance(base.transform.position, playerPos) > 2.5f)
		{
			nma.isStopped = false;
			if ((bool)MonoSingleton<NewMovement>.Instance.gc && !MonoSingleton<NewMovement>.Instance.gc.onGround)
			{
				if (Physics.Raycast(target.position, Vector3.down, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment)))
				{
					nma.SetDestination(hitInfo.point);
				}
				else
				{
					nma.SetDestination(target.position);
				}
			}
			else
			{
				nma.SetDestination(target.position);
			}
		}
		else if (inAction)
		{
			if ((bool)nma)
			{
				nma.enabled = false;
			}
			anim.SetBool("Walking", value: false);
		}
		if (tracking || fullTracking)
		{
			if (!fullTracking)
			{
				base.transform.LookAt(playerPos);
			}
			else
			{
				base.transform.rotation = Quaternion.LookRotation(target.position - new Vector3(base.transform.position.x, aimingBone.position.y, base.transform.position.z));
			}
		}
		if ((bool)nma && nma.enabled && nma.isOnNavMesh && !inAction)
		{
			if (nma.velocity.magnitude > 2f)
			{
				anim.SetBool("Walking", value: true);
			}
			else
			{
				anim.SetBool("Walking", value: false);
			}
		}
	}

	private void LateUpdate()
	{
		if (aiming && inAction && activated)
		{
			aimingBone.LookAt(target);
			aimingBone.Rotate(Vector3.up * -90f, Space.Self);
		}
	}

	private void CustomPhysics()
	{
		if ((difficulty >= 3 && !enraged && attackAmount >= 10) || (difficulty <= 2 && (((difficulty <= 1 || !enraged) && attackAmount >= 6) || attackAmount >= 12)))
		{
			attackAmount = 0;
			if (difficulty == 1)
			{
				cooldown = 3f;
			}
			else if (difficulty == 0)
			{
				cooldown = 4f;
			}
			else
			{
				cooldown = 2f;
			}
		}
		if (!inAction)
		{
			gravityInAction = false;
			if (gce.onGround && !jumping)
			{
				nma.enabled = true;
				rb.isKinematic = true;
				hasRiderKicked = false;
				hasProjectiled = false;
				downSwingAmount = 0;
				if (cooldown <= 0f && !anim.IsInTransition(0) && !BlindEnemies.Blind)
				{
					float num = Vector3.Distance(base.transform.position, playerPos);
					if (!Physics.Raycast(target.position, Vector3.down, 6f, LayerMaskDefaults.Get(LMD.Environment)) || MonoSingleton<NewMovement>.Instance.rb.velocity.y > 0f)
					{
						if (num < 25f && lastAttack != MPAttack.Jump)
						{
							if (activated)
							{
								Jump();
							}
							lastAttack = MPAttack.Jump;
						}
						else if (num > 25f && lastAttack != MPAttack.ProjectilePunch)
						{
							if (activated)
							{
								ProjectilePunch();
							}
							lastAttack = MPAttack.ProjectilePunch;
						}
						else if (activated)
						{
							int type = Random.Range(0, 4);
							PickAttack(type);
						}
					}
					else if (activated)
					{
						int type2 = Random.Range(0, 4);
						PickAttack(type2);
					}
				}
			}
			else
			{
				nma.enabled = false;
				rb.isKinematic = false;
				if (rb.velocity.y < 0f && !anim.IsInTransition(0) && activated && !BlindEnemies.Blind)
				{
					if (!hasProjectiled && Random.Range(0f, 1f) < 0.25f && enraged && Vector3.Distance(playerPos, base.transform.position) > 6f && !Physics.Raycast(base.transform.position, Vector3.down, 4f, LayerMaskDefaults.Get(LMD.Environment)))
					{
						hasProjectiled = true;
						ProjectilePunch();
					}
					else if (Vector3.Distance(playerPos, base.transform.position) < 5f)
					{
						if (target.position.y < base.transform.position.y)
						{
							DropAttack();
						}
						else if (target.position.y < base.transform.position.y + 10f && downSwingAmount < 2)
						{
							DownSwing();
						}
					}
					else if (Vector3.Angle(Vector3.up, target.position - base.transform.position) > 90f || Vector3.Distance(base.transform.position, target.position) < 10f || ignoreRiderkickAngle)
					{
						if (previouslyRiderKicked && downSwingAmount < 2)
						{
							TeleportAnywhere();
							DownSwing();
							hasRiderKicked = true;
						}
						else if (!hasRiderKicked)
						{
							RiderKick();
						}
					}
					ignoreRiderkickAngle = false;
				}
			}
		}
		else
		{
			nma.enabled = false;
			if (gravityInAction)
			{
				rb.isKinematic = false;
			}
			else
			{
				rb.isKinematic = true;
			}
			if (swinging && !Physics.Raycast(base.transform.position, base.transform.forward, 1f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
			{
				if (MonoSingleton<NewMovement>.Instance.sliding)
				{
					rb.MovePosition(base.transform.position + base.transform.forward * 125f * Time.fixedDeltaTime * eid.totalSpeedModifier);
				}
				else
				{
					rb.MovePosition(base.transform.position + base.transform.forward * 75f * Time.fixedDeltaTime * eid.totalSpeedModifier);
				}
			}
		}
		if (!rb.isKinematic && rb.useGravity)
		{
			rb.velocity -= Vector3.up * 100f * Time.fixedDeltaTime;
		}
		if (!jumping)
		{
			if (!rb.isKinematic)
			{
				anim.SetBool("Falling", value: true);
			}
			else
			{
				anim.SetBool("Falling", value: false);
			}
		}
		else
		{
			if (inAction || !(rb.velocity.y < 0f))
			{
				return;
			}
			jumping = false;
			if (uppercutting)
			{
				uppercutting = false;
				DamageStop();
				if (hitSuccessful && target.position.y > base.transform.position.y && activated && !BlindEnemies.Blind)
				{
					Jump();
					hitSuccessful = false;
				}
			}
		}
	}

	private void PickAttack(int type)
	{
		if (attacksSinceBoxing >= 5)
		{
			type = 0;
		}
		switch (type)
		{
		case 0:
			if (lastAttack != 0)
			{
				Boxing();
				lastAttack = MPAttack.Boxing;
				attacksSinceBoxing = 0;
			}
			else
			{
				PickAttack(type + 1);
			}
			break;
		case 1:
			if (lastAttack != MPAttack.Combo)
			{
				Combo();
				lastAttack = MPAttack.Combo;
				attacksSinceBoxing++;
			}
			else
			{
				PickAttack(type + 1);
			}
			break;
		case 2:
			if (lastAttack != MPAttack.Dropkick)
			{
				Dropkick();
				lastAttack = MPAttack.Dropkick;
				attacksSinceBoxing++;
			}
			else
			{
				PickAttack(type + 1);
			}
			break;
		case 3:
			Uppercut();
			attacksSinceBoxing++;
			break;
		}
	}

	private void Dropkick()
	{
		inAction = true;
		if ((bool)nma && nma.isOnNavMesh)
		{
			nma.isStopped = true;
		}
		anim.Play("Dropkick", 0, 0f);
		tracking = true;
		fullTracking = false;
		sc.knockBackForce = 100f;
		aiming = false;
		attackAmount += 2;
		if (!enraged)
		{
			cooldown += 1.25f;
		}
		else
		{
			cooldown += 0.25f;
		}
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Judgement!");
		PlayVoice(dropkickVoice);
	}

	private void ProjectilePunch()
	{
		inAction = true;
		if ((bool)nma && nma.isOnNavMesh)
		{
			nma.isStopped = true;
		}
		tracking = true;
		fullTracking = false;
		aiming = true;
		anim.Play("ProjectilePunch", 0, 0f);
		ProjectileCharge();
		aiming = false;
		attackAmount++;
		PlayVoice(projectileVoice);
	}

	private void Jump()
	{
		inAction = true;
		base.transform.LookAt(playerPos);
		tracking = true;
		fullTracking = false;
		gravityInAction = true;
		rb.isKinematic = false;
		rb.useGravity = true;
		jumping = true;
		anim.SetBool("Falling", value: false);
		anim.Play("Jump", 0, 0f);
		Invoke("StopAction", 0.1f);
		rb.AddForce(Vector3.up * 100f, ForceMode.VelocityChange);
		Object.Instantiate(swoosh, base.transform.position, Quaternion.identity);
		aiming = false;
	}

	private void Uppercut()
	{
		hitSuccessful = false;
		inAction = true;
		base.transform.LookAt(playerPos);
		tracking = true;
		fullTracking = false;
		gravityInAction = false;
		anim.Play("Uppercut", 0, 0f);
		anim.SetBool("Falling", value: false);
		Object.Instantiate(warningFlash, head.position, Quaternion.LookRotation(MonoSingleton<CameraController>.Instance.transform.position - head.position)).transform.localScale *= 5f;
		sc.knockBackForce = 100f;
		aiming = false;
		attackAmount++;
		PlayVoice(uppercutVoice);
	}

	private void RiderKick()
	{
		downSwingAmount = 0;
		previouslyRiderKicked = true;
		inAction = true;
		base.transform.LookAt(target);
		tracking = true;
		fullTracking = true;
		gravityInAction = false;
		anim.SetTrigger("RiderKick");
		if (difficulty >= 2)
		{
			Invoke("StopTracking", 0.5f / anim.speed);
		}
		else
		{
			Invoke("StopTracking", 0.25f / anim.speed);
		}
		Invoke("RiderKickActivate", 0.75f / anim.speed);
		Object.Instantiate(warningFlash, head.position, Quaternion.LookRotation(MonoSingleton<CameraController>.Instance.transform.position - head.position)).transform.localScale *= 5f;
		sc.knockBackForce = 50f;
		aiming = false;
		attackAmount++;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Die!");
		PlayVoice(riderKickVoice);
	}

	private void DropAttack()
	{
		downSwingAmount = 0;
		tracking = true;
		fullTracking = false;
		ResetRotation();
		inAction = true;
		base.transform.LookAt(playerPos);
		gravityInAction = false;
		anim.SetTrigger("DropAttack");
		Invoke("DropAttackActivate", 0.75f / anim.speed);
		Object.Instantiate(warningFlash, head.position, Quaternion.LookRotation(MonoSingleton<CameraController>.Instance.transform.position - head.position)).transform.localScale *= 5f;
		sc.knockBackForce = 50f;
		aiming = false;
		attackAmount++;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Crush!");
		PlayVoice(dropAttackVoice);
	}

	private void DownSwing()
	{
		downSwingAmount++;
		previouslyRiderKicked = false;
		tracking = true;
		fullTracking = true;
		inAction = true;
		base.transform.LookAt(target);
		gravityInAction = false;
		anim.SetTrigger("DownSwing");
		Object.Instantiate(warningFlash, head.position, Quaternion.LookRotation(MonoSingleton<CameraController>.Instance.transform.position - head.position)).transform.localScale *= 5f;
		sc.knockBackForce = 100f;
		sc.knockBackDirectionOverride = true;
		sc.knockBackDirection = Vector3.down;
		aiming = false;
		attackAmount++;
		PlayVoice(overheadVoice);
	}

	public void UppercutActivate()
	{
		base.transform.LookAt(playerPos);
		uppercutting = true;
		tracking = true;
		fullTracking = false;
		gravityInAction = true;
		rb.isKinematic = false;
		rb.useGravity = true;
		jumping = true;
		anim.SetBool("Falling", value: false);
		Invoke("StopAction", 0.1f);
		rb.AddForce(Vector3.up * 100f, ForceMode.VelocityChange);
		Object.Instantiate(swoosh, base.transform.position, Quaternion.identity);
		Transform child = Object.Instantiate(swingSnake, aimingBone.position + base.transform.forward * 4f, Quaternion.identity).transform.GetChild(0);
		child.SetParent(base.transform, worldPositionStays: true);
		child.rotation = Quaternion.LookRotation(Vector3.up);
		currentSwingSnakes.Add(child.gameObject);
		if (child.TryGetComponent<SwingCheck2>(out var component))
		{
			component.OverrideEnemyIdentifier(eid);
		}
		sc.knockBackDirectionOverride = true;
		sc.knockBackDirection = Vector3.up;
		DamageStart();
	}

	public void UppercutCancel(int parryable = 0)
	{
		if (target.transform.position.y > base.transform.position.y + 5f)
		{
			DamageStop();
			Uppercut();
		}
		else if (parryable == 1)
		{
			Parryable();
		}
	}

	public void Combo()
	{
		if (previousCombo == MPAttack.Combo)
		{
			Boxing();
			return;
		}
		previousCombo = MPAttack.Combo;
		inAction = true;
		base.transform.LookAt(playerPos);
		tracking = true;
		fullTracking = false;
		gravityInAction = false;
		anim.Play("Combo", 0, 0f);
		sc.knockBackForce = 50f;
		aiming = false;
		attackAmount += 3;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Prepare thyself!");
		PlayVoice(comboVoice);
	}

	public void Boxing()
	{
		if (previousCombo == MPAttack.Boxing)
		{
			Combo();
			return;
		}
		previousCombo = MPAttack.Boxing;
		inAction = true;
		base.transform.LookAt(playerPos);
		tracking = true;
		fullTracking = false;
		gravityInAction = false;
		anim.Play("Boxing", 0, 0f);
		sc.knockBackForce = 30f;
		aiming = false;
		attackAmount += 2;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Thy end is now!");
		PlayVoice(boxingVoice);
	}

	private void RiderKickActivate()
	{
		Physics.Raycast(aimingBone.position, base.transform.forward, out var hitInfo, 250f, LayerMaskDefaults.Get(LMD.Environment));
		LineRenderer component = Object.Instantiate(attackTrail, aimingBone.position, base.transform.rotation).GetComponent<LineRenderer>();
		component.SetPosition(0, aimingBone.position);
		RaycastHit[] array = Physics.SphereCastAll(aimingBone.position, 5f, base.transform.forward, Vector3.Distance(aimingBone.position, hitInfo.point), LayerMaskDefaults.Get(LMD.EnemiesAndPlayer));
		bool flag = false;
		new List<EnemyIdentifier>();
		RaycastHit[] array2 = array;
		foreach (RaycastHit raycastHit in array2)
		{
			if (raycastHit.collider.gameObject.tag == "Player" && !flag)
			{
				flag = true;
				MonoSingleton<NewMovement>.Instance.GetHurt(Mathf.RoundToInt(30f * eid.totalDamageModifier), invincible: true);
				MonoSingleton<NewMovement>.Instance.LaunchFromPoint(MonoSingleton<NewMovement>.Instance.transform.position + base.transform.forward * -1f, 100f, 100f);
			}
		}
		if (Vector3.Angle(Vector3.up, hitInfo.normal) < 35f)
		{
			ResetRotation();
			base.transform.position = hitInfo.point;
			anim.Play("DropRecovery", 0, 0f);
		}
		else if (Vector3.Angle(Vector3.up, hitInfo.normal) < 145f)
		{
			base.transform.position = hitInfo.point - base.transform.forward;
			ResetRotation();
			inAction = false;
			hasRiderKicked = true;
			anim.Play("Falling", 0, 0f);
		}
		else
		{
			base.transform.position = hitInfo.point - Vector3.up * 6.5f;
			ResetRotation();
			inAction = false;
			hasRiderKicked = true;
			anim.Play("Falling", 0, 0f);
		}
		ResolveStuckness();
		component.SetPosition(1, aimingBone.position);
		GameObject gameObject = Object.Instantiate(bigRubble, hitInfo.point, Quaternion.identity);
		if (Vector3.Angle(hitInfo.normal, Vector3.up) < 5f)
		{
			gameObject.transform.LookAt(new Vector3(gameObject.transform.position.x + base.transform.forward.x, gameObject.transform.position.y, gameObject.transform.position.z + base.transform.forward.z));
		}
		else
		{
			gameObject.transform.up = hitInfo.normal;
		}
		if (difficulty >= 2)
		{
			gameObject = Object.Instantiate(groundWave, hitInfo.point, Quaternion.identity);
			gameObject.transform.up = hitInfo.normal;
			gameObject.transform.SetParent(gz.transform);
			if (gameObject.TryGetComponent<PhysicalShockwave>(out var component2))
			{
				component2.enemyType = EnemyType.MinosPrime;
				component2.speed *= eid.totalSpeedModifier;
				component2.damage = Mathf.RoundToInt((float)component2.damage * eid.totalDamageModifier);
			}
		}
	}

	private void DropAttackActivate()
	{
		Physics.Raycast(aimingBone.position, Vector3.down, out var hitInfo, 250f, LayerMaskDefaults.Get(LMD.Environment));
		LineRenderer component = Object.Instantiate(attackTrail, aimingBone.position, base.transform.rotation).GetComponent<LineRenderer>();
		component.SetPosition(0, aimingBone.position);
		RaycastHit[] array = Physics.SphereCastAll(aimingBone.position, 5f, Vector3.down, Vector3.Distance(aimingBone.position, hitInfo.point), LayerMaskDefaults.Get(LMD.EnemiesAndPlayer));
		bool flag = false;
		new List<EnemyIdentifier>();
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			if (raycastHit.collider.gameObject.tag == "Player" && !flag)
			{
				flag = true;
				MonoSingleton<NewMovement>.Instance.GetHurt(Mathf.RoundToInt(50f * eid.totalDamageModifier), invincible: true);
				MonoSingleton<NewMovement>.Instance.LaunchFromPoint(MonoSingleton<NewMovement>.Instance.transform.position + (MonoSingleton<NewMovement>.Instance.transform.position - new Vector3(raycastHit.point.x, MonoSingleton<NewMovement>.Instance.transform.position.y, raycastHit.point.z)).normalized, 100f, 100f);
			}
		}
		base.transform.position = hitInfo.point;
		anim.Play("DropRecovery", 0, 0f);
		component.SetPosition(1, aimingBone.position);
		GameObject gameObject = Object.Instantiate(bigRubble, hitInfo.point, Quaternion.identity);
		if (Vector3.Angle(hitInfo.normal, Vector3.up) < 5f)
		{
			gameObject.transform.LookAt(new Vector3(gameObject.transform.position.x + base.transform.forward.x, gameObject.transform.position.y, gameObject.transform.position.z + base.transform.forward.z));
		}
		else
		{
			gameObject.transform.up = hitInfo.normal;
		}
		if (difficulty >= 2)
		{
			gameObject = Object.Instantiate(groundWave, hitInfo.point, Quaternion.identity);
			gameObject.transform.up = hitInfo.normal;
			gameObject.transform.SetParent(gz.transform);
			if (gameObject.TryGetComponent<PhysicalShockwave>(out var component2))
			{
				component2.enemyType = EnemyType.MinosPrime;
				component2.speed *= eid.totalSpeedModifier;
				component2.damage = Mathf.RoundToInt((float)component2.damage * eid.totalDamageModifier);
			}
		}
		Explosion();
	}

	public void SnakeSwingStart(int limb)
	{
		Transform child = Object.Instantiate(swingSnake, aimingBone.position + base.transform.forward * 4f, Quaternion.identity).transform.GetChild(0);
		child.SetParent(base.transform, worldPositionStays: true);
		child.LookAt(playerPos);
		currentSwingSnakes.Add(child.gameObject);
		if (!boxing)
		{
			swinging = true;
		}
		if (child.TryGetComponent<SwingCheck2>(out var component))
		{
			component.OverrideEnemyIdentifier(eid);
			component.knockBackDirectionOverride = true;
			if (sc.knockBackDirectionOverride)
			{
				component.knockBackDirection = sc.knockBackDirection;
			}
			else
			{
				component.knockBackDirection = base.transform.forward;
			}
			component.knockBackForce = sc.knockBackForce;
		}
		if (child.TryGetComponent<AttackTrail>(out var component2))
		{
			component2.target = swingLimbs[limb];
			component2.pivot = aimingBone;
		}
		DamageStart();
	}

	public void DamageStart()
	{
		sc.DamageStart();
	}

	public void DamageStop()
	{
		swinging = false;
		sc.DamageStop();
		sc.knockBackDirectionOverride = false;
		mach.parryable = false;
		if (currentSwingSnakes.Count <= 0)
		{
			return;
		}
		for (int num = currentSwingSnakes.Count - 1; num >= 0; num--)
		{
			if (currentSwingSnakes[num].TryGetComponent<SwingCheck2>(out var component))
			{
				component.DamageStop();
			}
			if (base.gameObject.activeInHierarchy && currentSwingSnakes[num].TryGetComponent<AttackTrail>(out var component2))
			{
				component2.DelayedDestroy(0.5f);
				currentSwingSnakes[num].transform.parent = null;
				component2.target = null;
				component2.pivot = null;
			}
			else
			{
				Object.Destroy(currentSwingSnakes[num]);
			}
		}
		currentSwingSnakes.Clear();
	}

	public void Explosion()
	{
		if (gotParried && difficulty <= 2 && !enraged)
		{
			gotParried = false;
			return;
		}
		GameObject gameObject = Object.Instantiate(this.explosion, base.transform.position, Quaternion.identity);
		mach.parryable = false;
		if (difficulty > 1 && eid.totalDamageModifier == 1f)
		{
			return;
		}
		Explosion[] componentsInChildren = gameObject.GetComponentsInChildren<Explosion>();
		foreach (Explosion explosion in componentsInChildren)
		{
			if (difficulty == 1)
			{
				explosion.speed *= 0.6f;
				explosion.maxSize *= 0.75f;
			}
			else if (difficulty == 0)
			{
				explosion.speed *= 0.5f;
				explosion.maxSize *= 0.5f;
			}
			explosion.speed *= eid.totalDamageModifier;
			explosion.maxSize *= eid.totalDamageModifier;
			explosion.damage = Mathf.RoundToInt((float)explosion.damage * eid.totalDamageModifier);
		}
	}

	public void ProjectileCharge()
	{
		Object.Instantiate(projectileCharge, swingLimbs[0].position, swingLimbs[0].rotation).transform.SetParent(swingLimbs[0]);
	}

	public void ProjectileShoot()
	{
		GameObject obj = Object.Instantiate(snakeProjectile, mach.chest.transform.position, Quaternion.LookRotation(target.position - (base.transform.position + Vector3.up)));
		obj.transform.SetParent(gz.transform);
		Projectile componentInChildren = obj.GetComponentInChildren<Projectile>();
		if ((bool)componentInChildren)
		{
			componentInChildren.target = MonoSingleton<CameraController>.Instance.transform;
			componentInChildren.speed *= eid.totalSpeedModifier;
			componentInChildren.damage *= eid.totalDamageModifier;
		}
		aiming = false;
		tracking = false;
		fullTracking = false;
	}

	public void TeleportOnGround()
	{
		Teleport(playerPos, base.transform.position);
		base.transform.LookAt(playerPos);
	}

	public void TeleportAnywhere()
	{
		Teleport(target.position, base.transform.position);
		base.transform.LookAt(target);
	}

	public void TeleportSide(int side)
	{
		int num = 1;
		boxing = true;
		if (side == 0)
		{
			num = -1;
		}
		if (Physics.Raycast(playerPos + Vector3.up, target.right * num + target.forward, out var hitInfo, 4f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
		{
			if (hitInfo.distance >= 2f)
			{
				Teleport(playerPos, playerPos + Vector3.ClampMagnitude(target.right * num + target.forward, 1f) * (hitInfo.distance - 1f));
			}
			else
			{
				Teleport(playerPos, base.transform.position);
			}
			base.transform.LookAt(playerPos);
		}
		else
		{
			Teleport(playerPos, playerPos + (target.right * num + target.forward) * 10f);
			base.transform.LookAt(playerPos);
		}
	}

	public void Teleport(Vector3 teleportTarget, Vector3 startPos)
	{
		float num = Vector3.Distance(teleportTarget, startPos);
		if (boxing && num > 2.5f)
		{
			num = 2.5f;
		}
		else if (num > 3f)
		{
			num = 3f;
		}
		Vector3 vector = teleportTarget + (startPos - teleportTarget).normalized * num;
		Collider[] array = Physics.OverlapCapsule(vector + base.transform.up * 0.75f, vector + base.transform.up * 5.25f, 0.75f, LayerMaskDefaults.Get(LMD.Environment));
		if (array != null && array.Length != 0)
		{
			for (int i = 0; i < 6; i++)
			{
				Collider collider = array[0];
				if (!Physics.ComputePenetration(col, vector + base.transform.up * 3f, base.transform.rotation, collider, collider.transform.position, collider.transform.rotation, out var direction, out var distance))
				{
					break;
				}
				if (distance > 0.5f)
				{
					Debug.Log("BRO WE FUCKIN WENT IN A WALL at " + vector);
				}
				vector += direction * distance;
				array = Physics.OverlapCapsule(vector + base.transform.up * 0.75f, vector + base.transform.up * 5.25f, 0.75f, LayerMaskDefaults.Get(LMD.Environment));
				if (array == null || array.Length == 0)
				{
					break;
				}
				if (i == 5)
				{
					Debug.Log("overflowed or something i dont know");
					ResolveStuckness();
					break;
				}
			}
		}
		float num2 = Vector3.Distance(base.transform.position, vector);
		for (int j = 0; (float)j < num2; j += 3)
		{
			if (Physics.Raycast(Vector3.Lerp(base.transform.position, vector, (num2 - (float)j) / num2) + Vector3.up, Vector3.down, out var hitInfo, 3f, LayerMaskDefaults.Get(LMD.Environment)))
			{
				Object.Instantiate(rubble, hitInfo.point, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
			}
		}
		MonoSingleton<CameraController>.Instance.CameraShake(0.5f);
		base.transform.position = vector;
		tracking = false;
		fullTracking = false;
		Object.Instantiate(swoosh, base.transform.position, Quaternion.identity);
	}

	public void Death()
	{
		anim.Play("Outro");
		anim.SetBool("Dead", value: true);
		if (bossVersion)
		{
			anim.speed = 1f;
		}
		else
		{
			anim.speed = 5f;
		}
		activated = false;
		if ((bool)currentPassiveEffect)
		{
			Object.Destroy(currentPassiveEffect);
		}
		CancelInvoke();
		DamageStop();
		Object.Destroy(nma);
		DisableGravity();
		rb.useGravity = false;
		rb.isKinematic = true;
		MonoSingleton<TimeController>.Instance.SlowDown(0.0001f);
	}

	public void Ascend()
	{
		if (!bossVersion)
		{
			OutroEnd();
			return;
		}
		rb.isKinematic = false;
		rb.constraints = (RigidbodyConstraints)122;
		ascending = true;
		LightShaft();
		Invoke("LightShaft", 1.5f);
		Invoke("LightShaft", 3f);
		Invoke("LightShaft", 4f);
		Invoke("LightShaft", 5f);
		Invoke("LightShaft", 5.5f);
		Invoke("LightShaft", 6f);
		Invoke("LightShaft", 6.25f);
		Invoke("LightShaft", 6.5f);
		Invoke("LightShaft", 6.7f);
		Invoke("LightShaft", 6.8f);
		Invoke("LightShaft", 6.85f);
		Invoke("LightShaft", 6.9f);
		Invoke("LightShaft", 6.925f);
		Invoke("LightShaft", 6.95f);
		Invoke("LightShaft", 6.975f);
		Invoke("OutroEnd", 7f);
	}

	private void LightShaft()
	{
		if (base.gameObject.activeInHierarchy)
		{
			Object.Instantiate(lightShaft, mach.chest.transform.position, Random.rotation).transform.SetParent(base.transform, worldPositionStays: true);
			MonoSingleton<CameraController>.Instance.CameraShake(1f);
		}
	}

	public void OutroEnd()
	{
		if (base.gameObject.activeInHierarchy)
		{
			onOutroEnd.Invoke();
			Object.Instantiate(outroExplosion, mach.chest.transform.position, Quaternion.identity);
			base.gameObject.SetActive(value: false);
			MonoSingleton<TimeController>.Instance.SlowDown(0.01f);
		}
	}

	public void EnableGravity(int earlyCancel)
	{
		if (!gce.onGround)
		{
			anim.SetBool("Falling", value: true);
			gravityInAction = true;
			if (earlyCancel == 1)
			{
				inAction = false;
			}
		}
		ResetRotation();
	}

	public void Parryable()
	{
		if (difficulty <= 2 || !enraged)
		{
			gotParried = false;
			mach.parryable = true;
			Object.Instantiate(parryableFlash, head.position, Quaternion.LookRotation(MonoSingleton<CameraController>.Instance.transform.position - head.position)).transform.localScale *= 10f;
		}
	}

	public void GotParried()
	{
		PlayVoice(hurtVoice);
		attackAmount -= 5;
		gotParried = true;
	}

	public void Rubble()
	{
		Object.Instantiate(bigRubble, base.transform.position + base.transform.forward, base.transform.rotation);
	}

	public void ResetRotation()
	{
		base.transform.LookAt(new Vector3(base.transform.position.x + base.transform.forward.x, base.transform.position.y, base.transform.position.z + base.transform.forward.z));
		ResolveStuckness();
	}

	public void DisableGravity()
	{
		gravityInAction = false;
	}

	public void StopTracking()
	{
		tracking = false;
		fullTracking = false;
	}

	public void StopAction()
	{
		gotParried = false;
		inAction = false;
		boxing = false;
		if ((bool)mach)
		{
			mach.parryable = false;
		}
	}

	public void PlayerBeenHit()
	{
		sc.DamageStop();
		hitSuccessful = true;
		mach.parryable = false;
		if (uppercutting)
		{
			ignoreRiderkickAngle = true;
		}
		foreach (GameObject currentSwingSnake in currentSwingSnakes)
		{
			if ((bool)currentSwingSnake && currentSwingSnake.TryGetComponent<SwingCheck2>(out var component))
			{
				component.DamageStop();
			}
		}
	}

	public void OutOfBounds()
	{
		base.transform.position = spawnPoint;
	}

	public void Vibrate()
	{
		origPos = base.transform.position;
		vibrating = true;
	}

	public void PlayVoice(AudioClip[] voice)
	{
		if (voice.Length != 0 && (!(aud.clip == phaseChangeVoice) || !aud.isPlaying))
		{
			aud.clip = voice[Random.Range(0, voice.Length)];
			aud.pitch = Random.Range(0.95f, 1f);
			aud.Play();
		}
	}

	public void ResolveStuckness()
	{
		Collider[] array = Physics.OverlapCapsule(base.transform.position + base.transform.up * 0.76f, base.transform.position + base.transform.up * 5.24f, 0.74f, LayerMaskDefaults.Get(LMD.Environment));
		if (array == null || array.Length == 0)
		{
			return;
		}
		if (gce.onGround)
		{
			gce.onGround = false;
			nma.enabled = false;
		}
		for (int i = 0; i < 6; i++)
		{
			RaycastHit[] array2 = Physics.CapsuleCastAll(spawnPoint + base.transform.up * 0.75f, spawnPoint + base.transform.up * 5.25f, 0.75f, base.transform.position - spawnPoint, Vector3.Distance(spawnPoint, base.transform.position), LayerMaskDefaults.Get(LMD.Environment));
			if (array2 == null || array2.Length == 0)
			{
				break;
			}
			RaycastHit[] array3 = array2;
			for (int j = 0; j < array3.Length; j++)
			{
				RaycastHit raycastHit = array3[j];
				bool flag = false;
				Collider[] array4 = array;
				for (int k = 0; k < array4.Length; k++)
				{
					if (array4[k] == raycastHit.collider)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					Debug.Log("Found one, moving");
					base.transform.position = spawnPoint + (base.transform.position - spawnPoint).normalized * raycastHit.distance + raycastHit.normal * 0.1f;
					break;
				}
			}
			if (i == 5)
			{
				Debug.Log("overflowed 2 or something i dont know");
			}
		}
	}
}
