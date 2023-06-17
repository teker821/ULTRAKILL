using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;

public class LeviathanHead : MonoBehaviour
{
	[HideInInspector]
	public bool active = true;

	private Animator anim;

	[SerializeField]
	private Transform shootPoint;

	private bool projectileBursting;

	private int projectilesLeftInBurst;

	private float projectileBurstCooldown;

	public float projectileSpreadAmount;

	public Transform tracker;

	private List<Transform> trackerBones = new List<Transform>();

	[SerializeField]
	private Transform tailBone;

	private Transform[] tailBones;

	private bool inAction = true;

	private float attackCooldown;

	public bool lookAtPlayer;

	private Quaternion defaultHeadRotation = new Quaternion(-0.645012f, 0.2603323f, 0.6614516f, 0.2804788f);

	private Quaternion previousHeadRotation;

	private bool notAtDefaultHeadRotation;

	private bool trackerOverrideAnimation;

	private bool trackerIgnoreLimits;

	private float cantTurnToPlayer;

	private float headRotationSpeedMultiplier = 1f;

	private bool freezeTail;

	private Vector3[] defaultTailPositions;

	private Quaternion[] defaultTailRotations;

	private bool rotateBody;

	private Quaternion defaultBodyRotation;

	private Vector3 defaultPosition;

	private bool bodyRotationOverride;

	private Vector3 bodyRotationOverrideTarget;

	[SerializeField]
	private SwingCheck2 biteSwingCheck;

	public Vector3[] spawnPositions;

	private int previousSpawnPosition;

	private int previousAttack = -1;

	private int recentAttacks;

	[HideInInspector]
	public LeviathanController lcon;

	[SerializeField]
	private UltrakillEvent onRoar;

	[SerializeField]
	private AudioSource projectileWindupSound;

	[SerializeField]
	private AudioSource biteWindupSound;

	[SerializeField]
	private AudioSource swingSound;

	[SerializeField]
	private GameObject warningFlash;

	private void Start()
	{
		SetSpeed();
		previousHeadRotation = tracker.rotation;
		defaultBodyRotation = base.transform.rotation;
		defaultPosition = base.transform.position;
		tailBones = tailBone.GetComponentsInChildren<Transform>();
		defaultTailPositions = new Vector3[tailBones.Length];
		for (int i = 0; i < tailBones.Length; i++)
		{
			defaultTailPositions[i] = tailBones[i].position;
		}
		defaultTailRotations = new Quaternion[tailBones.Length];
		for (int j = 0; j < tailBones.Length; j++)
		{
			defaultTailRotations[j] = tailBones[j].rotation;
		}
		if (!BlindEnemies.Blind)
		{
			anim.Play("AscendLong");
		}
		lookAtPlayer = false;
	}

	public void SetSpeed()
	{
		if (!anim)
		{
			anim = GetComponent<Animator>();
		}
		if (lcon.difficulty <= 2)
		{
			if (lcon.difficulty == 2)
			{
				anim.speed = 0.9f;
			}
			else if (lcon.difficulty == 1)
			{
				anim.speed = 0.8f;
			}
			else
			{
				anim.speed = 0.65f;
			}
		}
		else
		{
			anim.speed = 1f;
		}
		anim.speed *= lcon.eid.totalSpeedModifier;
	}

	private void OnEnable()
	{
		ResetDefaults();
	}

	private void ResetDefaults()
	{
		defaultBodyRotation = base.transform.rotation;
		headRotationSpeedMultiplier = 1f;
		defaultPosition = base.transform.position;
	}

	private void OnDisable()
	{
		trackerOverrideAnimation = false;
		trackerIgnoreLimits = false;
		projectileBursting = false;
		if ((bool)anim)
		{
			anim.SetBool("ProjectileBurst", value: false);
		}
		bodyRotationOverride = false;
	}

	private void LateUpdate()
	{
		if (!active)
		{
			return;
		}
		if (rotateBody && !BlindEnemies.Blind)
		{
			Vector3 vector = (bodyRotationOverride ? bodyRotationOverrideTarget : MonoSingleton<PlayerTracker>.Instance.GetPlayer().position);
			Quaternion quaternion = Quaternion.LookRotation(base.transform.position - ((vector.y < base.transform.position.y) ? new Vector3(vector.x, base.transform.position.y, vector.z) : vector));
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, Time.deltaTime * Mathf.Max(Mathf.Min(270f, Quaternion.Angle(base.transform.rotation, quaternion) * 13.5f), 10f) * lcon.eid.totalSpeedModifier);
			base.transform.position = defaultPosition + Vector3.up * (Mathf.Max(0f, base.transform.localRotation.eulerAngles.x) * 0.85f);
		}
		else
		{
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, defaultBodyRotation, Time.deltaTime * Mathf.Max(Mathf.Min(270f, Quaternion.Angle(base.transform.rotation, defaultBodyRotation) * 13.5f), 10f) * lcon.eid.totalSpeedModifier);
			base.transform.position = Vector3.MoveTowards(base.transform.position, defaultPosition, Time.deltaTime * Mathf.Max(10f, Vector3.Distance(base.transform.position, defaultPosition) * 5f) * lcon.eid.totalSpeedModifier);
		}
		if (lookAtPlayer && !BlindEnemies.Blind)
		{
			Quaternion quaternion2 = Quaternion.LookRotation(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position - tracker.position);
			quaternion2 *= Quaternion.Euler(Vector3.right * 90f);
			if (!trackerOverrideAnimation)
			{
				Quaternion quaternion3 = Quaternion.Inverse(tracker.parent.rotation * defaultHeadRotation) * tracker.rotation;
				quaternion2 *= quaternion3;
				if (!trackerIgnoreLimits)
				{
					float num = Quaternion.Angle(quaternion2, tracker.rotation);
					if (num > 50f)
					{
						quaternion2 = Quaternion.Lerp(tracker.rotation, quaternion2, 50f / num);
						cantTurnToPlayer = Mathf.MoveTowards(cantTurnToPlayer, 5f, Time.deltaTime);
					}
					else
					{
						cantTurnToPlayer = Mathf.MoveTowards(cantTurnToPlayer, 0f, Time.deltaTime);
					}
				}
				quaternion2 = Quaternion.RotateTowards(previousHeadRotation, quaternion2, Time.deltaTime * Mathf.Max(Mathf.Min(270f, Quaternion.Angle(previousHeadRotation, quaternion2) * 13.5f), 10f) * headRotationSpeedMultiplier * lcon.eid.totalSpeedModifier);
			}
			tracker.rotation = quaternion2;
			previousHeadRotation = tracker.rotation;
			notAtDefaultHeadRotation = true;
		}
		else if (notAtDefaultHeadRotation)
		{
			if (Quaternion.Angle(previousHeadRotation, tracker.rotation) > 1f)
			{
				tracker.rotation = Quaternion.RotateTowards(previousHeadRotation, tracker.rotation, Time.deltaTime * Mathf.Max(Mathf.Min(270f, Quaternion.Angle(previousHeadRotation, tracker.rotation) * 13.5f), 10f) * headRotationSpeedMultiplier * lcon.eid.totalSpeedModifier);
				previousHeadRotation = tracker.rotation;
			}
			else
			{
				previousHeadRotation = tracker.rotation;
				notAtDefaultHeadRotation = false;
			}
		}
		else
		{
			previousHeadRotation = tracker.rotation;
		}
		if (freezeTail)
		{
			for (int i = 0; i < tailBones.Length; i++)
			{
				tailBones[i].position = defaultTailPositions[i];
				tailBones[i].rotation = defaultTailRotations[i];
			}
		}
	}

	private void Update()
	{
		if (!active || inAction || BlindEnemies.Blind)
		{
			return;
		}
		attackCooldown = Mathf.MoveTowards(attackCooldown, 0f, Time.deltaTime * lcon.eid.totalSpeedModifier);
		if (!(attackCooldown <= 0f))
		{
			return;
		}
		if (recentAttacks >= 3)
		{
			Descend();
			return;
		}
		if (Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, tracker.position) < 50f)
		{
			Bite();
			previousAttack = 1;
			recentAttacks++;
			return;
		}
		int num = Random.Range(0, 2);
		if (num == previousAttack)
		{
			num++;
		}
		if (num >= 2)
		{
			num = 0;
		}
		switch (num)
		{
		case 0:
			ProjectileBurst();
			break;
		case 1:
			Bite();
			break;
		}
		previousAttack = num;
		recentAttacks++;
	}

	private void FixedUpdate()
	{
		if (!active)
		{
			return;
		}
		if (projectileBursting)
		{
			if (projectileBurstCooldown > 0f)
			{
				projectileBurstCooldown = Mathf.MoveTowards(projectileBurstCooldown, 0f, Time.deltaTime * lcon.eid.totalSpeedModifier);
			}
			else
			{
				if (lcon.difficulty >= 2)
				{
					projectileBurstCooldown = 0.025f;
				}
				else
				{
					projectileBurstCooldown = ((lcon.difficulty == 1) ? 0.0375f : 0.05f);
				}
				projectilesLeftInBurst--;
				GameObject gameObject = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.projectile, shootPoint.position, shootPoint.rotation);
				if (gameObject.TryGetComponent<Projectile>(out var component))
				{
					component.safeEnemyType = EnemyType.Leviathan;
					if (lcon.difficulty >= 2)
					{
						component.speed *= 2f;
					}
					else if (lcon.difficulty == 1)
					{
						component.speed *= 1.5f;
					}
					component.enemyDamageMultiplier = 0.5f;
					component.speed *= lcon.eid.totalSpeedModifier;
					component.damage *= lcon.eid.totalDamageModifier;
				}
				if (projectilesLeftInBurst % 10 != 0)
				{
					gameObject.transform.Rotate(new Vector3(Random.Range(0f - projectileSpreadAmount, projectileSpreadAmount), Random.Range(0f - projectileSpreadAmount, projectileSpreadAmount), Random.Range(0f - projectileSpreadAmount, projectileSpreadAmount)));
				}
				else
				{
					gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, Quaternion.LookRotation(MonoSingleton<PlayerTracker>.Instance.GetTarget().position - gameObject.transform.position), 5f);
				}
				gameObject.transform.localScale *= 2f;
			}
		}
		if (projectileBursting && (projectilesLeftInBurst <= 0 || BlindEnemies.Blind))
		{
			projectileBursting = false;
			trackerIgnoreLimits = false;
			anim.SetBool("ProjectileBurst", value: false);
		}
	}

	private void Descend()
	{
		if (active)
		{
			inAction = true;
			headRotationSpeedMultiplier = 0.5f;
			lookAtPlayer = false;
			rotateBody = false;
			anim.SetBool("Sunken", value: true);
			Object.Instantiate(biteWindupSound, tracker.position, Quaternion.identity, tracker).pitch = 0.5f;
			recentAttacks = 0;
			previousAttack = -1;
		}
	}

	private void DescendEnd()
	{
		if (active)
		{
			base.gameObject.SetActive(value: false);
			lcon.MainPhaseOver();
		}
	}

	public void ChangePosition()
	{
		if (active)
		{
			int num = Random.Range(0, spawnPositions.Length);
			if (spawnPositions.Length > 1 && num == previousSpawnPosition)
			{
				num++;
			}
			if (num >= spawnPositions.Length)
			{
				num = 0;
			}
			if ((bool)lcon.tail && lcon.tail.gameObject.activeInHierarchy && Vector3.Distance(spawnPositions[num], new Vector3(lcon.tail.transform.localPosition.x, spawnPositions[num].y, lcon.tail.transform.localPosition.z)) < 10f)
			{
				num++;
			}
			if (num >= spawnPositions.Length)
			{
				num = 0;
			}
			base.transform.localPosition = spawnPositions[num];
			previousSpawnPosition = num;
			base.transform.rotation = Quaternion.LookRotation(base.transform.position - new Vector3(base.transform.parent.position.x, base.transform.position.y, base.transform.parent.position.z));
			base.gameObject.SetActive(value: true);
			ResetDefaults();
			Ascend();
		}
	}

	private void Ascend()
	{
		if (active)
		{
			inAction = true;
			headRotationSpeedMultiplier = 0.5f;
			lookAtPlayer = false;
			rotateBody = false;
			anim.SetBool("Sunken", value: false);
			BigSplash();
			if (lcon.difficulty <= 2)
			{
				attackCooldown = 1 + (2 - lcon.difficulty);
			}
		}
	}

	private void StartHeadTracking()
	{
		lookAtPlayer = true;
	}

	private void StartBodyTracking()
	{
		rotateBody = true;
	}

	private void Bite()
	{
		if (active)
		{
			rotateBody = true;
			anim.SetTrigger("Bite");
			trackerOverrideAnimation = true;
			inAction = true;
			Object.Instantiate(biteWindupSound, tracker.position, Quaternion.identity, tracker);
			if (lcon.difficulty <= 2)
			{
				attackCooldown = 0.2f + (float)(2 - lcon.difficulty);
			}
		}
	}

	private void BiteStopTracking()
	{
		if (active)
		{
			lookAtPlayer = false;
			trackerOverrideAnimation = false;
			bodyRotationOverride = true;
			if (lcon.difficulty == 0)
			{
				bodyRotationOverrideTarget = MonoSingleton<PlayerTracker>.Instance.GetPlayer().position;
			}
			else
			{
				bodyRotationOverrideTarget = MonoSingleton<PlayerTracker>.Instance.GetPlayer().position + MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity() * ((lcon.difficulty >= 2) ? 0.85f : 0.4f);
			}
			GameObject gameObject = Object.Instantiate(warningFlash, lcon.eid.weakPoint.transform.position + lcon.eid.weakPoint.transform.up, Quaternion.LookRotation(MonoSingleton<CameraController>.Instance.transform.position - tracker.position), tracker);
			gameObject.transform.localScale *= 0.05f;
			gameObject.transform.position += gameObject.transform.forward * 10f;
		}
	}

	private void BiteDamageStart()
	{
		if (active)
		{
			biteSwingCheck.DamageStart();
			Object.Instantiate(swingSound, base.transform.position, Quaternion.identity);
			if (trackerBones == null || trackerBones.Count == 0)
			{
				trackerBones.AddRange(tracker.GetComponentsInChildren<Transform>());
			}
			lcon.stat.parryables = trackerBones;
			lcon.stat.partiallyParryable = true;
		}
	}

	public void BiteDamageStop()
	{
		biteSwingCheck.DamageStop();
		lcon.stat.partiallyParryable = false;
	}

	private void BiteResetRotation()
	{
		rotateBody = false;
		bodyRotationOverride = false;
	}

	private void BiteEnd()
	{
		headRotationSpeedMultiplier = 1f;
		lookAtPlayer = true;
		StopAction();
	}

	private void ProjectileBurst()
	{
		if (active)
		{
			anim.SetBool("ProjectileBurst", value: true);
			if (lcon.difficulty >= 2)
			{
				projectilesLeftInBurst = 80;
			}
			else
			{
				projectilesLeftInBurst = ((lcon.difficulty == 1) ? 60 : 40);
			}
			inAction = true;
			lookAtPlayer = true;
			if (lcon.difficulty <= 2)
			{
				attackCooldown = 0.5f + (float)(2 - lcon.difficulty);
			}
			Object.Instantiate(projectileWindupSound, tracker.position, Quaternion.identity, tracker);
		}
	}

	private void ProjectileBurstStart()
	{
		projectileBursting = true;
	}

	private void StopAction()
	{
		inAction = false;
	}

	private void Roar()
	{
		onRoar?.Invoke();
	}

	private void BigSplash()
	{
		Object.Instantiate(lcon.bigSplash, new Vector3(tracker.position.x, base.transform.position.y, tracker.position.z), Quaternion.LookRotation(Vector3.up));
	}
}
