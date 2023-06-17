using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;

public class SisyphusPrime : MonoBehaviour
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

	private SPAttack lastPrimaryAttack;

	private SPAttack lastSecondaryAttack;

	private int secondariesSinceLastPrimary;

	private int attacksSinceLastExplosion;

	private Transform target;

	private Vector3 playerPos;

	private bool tracking;

	private bool fullTracking;

	private bool aiming;

	private bool jumping;

	public GameObject explosion;

	public GameObject explosionChargeEffect;

	private GameObject currentExplosionChargeEffect;

	public GameObject rubble;

	public GameObject bigRubble;

	public GameObject groundWave;

	public GameObject swoosh;

	public Transform aimingBone;

	private Transform head;

	public GameObject projectileCharge;

	private GameObject currentProjectileCharge;

	public GameObject sparkleExplosion;

	private bool hasProjectiled;

	public GameObject warningFlash;

	public GameObject parryableFlash;

	private bool gravityInAction;

	public GameObject attackTrail;

	public GameObject swingSnake;

	private List<GameObject> currentSwingSnakes = new List<GameObject>();

	private bool uppercutting;

	private bool hitSuccessful;

	private bool gotParried;

	private Vector3 teleportToGroundFailsafe;

	public Transform[] swingLimbs;

	private bool swinging;

	private bool boxing;

	private SwingCheck2 sc;

	private GoreZone gz;

	private int attackAmount;

	private bool enraged;

	public GameObject passiveEffect;

	private GameObject currentPassiveEffect;

	public GameObject flameEffect;

	public GameObject phaseChangeEffect;

	private int difficulty = -1;

	private SPAttack previousCombo = SPAttack.Explosion;

	private bool activated = true;

	private bool ascending;

	private bool vibrating;

	private Vector3 origPos;

	public GameObject lightShaft;

	public GameObject outroExplosion;

	public UltrakillEvent onPhaseChange;

	public UltrakillEvent onOutroEnd;

	private Vector3 spawnPoint;

	[Header("Voice clips")]
	public AudioClip[] uppercutComboVoice;

	public AudioClip[] stompComboVoice;

	public AudioClip phaseChangeVoice;

	public AudioClip[] hurtVoice;

	public AudioClip[] explosionVoice;

	public AudioClip[] tauntVoice;

	public AudioClip[] clapVoice;

	private bool bossVersion;

	private bool taunting;

	private bool tauntCheck;

	private int attacksSinceTaunt;

	private float defaultMoveSpeed;

	private void Start()
	{
		nma = GetComponent<NavMeshAgent>();
		mach = GetComponent<Machine>();
		gce = GetComponentInChildren<GroundCheckEnemy>();
		rb = GetComponent<Rigidbody>();
		sc = GetComponentInChildren<SwingCheck2>();
		col = GetComponent<Collider>();
		aud = GetComponent<AudioSource>();
		defaultMoveSpeed = nma.speed;
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
			if (!inAction || taunting)
			{
				cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
			}
			if (!enraged && mach.health < originalHp / 2f)
			{
				enraged = true;
				MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("YES! That's it!");
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
				onPhaseChange?.Invoke();
			}
		}
		else if (ascending)
		{
			rb.velocity = Vector3.MoveTowards(rb.velocity, Vector3.up * 3f, Time.deltaTime);
			MonoSingleton<CameraController>.Instance.CameraShake(0.1f);
		}
		else if (vibrating)
		{
			float num = 0.1f;
			if (activated)
			{
				num = 0.25f;
			}
			base.transform.position = new Vector3(origPos.x + Random.Range(0f - num, num), origPos.y + Random.Range(0f - num, num), origPos.z + Random.Range(0f - num, num));
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
		if (!inAction && gce.onGround && (bool)nma && nma.enabled && nma.isOnNavMesh)
		{
			if (Vector3.Distance(base.transform.position, playerPos) > 10f)
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
			else if (cooldown > 0f)
			{
				if (!tauntCheck)
				{
					tauntCheck = true;
					if (attacksSinceTaunt >= (enraged ? 15 : 10) && mach.health > 20f)
					{
						Taunt();
					}
					else
					{
						LookAtTarget();
					}
				}
				else
				{
					LookAtTarget();
				}
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
			bool flag = cooldown > 0f && Vector3.Distance(base.transform.position, playerPos) < 20f;
			nma.speed = (flag ? (defaultMoveSpeed / 2f) : defaultMoveSpeed);
			anim.SetBool("Cooldown", flag);
			anim.SetBool("Walking", nma.velocity.magnitude > 2f);
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
		if ((difficulty >= 3 && ((!enraged && attackAmount >= 8) || attackAmount >= 16)) || (difficulty <= 2 && (((difficulty <= 1 || !enraged) && attackAmount >= 6) || attackAmount >= 10)))
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
			tauntCheck = false;
		}
		if (!inAction)
		{
			gravityInAction = false;
			if (gce.onGround && !jumping)
			{
				nma.enabled = true;
				rb.isKinematic = true;
				hasProjectiled = false;
				if (cooldown <= 0f && !anim.IsInTransition(0) && !BlindEnemies.Blind && activated)
				{
					if (!Physics.Raycast(target.position, Vector3.down, (MonoSingleton<NewMovement>.Instance.rb.velocity.y > 0f) ? 11 : 15, LayerMaskDefaults.Get(LMD.Environment)))
					{
						attacksSinceTaunt++;
						secondariesSinceLastPrimary++;
						PickSecondaryAttack();
					}
					else
					{
						PickAnyAttack();
					}
				}
			}
			else
			{
				nma.enabled = false;
				rb.isKinematic = false;
				if (cooldown <= 0f && !anim.IsInTransition(0) && !BlindEnemies.Blind && activated)
				{
					if (secondariesSinceLastPrimary <= (enraged ? 3 : 2))
					{
						attacksSinceTaunt++;
						secondariesSinceLastPrimary++;
						PickSecondaryAttack();
					}
					else if (enraged)
					{
						TeleportOnGround();
					}
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
					hitSuccessful = false;
				}
			}
		}
	}

	private void PickAnyAttack()
	{
		if (secondariesSinceLastPrimary != 0 && (secondariesSinceLastPrimary > ((!enraged) ? 1 : 2) || Random.Range(0f, 1f) > 0.5f))
		{
			attacksSinceTaunt++;
			secondariesSinceLastPrimary = 0;
			PickPrimaryAttack();
		}
		else
		{
			attacksSinceTaunt++;
			secondariesSinceLastPrimary++;
			PickSecondaryAttack();
		}
	}

	private void PickPrimaryAttack(int type = -1)
	{
		if (type == -1)
		{
			type = Random.Range(0, 3);
		}
		switch (type)
		{
		case 0:
			if (lastPrimaryAttack != 0)
			{
				attacksSinceLastExplosion++;
				UppercutCombo();
				lastPrimaryAttack = SPAttack.UppercutCombo;
			}
			else
			{
				PickPrimaryAttack(type + 1);
			}
			break;
		case 1:
			if (lastPrimaryAttack != SPAttack.StompCombo)
			{
				attacksSinceLastExplosion++;
				StompCombo();
				lastPrimaryAttack = SPAttack.StompCombo;
			}
			else
			{
				PickPrimaryAttack(type + 1);
			}
			break;
		case 2:
			if (attacksSinceLastExplosion >= 2)
			{
				lastSecondaryAttack = SPAttack.Explosion;
				attacksSinceLastExplosion = 0;
				TeleportAnywhere();
				ExplodeAttack();
			}
			else
			{
				PickPrimaryAttack(Random.Range(0, 2));
			}
			break;
		}
	}

	private void PickSecondaryAttack(int type = -1)
	{
		if (type == -1)
		{
			type = Random.Range(0, 4);
		}
		switch (type)
		{
		case 0:
			if (lastSecondaryAttack != SPAttack.Chop)
			{
				lastSecondaryAttack = SPAttack.Chop;
				TeleportSide(Random.Range(0, 2), inAir: true);
				Chop();
			}
			else
			{
				PickSecondaryAttack(type + 1);
			}
			break;
		case 1:
			if (lastSecondaryAttack != SPAttack.Clap)
			{
				lastSecondaryAttack = SPAttack.Clap;
				TeleportAnywhere();
				Clap();
			}
			else
			{
				PickSecondaryAttack(type + 1);
			}
			break;
		case 2:
			if (lastSecondaryAttack != SPAttack.AirStomp)
			{
				lastSecondaryAttack = SPAttack.AirStomp;
				TeleportAbove();
				AirStomp();
			}
			else
			{
				PickSecondaryAttack(type + 1);
			}
			break;
		case 3:
			if (lastSecondaryAttack != SPAttack.AirKick)
			{
				lastSecondaryAttack = SPAttack.AirKick;
				TeleportAnywhere(predictive: true);
				AirKick();
			}
			else
			{
				PickSecondaryAttack(0);
			}
			break;
		}
	}

	public void CancelIntoSecondary()
	{
		if (enraged)
		{
			secondariesSinceLastPrimary++;
			int type = Random.Range(0, 3);
			PickSecondaryAttack(type);
		}
	}

	public void Taunt()
	{
		attacksSinceTaunt = 0;
		inAction = true;
		base.transform.LookAt(playerPos);
		tracking = true;
		fullTracking = false;
		gravityInAction = false;
		anim.Play("Taunt", 0, 0f);
		aiming = false;
		taunting = true;
		attackAmount += 2;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Nice try!");
		PlayVoice(tauntVoice);
	}

	public void UppercutCombo()
	{
		previousCombo = SPAttack.UppercutCombo;
		inAction = true;
		base.transform.LookAt(playerPos);
		tracking = true;
		fullTracking = false;
		gravityInAction = false;
		anim.Play("UppercutCombo", 0, 0f);
		sc.knockBackForce = 50f;
		aiming = false;
		attackAmount += 3;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("DESTROY!");
		PlayVoice(uppercutComboVoice);
	}

	public void StompCombo()
	{
		previousCombo = SPAttack.StompCombo;
		inAction = true;
		base.transform.LookAt(playerPos);
		tracking = true;
		fullTracking = false;
		gravityInAction = false;
		anim.Play("StompCombo", 0, 0f);
		sc.knockBackForce = 50f;
		aiming = false;
		attackAmount += 3;
		teleportToGroundFailsafe = base.transform.position;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("You can't escape!");
		PlayVoice(stompComboVoice);
	}

	private void Chop()
	{
		tracking = true;
		fullTracking = true;
		inAction = true;
		base.transform.LookAt(target);
		gravityInAction = false;
		anim.SetTrigger("Chop");
		Unparryable();
		sc.knockBackForce = 50f;
		sc.knockBackDirectionOverride = true;
		sc.knockBackDirection = Vector3.down;
		aiming = false;
		attackAmount++;
	}

	private void Clap()
	{
		tracking = true;
		fullTracking = true;
		inAction = true;
		base.transform.LookAt(target);
		gravityInAction = false;
		anim.SetTrigger("Clap");
		Parryable();
		sc.knockBackForce = 100f;
		aiming = false;
		attackAmount++;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("BE GONE!");
		PlayVoice(clapVoice);
	}

	private void AirStomp()
	{
		tracking = true;
		fullTracking = false;
		inAction = true;
		base.transform.LookAt(target);
		gravityInAction = false;
		anim.SetTrigger("AirStomp");
		Unparryable();
		aiming = false;
		attackAmount++;
	}

	private void AirKick()
	{
		tracking = false;
		fullTracking = false;
		inAction = true;
		gravityInAction = false;
		anim.SetTrigger("AirKick");
		Parryable();
		sc.knockBackForce = 100f;
		sc.ignoreSlidingPlayer = true;
		aiming = false;
		attackAmount++;
	}

	private void ExplodeAttack()
	{
		tracking = true;
		fullTracking = true;
		inAction = true;
		base.transform.LookAt(target);
		gravityInAction = false;
		anim.SetTrigger("Explosion");
		aiming = false;
		attackAmount++;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("This will hurt.");
		PlayVoice(explosionVoice);
	}

	public void ClapStart()
	{
		SnakeSwingStart(0);
		SnakeSwingStart(1);
	}

	public void ClapShockwave()
	{
		DamageStop();
		if (gotParried && difficulty <= 2 && !enraged)
		{
			gotParried = false;
		}
		else if (difficulty >= 2)
		{
			PhysicalShockwave physicalShockwave = CreateShockwave(Vector3.Lerp(swingLimbs[0].position, swingLimbs[1].position, 0.5f));
			physicalShockwave.transform.rotation = base.transform.rotation;
			physicalShockwave.transform.Rotate(Vector3.forward * 90f, Space.Self);
			physicalShockwave.speed *= 2f;
		}
	}

	public void StompShockwave()
	{
		DamageStop();
		if (gotParried && difficulty <= 2 && !enraged)
		{
			gotParried = false;
		}
		else if (difficulty >= 2)
		{
			CreateShockwave(new Vector3(swingLimbs[2].position.x, base.transform.position.y, swingLimbs[2].position.z));
		}
	}

	private PhysicalShockwave CreateShockwave(Vector3 position)
	{
		GameObject obj = Object.Instantiate(groundWave, position, Quaternion.identity);
		obj.transform.SetParent(gz.transform);
		if (obj.TryGetComponent<PhysicalShockwave>(out var component))
		{
			component.enemyType = EnemyType.SisyphusPrime;
			component.speed *= eid.totalSpeedModifier;
			component.damage = Mathf.RoundToInt((float)component.damage * eid.totalDamageModifier);
			return component;
		}
		return null;
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
			anim.Play("Falling", 0, 0f);
		}
		else
		{
			base.transform.position = hitInfo.point - Vector3.up * 6.5f;
			ResetRotation();
			inAction = false;
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
				MonoSingleton<NewMovement>.Instance.GetHurt(Mathf.RoundToInt(30f * eid.totalDamageModifier), invincible: true);
				MonoSingleton<NewMovement>.Instance.LaunchFromPoint(MonoSingleton<NewMovement>.Instance.transform.position + (MonoSingleton<NewMovement>.Instance.transform.position - new Vector3(raycastHit.point.x, MonoSingleton<NewMovement>.Instance.transform.position.y, raycastHit.point.z)).normalized, 100f, 100f);
			}
		}
		base.transform.position = hitInfo.point;
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
				component2.enemyType = EnemyType.SisyphusPrime;
				component2.speed *= eid.totalSpeedModifier;
				component2.damage = Mathf.RoundToInt((float)component2.damage * eid.totalDamageModifier);
			}
		}
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
		SwingCheck2 componentInChildren = child.GetComponentInChildren<SwingCheck2>();
		if ((bool)componentInChildren)
		{
			componentInChildren.OverrideEnemyIdentifier(eid);
			componentInChildren.knockBackDirectionOverride = true;
			if (sc.knockBackDirectionOverride)
			{
				componentInChildren.knockBackDirection = sc.knockBackDirection;
			}
			else
			{
				componentInChildren.knockBackDirection = base.transform.forward;
			}
			componentInChildren.knockBackForce = sc.knockBackForce;
			componentInChildren.ignoreSlidingPlayer = sc.ignoreSlidingPlayer;
		}
		AttackTrail componentInChildren2 = child.GetComponentInChildren<AttackTrail>();
		if ((bool)componentInChildren2)
		{
			componentInChildren2.target = swingLimbs[limb];
			componentInChildren2.pivot = aimingBone;
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
		sc.ignoreSlidingPlayer = false;
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
		vibrating = false;
		if ((bool)currentExplosionChargeEffect)
		{
			Object.Destroy(currentExplosionChargeEffect);
		}
		if (gotParried)
		{
			gotParried = false;
			return;
		}
		GameObject gameObject = Object.Instantiate(this.explosion, aimingBone.position, Quaternion.identity);
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
		if ((bool)currentProjectileCharge)
		{
			Object.Destroy(currentProjectileCharge);
		}
		currentProjectileCharge = Object.Instantiate(projectileCharge, swingLimbs[1].position, swingLimbs[1].rotation);
		currentProjectileCharge.transform.SetParent(swingLimbs[1]);
	}

	public void ProjectileShoot()
	{
		if ((bool)currentProjectileCharge)
		{
			Object.Destroy(currentProjectileCharge);
		}
		mach.parryable = false;
		Vector3 position = MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(0.5f / eid.totalSpeedModifier);
		GameObject obj = Object.Instantiate(sparkleExplosion, position, Quaternion.identity);
		obj.transform.SetParent(gz.transform);
		base.transform.LookAt(new Vector3(position.x, base.transform.position.y, position.z));
		ObjectActivator component = obj.GetComponent<ObjectActivator>();
		if ((bool)component)
		{
			component.delay /= eid.totalSpeedModifier;
		}
		LineRenderer componentInChildren = obj.GetComponentInChildren<LineRenderer>();
		if ((bool)componentInChildren)
		{
			componentInChildren.SetPosition(0, position);
			componentInChildren.SetPosition(1, swingLimbs[1].position);
		}
		Explosion[] componentsInChildren = obj.GetComponentsInChildren<Explosion>();
		foreach (Explosion obj2 in componentsInChildren)
		{
			obj2.speed *= eid.totalSpeedModifier;
			obj2.damage = Mathf.RoundToInt((float)obj2.damage * eid.totalDamageModifier);
			obj2.maxSize *= eid.totalDamageModifier;
		}
		aiming = false;
		tracking = false;
		fullTracking = false;
	}

	public void TeleportOnGround(int forceNoPrediction = 0)
	{
		ResetRotation();
		Vector3 point = teleportToGroundFailsafe;
		if (Physics.Raycast(base.transform.position + Vector3.up, Vector3.down, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment)))
		{
			point = hitInfo.point;
		}
		base.transform.position = point;
		playerPos = new Vector3(target.position.x, base.transform.position.y, target.position.z);
		Teleport(playerPos, base.transform.position);
		if (difficulty < 2 || forceNoPrediction == 1)
		{
			base.transform.LookAt(playerPos);
			return;
		}
		Vector3 vector = MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(0.5f);
		base.transform.LookAt(new Vector3(vector.x, base.transform.position.y, vector.z));
	}

	public void TeleportAnywhere()
	{
		TeleportAnywhere(predictive: false);
	}

	public void TeleportAnywhere(bool predictive = false)
	{
		Teleport(predictive ? MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(0.5f) : target.position, base.transform.position);
		if (difficulty < 2)
		{
			base.transform.LookAt(target);
		}
		else
		{
			base.transform.LookAt(MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(0.5f));
		}
	}

	public void TeleportAbove()
	{
		TeleportAbove(predictive: true);
	}

	public void TeleportAbove(bool predictive = true)
	{
		Vector3 vector = (predictive ? MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(0.5f) : target.position);
		if (vector.y < target.position.y)
		{
			vector.y = target.position.y;
		}
		Teleport(vector + Vector3.up * 25f, vector);
	}

	public void TeleportSideRandom(int predictive)
	{
		TeleportSide(Random.Range(0, 2), inAir: false, predictive == 1);
	}

	public void TeleportSideRandomAir(int predictive)
	{
		TeleportSide(Random.Range(0, 2), inAir: true, predictive == 1);
	}

	public void TeleportSide(int side, bool inAir = false, bool predictive = false)
	{
		int num = 1;
		Vector3 vector = (predictive ? MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(0.5f) : target.position);
		if (!inAir)
		{
			vector = new Vector3(vector.x, base.transform.position.y, vector.z);
		}
		if (side == 0)
		{
			num = -1;
		}
		if (Physics.Raycast(vector + Vector3.up, target.right * num + target.forward, out var hitInfo, 4f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
		{
			if (hitInfo.distance >= 2f)
			{
				Teleport(vector, vector + Vector3.ClampMagnitude(target.right * num + target.forward, 1f) * (hitInfo.distance - 1f));
			}
			else
			{
				Teleport(vector, base.transform.position);
			}
			base.transform.LookAt(vector);
		}
		else
		{
			Teleport(vector, vector + (target.right * num + target.forward) * 10f);
			base.transform.LookAt(vector);
		}
	}

	public void Teleport(Vector3 teleportTarget, Vector3 startPos)
	{
		float num = Vector3.Distance(teleportTarget, startPos);
		if (boxing && num > 4.5f)
		{
			num = 4.5f;
		}
		else if (num > 6f)
		{
			num = 6f;
		}
		LineRenderer component = Object.Instantiate(attackTrail, aimingBone.position, base.transform.rotation).GetComponent<LineRenderer>();
		component.SetPosition(0, aimingBone.position);
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
		component.SetPosition(1, aimingBone.position);
	}

	public void LookAtTarget()
	{
		if (!target)
		{
			target = MonoSingleton<PlayerTracker>.Instance.GetPlayer();
		}
		playerPos = new Vector3(target.position.x, base.transform.position.y, target.position.z);
		base.transform.LookAt(playerPos);
	}

	public void Death()
	{
		if ((bool)currentProjectileCharge)
		{
			Object.Destroy(currentProjectileCharge);
		}
		DamageStop();
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
			MonoSingleton<TimeController>.Instance.SlowDown(0.001f);
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
		gotParried = false;
		mach.parryable = true;
		Object.Instantiate(parryableFlash, head.position, Quaternion.LookRotation(MonoSingleton<CameraController>.Instance.transform.position - head.position)).transform.localScale *= 30f;
	}

	public void Unparryable()
	{
		Object.Instantiate(warningFlash, head.position, Quaternion.LookRotation(MonoSingleton<CameraController>.Instance.transform.position - head.position)).transform.localScale *= 15f;
	}

	public void GotParried()
	{
		PlayVoice(hurtVoice);
		attackAmount -= 5;
		gotParried = true;
		if ((bool)currentExplosionChargeEffect)
		{
			Object.Destroy(currentExplosionChargeEffect);
		}
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

	public void StartTracking()
	{
		tracking = true;
	}

	public void StopTracking()
	{
		tracking = false;
		fullTracking = false;
	}

	public void StopAction()
	{
		fullTracking = false;
		ResetRotation();
		gotParried = false;
		inAction = false;
		boxing = false;
		taunting = false;
		sc.knockBackDirectionOverride = false;
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
		if ((bool)currentExplosionChargeEffect)
		{
			Object.Destroy(currentExplosionChargeEffect);
		}
		if (activated)
		{
			currentExplosionChargeEffect = Object.Instantiate(explosionChargeEffect, aimingBone.position, Quaternion.identity);
		}
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

	public void ForceKnockbackDown()
	{
		sc.knockBackDirectionOverride = true;
		sc.knockBackDirection = Vector3.down;
	}

	public void SwingIgnoreSliding()
	{
		sc.ignoreSlidingPlayer = true;
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
