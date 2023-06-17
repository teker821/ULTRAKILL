using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;

public class Mass : MonoBehaviour
{
	private Animator anim;

	private Transform target;

	private bool battleMode;

	private Vector3 targetPos;

	private Quaternion targetRot;

	private float transformCooldown;

	private bool walking;

	private float walkWeight;

	public bool inAction;

	private bool inSemiAction;

	public Transform[] shootPoints;

	public GameObject homingProjectile;

	private float homingAttackChance = 50f;

	public float attackCooldown = 2f;

	public GameObject explosiveProjectile;

	public GameObject slamExplosion;

	private SwingCheck2[] swingChecks;

	private float swingCooldown = 2f;

	private bool attackedOnce;

	private float playerDistanceCooldown = 1.5f;

	public Transform tailEnd;

	private GameObject tailSpear;

	private float spearCooldown = 5f;

	public GameObject spear;

	public bool spearShot;

	public GameObject spearFlash;

	public GameObject tempSpear;

	public List<GameObject> tailHitboxes = new List<GameObject>();

	public GameObject regurgitateSound;

	public GameObject bigPainSound;

	public GameObject windupSound;

	public bool dead;

	public bool crazyMode;

	public float crazyModeHealth;

	private Statue stat;

	private EnemyIdentifier eid;

	private int crazyPoint;

	public GameObject enrageEffect;

	public GameObject currentEnrageEffect;

	public Material enrageMaterial;

	public Material highVisShockwave;

	public GameObject[] activateOnEnrage;

	private int difficulty = -1;

	private void Start()
	{
		target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
		transformCooldown = 10f;
		swingChecks = GetComponentsInChildren<SwingCheck2>();
		stat = GetComponent<Statue>();
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
			anim = GetComponentInChildren<Animator>();
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
			anim.speed = 0.85f;
		}
		else if (difficulty == 0)
		{
			anim.speed = 0.65f;
		}
		else
		{
			anim.speed = 1f;
		}
		anim.speed *= eid.totalSpeedModifier;
	}

	private void OnDisable()
	{
		StopAction();
		inSemiAction = false;
		if (spearShot)
		{
			SpearReturned();
		}
		if (swingChecks != null)
		{
			SwingCheck2[] array = swingChecks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DamageStop();
			}
		}
	}

	private void OnEnable()
	{
		if (battleMode)
		{
			anim.Play("BattlePose");
		}
	}

	private void Update()
	{
		if (dead || BlindEnemies.Blind)
		{
			return;
		}
		targetPos = new Vector3(target.position.x, base.transform.position.y, target.position.z);
		targetRot = Quaternion.LookRotation(targetPos - base.transform.position, Vector3.up);
		if (!inAction && base.transform.rotation != targetRot)
		{
			if (battleMode || crazyMode)
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, targetRot, Time.deltaTime * Quaternion.Angle(base.transform.rotation, targetRot) + Time.deltaTime * 50f * eid.totalSpeedModifier);
			}
			else
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, targetRot, Time.deltaTime * Quaternion.Angle(base.transform.rotation, targetRot) + Time.deltaTime * 120f * eid.totalSpeedModifier);
			}
			if (stat.health >= 35f)
			{
				walking = true;
			}
			else
			{
				walking = false;
			}
		}
		else
		{
			walking = false;
		}
		if (walking && walkWeight != 1f)
		{
			walkWeight = Mathf.MoveTowards(walkWeight, 1f, Time.deltaTime * 4f);
			anim.SetLayerWeight(1, walkWeight);
		}
		else if (!walking && walkWeight != 0f)
		{
			walkWeight = Mathf.MoveTowards(walkWeight, 0f, Time.deltaTime * 2f);
			anim.SetLayerWeight(1, walkWeight);
		}
		if (spearCooldown != 0f && !spearShot)
		{
			spearCooldown = Mathf.MoveTowards(spearCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (swingCooldown != 0f)
		{
			swingCooldown = Mathf.MoveTowards(swingCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		else if (!inAction && !inSemiAction && battleMode && transformCooldown > 0f)
		{
			base.transform.LookAt(targetPos);
			if (target.position.y - base.transform.position.y < 15f && target.position.y - base.transform.position.y > -5f && attackedOnce && ((Vector3.Distance(targetPos, base.transform.position) > 7f && Random.Range(0f, 1f) < 0.5f) || Vector3.Distance(targetPos, base.transform.position) > 15f))
			{
				BattleSlam();
			}
			else
			{
				SwingAttack();
			}
		}
		if (Vector3.Distance(targetPos, base.transform.position) < 7f)
		{
			playerDistanceCooldown = Mathf.MoveTowards(playerDistanceCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		else
		{
			playerDistanceCooldown = Mathf.MoveTowards(playerDistanceCooldown, 3f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (!battleMode && !crazyMode && playerDistanceCooldown == 0f && !inAction && !inSemiAction && !spearShot)
		{
			base.transform.LookAt(targetPos);
			ToBattle();
		}
		if (stat.health < crazyModeHealth)
		{
			if (battleMode)
			{
				ToScout();
			}
			else
			{
				anim.SetBool("Crazy", value: true);
			}
		}
		else if (transformCooldown != 0f)
		{
			if (battleMode)
			{
				transformCooldown = Mathf.MoveTowards(transformCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
			}
			else
			{
				transformCooldown = Mathf.MoveTowards(transformCooldown, 0f, Time.deltaTime * 1.5f * eid.totalSpeedModifier);
			}
		}
		else if (!inAction && !inSemiAction && !spearShot)
		{
			if (battleMode)
			{
				ToScout();
			}
			else
			{
				base.transform.LookAt(targetPos);
				ToBattle();
			}
		}
		if (attackCooldown != 0f)
		{
			attackCooldown = Mathf.MoveTowards(attackCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		else if (!inAction && transformCooldown > 0f && stat.health >= crazyModeHealth && !battleMode)
		{
			ExplosiveAttack();
		}
	}

	private void LateUpdate()
	{
		if ((!battleMode && !crazyMode) || inAction || inSemiAction || dead)
		{
			return;
		}
		tailEnd.LookAt(target.position);
		if (spearCooldown == 0f)
		{
			if (!Physics.Raycast(tailEnd.position, target.position - tailEnd.position, Vector3.Distance(target.position, tailEnd.position), LayerMaskDefaults.Get(LMD.Environment)))
			{
				spearCooldown = Random.Range(2, 4);
				ReadySpear();
			}
			else
			{
				spearCooldown = 0.1f;
			}
		}
	}

	public void HomingAttack()
	{
		inAction = true;
		anim.SetTrigger("HomingAttack");
		attackCooldown = Random.Range(3, 5);
	}

	public void ExplosiveAttack()
	{
		inAction = true;
		anim.SetTrigger("ExplosiveAttack");
		attackCooldown = Random.Range(3, 5);
	}

	public void SwingAttack()
	{
		inAction = true;
		anim.SetTrigger("Swing");
		swingCooldown = Random.Range(3, 5);
		Object.Instantiate(windupSound, shootPoints[2].position, Quaternion.identity);
		attackedOnce = true;
	}

	public void ToScout()
	{
		if (battleMode)
		{
			transformCooldown = Random.Range(8, 12);
			inAction = true;
			anim.SetBool("Transform", value: true);
			battleMode = false;
			eid.weakPoint = stat.extraDamageZones[0];
		}
	}

	public void ToBattle()
	{
		if (!battleMode)
		{
			anim.SetBool("Transform", value: false);
			transformCooldown = Random.Range(8, 12);
			inAction = true;
			anim.SetTrigger("Slam");
			battleMode = true;
			spearCooldown = 3f;
			AudioSource component = Object.Instantiate(windupSound, shootPoints[2].position, Quaternion.identity).GetComponent<AudioSource>();
			component.pitch = 1f;
			component.volume = 0.75f;
			eid.weakPoint = stat.extraDamageZones[1];
			attackedOnce = false;
		}
	}

	public void SlamImpact()
	{
		if (!dead)
		{
			GameObject gameObject = Object.Instantiate(slamExplosion, new Vector3(shootPoints[2].position.x, base.transform.position.y, shootPoints[2].position.z), Quaternion.identity);
			if (difficulty >= 2)
			{
				gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y * 2.5f, gameObject.transform.localScale.z);
			}
			else if (difficulty == 1)
			{
				gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y * 2f, gameObject.transform.localScale.z);
			}
			else if (difficulty == 0)
			{
				gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y * 1.5f, gameObject.transform.localScale.z);
			}
			PhysicalShockwave component = gameObject.GetComponent<PhysicalShockwave>();
			component.damage = Mathf.RoundToInt(30f * eid.totalDamageModifier);
			if (difficulty == 1)
			{
				component.speed = 20f;
			}
			else if (difficulty == 0)
			{
				component.speed = 15f;
			}
			else
			{
				component.speed = 25f;
			}
			component.speed *= eid.totalSpeedModifier;
			component.maxSize = 100f;
			component.enemy = true;
			component.enemyType = EnemyType.HideousMass;
			MeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material = highVisShockwave;
			}
			gameObject.transform.SetParent(GetComponentInParent<GoreZone>().transform, worldPositionStays: true);
		}
	}

	public void ShootHoming(int arm)
	{
		if (!dead)
		{
			Transform transform = shootPoints[arm];
			Projectile component = Object.Instantiate(homingProjectile, transform.position, transform.rotation).GetComponent<Projectile>();
			component.target = target;
			component.GetComponent<Rigidbody>().velocity = transform.up * 5f;
			component.speed *= eid.totalSpeedModifier;
			component.damage *= eid.totalDamageModifier;
		}
	}

	public void ShootExplosive(int arm)
	{
		if (!dead)
		{
			Transform transform = shootPoints[arm];
			Projectile component = Object.Instantiate(explosiveProjectile, transform.position, transform.rotation).GetComponent<Projectile>();
			component.target = target;
			component.GetComponent<Rigidbody>().AddForce(transform.up * 50f, ForceMode.VelocityChange);
			component.safeEnemyType = EnemyType.HideousMass;
			component.transform.SetParent(GetComponentInParent<GoreZone>().transform, worldPositionStays: true);
			component.speed *= eid.totalSpeedModifier;
			component.damage *= eid.totalDamageModifier;
		}
	}

	private void ReadySpear()
	{
		if (!dead && difficulty != 0)
		{
			if (tailSpear == null)
			{
				tailSpear = tailEnd.GetChild(1).gameObject;
			}
			inSemiAction = true;
			GameObject obj = Object.Instantiate(spearFlash, tailSpear.transform.position, Quaternion.identity);
			Object.Instantiate(regurgitateSound, tailSpear.transform.position, Quaternion.identity);
			obj.transform.SetParent(tailSpear.transform, worldPositionStays: true);
			anim.SetTrigger("ShootSpear");
		}
	}

	public void ShootSpear()
	{
		if (!dead && difficulty != 0)
		{
			inSemiAction = false;
			tailEnd.LookAt(target.position);
			tempSpear = Object.Instantiate(spear, tailSpear.transform.position, tailEnd.rotation);
			tempSpear.transform.LookAt(target);
			if (tempSpear.TryGetComponent<MassSpear>(out var component))
			{
				component.originPoint = tailSpear.transform;
				component.speedMultiplier = eid.totalSpeedModifier;
				component.damageMultiplier = eid.totalDamageModifier;
			}
			tailSpear.SetActive(value: false);
			spearShot = true;
		}
	}

	public void SpearParried()
	{
		if (!dead)
		{
			inAction = true;
			anim.SetTrigger("SpearParried");
			Object.Instantiate(bigPainSound, tailSpear.transform);
		}
	}

	public void SpearReturned()
	{
		tailSpear.SetActive(value: true);
		spearShot = false;
	}

	public void StopAction()
	{
		inAction = false;
	}

	public void BattleSlam()
	{
		inAction = true;
		anim.SetTrigger("BattleSlam");
		swingCooldown = Random.Range(3, 5);
		AudioSource component = Object.Instantiate(windupSound, shootPoints[2].position, Quaternion.identity).GetComponent<AudioSource>();
		component.pitch = 1f;
		component.volume = 0.75f;
	}

	public void SwingStart()
	{
		if (!dead)
		{
			SwingCheck2[] array = swingChecks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DamageStart();
			}
		}
	}

	public void SwingEnd()
	{
		if (!dead)
		{
			SwingCheck2[] array = swingChecks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DamageStop();
			}
			GameObject obj = Object.Instantiate(slamExplosion, (shootPoints[0].position + shootPoints[1].position) / 2f, Quaternion.identity);
			obj.transform.up = base.transform.right;
			PhysicalShockwave component = obj.GetComponent<PhysicalShockwave>();
			component.damage = Mathf.RoundToInt(20f * eid.totalDamageModifier);
			component.speed = 100f * eid.totalSpeedModifier;
			if (difficulty < 2)
			{
				component.maxSize = 10f;
			}
			else
			{
				component.maxSize = 100f;
			}
			component.enemy = true;
			component.enemyType = EnemyType.HideousMass;
			obj.transform.SetParent(GetComponentInParent<GoreZone>().transform, worldPositionStays: true);
			AudioSource component2 = obj.GetComponent<AudioSource>();
			component2.pitch = 1.5f;
			component2.volume = 0.5f;
		}
	}

	public void Enrage()
	{
		currentEnrageEffect = Object.Instantiate(enrageEffect, stat.chest.transform);
		currentEnrageEffect.transform.localScale = Vector3.one * 2f;
		currentEnrageEffect.transform.localPosition = new Vector3(-0.25f, 0f, 0f);
		stat.smr.material = enrageMaterial;
		GetComponentInChildren<EnemySimplifier>().enraged = true;
		eid.UpdateBuffs(visualsOnly: true);
		if (eid.sandified)
		{
			stat.smr.material.SetFloat("_HasSandBuff", 1f);
		}
		GameObject[] array = activateOnEnrage;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
		GetComponent<AudioSource>().Play();
	}

	public void CrazyReady()
	{
		inAction = false;
		inSemiAction = false;
		crazyMode = true;
		Invoke("CrazyShoot", 0.5f / eid.totalSpeedModifier);
		Invoke("CrazyShoot", 1.5f / eid.totalSpeedModifier);
	}

	public void CrazyShoot()
	{
		if (!dead)
		{
			ShootExplosive(crazyPoint);
			if (crazyPoint == 0)
			{
				crazyPoint = 1;
			}
			else
			{
				crazyPoint = 0;
			}
			Invoke("CrazyShoot", Random.Range(2f, 3f) / eid.totalSpeedModifier);
		}
	}
}
