using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;

public class V2 : MonoBehaviour
{
	private Animator anim;

	private Transform target;

	private Rigidbody targetRb;

	private Transform overrideTarget;

	private Rigidbody overrideTargetRb;

	private Vector3 targetPos;

	private Quaternion targetRot;

	public Transform[] aimAtTarget;

	private Rigidbody rb;

	private NavMeshAgent nma;

	private int currentWeapon;

	public SkinnedMeshRenderer smr;

	private Material wingMaterial;

	public Texture[] wingTextures;

	public GameObject wingChangeEffect;

	public Color[] wingColors;

	public GameObject[] weapons;

	private GameObject currentWingChangeEffect;

	private TrailRenderer[] wingTrails;

	private DragBehind[] drags;

	private int currentPattern;

	private bool inPattern;

	public LayerMask environmentMask;

	public GroundCheckEnemy gc;

	public GroundCheckEnemy wc;

	private int pattern1direction = 1;

	public GameObject jumpSound;

	public GameObject dashJumpSound;

	public bool secondEncounter;

	public bool slowMode;

	public float movementSpeed;

	private float originalMovementSpeed;

	public float jumpPower;

	public float wallJumpPower;

	public float airAcceleration;

	public bool intro;

	[HideInInspector]
	public bool inIntro;

	public bool active;

	private bool running;

	private bool aiming;

	private bool sliding;

	private bool dodging;

	private bool jumping;

	private float patternCooldown;

	private float dodgeCooldown = 3f;

	private float dodgeLeft;

	public GameObject dodgeEffect;

	public GameObject slideEffect;

	private int difficulty = -1;

	private float slideStopTimer;

	private float shootCooldown;

	private float altShootCooldown;

	public GameObject gunFlash;

	public GameObject altFlash;

	private bool aboutToShoot;

	private bool chargingAlt;

	private float predictAmount;

	private bool aimAtGround;

	public bool dontDie;

	public Transform escapeTarget;

	private bool escaping;

	private bool dead;

	public bool longIntro;

	private bool staringAtPlayer;

	private bool introHitGround;

	private EnemyIdentifierIdentifier[] eidids;

	private BossHealthBar bhb;

	public GameObject shockwave;

	public GameObject KoScream;

	private RaycastHit rhit;

	private float distancePatience;

	private bool enraged;

	public GameObject enrageEffect;

	private GameObject currentEnrageEffect;

	private Machine mac;

	private EnemyIdentifier eid;

	private bool drilled;

	private float circleTimer = 5f;

	public GameObject spawnOnDeath;

	private bool playerInSight;

	private int coinsToThrow;

	private bool shootingForCoin;

	public GameObject coin;

	[HideInInspector]
	public bool firstPhase = true;

	public float knockOutHealth;

	public bool slideOnly;

	public Vector3 forceSlideDirection;

	private bool cowardPattern;

	public UltrakillEvent onKnockout;

	private float flashTimer;

	private List<Coin> coins = new List<Coin>();

	private bool bossVersion = true;

	private float coinsInSightCooldown;

	private void Start()
	{
		anim = GetComponentInChildren<Animator>();
		target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
		targetRb = MonoSingleton<PlayerTracker>.Instance.GetRigidbody();
		rb = GetComponent<Rigidbody>();
		gc = GetComponentInChildren<GroundCheckEnemy>();
		wingMaterial = smr.sharedMaterials[1];
		bhb = GetComponent<BossHealthBar>();
		mac = GetComponent<Machine>();
		if ((bool)MonoSingleton<StatueIntroChecker>.Instance && MonoSingleton<StatueIntroChecker>.Instance.beenSeen)
		{
			longIntro = false;
		}
		if (!intro)
		{
			active = true;
			if ((bool)bhb)
			{
				bhb.enabled = true;
			}
		}
		else
		{
			inIntro = true;
			rb.AddForce(base.transform.forward * 20f, ForceMode.VelocityChange);
			anim.SetBool("InAir", value: true);
			anim.SetLayerWeight(1, 1f);
			anim.SetLayerWeight(2, 0f);
			if (longIntro)
			{
				eidids = GetComponentsInChildren<EnemyIdentifierIdentifier>();
				EnemyIdentifierIdentifier[] array = eidids;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].GetComponent<Collider>().enabled = false;
				}
				if ((bool)bhb)
				{
					bhb.enabled = false;
				}
			}
			else if ((bool)bhb)
			{
				bhb.enabled = true;
			}
		}
		SetSpeed();
		running = true;
		aiming = true;
		inPattern = true;
		wingMaterial.mainTexture = wingTextures[0];
		wingTrails = GetComponentsInChildren<TrailRenderer>();
		drags = GetComponentsInChildren<DragBehind>();
		ChangeDirection(Random.Range(-90f, 90f));
		SwitchPattern(0);
		shootCooldown = 1f;
		altShootCooldown = 5f;
		if (!weapons[currentWeapon].activeInHierarchy)
		{
			GameObject[] array2 = weapons;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].SetActive(value: false);
			}
			weapons[currentWeapon].SetActive(value: true);
		}
		if (!bhb)
		{
			bossVersion = false;
		}
		if (secondEncounter)
		{
			SlowUpdate();
		}
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	private void SetSpeed()
	{
		if (!nma)
		{
			nma = GetComponent<NavMeshAgent>();
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
		if (originalMovementSpeed != 0f)
		{
			movementSpeed = originalMovementSpeed;
		}
		if (difficulty == 2)
		{
			movementSpeed *= 0.85f;
		}
		else if (difficulty == 1)
		{
			movementSpeed *= 0.75f;
		}
		else if (difficulty == 0)
		{
			movementSpeed *= 0.65f;
		}
		movementSpeed *= eid.totalSpeedModifier;
		originalMovementSpeed = movementSpeed;
		if (enraged)
		{
			movementSpeed *= 2f;
		}
		if ((bool)nma)
		{
			nma.speed = originalMovementSpeed;
		}
		GameObject[] array = weapons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].transform.GetChild(0).SendMessage("UpdateBuffs", eid, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void Update()
	{
		if (active && !escaping)
		{
			if (!sliding && slideOnly && gc.onGround && !dodging)
			{
				anim.Play("Slide");
				base.transform.LookAt(new Vector3(base.transform.position.x + forceSlideDirection.x, base.transform.position.y + forceSlideDirection.y, base.transform.position.z + forceSlideDirection.z));
				Slide();
			}
			targetPos = new Vector3(target.position.x, base.transform.position.y, target.position.z);
			if (!slideOnly)
			{
				if (dodging)
				{
					anim.SetBool("InAir", value: true);
					anim.SetLayerWeight(1, 1f);
					anim.SetLayerWeight(2, 0f);
					if (!drags[0].active)
					{
						DragBehind[] array = drags;
						for (int i = 0; i < array.Length; i++)
						{
							array[i].active = true;
						}
					}
				}
				else if (!gc.onGround)
				{
					anim.SetBool("InAir", value: true);
					anim.SetLayerWeight(1, 1f);
					anim.SetLayerWeight(2, 0f);
					if (!drags[0].active)
					{
						DragBehind[] array = drags;
						for (int i = 0; i < array.Length; i++)
						{
							array[i].active = true;
						}
					}
				}
				else if (running && !sliding)
				{
					anim.SetBool("InAir", value: false);
					anim.SetLayerWeight(1, 1f);
					if (anim.transform.rotation.eulerAngles.y > base.transform.rotation.eulerAngles.y)
					{
						anim.SetBool("RunningLeft", value: true);
					}
					else
					{
						anim.SetBool("RunningLeft", value: false);
					}
					float num = Quaternion.Angle(anim.transform.rotation, base.transform.rotation);
					if (num > 90f)
					{
						anim.SetBool("RunningBack", value: true);
					}
					else
					{
						anim.SetBool("RunningBack", value: false);
					}
					if (num <= 90f)
					{
						anim.SetLayerWeight(2, num / 90f);
					}
					else
					{
						anim.SetLayerWeight(2, Mathf.Abs(-180f + num) / 90f);
					}
					if (drags[0].active)
					{
						DragBehind[] array = drags;
						for (int i = 0; i < array.Length; i++)
						{
							array[i].active = false;
						}
					}
				}
				else
				{
					anim.SetBool("InAir", value: false);
					anim.SetLayerWeight(1, 0f);
					anim.SetLayerWeight(2, 0f);
					if (sliding && !drags[0].active)
					{
						DragBehind[] array = drags;
						for (int i = 0; i < array.Length; i++)
						{
							array[i].active = true;
						}
					}
					else if (!sliding && drags[0].active)
					{
						DragBehind[] array = drags;
						for (int i = 0; i < array.Length; i++)
						{
							array[i].active = false;
						}
					}
				}
			}
			if (BlindEnemies.Blind)
			{
				running = false;
				if (mac.health <= knockOutHealth && knockOutHealth != 0f && firstPhase)
				{
					firstPhase = false;
					KnockedOut();
					eid.totalDamageTakenMultiplier = 0f;
				}
				return;
			}
			if (!sliding)
			{
				targetRot = Quaternion.LookRotation(targetPos - base.transform.position, Vector3.up);
				if (inPattern && currentPattern != 0)
				{
					if (cowardPattern)
					{
						base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(base.transform.position - targetPos, Vector3.up), Time.deltaTime * 350f * eid.totalSpeedModifier);
					}
					else if (currentPattern == 1 || Vector3.Distance(base.transform.position, targetPos) < 10f)
					{
						float num2 = 90f;
						if (Vector3.Distance(base.transform.position, targetPos) > 10f)
						{
							num2 = 80f;
						}
						else if (Vector3.Distance(base.transform.position, targetPos) < 5f)
						{
							num2 = 100f;
						}
						Quaternion rotation = targetRot;
						rotation.eulerAngles = new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y + num2 * (float)pattern1direction, rotation.eulerAngles.z);
						base.transform.rotation = rotation;
					}
					else
					{
						base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, targetRot, Time.deltaTime * 350f * eid.totalSpeedModifier);
						if (base.transform.rotation == targetRot && playerInSight && gc.onGround && !jumping && ((difficulty <= 2 && MonoSingleton<NewMovement>.Instance.hp > 50 && Vector3.Distance(base.transform.position, targetPos) > 10f) || Vector3.Distance(base.transform.position, targetPos) > 20f))
						{
							Slide();
						}
					}
				}
				anim.transform.rotation = Quaternion.RotateTowards(anim.transform.rotation, targetRot, Time.deltaTime * 10f * Quaternion.Angle(anim.transform.rotation, targetRot) * eid.totalSpeedModifier);
			}
			else if (!slideOnly)
			{
				Quaternion a = Quaternion.LookRotation(base.transform.forward, Vector3.up);
				Quaternion b = Quaternion.LookRotation(targetPos - base.transform.position, Vector3.up);
				if ((bool)nma && !playerInSight)
				{
					StopSlide();
				}
				else if (Quaternion.Angle(a, b) > 90f || (distancePatience >= 5f && Quaternion.Angle(a, b) > 45f))
				{
					slideStopTimer = Mathf.MoveTowards(slideStopTimer, 0f, Time.deltaTime * eid.totalSpeedModifier);
					if (slideStopTimer <= 0f || enraged || (difficulty <= 2 && MonoSingleton<NewMovement>.Instance.hp < 50))
					{
						StopSlide();
					}
				}
			}
			else
			{
				anim.transform.localRotation = Quaternion.identity;
			}
			if (dodgeCooldown < 6f)
			{
				if (difficulty > 2)
				{
					dodgeCooldown = Mathf.MoveTowards(dodgeCooldown, 6f, Time.deltaTime * 0.5f * eid.totalSpeedModifier);
				}
				else
				{
					dodgeCooldown = Mathf.MoveTowards(dodgeCooldown, 6f, Time.deltaTime * 0.1f * eid.totalSpeedModifier);
				}
			}
			if (dodgeLeft > 0f)
			{
				dodgeLeft = Mathf.MoveTowards(dodgeLeft, 0f, Time.deltaTime * 3f * eid.totalSpeedModifier);
				if (dodgeLeft <= 0f)
				{
					dodging = false;
					eid.hookIgnore = false;
					inPattern = true;
					CheckPattern();
					if (currentPattern == 2 && !cowardPattern)
					{
						Quaternion rotation2 = anim.transform.rotation;
						base.transform.LookAt(targetPos);
						anim.transform.rotation = rotation2;
					}
				}
			}
			if (patternCooldown > 0f)
			{
				patternCooldown = Mathf.MoveTowards(patternCooldown, 0f, Time.deltaTime);
			}
			if (inPattern)
			{
				if (playerInSight)
				{
					_ = currentPattern;
					if (gc.onGround && !jumping)
					{
						if (Physics.Raycast(base.transform.position + Vector3.up, base.transform.forward, 4f, environmentMask) && !slideOnly)
						{
							Jump();
						}
					}
					else if (wc.onGround)
					{
						if (!gc.onGround && !jumping)
						{
							WallJump();
						}
						else if (gc.onGround)
						{
							ChangeDirection(Random.Range(100, 260));
						}
					}
				}
				if (Vector3.Distance(base.transform.position, target.position) > 15f || !playerInSight)
				{
					distancePatience = Mathf.MoveTowards(distancePatience, 12f, Time.deltaTime * eid.totalSpeedModifier);
					if ((distancePatience >= 4f || Vector3.Distance(base.transform.position, target.position) > 30f) && currentPattern != 2)
					{
						currentPattern = 2;
						SwitchPattern(2);
					}
					if (distancePatience == 12f && !enraged)
					{
						Enrage();
					}
				}
				else if (distancePatience > 0f)
				{
					if (!enraged)
					{
						distancePatience = Mathf.MoveTowards(distancePatience, 0f, Time.deltaTime * 2f * eid.totalSpeedModifier);
					}
					else
					{
						distancePatience = Mathf.MoveTowards(distancePatience, 0f, Time.deltaTime * eid.totalSpeedModifier);
					}
					if (enraged && distancePatience < 10f)
					{
						Unenrage();
					}
				}
			}
			if (!slideOnly)
			{
				if ((currentPattern == 1 && !cowardPattern) || Vector3.Distance(base.transform.position, target.position) < 10f)
				{
					if (currentPattern == 1)
					{
						circleTimer = Mathf.MoveTowards(circleTimer, 0f, Time.deltaTime * eid.totalSpeedModifier);
					}
					else
					{
						circleTimer = Mathf.MoveTowards(circleTimer, 0f, Time.deltaTime * 1.5f * eid.totalSpeedModifier);
					}
					if (circleTimer <= 0f && !dodging && dodgeLeft <= 0f && !enraged && (MonoSingleton<NewMovement>.Instance.hp > 33 || difficulty >= 4))
					{
						circleTimer = 1f;
						ForceDodge(base.transform.position - targetPos);
						if (!cowardPattern && currentPattern != 1)
						{
							cowardPattern = true;
							SwitchPattern(3);
						}
					}
				}
				else
				{
					circleTimer = Mathf.MoveTowards(circleTimer, 5f, Time.deltaTime * eid.totalSpeedModifier);
					if (cowardPattern && circleTimer > 2f)
					{
						cowardPattern = false;
						CheckPattern();
						SwitchPattern(currentPattern);
					}
				}
			}
			float num3 = 1f;
			if (difficulty == 1)
			{
				num3 = 0.85f;
			}
			if (difficulty == 0)
			{
				num3 = 0.75f;
			}
			if (altShootCooldown > 0f)
			{
				altShootCooldown = Mathf.MoveTowards(altShootCooldown, 0f, Time.deltaTime * num3 * eid.totalSpeedModifier);
			}
			if (secondEncounter && !enraged && coinsToThrow <= 0)
			{
				if (coins.Count > 0)
				{
					Coin coin = null;
					float num4 = 60f;
					foreach (Coin coin2 in coins)
					{
						float num5 = Vector3.Distance(coin2.transform.position, aimAtTarget[1].position);
						if (!coin2.shot && Vector3.Distance(coin2.transform.position, base.transform.position) < num4 && !Physics.Raycast(aimAtTarget[1].position, coin2.transform.position - aimAtTarget[1].position, num5, LayerMaskDefaults.Get(LMD.Environment)))
						{
							num4 = num5;
							coin = coin2;
						}
						if (eid.difficultyOverride >= 0)
						{
							coin2.difficulty = eid.difficultyOverride;
						}
					}
					if (coin != null)
					{
						if (coinsInSightCooldown > 0f)
						{
							coinsInSightCooldown = Mathf.MoveTowards(coinsInSightCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
						}
						else
						{
							overrideTarget = coin.transform;
							overrideTargetRb = coin.GetComponent<Rigidbody>();
							if (currentWeapon != 0 || !aboutToShoot || !shootingForCoin)
							{
								if (currentWeapon != 0 || !shootingForCoin)
								{
									CancelInvoke("ShootWeapon");
									CancelInvoke("AltShootWeapon");
									weapons[currentWeapon].transform.GetChild(0).SendMessage("CancelAltCharge", SendMessageOptions.DontRequireReceiver);
									if (currentWeapon != 0)
									{
										SwitchWeapon(0);
									}
								}
								shootCooldown = 1f;
								shootingForCoin = true;
								aboutToShoot = true;
								Object.Instantiate(gunFlash, aimAtTarget[1].transform.position, Quaternion.LookRotation(target.transform.position - aimAtTarget[1].transform.position)).transform.localScale *= 20f;
								Invoke("ShootWeapon", 0.4f / eid.totalSpeedModifier);
							}
						}
					}
					else
					{
						if (shootingForCoin && aboutToShoot)
						{
							CancelInvoke("ShootWeapon");
						}
						shootingForCoin = false;
						overrideTarget = null;
					}
				}
				else if ((bool)overrideTarget)
				{
					if (shootingForCoin && aboutToShoot)
					{
						CancelInvoke("ShootWeapon");
					}
					shootingForCoin = false;
					overrideTarget = null;
				}
				else
				{
					shootingForCoin = false;
				}
			}
			else if ((bool)overrideTarget && enraged)
			{
				if (shootingForCoin && aboutToShoot)
				{
					CancelInvoke("ShootWeapon");
				}
				overrideTarget = null;
				shootingForCoin = false;
			}
			if (secondEncounter && (coins.Count == 0 || (aboutToShoot && shootingForCoin)))
			{
				if (difficulty > 3)
				{
					coinsInSightCooldown = 0f;
				}
				else
				{
					switch (difficulty)
					{
					case 3:
						coinsInSightCooldown = 0.2f;
						break;
					case 2:
						coinsInSightCooldown = 0.4f;
						break;
					case 1:
						coinsInSightCooldown = 0.6f;
						break;
					case 0:
						coinsInSightCooldown = 0.8f;
						break;
					}
				}
			}
			if (shootCooldown > 0f)
			{
				if (cowardPattern)
				{
					shootCooldown = Mathf.MoveTowards(shootCooldown, 0f, Time.deltaTime * num3 * 0.5f * eid.totalSpeedModifier);
				}
				else
				{
					shootCooldown = Mathf.MoveTowards(shootCooldown, 0f, Time.deltaTime * num3 * eid.totalSpeedModifier);
				}
			}
			else if (aiming && (!nma || playerInSight))
			{
				if (!aboutToShoot)
				{
					if ((weapons.Length < 2 && Vector3.Distance(target.position, base.transform.position) > 15f) || Vector3.Distance(target.position, base.transform.position) > 25f)
					{
						SwitchWeapon(0);
					}
					else if (weapons.Length > 2 && Vector3.Distance(target.position, base.transform.position) > 15f)
					{
						if (eid.stuckMagnets.Count <= 0)
						{
							SwitchWeapon(2);
						}
						else
						{
							SwitchWeapon(0);
						}
					}
					else
					{
						SwitchWeapon(1);
					}
				}
				if (!Physics.Raycast(base.transform.position + Vector3.up * 2f, target.position - base.transform.position, out rhit, Vector3.Distance(base.transform.position, target.position), environmentMask))
				{
					if (altShootCooldown <= 0f || (distancePatience >= 8f && currentWeapon == 0))
					{
						if (currentWeapon == 0)
						{
							predictAmount = 0.15f / eid.totalSpeedModifier;
						}
						else
						{
							aimAtGround = true;
							if (currentWeapon == 1 || difficulty > 2)
							{
								predictAmount = 0.25f / eid.totalSpeedModifier;
							}
							else
							{
								predictAmount = -0.25f / eid.totalSpeedModifier;
							}
						}
						if (difficulty > 2)
						{
							shootCooldown = Random.Range(1f, 2f);
						}
						else
						{
							shootCooldown = 2f;
						}
						altShootCooldown = 5f;
						aboutToShoot = true;
						if (!secondEncounter || Vector3.Distance(target.position, base.transform.position) < 8f || Random.Range(0f, 1f) < 0.5f || enraged)
						{
							chargingAlt = true;
							weapons[currentWeapon].transform.GetChild(0).SendMessage("PrepareAltFire");
							if (difficulty >= 2)
							{
								Invoke("AltShootWeapon", 1f / eid.totalSpeedModifier);
							}
							else if (difficulty == 1)
							{
								Invoke("AltShootWeapon", 1.25f / eid.totalSpeedModifier);
							}
							else
							{
								Invoke("AltShootWeapon", 1.5f / eid.totalSpeedModifier);
							}
						}
						else
						{
							SwitchWeapon(0);
							if (difficulty >= 2)
							{
								coinsToThrow = 3;
							}
							else
							{
								coinsToThrow = 1;
							}
							ThrowCoins();
						}
					}
					else
					{
						if (currentWeapon == 0)
						{
							if (distancePatience >= 4f)
							{
								shootCooldown = 1f;
							}
							if (difficulty > 2)
							{
								shootCooldown = Random.Range(1.5f, 2f);
							}
							else
							{
								shootCooldown = 2f;
							}
						}
						else
						{
							if (currentWeapon == 1 || difficulty > 2)
							{
								predictAmount = 0.15f / eid.totalSpeedModifier;
							}
							else
							{
								predictAmount = -0.25f / eid.totalSpeedModifier;
							}
							if (difficulty > 2)
							{
								shootCooldown = Random.Range(1.5f, 2f);
							}
							else
							{
								shootCooldown = 2f;
							}
						}
						weapons[currentWeapon].transform.GetChild(0).SendMessage("PrepareFire");
						aboutToShoot = true;
						if (currentWeapon == 0)
						{
							Object.Instantiate(gunFlash, aimAtTarget[1].transform.position, Quaternion.LookRotation(target.transform.position - aimAtTarget[1].transform.position)).transform.localScale *= 20f;
							shootingForCoin = false;
							if (difficulty >= 2)
							{
								Invoke("ShootWeapon", 0.75f / eid.totalSpeedModifier);
							}
							if (difficulty >= 1)
							{
								Invoke("ShootWeapon", 0.95f / eid.totalSpeedModifier);
							}
							Invoke("ShootWeapon", 1.15f / eid.totalSpeedModifier);
						}
						else if (difficulty >= 2)
						{
							Invoke("ShootWeapon", 0.75f / eid.totalSpeedModifier);
						}
						else if (difficulty == 1)
						{
							Invoke("ShootWeapon", 1f / eid.totalSpeedModifier);
						}
						else
						{
							Invoke("ShootWeapon", 1.25f / eid.totalSpeedModifier);
						}
					}
				}
				else if (altShootCooldown <= 0f && rhit.transform != null && rhit.transform.gameObject.tag == "Breakable")
				{
					predictAmount = 0f;
					aimAtGround = false;
					if (distancePatience >= 4f)
					{
						shootCooldown = 1f;
					}
					else if (difficulty > 2)
					{
						shootCooldown = Random.Range(1f, 2f);
					}
					else
					{
						shootCooldown = 2f;
					}
					altShootCooldown = 5f;
					weapons[currentWeapon].transform.GetChild(0).SendMessage("PrepareAltFire");
					aboutToShoot = true;
					chargingAlt = true;
					Invoke("AltShootWeapon", 1f / eid.totalSpeedModifier);
				}
			}
			if ((bool)eid)
			{
				if (eid.drillers.Count > 0)
				{
					slowMode = true;
					drilled = true;
				}
				else if (drilled)
				{
					slowMode = false;
					drilled = false;
				}
			}
		}
		else if (inIntro)
		{
			if (gc.onGround)
			{
				GameObject gameObject = null;
				if (longIntro)
				{
					rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
					if (!introHitGround)
					{
						anim.SetTrigger("Intro");
						introHitGround = true;
						anim.SetLayerWeight(1, 0f);
						anim.SetLayerWeight(2, 0f);
						gameObject = Object.Instantiate(shockwave, base.transform.position, Quaternion.identity);
					}
				}
				else
				{
					inIntro = false;
					active = true;
					if ((bool)bhb)
					{
						bhb.enabled = true;
					}
					gameObject = Object.Instantiate(shockwave, base.transform.position, Quaternion.identity);
				}
				if ((bool)gameObject && gameObject.TryGetComponent<PhysicalShockwave>(out var component))
				{
					component.enemyType = EnemyType.V2;
				}
			}
			if (staringAtPlayer)
			{
				targetPos = new Vector3(target.position.x, base.transform.position.y, target.position.z);
				targetRot = Quaternion.LookRotation(targetPos - base.transform.position, Vector3.up);
				anim.transform.rotation = Quaternion.RotateTowards(anim.transform.rotation, targetRot, Time.deltaTime * 10f * Quaternion.Angle(anim.transform.rotation, targetRot));
			}
		}
		if (mac.health <= knockOutHealth && knockOutHealth != 0f && firstPhase)
		{
			firstPhase = false;
			KnockedOut();
			eid.totalDamageTakenMultiplier = 0f;
		}
		if (!bhb)
		{
			return;
		}
		if (!enraged)
		{
			bhb.UpdateSecondaryBar(distancePatience / 12f);
		}
		else
		{
			bhb.UpdateSecondaryBar((distancePatience - 10f) / 2f);
		}
		if (enraged)
		{
			flashTimer = Mathf.MoveTowards(flashTimer, 1f, Time.deltaTime * 5f);
			if (flashTimer < 0.5f)
			{
				bhb.SetSecondaryBarColor(Color.red);
			}
			else
			{
				bhb.SetSecondaryBarColor(Color.black);
			}
			if (flashTimer >= 1f)
			{
				flashTimer = 0f;
			}
		}
		else if (distancePatience < 4f)
		{
			bhb.SetSecondaryBarColor(Color.green);
		}
		else if (distancePatience < 8f)
		{
			bhb.SetSecondaryBarColor(Color.yellow);
		}
		else
		{
			bhb.SetSecondaryBarColor(new Color(1f, 0.35f, 0f));
		}
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", 0.1f);
		coins = MonoSingleton<CoinList>.Instance.revolverCoinsList;
	}

	private void ThrowCoins()
	{
		if (coinsToThrow != 0)
		{
			GameObject gameObject = Object.Instantiate(coin, base.transform.position, base.transform.rotation);
			if (gameObject.TryGetComponent<Rigidbody>(out var component))
			{
				component.AddForce((target.transform.position - anim.transform.position).normalized * 20f + Vector3.up * 30f, ForceMode.VelocityChange);
			}
			if (gameObject.TryGetComponent<Coin>(out var component2))
			{
				GameObject obj = Object.Instantiate(component2.flash, component2.transform.position, MonoSingleton<CameraController>.Instance.transform.rotation);
				obj.transform.localScale *= 2f;
				obj.transform.SetParent(gameObject.transform, worldPositionStays: true);
			}
			coinsToThrow--;
			if (coinsToThrow > 0)
			{
				Invoke("ThrowCoins", 0.2f / eid.totalSpeedModifier);
			}
			else
			{
				aboutToShoot = false;
			}
		}
	}

	private void FixedUpdate()
	{
		if (escaping && (bool)nma && nma.isOnNavMesh)
		{
			rb.isKinematic = true;
			nma.SetDestination(escapeTarget.position);
		}
		else if (escaping)
		{
			rb.isKinematic = false;
			targetPos = new Vector3(escapeTarget.position.x, base.transform.position.y, escapeTarget.position.z);
			if (Vector3.Distance(base.transform.position, targetPos) > 8f || (escapeTarget.position.y < base.transform.position.y + 5f && Vector3.Distance(base.transform.position, escapeTarget.position) > 1f))
			{
				aiming = false;
				inPattern = false;
				base.transform.LookAt(targetPos);
				anim.transform.LookAt(targetPos);
				rb.velocity = new Vector3(base.transform.forward.x * movementSpeed, rb.velocity.y, base.transform.forward.z * movementSpeed);
			}
			else
			{
				if (!jumping && gc.onGround && !slideOnly)
				{
					Jump();
				}
				rb.velocity = targetPos - base.transform.position + Vector3.up * 50f;
				GetComponent<Collider>().enabled = false;
			}
			if (base.transform.position.y > escapeTarget.position.y - 20f && spawnOnDeath != null)
			{
				spawnOnDeath.SetActive(value: true);
				spawnOnDeath.transform.position = base.transform.position;
				spawnOnDeath = null;
			}
		}
		else if (active && (inPattern || dodging) && !BlindEnemies.Blind)
		{
			if ((bool)nma && Physics.Raycast(base.transform.position + Vector3.up, target.position - (base.transform.position + Vector3.up), Vector3.Distance(target.position, base.transform.position + Vector3.up), LayerMaskDefaults.Get(LMD.Environment)))
			{
				playerInSight = false;
			}
			else
			{
				playerInSight = true;
			}
			if (running)
			{
				Move();
			}
		}
	}

	private void ShootWeapon()
	{
		if (aiming)
		{
			shootingForCoin = false;
			weapons[currentWeapon].transform.GetChild(0).SendMessage("Fire");
			aboutToShoot = false;
			predictAmount = 0f;
			aimAtGround = false;
		}
	}

	private void AltShootWeapon()
	{
		if (aiming)
		{
			weapons[currentWeapon].transform.GetChild(0).SendMessage("AltFire");
			aboutToShoot = false;
			if (!enraged)
			{
				predictAmount = 0f;
			}
			aimAtGround = false;
			chargingAlt = false;
		}
	}

	private void Move()
	{
		if (BlindEnemies.Blind)
		{
			return;
		}
		if ((bool)nma)
		{
			if (nma.isOnOffMeshLink || (!dodging && !sliding && gc.onGround && !playerInSight))
			{
				nma.enabled = true;
				nma.SetDestination(target.position);
				if (distancePatience > 4f && !enraged)
				{
					nma.speed = movementSpeed * 1.5f;
				}
				else
				{
					nma.speed = movementSpeed;
				}
				return;
			}
			nma.enabled = false;
		}
		rb.isKinematic = false;
		if (dodging)
		{
			rb.velocity = new Vector3(base.transform.forward.x * (movementSpeed * 5f * dodgeLeft), 0f, base.transform.forward.z * (movementSpeed * 5f * dodgeLeft));
		}
		else if (sliding)
		{
			if (slideOnly)
			{
				Vector3 vector = target.position + (base.transform.position - target.position).normalized * 10f;
				Vector3 normalized = new Vector3(vector.x - base.transform.position.x, 0f, vector.z - base.transform.position.z).normalized;
				float num = difficulty;
				rb.velocity = Vector3.MoveTowards(rb.velocity, normalized * movementSpeed * Mathf.Max(1f, num / 1.75f), Time.fixedDeltaTime * 75f);
				Quaternion to = Quaternion.LookRotation(forceSlideDirection, Vector3.up);
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * 360f);
				if (difficulty >= 2)
				{
					if (Vector3.Distance(target.position, base.transform.position) < 8f)
					{
						circleTimer = Mathf.MoveTowards(circleTimer, 1f, Time.deltaTime * eid.totalSpeedModifier);
					}
					else
					{
						circleTimer = Mathf.MoveTowards(circleTimer, 0f, Time.deltaTime * eid.totalSpeedModifier);
					}
					if (circleTimer >= 1f)
					{
						circleTimer = 0.65f;
						ForceDodge((base.transform.position - targetPos).normalized + base.transform.right * Random.Range(-1f, 1f));
					}
				}
			}
			else if (distancePatience > 4f && !enraged)
			{
				rb.velocity = new Vector3(base.transform.forward.x * movementSpeed * 2f, rb.velocity.y, base.transform.forward.z * movementSpeed * 3f);
			}
			else
			{
				rb.velocity = new Vector3(base.transform.forward.x * movementSpeed * 2f, rb.velocity.y, base.transform.forward.z * movementSpeed * 2f);
			}
		}
		else if (gc.onGround)
		{
			if (distancePatience > 4f && !enraged)
			{
				rb.velocity = new Vector3(base.transform.forward.x * movementSpeed, rb.velocity.y, base.transform.forward.z * movementSpeed * 1.5f);
				return;
			}
			float num2 = 1f;
			if (MonoSingleton<NewMovement>.Instance.hp <= 33 && difficulty <= 3)
			{
				num2 -= 0.1f;
			}
			if (Vector3.Distance(base.transform.position, targetPos) < 10f && difficulty <= 2 && distancePatience < 4f)
			{
				num2 -= 0.1f;
			}
			if (slowMode)
			{
				rb.velocity = new Vector3(base.transform.forward.x * movementSpeed, rb.velocity.y, base.transform.forward.z * movementSpeed * num2 * 0.75f);
			}
			else
			{
				rb.velocity = new Vector3(base.transform.forward.x * movementSpeed, rb.velocity.y, base.transform.forward.z * movementSpeed * num2);
			}
		}
		else
		{
			bool flag = Vector3.Distance(base.transform.position, targetPos) < 10f && difficulty <= 2;
			Vector3 vector2 = ((slowMode || (flag && MonoSingleton<NewMovement>.Instance.hp <= 33 && !enraged)) ? ((!(distancePatience < 4f)) ? new Vector3(base.transform.forward.x * movementSpeed * Time.deltaTime * 1.25f, rb.velocity.y, base.transform.forward.z * movementSpeed * Time.deltaTime * 2f) : new Vector3(base.transform.forward.x * movementSpeed * Time.deltaTime * 1.25f, rb.velocity.y, base.transform.forward.z * movementSpeed * Time.deltaTime * 1.25f)) : ((flag && distancePatience < 4f) ? new Vector3(base.transform.forward.x * movementSpeed * Time.deltaTime * 1.25f, rb.velocity.y, base.transform.forward.z * movementSpeed * Time.deltaTime * 2f) : ((!(distancePatience > 4f) || enraged) ? new Vector3(base.transform.forward.x * movementSpeed * Time.deltaTime * 2.5f, rb.velocity.y, base.transform.forward.z * movementSpeed * Time.deltaTime * 2.5f) : new Vector3(base.transform.forward.x * movementSpeed * Time.deltaTime * 3f, rb.velocity.y, base.transform.forward.z * movementSpeed * Time.deltaTime * 3f))));
			Vector3 zero = Vector3.zero;
			if ((vector2.x > 0f && rb.velocity.x < vector2.x) || (vector2.x < 0f && rb.velocity.x > vector2.x))
			{
				zero.x = vector2.x;
			}
			else
			{
				zero.x = 0f;
			}
			if ((vector2.z > 0f && rb.velocity.z < vector2.z) || (vector2.z < 0f && rb.velocity.z > vector2.z))
			{
				zero.z = vector2.z;
			}
			else
			{
				zero.z = 0f;
			}
			rb.AddForce(zero.normalized * airAcceleration);
		}
	}

	private void LateUpdate()
	{
		if (!BlindEnemies.Blind && ((active && aiming) || escaping))
		{
			if (difficulty <= 1)
			{
				predictAmount = 0f;
			}
			Vector3 position = target.position;
			Rigidbody rigidbody = targetRb;
			if (escaping)
			{
				predictAmount = 0f;
				position = escapeTarget.position;
			}
			else if ((bool)overrideTarget)
			{
				predictAmount = 0.05f * (Vector3.Distance(overrideTarget.position, base.transform.position) / 20f);
				position = overrideTarget.position;
				rigidbody = overrideTargetRb;
			}
			else if (Vector3.Distance(base.transform.position, targetPos) < 8f)
			{
				predictAmount *= 0.2f;
			}
			aimAtTarget[0].LookAt(position + rigidbody.velocity * (Vector3.Distance(position, aimAtTarget[0].position) * (predictAmount / 10f)));
			aimAtTarget[0].Rotate(Vector3.right, 10f, Space.Self);
			Quaternion quaternion = ((!aimAtGround) ? Quaternion.LookRotation(position + rigidbody.velocity * predictAmount - aimAtTarget[1].position, Vector3.up) : Quaternion.LookRotation(rigidbody.transform.position + rigidbody.velocity * predictAmount - aimAtTarget[1].position, Vector3.up));
			quaternion = Quaternion.Euler(quaternion.eulerAngles.x + 90f, quaternion.eulerAngles.y, quaternion.eulerAngles.z);
			aimAtTarget[1].rotation = quaternion;
			aimAtTarget[1].Rotate(Vector3.up, 180f, Space.Self);
		}
	}

	private void Jump()
	{
		jumping = true;
		anim.SetLayerWeight(1, 1f);
		anim.SetLayerWeight(2, 0f);
		anim.SetTrigger("Jump");
		Invoke("NotJumping", 0.25f);
		bool flag = slowMode || (Vector3.Distance(base.transform.position, targetPos) < 10f && difficulty <= 2 && MonoSingleton<NewMovement>.Instance.hp <= 33 && !enraged);
		if (sliding)
		{
			Object.Instantiate(jumpSound, base.transform.position, Quaternion.identity);
			if (flag)
			{
				rb.AddForce(Vector3.up * jumpPower * 1500f);
			}
			else
			{
				rb.AddForce(Vector3.up * jumpPower * 1500f * 2f);
			}
			StopSlide();
		}
		else if (dodging)
		{
			Object.Instantiate(dashJumpSound);
			if (flag)
			{
				rb.AddForce(Vector3.up * jumpPower * 1500f * 0.75f);
			}
			else
			{
				rb.AddForce(Vector3.up * jumpPower * 1500f * 1.5f);
			}
		}
		else
		{
			Object.Instantiate(jumpSound, base.transform.position, Quaternion.identity);
			if (flag)
			{
				rb.AddForce(Vector3.up * jumpPower * 1500f * 1.25f);
			}
			else
			{
				rb.AddForce(Vector3.up * jumpPower * 1500f * 2.5f);
			}
		}
	}

	private void WallJump()
	{
		if (sliding)
		{
			StopSlide();
		}
		jumping = true;
		Invoke("NotJumping", 0.25f);
		Object.Instantiate(jumpSound, base.transform.position, Quaternion.identity).GetComponent<AudioSource>().pitch = 2f;
		Vector3 vector = base.transform.position - wc.ClosestPoint();
		rb.velocity = Vector3.zero;
		Vector3 vector2 = new Vector3(vector.normalized.x * 3f, 0.75f, vector.normalized.z * 3f);
		CheckPattern();
		if (currentPattern == 0)
		{
			Quaternion rotation = anim.transform.rotation;
			Vector3 forward = new Vector3(vector.normalized.x, 0f, vector.normalized.z);
			base.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
			anim.transform.rotation = rotation;
			ChangeDirection(Random.Range(-90, 90));
		}
		else if (currentPattern == 1)
		{
			if (pattern1direction < 0)
			{
				pattern1direction = 1;
			}
			else
			{
				pattern1direction = -1;
			}
		}
		else
		{
			Quaternion rotation2 = anim.transform.rotation;
			base.transform.LookAt(targetPos);
			anim.transform.rotation = rotation2;
		}
		float num = 2000f;
		bool flag = slowMode || (Vector3.Distance(base.transform.position, targetPos) < 10f && difficulty <= 2 && MonoSingleton<NewMovement>.Instance.hp <= 33 && !enraged);
		if (difficulty == 1 || flag)
		{
			num = 1000f;
		}
		else if (difficulty == 0)
		{
			num = 500f;
		}
		rb.AddForce(vector2 * wallJumpPower * num);
	}

	private void CheckPattern()
	{
		if (!(patternCooldown <= 0f) || !(distancePatience < 4f) || cowardPattern)
		{
			return;
		}
		int num = currentPattern;
		currentPattern = Random.Range(0, 3);
		if (num == currentPattern)
		{
			patternCooldown = Random.Range(0.5f, 1f);
			if (currentPattern == 1)
			{
				circleTimer += 1f;
			}
		}
		else
		{
			patternCooldown = Random.Range(2, 5);
			SwitchPattern(currentPattern);
		}
		if (currentPattern == 1 && Random.Range(0f, 1f) > 0.5f)
		{
			pattern1direction = -1;
		}
		else
		{
			pattern1direction = 1;
		}
	}

	private void ChangeDirection(float degrees)
	{
		Quaternion rotation = anim.transform.rotation;
		base.transform.Rotate(base.transform.up, degrees, Space.World);
		anim.transform.rotation = rotation;
	}

	public void Dodge(Transform projectile)
	{
		if (!active || !(dodgeLeft <= 0f) || chargingAlt || !(Vector3.Distance(base.transform.position, target.position) > 15f))
		{
			return;
		}
		if (sliding && !slideOnly)
		{
			StopSlide();
		}
		if (dodgeCooldown >= (float)(6 - difficulty))
		{
			dodgeCooldown -= 6 - difficulty;
			dodgeLeft = 1f;
			dodging = true;
			eid.hookIgnore = true;
			inPattern = false;
			Object.Instantiate(dodgeEffect, base.transform.position + Vector3.up * 2f, base.transform.rotation);
			Vector3 vector = new Vector3(base.transform.position.x - projectile.position.x, 0f, base.transform.position.z - projectile.position.z);
			if (currentPattern == 2)
			{
				vector = vector.normalized + (targetPos - base.transform.position).normalized;
			}
			base.transform.LookAt(base.transform.position + vector);
			if (Random.Range(0f, 1f) > 0.5f)
			{
				ChangeDirection(90f);
			}
			else
			{
				ChangeDirection(-90f);
			}
			if (!slideOnly)
			{
				anim.SetTrigger("Jump");
			}
		}
		else
		{
			if (!gc.onGround || jumping || slideOnly)
			{
				return;
			}
			float num = ((difficulty <= 2) ? Random.Range(0f, 3f) : Random.Range(0f, 2f));
			if (num < 1f)
			{
				if (num > 0.75f || cowardPattern)
				{
					Jump();
				}
				else
				{
					Slide();
				}
			}
		}
	}

	public void ForceDodge(Vector3 direction)
	{
		if (sliding && !slideOnly)
		{
			StopSlide();
		}
		dodgeLeft = 1f;
		dodging = true;
		eid.hookIgnore = true;
		inPattern = false;
		Object.Instantiate(dodgeEffect, base.transform.position + Vector3.up * 2f, base.transform.rotation);
		direction = new Vector3(direction.x, 0f, direction.z);
		base.transform.LookAt(base.transform.position + direction);
		if (!slideOnly)
		{
			anim.SetTrigger("Jump");
		}
	}

	private void NotJumping()
	{
		jumping = false;
	}

	private void Slide()
	{
		anim.SetBool("Sliding", value: true);
		sliding = true;
		slideEffect.SetActive(value: true);
		slideStopTimer = 0.2f;
	}

	private void StopSlide()
	{
		sliding = false;
		anim.SetBool("Sliding", value: false);
		slideEffect.SetActive(value: false);
		CheckPattern();
	}

	private void SwitchWeapon(int weapon)
	{
		if (currentWeapon != weapon)
		{
			currentWeapon = weapon;
			GameObject[] array = weapons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			weapons[weapon].SetActive(value: true);
		}
	}

	public void SwitchPattern(int pattern)
	{
		if (currentWingChangeEffect != null)
		{
			Object.Destroy(currentWingChangeEffect);
		}
		wingMaterial.mainTexture = wingTextures[pattern];
		currentWingChangeEffect = Object.Instantiate(wingChangeEffect, base.transform.position + Vector3.up * 2f, Quaternion.identity);
		currentWingChangeEffect.GetComponent<Light>().color = wingColors[pattern];
		TrailRenderer[] array = wingTrails;
		foreach (TrailRenderer trailRenderer in array)
		{
			if ((bool)trailRenderer)
			{
				trailRenderer.startColor = new Color(wingColors[pattern].r, wingColors[pattern].g, wingColors[pattern].b, 0.5f);
			}
		}
		switch (pattern)
		{
		case 0:
			currentWingChangeEffect.GetComponent<AudioSource>().pitch = 1.5f;
			break;
		case 1:
			currentWingChangeEffect.GetComponent<AudioSource>().pitch = 1.25f;
			break;
		}
	}

	public void Die()
	{
		if (!dontDie || dead)
		{
			return;
		}
		dead = true;
		if (!bossVersion)
		{
			EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
			foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in componentsInChildren)
			{
				eid.DeliverDamage(enemyIdentifierIdentifier.gameObject, Vector3.zero, enemyIdentifierIdentifier.transform.position, 10f, tryForExplode: false);
			}
			base.gameObject.SetActive(value: false);
			Object.Destroy(base.gameObject);
		}
		else
		{
			MonoSingleton<MusicManager>.Instance.off = true;
			if (secondEncounter)
			{
				KnockedOut("Flailing");
			}
			else
			{
				KnockedOut();
			}
		}
	}

	public void KnockedOut(string triggerName = "KnockedDown")
	{
		active = false;
		inPattern = false;
		aiming = false;
		inIntro = false;
		anim.transform.LookAt(new Vector3(target.transform.position.x, anim.transform.position.y, target.transform.position.z));
		anim.SetTrigger(triggerName);
		anim.SetLayerWeight(1, 0f);
		anim.SetLayerWeight(2, 0f);
		if (!secondEncounter || !dead)
		{
			rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
		}
		else
		{
			rb.constraints = RigidbodyConstraints.None;
			rb.velocity = new Vector3(0f, 15f, 0f);
			rb.AddTorque(-180f, Random.Range(-35, 35), Random.Range(-35, 35), ForceMode.VelocityChange);
			rb.useGravity = false;
		}
		Object.Instantiate(KoScream, base.transform.position, Quaternion.identity);
		weapons[currentWeapon].transform.GetChild(0).SendMessage("CancelAltCharge", SendMessageOptions.DontRequireReceiver);
		eidids = GetComponentsInChildren<EnemyIdentifierIdentifier>();
		EnemyIdentifierIdentifier[] array = eidids;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].GetComponent<Collider>().enabled = false;
		}
		onKnockout.Invoke();
		Unenrage();
		if ((bool)nma)
		{
			mac.StopKnockBack();
			nma.speed = 25f;
		}
	}

	public void Undie()
	{
		active = true;
		inPattern = true;
		aiming = true;
		eid.totalDamageTakenMultiplier = 1f;
		EnemyIdentifierIdentifier[] array = eidids;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].GetComponent<Collider>().enabled = true;
		}
	}

	public void IntroEnd()
	{
		inIntro = false;
		active = true;
		staringAtPlayer = false;
		EnemyIdentifierIdentifier[] array = eidids;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].GetComponent<Collider>().enabled = true;
		}
		if ((bool)bhb)
		{
			bhb.enabled = true;
		}
		longIntro = false;
		MonoSingleton<StatueIntroChecker>.Instance.beenSeen = true;
	}

	public void StareAtPlayer()
	{
		staringAtPlayer = true;
	}

	public void BeginEscape()
	{
		escaping = true;
		anim.SetLayerWeight(1, 1f);
		anim.SetLayerWeight(2, 0f);
		anim.SetBool("RunningBack", value: false);
		anim.SetBool("InAir", value: false);
		base.transform.LookAt(new Vector3(target.transform.position.x, base.transform.position.y, target.transform.position.z));
		anim.transform.LookAt(new Vector3(target.transform.position.x, anim.transform.position.y, target.transform.position.z));
		if (gc.onGround && (bool)nma && !mac.knockedBack)
		{
			nma.enabled = true;
		}
	}

	public void InstaEnrage()
	{
		distancePatience = 12f;
		Enrage();
	}

	public void Enrage()
	{
		if (!enraged)
		{
			enraged = true;
			currentEnrageEffect = Object.Instantiate(enrageEffect, mac.chest.transform.position, base.transform.rotation);
			currentEnrageEffect.transform.SetParent(mac.chest.transform, worldPositionStays: true);
			EnemySimplifier[] componentsInChildren = GetComponentsInChildren<EnemySimplifier>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enraged = true;
			}
			movementSpeed = originalMovementSpeed * 2f;
		}
	}

	public void Unenrage()
	{
		if (currentEnrageEffect != null)
		{
			Object.Destroy(currentEnrageEffect);
		}
		enraged = false;
		EnemySimplifier[] componentsInChildren = GetComponentsInChildren<EnemySimplifier>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enraged = false;
		}
		movementSpeed = originalMovementSpeed;
	}

	public void SlideOnly(bool value)
	{
		slideOnly = value;
		if (value)
		{
			rb.constraints = (RigidbodyConstraints)116;
			anim.Play("Slide", 0, 0f);
		}
		else
		{
			rb.constraints = RigidbodyConstraints.FreezeRotation;
		}
	}
}
