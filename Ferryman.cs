using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;

public class Ferryman : MonoBehaviour
{
	private Animator anim;

	private Machine mach;

	private NavMeshAgent nma;

	private Rigidbody rb;

	private GroundCheckEnemy gce;

	private EnemyIdentifier eid;

	private NavMeshPath path;

	private int difficulty = -1;

	private bool inAction;

	private bool tracking;

	private bool moving;

	private float movingSpeed;

	private bool uppercutting;

	private Vector3 playerPos;

	private bool playerApproaching;

	private bool playerRetreating;

	private bool playerAbove;

	private bool playerBelow;

	private float overheadChance = 0.5f;

	private float stingerChance = 0.5f;

	private float kickComboChance = 0.5f;

	[HideInInspector]
	public float defaultMovementSpeed;

	[SerializeField]
	private GameObject parryableFlash;

	[SerializeField]
	private GameObject unparryableFlash;

	[SerializeField]
	private Transform head;

	[SerializeField]
	private GameObject slamExplosion;

	[SerializeField]
	private GameObject lightningBoltWindup;

	private GameObject currentWindup;

	[SerializeField]
	private LightningStrikeExplosive lightningBolt;

	[SerializeField]
	private AudioSource lightningBoltChimes;

	[Header("SwingChecks")]
	[SerializeField]
	private SwingCheck2 mainSwingCheck;

	[SerializeField]
	private SwingCheck2 oarSwingCheck;

	[SerializeField]
	private SwingCheck2 kickSwingCheck;

	private SwingCheck2[] swingChecks;

	[SerializeField]
	private AudioSource swingAudioSource;

	[SerializeField]
	private AudioClip[] swingSounds;

	private bool useMain;

	private bool useOar;

	private bool useKick;

	private bool knockBack;

	[Header("Trails")]
	[SerializeField]
	private TrailRenderer frontTrail;

	[SerializeField]
	private TrailRenderer backTrail;

	[SerializeField]
	private TrailRenderer bodyTrail;

	private bool backTrailActive;

	[Header("Footsteps")]
	[SerializeField]
	private ParticleSystem[] footstepParticles;

	[SerializeField]
	private AudioSource footstepAudio;

	private float rollCooldown;

	private float vaultCooldown;

	[Header("Boss Version")]
	[SerializeField]
	private bool bossVersion;

	[SerializeField]
	private float phaseChangeHealth;

	[SerializeField]
	private Transform[] phaseChangePositions;

	private int currentPosition;

	[SerializeField]
	private UltrakillEvent onPhaseChange;

	private bool inPhaseChange;

	private bool hasPhaseChanged;

	private bool hasReachedFinalPosition;

	private bool jumping;

	private float lightningBoltCooldown;

	private float lightningOutOfReachCharge;

	private bool lightningCancellable;

	private Vector3 lastGroundedPosition;

	private void Start()
	{
		mach = GetComponent<Machine>();
		rb = GetComponent<Rigidbody>();
		gce = GetComponentInChildren<GroundCheckEnemy>();
		path = new NavMeshPath();
		swingChecks = GetComponentsInChildren<SwingCheck2>();
		SetSpeed();
		SlowUpdate();
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
		if (!nma)
		{
			nma = GetComponent<NavMeshAgent>();
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
		if (difficulty == 2)
		{
			anim.speed = 0.9f;
		}
		else if (difficulty == 1)
		{
			anim.speed = 0.8f;
		}
		else if (difficulty == 0)
		{
			anim.speed = 0.6f;
		}
		else
		{
			anim.speed = 1f;
		}
		if (defaultMovementSpeed == 0f)
		{
			defaultMovementSpeed = nma.speed;
		}
		anim.speed *= eid.totalSpeedModifier;
		nma.speed = defaultMovementSpeed * eid.totalSpeedModifier;
	}

	private void OnDisable()
	{
		if (base.gameObject.scene.isLoaded && (bool)currentWindup)
		{
			currentWindup.SetActive(value: false);
		}
		inAction = false;
		tracking = false;
		moving = false;
		uppercutting = false;
		StopDamage();
	}

	private void OnEnable()
	{
		if ((bool)currentWindup)
		{
			currentWindup.SetActive(value: true);
		}
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", 0.1f);
		if (!nma || (!lightningCancellable && !nma.isOnNavMesh) || BlindEnemies.Blind)
		{
			return;
		}
		bool flag = false;
		if (inPhaseChange)
		{
			if (!inAction && nma.isOnNavMesh)
			{
				nma.SetDestination(phaseChangePositions[currentPosition].position);
			}
			return;
		}
		if (!inAction || lightningCancellable)
		{
			RaycastHit hitInfo;
			if (MonoSingleton<PlayerTracker>.Instance.GetOnGround())
			{
				NavMesh.CalculatePath(base.transform.position, PredictPlayerPos(vertical: true), nma.areaMask, path);
			}
			else if (Physics.Raycast(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, Vector3.down, out hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment)))
			{
				if (NavMesh.SamplePosition(hitInfo.point, out var hit, 1f, nma.areaMask))
				{
					NavMesh.CalculatePath(base.transform.position, hit.position, nma.areaMask, path);
				}
				else
				{
					NavMesh.CalculatePath(base.transform.position, hitInfo.point, nma.areaMask, path);
					flag = true;
				}
			}
			if (!inAction && nma.isOnNavMesh)
			{
				nma.path = path;
			}
		}
		if (MonoSingleton<PlayerTracker>.Instance.GetPlayer().position.y > base.transform.position.y + 20f || (path.status != 0 && path.corners != null && path.corners.Length != 0 && (!MonoSingleton<PlayerTracker>.Instance.GetOnGround() || Vector3.Distance(path.corners[path.corners.Length - 1], PredictPlayerPos(vertical: true)) > 5f)))
		{
			flag = true;
		}
		else if (inAction && lightningCancellable)
		{
			CancelLightningBolt();
		}
		if (flag && difficulty >= 2)
		{
			lightningOutOfReachCharge += 0.1f * eid.totalSpeedModifier;
			if (!inAction && lightningOutOfReachCharge > 3f)
			{
				lightningOutOfReachCharge = 0f;
				LightningBolt();
			}
		}
		else
		{
			lightningOutOfReachCharge = 0f;
		}
	}

	private void Update()
	{
		PlayerStatus();
		anim.SetBool("Falling", !gce.onGround);
		if (mach.health < phaseChangeHealth && bossVersion && !hasPhaseChanged)
		{
			PhaseChange();
		}
		if (inPhaseChange && !inAction && !BlindEnemies.Blind)
		{
			if (Vector3.Distance(base.transform.position, phaseChangePositions[currentPosition].position) < 3.5f)
			{
				if (currentPosition < phaseChangePositions.Length - 1)
				{
					currentPosition++;
					nma.destination = phaseChangePositions[currentPosition].position;
				}
				else
				{
					if (!hasReachedFinalPosition)
					{
						base.transform.position = phaseChangePositions[phaseChangePositions.Length - 1].position;
						rb.isKinematic = true;
						rb.useGravity = false;
						hasReachedFinalPosition = true;
					}
					anim.SetBool("Running", value: false);
					if (!inAction && lightningBoltCooldown <= 0f)
					{
						LightningBolt();
					}
					if (!inAction)
					{
						base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(playerPos - base.transform.position), Time.deltaTime * 600f * eid.totalSpeedModifier);
					}
				}
			}
			else if (!nma || !nma.enabled || !nma.isOnNavMesh || !gce.onGround)
			{
				anim.SetBool("Falling", value: true);
				anim.SetBool("Running", value: true);
				rb.isKinematic = false;
				rb.useGravity = true;
				Vector3 vector = new Vector3(phaseChangePositions[currentPosition].position.x, base.transform.position.y, phaseChangePositions[currentPosition].position.z);
				base.transform.position = Vector3.MoveTowards(base.transform.position, vector, Time.deltaTime * Mathf.Max(10f, Vector3.Distance(base.transform.position, vector) * eid.totalSpeedModifier));
			}
			else if (nma.pathStatus != 0 && !jumping)
			{
				anim.SetBool("Falling", value: true);
				anim.SetBool("Running", value: true);
				rb.isKinematic = false;
				rb.useGravity = true;
				nma.enabled = false;
				ParticleSystem[] array = footstepParticles;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Play();
				}
				Footstep();
				base.transform.position += Vector3.up * 5f;
				rb.AddForce(Vector3.up * Mathf.Abs(base.transform.position.y - phaseChangePositions[currentPosition].position.y) * 2f, ForceMode.VelocityChange);
				jumping = true;
				base.transform.rotation = Quaternion.LookRotation(new Vector3(phaseChangePositions[currentPosition].position.x, base.transform.position.y, phaseChangePositions[currentPosition].position.z) - base.transform.position);
			}
			else
			{
				nma.enabled = true;
				anim.SetBool("Running", value: true);
			}
			if (lightningBoltCooldown > 0f)
			{
				lightningBoltCooldown = Mathf.MoveTowards(lightningBoltCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
			}
			return;
		}
		if ((bool)nma && nma.isOnNavMesh && nma.velocity.magnitude > 2f && gce.onGround)
		{
			anim.SetBool("Running", value: true);
		}
		else
		{
			anim.SetBool("Running", value: false);
		}
		if (!inAction && !BlindEnemies.Blind)
		{
			if (gce.onGround)
			{
				if (Vector3.Distance(playerPos, base.transform.position) < 8f && (MonoSingleton<PlayerTracker>.Instance.GetPlayer().position.y > base.transform.position.y + 5f || (MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().y > 5f && !MonoSingleton<PlayerTracker>.Instance.GetOnGround())))
				{
					if (playerRetreating && rollCooldown <= 0f)
					{
						Roll();
					}
					else if (Vector3.Distance(playerPos, base.transform.position) < 5f && MonoSingleton<PlayerTracker>.Instance.GetPlayer().position.y < base.transform.position.y + 20f)
					{
						Uppercut();
					}
				}
				else if (Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, base.transform.position) > 8f || (playerRetreating && MonoSingleton<NewMovement>.Instance.sliding))
				{
					if (vaultCooldown <= 0f && Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, base.transform.position) < 35f && Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, base.transform.position) > 30f && !playerApproaching && MonoSingleton<PlayerTracker>.Instance.GetPlayer().position.y <= base.transform.position.y + 20f)
					{
						vaultCooldown = 2f;
						if (difficulty >= 3)
						{
							VaultSwing();
						}
						else
						{
							Vault();
						}
					}
					else if (Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, base.transform.position) < 14f && playerRetreating && !playerAbove)
					{
						if (Random.Range(0f, 1f) < stingerChance || rollCooldown > 0f)
						{
							stingerChance = Mathf.Min(0.25f, stingerChance - 0.25f);
							Stinger();
						}
						else
						{
							stingerChance = Mathf.Max(0.75f, stingerChance + 0.25f);
							Roll();
						}
					}
				}
				else if (playerApproaching)
				{
					if (Random.Range(0f, 1f) < 0.25f)
					{
						if (Random.Range(0f, 1f) < 0.75f && rollCooldown <= 0f)
						{
							Roll(toPlayerSide: true);
						}
						else if (Random.Range(0f, 1f) < 0.5f)
						{
							KickCombo();
						}
						else
						{
							OarCombo();
						}
					}
					else if (Random.Range(0f, 1f) < overheadChance)
					{
						overheadChance = Mathf.Min(0.25f, overheadChance - 0.25f);
						Downslam();
					}
					else
					{
						overheadChance = Mathf.Max(0.75f, overheadChance + 0.25f);
						BackstepAttack();
					}
				}
				else if (Random.Range(0f, 1f) < kickComboChance)
				{
					kickComboChance = Mathf.Min(0.25f, kickComboChance - 0.25f);
					KickCombo();
				}
				else
				{
					kickComboChance = Mathf.Max(0.75f, kickComboChance + 0.25f);
					OarCombo();
				}
			}
		}
		else
		{
			nma.enabled = false;
			if (tracking && !BlindEnemies.Blind)
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(PredictPlayerPos() - base.transform.position), Time.deltaTime * 600f * eid.totalSpeedModifier);
			}
			if (moving)
			{
				if (Physics.Raycast(base.transform.position + Vector3.up + base.transform.forward, Vector3.down, out var _, Mathf.Max(22f, base.transform.position.y - MonoSingleton<PlayerTracker>.Instance.GetPlayer().position.y + 2.5f), LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
				{
					rb.velocity = base.transform.forward * movingSpeed * anim.speed;
				}
				else
				{
					rb.velocity = Vector3.zero;
				}
			}
			if (uppercutting)
			{
				Vector3 velocity = Vector3.up * 100f * anim.speed;
				if (Vector3.Distance(base.transform.position, playerPos) > 5f)
				{
					velocity += base.transform.forward * Mathf.Min(100f, Vector3.Distance(base.transform.position, playerPos) * 40f) * anim.speed;
				}
				if (Physics.Raycast(lastGroundedPosition + Vector3.up + base.transform.forward, Vector3.down, out var _, Mathf.Max(22f, base.transform.position.y - MonoSingleton<PlayerTracker>.Instance.GetPlayer().position.y + 2.5f), LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
				{
					rb.velocity = velocity;
				}
				else
				{
					rb.velocity = Vector3.up * 100f * anim.speed;
				}
			}
		}
		if (rollCooldown > 0f)
		{
			rollCooldown = Mathf.MoveTowards(rollCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (vaultCooldown > 0f)
		{
			vaultCooldown = Mathf.MoveTowards(vaultCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
	}

	private void FixedUpdate()
	{
		if (gce.onGround && !moving && !uppercutting && !jumping)
		{
			nma.enabled = !inAction;
			rb.useGravity = false;
			rb.isKinematic = true;
		}
		else if (!gce.onGround && !inAction)
		{
			rb.useGravity = true;
			rb.isKinematic = false;
			nma.enabled = false;
			jumping = false;
			if ((bool)rb)
			{
				rb.AddForce(Vector3.down * 20f * Time.fixedDeltaTime, ForceMode.VelocityChange);
			}
		}
		if (gce.onGround)
		{
			lastGroundedPosition = base.transform.position;
		}
	}

	private void Downslam()
	{
		SnapToGround();
		inAction = true;
		tracking = true;
		if (nma.isOnNavMesh)
		{
			nma.SetDestination(base.transform.position);
		}
		anim.SetTrigger("Downslam");
		backTrailActive = false;
		useMain = true;
		useOar = true;
		useKick = false;
	}

	private void BackstepAttack()
	{
		SnapToGround();
		inAction = true;
		if (nma.isOnNavMesh)
		{
			nma.SetDestination(base.transform.position);
		}
		anim.SetTrigger("BackstepAttack");
		backTrailActive = true;
		StartMoving(-3.5f);
		knockBack = true;
		useMain = true;
		useOar = true;
		useKick = false;
	}

	private void Stinger()
	{
		SnapToGround();
		inAction = true;
		tracking = true;
		if (nma.isOnNavMesh)
		{
			nma.SetDestination(base.transform.position);
		}
		anim.SetTrigger("Stinger");
		backTrailActive = true;
		useMain = true;
		useOar = true;
		useKick = false;
	}

	private void Vault()
	{
		SnapToGround();
		bodyTrail.emitting = true;
		inAction = true;
		tracking = true;
		StartMoving(0.5f);
		anim.SetTrigger("Vault");
		backTrailActive = false;
		useMain = false;
		useOar = false;
		useKick = true;
	}

	private void VaultSwing()
	{
		SnapToGround();
		inAction = true;
		tracking = true;
		StartMoving(0.5f);
		anim.SetTrigger("VaultSwing");
		backTrailActive = true;
		useMain = true;
		useOar = true;
		useKick = false;
	}

	private void KickCombo()
	{
		SnapToGround();
		inAction = true;
		tracking = true;
		if (nma.isOnNavMesh)
		{
			nma.SetDestination(base.transform.position);
		}
		anim.SetTrigger("KickCombo");
		useMain = true;
		useOar = false;
		useKick = true;
	}

	private void OarCombo()
	{
		SnapToGround();
		inAction = true;
		tracking = true;
		if (nma.isOnNavMesh)
		{
			nma.SetDestination(base.transform.position);
		}
		anim.SetTrigger("OarCombo");
		backTrailActive = true;
		useMain = true;
		useOar = true;
		useKick = false;
	}

	private void Uppercut()
	{
		SnapToGround();
		inAction = true;
		tracking = true;
		if (nma.isOnNavMesh)
		{
			nma.SetDestination(base.transform.position);
		}
		anim.SetTrigger("Uppercut");
		backTrailActive = true;
		useMain = true;
		useOar = true;
		useKick = false;
	}

	public void Roll(bool toPlayerSide = false)
	{
		SnapToGround();
		inAction = true;
		tracking = false;
		if (nma.isOnNavMesh)
		{
			nma.SetDestination(base.transform.position);
		}
		nma.enabled = false;
		anim.SetTrigger("Roll");
		bodyTrail.emitting = true;
		if (!toPlayerSide)
		{
			base.transform.rotation = Quaternion.LookRotation(PredictPlayerPos(vertical: false, 20f) - base.transform.position);
		}
		else
		{
			float num = 5f;
			if (Random.Range(0f, 1f) > 0.5f)
			{
				num = -5f;
			}
			base.transform.rotation = Quaternion.LookRotation(playerPos + MonoSingleton<CameraController>.Instance.transform.right * num - base.transform.position);
		}
		StartMoving(5f);
		if (difficulty < 3)
		{
			rollCooldown = 5.5f - (float)(difficulty * 2);
		}
	}

	public void LightningBolt()
	{
		inAction = true;
		lightningBoltCooldown = 8 - difficulty * 2;
		anim.SetTrigger("LightningBolt");
		tracking = true;
		lightningCancellable = true;
	}

	public void LightningBoltWindup()
	{
		if (eid.dead)
		{
			return;
		}
		currentWindup = Object.Instantiate(lightningBoltWindup, PredictPlayerPos(), Quaternion.identity);
		if ((bool)base.transform.parent)
		{
			currentWindup.transform.SetParent(base.transform.parent, worldPositionStays: true);
		}
		Follow[] components = currentWindup.GetComponents<Follow>();
		foreach (Follow follow in components)
		{
			if (follow.speed != 0f)
			{
				if (difficulty >= 2)
				{
					follow.speed *= difficulty;
				}
				else if (difficulty == 1)
				{
					follow.speed /= 2f;
				}
				else
				{
					follow.enabled = false;
				}
				follow.speed *= eid.totalSpeedModifier;
			}
		}
		tracking = false;
		lightningBoltChimes.Play();
	}

	public void LightningBoltWindupOver()
	{
		if ((bool)currentWindup)
		{
			SpawnLightningBolt(currentWindup.transform.position);
			Object.Destroy(currentWindup);
			lightningCancellable = false;
		}
	}

	public void SpawnLightningBolt(Vector3 position, bool safeForPlayer = false)
	{
		LightningStrikeExplosive lightningStrikeExplosive = Object.Instantiate(lightningBolt, position, Quaternion.identity);
		lightningStrikeExplosive.safeForPlayer = safeForPlayer;
		lightningStrikeExplosive.damageMultiplier = eid.totalDamageModifier;
		if ((bool)base.transform.parent)
		{
			lightningStrikeExplosive.transform.SetParent(base.transform.parent, worldPositionStays: true);
		}
	}

	public void CancelLightningBolt()
	{
		Debug.Log("Cancelling Lightning Bolt");
		if ((bool)currentWindup)
		{
			Object.Destroy(currentWindup);
		}
		lightningCancellable = false;
		anim.Play("Idle");
		StopAction();
	}

	public void OnDeath()
	{
		Object.Destroy(this);
	}

	private void StartTracking()
	{
		tracking = true;
	}

	private void StopTracking()
	{
		tracking = false;
	}

	private void StartMoving(float speed)
	{
		movingSpeed = speed * 10f;
		moving = true;
		rb.isKinematic = false;
		ParticleSystem[] array = footstepParticles;
		foreach (ParticleSystem particleSystem in array)
		{
			if (Mathf.Abs(particleSystem.transform.position.y - base.transform.position.y) < 1f)
			{
				particleSystem.Play();
			}
		}
		Footstep(0.75f);
	}

	private void StopMoving()
	{
		bodyTrail.emitting = false;
		moving = false;
		rb.isKinematic = true;
		ParticleSystem[] array = footstepParticles;
		foreach (ParticleSystem particleSystem in array)
		{
			if (Mathf.Abs(particleSystem.transform.position.y - base.transform.position.y) < 1f)
			{
				particleSystem.Play();
			}
		}
		Footstep(0.75f);
	}

	public void SlamHit()
	{
		Object.Instantiate(slamExplosion, new Vector3(frontTrail.transform.position.x, base.transform.position.y, frontTrail.transform.position.z), Quaternion.identity);
		ParticleSystem[] array = footstepParticles;
		foreach (ParticleSystem particleSystem in array)
		{
			if (Mathf.Abs(particleSystem.transform.position.y - base.transform.position.y) < 1f)
			{
				particleSystem.Play();
			}
		}
		Footstep(0.75f);
	}

	private void Footstep(float volume = 0.5f)
	{
		if (volume == 0f)
		{
			volume = 0.5f;
		}
		footstepAudio.volume = volume;
		footstepAudio.pitch = Random.Range(1.15f, 1.35f);
		footstepAudio.Play();
	}

	private void StartUppercut()
	{
		uppercutting = true;
		rb.isKinematic = false;
		StartDamage();
	}

	private void StopUppercut()
	{
		uppercutting = false;
		rb.useGravity = true;
		rb.velocity = Vector3.up * 10f;
		StopDamage();
	}

	private void StartDamage(int damage = 25)
	{
		if (damage == 0)
		{
			damage = 25;
		}
		SwingCheck2[] array = swingChecks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].damage = damage;
		}
		if (useMain)
		{
			mainSwingCheck.DamageStart();
		}
		if (useOar)
		{
			oarSwingCheck.DamageStart();
		}
		if (useKick)
		{
			kickSwingCheck.DamageStart();
		}
		if (useOar)
		{
			swingAudioSource.pitch = Random.Range(0.65f, 0.9f);
			swingAudioSource.volume = 1f;
		}
		else if (useKick)
		{
			swingAudioSource.pitch = Random.Range(2.1f, 2.55f);
			swingAudioSource.volume = 0.75f;
		}
		swingAudioSource.clip = swingSounds[Random.Range(0, swingSounds.Length)];
		swingAudioSource.Play();
		if (useMain || useOar)
		{
			frontTrail.emitting = true;
		}
		if (backTrailActive)
		{
			backTrail.emitting = true;
		}
	}

	private void StopDamage()
	{
		if (swingChecks != null)
		{
			SwingCheck2[] array = swingChecks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DamageStop();
			}
			knockBack = false;
			frontTrail.emitting = false;
			backTrail.emitting = false;
			mach.parryable = false;
		}
	}

	private void PlayerBeenHit()
	{
		if (knockBack)
		{
			Debug.Log("Knockback");
			MonoSingleton<NewMovement>.Instance.Launch((playerPos - base.transform.position).normalized * 2500000f + Vector3.up * 250000f);
		}
		SwingCheck2[] array = swingChecks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DamageStop();
		}
	}

	private void StopAction()
	{
		if (moving)
		{
			StopMoving();
		}
		inAction = false;
		nma.enabled = true;
		tracking = false;
		if (bodyTrail.emitting)
		{
			bodyTrail.emitting = false;
		}
	}

	public void ParryableFlash()
	{
		Object.Instantiate(parryableFlash, head.position + (MonoSingleton<CameraController>.Instance.defaultPos - head.position).normalized, Quaternion.LookRotation(MonoSingleton<CameraController>.Instance.defaultPos - head.position), head).transform.localScale *= 0.025f;
		mach.parryable = true;
	}

	public void UnparryableFlash()
	{
		Object.Instantiate(unparryableFlash, head.position + (MonoSingleton<CameraController>.Instance.defaultPos - head.position).normalized, Quaternion.LookRotation(MonoSingleton<CameraController>.Instance.defaultPos - head.position), head).transform.localScale *= 0.025f;
	}

	public void GotParried()
	{
		SpawnLightningBolt(mach.chest.transform.position, safeForPlayer: true);
	}

	private void PlayerStatus()
	{
		playerPos = new Vector3(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position.x, base.transform.position.y, MonoSingleton<PlayerTracker>.Instance.GetPlayer().position.z);
		playerAbove = MonoSingleton<PlayerTracker>.Instance.GetPlayer().position.y > base.transform.position.y + 3f;
		playerBelow = MonoSingleton<PlayerTracker>.Instance.GetPlayer().position.y < base.transform.position.y - 4f;
		Vector3 vector = new Vector3(MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().x, 0f, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().z);
		if (vector.magnitude < 1f)
		{
			playerApproaching = false;
			playerRetreating = false;
		}
		else
		{
			float num = Mathf.Abs(Vector3.Angle(vector.normalized, playerPos - base.transform.position));
			playerRetreating = num < 80f;
			playerApproaching = num > 135f;
		}
	}

	private Vector3 PredictPlayerPos(bool vertical = false, float maxPrediction = 5f)
	{
		if (vertical)
		{
			if (difficulty <= 1)
			{
				return MonoSingleton<PlayerTracker>.Instance.GetPlayer().position;
			}
			return MonoSingleton<PlayerTracker>.Instance.GetPlayer().position + MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().normalized * Mathf.Min(MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude, 5f) + ((MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer) ? Vector3.down : Vector3.zero);
		}
		if (difficulty <= 1)
		{
			return playerPos;
		}
		Vector3 vector = new Vector3(MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().x, 0f, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().z);
		return playerPos + vector.normalized * Mathf.Min(vector.magnitude, maxPrediction);
	}

	private void SnapToGround()
	{
		if (!nma.isOnNavMesh && gce.onGround && Physics.Raycast(base.transform.position + Vector3.up * 0.1f, Vector3.down, out var hitInfo, 2f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			base.transform.position = hitInfo.point;
		}
		base.transform.rotation = Quaternion.LookRotation(playerPos - base.transform.position);
	}

	public void PhaseChange()
	{
		inPhaseChange = true;
		onPhaseChange.Invoke();
	}

	public void EndPhaseChange()
	{
		inPhaseChange = false;
		hasPhaseChanged = true;
		if (!hasReachedFinalPosition)
		{
			base.transform.position = phaseChangePositions[phaseChangePositions.Length - 1].position;
		}
	}
}
