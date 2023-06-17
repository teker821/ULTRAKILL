using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;

public class ZombieProjectiles : MonoBehaviour
{
	public bool stationary;

	public bool smallRay;

	public bool wanderer;

	public bool afraid;

	public bool chaser;

	public bool hasMelee;

	private Zombie zmb;

	private GameObject player;

	private GameObject camObj;

	private NavMeshAgent nma;

	private NavMeshPath nmp;

	private NavMeshHit hit;

	private Animator anim;

	public Vector3 targetPosition;

	private float coolDown;

	private AudioSource aud;

	public TrailRenderer tr;

	public GameObject projectile;

	private GameObject currentProjectile;

	public Transform shootPos;

	public GameObject head;

	public bool playerSpotted;

	private RaycastHit rhit;

	private RaycastHit bhit;

	public LayerMask lookForPlayerMask;

	public bool seekingPlayer = true;

	private float raySize = 1f;

	private bool musicRequested;

	public GameObject decProjectileSpawner;

	public GameObject decProjectile;

	private GameObject currentDecProjectile;

	public bool swinging;

	[HideInInspector]
	public bool blocking;

	[HideInInspector]
	public int difficulty;

	private float coolDownReduce;

	private EnemyIdentifier eid;

	private GameObject origWP;

	public Transform aimer;

	private bool aiming;

	private Quaternion origRotation;

	private float aimEase;

	private SwingCheck2[] swingChecks;

	private float lengthOfStop;

	private Vector3 spawnPos;

	private bool valuesSet;

	private void SetValues()
	{
		if (!valuesSet)
		{
			valuesSet = true;
			zmb = GetComponent<Zombie>();
			player = MonoSingleton<PlayerTracker>.Instance.GetPlayer().gameObject;
			camObj = MonoSingleton<PlayerTracker>.Instance.GetTarget().gameObject;
			nma = GetComponent<NavMeshAgent>();
			nmp = new NavMeshPath();
			anim = GetComponent<Animator>();
			eid = GetComponent<EnemyIdentifier>();
			if (hasMelee && (swingChecks == null || swingChecks.Length == 0))
			{
				swingChecks = GetComponentsInChildren<SwingCheck2>();
			}
			if (eid.difficultyOverride >= 0)
			{
				difficulty = eid.difficultyOverride;
			}
			else
			{
				difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
			}
			origWP = eid.weakPoint;
			spawnPos = base.transform.position;
			if (stationary || smallRay)
			{
				raySize = 0.25f;
			}
			if (difficulty >= 3)
			{
				coolDownReduce = 1f;
			}
		}
	}

	private void Start()
	{
		SetValues();
		if (!stationary && wanderer && !BlindEnemies.Blind)
		{
			Invoke("Wander", 0.5f);
		}
		SlowUpdate();
	}

	private void OnEnable()
	{
		SetValues();
		if (!musicRequested && playerSpotted && (bool)zmb && !zmb.friendly)
		{
			musicRequested = true;
			zmb.musicRequested = true;
			MusicManager instance = MonoSingleton<MusicManager>.Instance;
			if (instance != null)
			{
				instance.PlayBattleMusic();
			}
		}
		if (hasMelee)
		{
			MeleeDamageEnd();
		}
		if (tr != null)
		{
			tr.emitting = false;
		}
		if (currentDecProjectile != null)
		{
			Object.Destroy(currentDecProjectile);
			eid.weakPoint = origWP;
		}
		swinging = false;
	}

	private void OnDisable()
	{
		if (musicRequested && !zmb.friendly && !zmb.limp)
		{
			musicRequested = false;
			zmb.musicRequested = false;
			MusicManager instance = MonoSingleton<MusicManager>.Instance;
			if (instance != null)
			{
				instance.PlayCleanMusic();
			}
		}
		coolDown = Random.Range(1f, 2.5f) - coolDownReduce;
	}

	private void SlowUpdate()
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (zmb.grounded && (bool)nma && nma.enabled && !zmb.limp && zmb.target != null && !swinging)
			{
				Vector3 vector = zmb.target.position - base.transform.position;
				Vector3 normalized = (zmb.target.position - head.transform.position).normalized;
				float num = Vector3.Distance(zmb.target.position, base.transform.position);
				if (afraid && !swinging && num < 15f)
				{
					nma.updateRotation = true;
					targetPosition = new Vector3(base.transform.position.x + vector.normalized.x * -10f, base.transform.position.y, base.transform.position.z + vector.normalized.z * -10f);
					if (nma.enabled && nma.isOnNavMesh)
					{
						if (NavMesh.SamplePosition(targetPosition, out hit, 1f, nma.areaMask))
						{
							SetDestination(targetPosition);
						}
						else if (NavMesh.FindClosestEdge(targetPosition, out hit, nma.areaMask))
						{
							targetPosition = hit.position;
							SetDestination(targetPosition);
						}
					}
					if (nma.velocity.magnitude < 1f)
					{
						lengthOfStop += 0.5f;
					}
					else
					{
						lengthOfStop = 0f;
					}
				}
				if (num > 15f || lengthOfStop > 0.75f || !afraid)
				{
					lengthOfStop = 0f;
					if (playerSpotted && (!chaser || Vector3.Distance(base.transform.position, zmb.target.position) < 3f || coolDown == 0f) && (Vector3.Distance(base.transform.position, zmb.target.position) < 30f || (Vector3.Distance(base.transform.position, zmb.target.position) < 60f && coolDown == 0f) || stationary || (nmp.status != 0 && (nmp.corners.Length == 0 || Vector3.Distance(base.transform.position, nmp.corners[nmp.corners.Length - 1]) < 3f))) && !Physics.Raycast(head.transform.position, normalized, out bhit, Vector3.Distance(zmb.target.position, head.transform.position), lookForPlayerMask))
					{
						seekingPlayer = false;
						if (!wanderer)
						{
							SetDestination(base.transform.position);
						}
						else if (wanderer && !chaser && coolDown <= 0f)
						{
							SetDestination(base.transform.position);
						}
						if (hasMelee && Vector3.Distance(base.transform.position, zmb.target.position) <= 3f)
						{
							Melee();
						}
						else if (coolDown <= 0f && nma.velocity.magnitude <= 2.5f)
						{
							Swing();
						}
					}
					else if (!stationary)
					{
						if (chaser)
						{
							if (nma == null)
							{
								nma = zmb.nma;
							}
							if (zmb.grounded && nma != null && nma.enabled && zmb.target != null)
							{
								if (Physics.Raycast(zmb.target.position + Vector3.up * 0.1f, Vector3.down, out var hitInfo, float.PositiveInfinity, lookForPlayerMask))
								{
									SetDestination(hitInfo.point);
								}
								else
								{
									SetDestination(zmb.target.position);
								}
							}
						}
						else if ((bool)nma && nma.enabled && nma.isOnNavMesh)
						{
							seekingPlayer = true;
							nma.updateRotation = true;
							if (Physics.Raycast(zmb.target.position + Vector3.up * 0.1f, Vector3.down, out rhit, float.PositiveInfinity, lookForPlayerMask))
							{
								SetDestination(rhit.point);
							}
							else
							{
								SetDestination(zmb.target.position);
							}
						}
					}
				}
			}
			if (stationary && Vector3.Distance(base.transform.position, spawnPos) > 5f)
			{
				stationary = false;
			}
		}
		if (!eid.dead)
		{
			if (chaser)
			{
				Invoke("SlowUpdate", 0.1f);
			}
			else
			{
				Invoke("SlowUpdate", 0.5f);
			}
		}
	}

	private void Update()
	{
		if (!zmb.grounded || !nma.enabled || zmb.limp || !(zmb.target != null))
		{
			return;
		}
		if (coolDown > 0f)
		{
			coolDown = Mathf.MoveTowards(coolDown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (nma.velocity.magnitude <= 2.5f && playerSpotted && !seekingPlayer && (!wanderer || !swinging || chaser))
		{
			anim.SetBool("Running", value: false);
			nma.updateRotation = false;
			base.transform.LookAt(new Vector3(zmb.target.position.x, base.transform.position.y, zmb.target.position.z));
		}
		else if (nma.velocity.magnitude > 2.5f)
		{
			anim.SetBool("Running", value: true);
			nma.updateRotation = true;
		}
		else if (nma.velocity.magnitude <= 2.5f && playerSpotted && !seekingPlayer && wanderer && swinging)
		{
			anim.SetBool("Running", value: false);
			nma.updateRotation = false;
			if (difficulty >= 2)
			{
				Vector3 vector = new Vector3(zmb.target.position.x, base.transform.position.y, zmb.target.position.z);
				Quaternion b = Quaternion.LookRotation((vector - base.transform.position).normalized);
				if (difficulty == 2)
				{
					base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * 3.5f * eid.totalSpeedModifier);
				}
				else if (difficulty == 3)
				{
					base.transform.LookAt(vector);
				}
				else if (difficulty > 3)
				{
					base.transform.LookAt(vector);
				}
			}
		}
		if (playerSpotted)
		{
			return;
		}
		Vector3 normalized = (zmb.target.position - head.transform.position).normalized;
		if (Physics.Raycast(head.transform.position, normalized, out rhit, Vector3.Distance(zmb.target.position, head.transform.position), lookForPlayerMask))
		{
			return;
		}
		seekingPlayer = false;
		playerSpotted = true;
		coolDown = (float)Random.Range(1, 2) - coolDownReduce / 2f;
		if (zmb.target == zmb.player.transform && !musicRequested)
		{
			musicRequested = true;
			zmb.musicRequested = true;
			MusicManager instance = MonoSingleton<MusicManager>.Instance;
			if (instance != null)
			{
				instance.PlayBattleMusic();
			}
		}
	}

	private void LateUpdate()
	{
		if (aimer != null && aiming && (bool)zmb.target)
		{
			Quaternion b = Quaternion.LookRotation((zmb.target.position - aimer.position).normalized);
			if (aimEase < 1f)
			{
				aimEase = Mathf.MoveTowards(aimEase, 1f, Time.deltaTime * (20f - aimEase * 20f) * eid.totalSpeedModifier);
			}
			aimer.rotation = Quaternion.Slerp(origRotation, b, aimEase);
		}
	}

	private void SetDestination(Vector3 position)
	{
		NavMesh.CalculatePath(base.transform.position, position, nma.areaMask, nmp);
		nma.SetPath(nmp);
	}

	public void Melee()
	{
		swinging = true;
		seekingPlayer = false;
		nma.updateRotation = false;
		base.transform.LookAt(new Vector3(zmb.target.position.x, base.transform.position.y, zmb.target.position.z));
		if (nma.enabled)
		{
			nma.isStopped = true;
		}
		if (tr == null)
		{
			tr = GetComponentInChildren<TrailRenderer>();
		}
		tr.GetComponent<AudioSource>().Play();
		anim.SetTrigger("Melee");
	}

	public void MeleePrep()
	{
		zmb.attacking = true;
	}

	public void MeleeDamageStart()
	{
		if (tr == null)
		{
			tr = GetComponentInChildren<TrailRenderer>();
		}
		if (tr != null)
		{
			tr.enabled = true;
			tr.emitting = true;
		}
		SwingCheck2[] array = swingChecks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DamageStart();
		}
	}

	public void MeleeDamageEnd()
	{
		if (tr != null)
		{
			tr.emitting = false;
		}
		SwingCheck2[] array = swingChecks;
		foreach (SwingCheck2 swingCheck in array)
		{
			if ((bool)swingCheck)
			{
				swingCheck.DamageStop();
			}
		}
		zmb.attacking = false;
	}

	public void Swing()
	{
		swinging = true;
		seekingPlayer = false;
		nma.updateRotation = false;
		base.transform.LookAt(new Vector3(zmb.target.position.x, base.transform.position.y, zmb.target.position.z));
		if (nma.enabled)
		{
			nma.isStopped = true;
		}
		if (zmb.target.position.y - 5f > base.transform.position.y || zmb.target.position.y + 5f < base.transform.position.y)
		{
			anim.SetFloat("AttackType", 1f);
		}
		else
		{
			anim.SetFloat("AttackType", Random.Range(0, 2));
		}
		anim.SetTrigger("Swing");
		coolDown = 99f;
	}

	public void SwingEnd()
	{
		swinging = false;
		if (nma.enabled)
		{
			nma.isStopped = false;
		}
		coolDown = Random.Range(1f, 2.5f) - coolDownReduce;
		if (wanderer)
		{
			Wander();
			coolDown = Random.Range(0f, 2f) - coolDownReduce;
		}
		if (blocking)
		{
			coolDown = 0f;
		}
		blocking = false;
		if (tr != null)
		{
			tr.enabled = false;
		}
	}

	public void SpawnProjectile()
	{
		if (swinging)
		{
			currentDecProjectile = Object.Instantiate(decProjectile, decProjectileSpawner.transform.position, decProjectileSpawner.transform.rotation);
			currentDecProjectile.transform.SetParent(decProjectileSpawner.transform, worldPositionStays: true);
			currentDecProjectile.GetComponentInChildren<Breakable>().interruptEnemy = eid;
			eid.weakPoint = currentDecProjectile;
		}
	}

	public void DamageStart()
	{
		if (!hasMelee)
		{
			if (tr == null)
			{
				tr = GetComponentInChildren<TrailRenderer>();
			}
			if (tr != null)
			{
				tr.enabled = true;
			}
		}
		zmb.attacking = true;
		if (aimer != null)
		{
			origRotation = aimer.rotation;
			aiming = true;
		}
	}

	public void ThrowProjectile()
	{
		if (currentDecProjectile != null)
		{
			Object.Destroy(currentDecProjectile);
			eid.weakPoint = origWP;
		}
		currentProjectile = Object.Instantiate(projectile, shootPos.position, base.transform.rotation);
		if (zmb.target == player.transform)
		{
			currentProjectile.transform.LookAt(camObj.transform);
		}
		else if ((bool)zmb.target)
		{
			EnemyIdentifierIdentifier componentInChildren = zmb.target.GetComponentInChildren<EnemyIdentifierIdentifier>();
			if ((bool)componentInChildren)
			{
				currentProjectile.transform.LookAt(componentInChildren.transform);
			}
			else
			{
				currentProjectile.transform.LookAt(zmb.target);
			}
		}
		else
		{
			currentProjectile.transform.rotation = base.transform.rotation;
		}
		Projectile componentInChildren2 = currentProjectile.GetComponentInChildren<Projectile>();
		if (componentInChildren2 != null)
		{
			componentInChildren2.safeEnemyType = EnemyType.Stray;
			if (difficulty > 2)
			{
				componentInChildren2.speed *= 1.35f;
			}
			else if (difficulty == 1)
			{
				componentInChildren2.speed *= 0.75f;
			}
			else if (difficulty == 0)
			{
				componentInChildren2.speed *= 0.5f;
			}
			componentInChildren2.speed *= eid.totalSpeedModifier;
			componentInChildren2.damage *= eid.totalDamageModifier;
		}
		ProjectileSpread componentInChildren3 = currentProjectile.GetComponentInChildren<ProjectileSpread>();
		if ((bool)componentInChildren3 && difficulty <= 2)
		{
			if (difficulty == 2)
			{
				componentInChildren3.spreadAmount = 5f;
			}
			else if (difficulty == 1)
			{
				componentInChildren3.spreadAmount = 3f;
			}
			else if (difficulty == 0)
			{
				componentInChildren3.spreadAmount = 2f;
			}
			componentInChildren3.projectileAmount = 3;
		}
	}

	public void ShootProjectile(int skipOnEasy)
	{
		if (skipOnEasy <= 0 || difficulty >= 2)
		{
			swinging = true;
			if (currentDecProjectile != null)
			{
				Object.Destroy(currentDecProjectile);
				eid.weakPoint = origWP;
			}
			currentProjectile = Object.Instantiate(projectile, decProjectileSpawner.transform.position, decProjectileSpawner.transform.rotation);
			Projectile component = currentProjectile.GetComponent<Projectile>();
			component.safeEnemyType = EnemyType.Schism;
			if (difficulty > 2)
			{
				component.speed *= 1.25f;
			}
			else if (difficulty == 1)
			{
				component.speed *= 0.75f;
			}
			else if (difficulty == 0)
			{
				component.speed *= 0.5f;
			}
			component.speed *= eid.totalSpeedModifier;
			component.damage *= eid.totalDamageModifier;
		}
	}

	public void StopTracking()
	{
	}

	public void DamageEnd()
	{
		if (!hasMelee && tr != null)
		{
			tr.enabled = false;
		}
		if (currentDecProjectile != null)
		{
			Object.Destroy(currentDecProjectile);
			eid.weakPoint = origWP;
		}
		zmb.attacking = false;
		if (aimer != null)
		{
			aimEase = 0f;
			aiming = false;
		}
	}

	public void CancelAttack()
	{
		swinging = false;
		blocking = false;
		aiming = false;
		coolDown = 0f;
		if (currentDecProjectile != null)
		{
			Object.Destroy(currentDecProjectile);
			eid.weakPoint = origWP;
		}
		if (tr != null)
		{
			tr.enabled = false;
		}
		zmb.attacking = false;
	}

	private void Wander()
	{
		if (nma.isOnNavMesh && NavMesh.SamplePosition(Random.onUnitSphere * 10f + base.transform.position, out var navMeshHit, 10f, 1))
		{
			SetDestination(navMeshHit.position);
		}
	}

	public void Block(Vector3 attackPosition)
	{
		if (swinging)
		{
			CancelAttack();
		}
		swinging = true;
		blocking = true;
		aiming = false;
		seekingPlayer = false;
		nma.updateRotation = false;
		base.transform.LookAt(new Vector3(attackPosition.x, base.transform.position.y, attackPosition.z));
		zmb.KnockBack(base.transform.forward * -1f * 500f);
		Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.ineffectiveSound, base.transform.position, Quaternion.identity);
		if (nma.enabled)
		{
			nma.isStopped = true;
		}
		anim.Play("Block", -1, 0f);
	}
}
