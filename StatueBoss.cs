using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;

public class StatueBoss : MonoBehaviour
{
	private Animator anim;

	private NavMeshAgent nma;

	public GameObject player;

	private CameraController cc;

	private Rigidbody rb;

	private AudioSource aud;

	public bool inAction;

	public bool friendly;

	private Transform target;

	public Transform stompPos;

	public GameObject stompWave;

	private bool tracking;

	private bool dashing;

	private float dashPower;

	private GameObject currentStompWave;

	private float meleeRecharge = 1f;

	private float playerInCloseRange;

	public bool damaging;

	public bool launching;

	public int damage;

	private int tackleChance = 50;

	private float rangedRecharge = 1f;

	private int throwChance = 50;

	public float attackCheckCooldown = 1f;

	public GameObject orbProjectile;

	private Light orbLight;

	private Vector3 projectedPlayerPos;

	private bool orbGrowing;

	private float origRange;

	public GameObject stepSound;

	private ParticleSystem part;

	private AudioSource partAud;

	private Statue st;

	public GameObject backUp;

	public GameObject statueChargeSound;

	public GameObject statueChargeSound2;

	public GameObject statueChargeSound3;

	public bool enraged;

	public GameObject enrageEffect;

	public GameObject currentEnrageEffect;

	private EnemySimplifier[] ensims;

	private int difficulty = -1;

	public LayerMask lmask;

	private SwingCheck2 swingCheck;

	private GroundCheckEnemy gc;

	private EnemyIdentifier eid;

	private Collider enemyCollider;

	private void Start()
	{
		player = MonoSingleton<PlayerTracker>.Instance.GetPlayer().gameObject;
		cc = MonoSingleton<CameraController>.Instance;
		rb = GetComponentInChildren<Rigidbody>();
		aud = GetComponent<AudioSource>();
		part = base.transform.Find("DodgeParticle").GetComponent<ParticleSystem>();
		partAud = part.GetComponent<AudioSource>();
		st = GetComponent<Statue>();
		orbLight = GetComponentInChildren<Light>();
		origRange = orbLight.range;
		if (!friendly)
		{
			target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
		}
		SetSpeed();
		gc = GetComponentInChildren<GroundCheckEnemy>();
		enemyCollider = GetComponent<Collider>();
		if (inAction)
		{
			StopAction();
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
			nma = GetComponentInChildren<NavMeshAgent>();
		}
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
		if (difficulty > 2)
		{
			anim.speed = 1.2f;
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
		anim.speed *= eid.totalSpeedModifier;
		if (enraged)
		{
			if (difficulty <= 2)
			{
				anim.speed *= 1.2f;
			}
			else if (difficulty > 3)
			{
				anim.speed = 1.5f * eid.totalSpeedModifier;
			}
			else
			{
				anim.speed = 1.25f * eid.totalSpeedModifier;
			}
			anim.SetFloat("WalkSpeed", 1.5f);
		}
		if (nma.enabled)
		{
			nma.speed = (enraged ? 25 : 5);
			nma.acceleration *= (enraged ? 120 : 24);
			nma.angularSpeed *= (enraged ? 6000 : 1200);
		}
	}

	private void OnEnable()
	{
		if ((bool)st)
		{
			StopAction();
			StopDamage();
			StopDash();
		}
	}

	private void OnDisable()
	{
		if (currentStompWave != null)
		{
			Object.Destroy(currentStompWave);
		}
	}

	private void Update()
	{
		if (BlindEnemies.Blind)
		{
			return;
		}
		Vector3 vector = new Vector3(target.position.x, base.transform.position.y, target.position.z);
		if (!inAction && nma.isOnNavMesh)
		{
			if (Vector3.Distance(vector, base.transform.position) > 3f)
			{
				nma.SetDestination(target.position);
			}
			else
			{
				nma.SetDestination(base.transform.position);
				base.transform.LookAt(vector);
			}
			if (nma.velocity.magnitude > 1f)
			{
				anim.SetBool("Walking", value: true);
			}
			else
			{
				anim.SetBool("Walking", value: false);
			}
		}
		if (attackCheckCooldown > 0f)
		{
			attackCheckCooldown = Mathf.MoveTowards(attackCheckCooldown, 0f, Time.deltaTime);
		}
		if (!inAction && gc.onGround && attackCheckCooldown <= 0f)
		{
			attackCheckCooldown = 0.2f;
			if (!Physics.Raycast(st.chest.transform.position, target.position - st.chest.transform.position, Vector3.Distance(target.position, st.chest.transform.position), LayerMaskDefaults.Get(LMD.Environment)))
			{
				if (meleeRecharge >= 2f || (meleeRecharge >= 1f && Vector3.Distance(base.transform.position, vector) < 15f && (Mathf.Abs(base.transform.position.y - player.transform.position.y) < 9f || (Mathf.Abs(MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().y) > 2f && Mathf.Abs(base.transform.position.y - player.transform.position.y) < 19f))))
				{
					int num = Random.Range(0, 100);
					meleeRecharge = 0f;
					if (target.transform.position.y < base.transform.position.y + 5f && num > tackleChance)
					{
						if (tackleChance < 50)
						{
							tackleChance = 50;
						}
						tackleChance += 20;
						inAction = true;
						Stomp();
					}
					else
					{
						if (tackleChance > 50)
						{
							tackleChance = 50;
						}
						tackleChance -= 20;
						inAction = true;
						Tackle();
					}
				}
				else if (rangedRecharge >= 1f && Vector3.Distance(base.transform.position, vector) >= 9f)
				{
					rangedRecharge = 0f;
					inAction = true;
					Throw();
				}
			}
		}
		if (tracking)
		{
			base.transform.LookAt(new Vector3(target.position.x, base.transform.position.y, target.position.z));
		}
		if (backUp != null && st.health < 40f)
		{
			backUp.SetActive(value: true);
			backUp = null;
		}
		if (difficulty > 3 && !enraged && st.health < 20f)
		{
			enraged = true;
			Invoke("EnrageNow", 1f);
		}
		if (orbGrowing)
		{
			orbLight.range = Mathf.MoveTowards(orbLight.range, origRange, Time.deltaTime * 20f * eid.totalSpeedModifier);
			if (orbLight.range == origRange)
			{
				orbGrowing = false;
			}
		}
		if (rangedRecharge < 1f)
		{
			float num2 = 1f;
			if (Vector3.Distance(base.transform.position, vector) < 15f)
			{
				num2 = 0.5f;
			}
			if (difficulty == 1)
			{
				num2 -= 0.2f;
			}
			else if (difficulty == 0)
			{
				num2 -= 0.35f;
			}
			num2 *= eid.totalSpeedModifier;
			if (enraged)
			{
				rangedRecharge = Mathf.MoveTowards(rangedRecharge, 1f, Time.deltaTime * 0.4f * num2);
			}
			else if (st.health < 60f)
			{
				rangedRecharge = Mathf.MoveTowards(rangedRecharge, 1f, Time.deltaTime * 0.15f * num2);
			}
			else if (difficulty > 3)
			{
				rangedRecharge = Mathf.MoveTowards(rangedRecharge, 1f, Time.deltaTime * 0.32f * num2);
			}
			else if (difficulty == 3)
			{
				rangedRecharge = Mathf.MoveTowards(rangedRecharge, 1f, Time.deltaTime * 0.285f * num2);
			}
			else
			{
				rangedRecharge = Mathf.MoveTowards(rangedRecharge, 1f, Time.deltaTime * 0.275f * num2);
			}
		}
		if (!(meleeRecharge < 1f))
		{
			return;
		}
		float num3 = 1f;
		if (Vector3.Distance(base.transform.position, vector) < 9f)
		{
			playerInCloseRange = Mathf.MoveTowards(playerInCloseRange, 1f, Time.deltaTime);
			if (playerInCloseRange >= 1f)
			{
				num3 = 2f;
			}
		}
		else
		{
			playerInCloseRange = Mathf.MoveTowards(playerInCloseRange, 0f, Time.deltaTime);
		}
		if (difficulty == 1)
		{
			num3 -= 0.25f;
		}
		else if (difficulty == 0)
		{
			num3 -= 0.5f;
		}
		num3 *= eid.totalSpeedModifier;
		if (enraged)
		{
			if (meleeRecharge < 1f && difficulty >= 2)
			{
				meleeRecharge = 1f;
			}
			else
			{
				meleeRecharge = Mathf.MoveTowards(meleeRecharge, 2f, Time.deltaTime * 0.4f);
			}
		}
		else if (st.health < 60f)
		{
			meleeRecharge = Mathf.MoveTowards(meleeRecharge, 2f, Time.deltaTime * 0.25f * num3);
		}
		else if (difficulty > 3)
		{
			meleeRecharge = Mathf.MoveTowards(meleeRecharge, 2f, Time.deltaTime * 0.4f * num3);
		}
		else if (difficulty == 3)
		{
			meleeRecharge = Mathf.MoveTowards(meleeRecharge, 2f, Time.deltaTime * 0.385f * num3);
		}
		else
		{
			meleeRecharge = Mathf.MoveTowards(meleeRecharge, 2f, Time.deltaTime * 0.375f * num3);
		}
	}

	private void FixedUpdate()
	{
		if (dashPower > 1f)
		{
			RaycastHit hitInfo;
			bool flag = enraged || Physics.Raycast(base.transform.position + Vector3.up + base.transform.forward, Vector3.down, out hitInfo, 12f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore);
			Vector3 vector = new Vector3(0f, rb.velocity.y, 0f);
			if (difficulty > 2)
			{
				rb.velocity = (flag ? new Vector3(base.transform.forward.x * dashPower * eid.totalSpeedModifier, rb.velocity.y, base.transform.forward.z * dashPower * eid.totalSpeedModifier) : vector);
				dashPower /= 1.075f;
			}
			else if (difficulty == 2)
			{
				rb.velocity = (flag ? new Vector3(base.transform.forward.x * dashPower / 1.25f * eid.totalSpeedModifier, rb.velocity.y, base.transform.forward.z * dashPower / 1.25f * eid.totalSpeedModifier) : vector);
				dashPower /= 1.065625f;
			}
			else if (difficulty == 1)
			{
				rb.velocity = (flag ? new Vector3(base.transform.forward.x * dashPower / 1.5f * eid.totalSpeedModifier, rb.velocity.y, base.transform.forward.z * dashPower / 1.5f * eid.totalSpeedModifier) : vector);
				dashPower /= 1.05625f;
			}
			else
			{
				rb.velocity = (flag ? new Vector3(base.transform.forward.x * dashPower / 2f * eid.totalSpeedModifier, rb.velocity.y, base.transform.forward.z * dashPower / 2f * eid.totalSpeedModifier) : vector);
				dashPower /= 1.0375f;
			}
			if (rb.velocity.y > 0f)
			{
				rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
			}
		}
		else if (dashPower != 0f)
		{
			rb.velocity = Vector3.zero;
			dashPower = 0f;
			damaging = false;
		}
	}

	private void Stomp()
	{
		nma.updatePosition = false;
		nma.updateRotation = false;
		nma.enabled = false;
		base.transform.LookAt(new Vector3(target.position.x, base.transform.position.y, target.position.z));
		anim.SetTrigger("Stomp");
		launching = false;
		Object.Instantiate(statueChargeSound, base.transform.position, Quaternion.identity);
	}

	private void Tackle()
	{
		nma.updatePosition = false;
		nma.updateRotation = false;
		nma.enabled = false;
		base.transform.LookAt(new Vector3(target.position.x, base.transform.position.y, target.position.z));
		tracking = true;
		anim.SetTrigger("Tackle");
		damage = 25;
		launching = true;
		Object.Instantiate(statueChargeSound3, base.transform.position, Quaternion.identity);
	}

	private void Throw()
	{
		nma.updatePosition = false;
		nma.updateRotation = false;
		nma.enabled = false;
		base.transform.LookAt(new Vector3(target.position.x, base.transform.position.y, target.position.z));
		tracking = true;
		anim.SetTrigger("Throw");
		Object.Instantiate(statueChargeSound2, base.transform.position, Quaternion.identity);
	}

	public void StompHit()
	{
		cc.CameraShake(1f);
		if (currentStompWave != null)
		{
			Object.Destroy(currentStompWave);
		}
		currentStompWave = Object.Instantiate(stompWave, new Vector3(stompPos.position.x, base.transform.position.y, stompPos.position.z), Quaternion.identity);
		PhysicalShockwave component = currentStompWave.GetComponent<PhysicalShockwave>();
		component.damage = 25;
		if (difficulty > 2)
		{
			component.speed = 50f;
		}
		else if (difficulty == 2)
		{
			component.speed = 35f;
		}
		else if (difficulty == 1)
		{
			component.speed = 25f;
		}
		else if (difficulty == 0)
		{
			component.speed = 15f;
		}
		component.speed *= eid.totalSpeedModifier;
		component.damage = Mathf.RoundToInt((float)component.damage * eid.totalDamageModifier);
		component.maxSize = 100f;
		component.enemy = true;
		component.enemyType = EnemyType.Cerberus;
	}

	public void OrbSpawn()
	{
		GameObject gameObject = Object.Instantiate(orbProjectile, orbLight.transform.position, Quaternion.identity);
		gameObject.transform.LookAt(projectedPlayerPos);
		if (difficulty > 2)
		{
			gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * 20000f * eid.totalSpeedModifier);
		}
		else if (difficulty == 2)
		{
			gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * 15000f * eid.totalSpeedModifier);
		}
		else
		{
			gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * 10000f * eid.totalSpeedModifier);
		}
		if ((difficulty <= 2 || eid.totalDamageModifier != 1f) && gameObject.TryGetComponent<Projectile>(out var component))
		{
			if (difficulty <= 2)
			{
				component.bigExplosion = false;
			}
			component.damage *= eid.totalDamageModifier;
		}
		orbGrowing = false;
		orbLight.range = 0f;
		part.Play();
	}

	public void OrbRespawn()
	{
		orbGrowing = true;
	}

	public void StopAction()
	{
		if (gc.onGround)
		{
			nma.updatePosition = true;
			nma.updateRotation = true;
			nma.enabled = true;
		}
		tracking = false;
		inAction = false;
	}

	public void StopTracking()
	{
		tracking = false;
		if (MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude == 0f)
		{
			base.transform.LookAt(new Vector3(target.position.x, base.transform.position.y, target.position.z));
			projectedPlayerPos = target.position;
			return;
		}
		if (Physics.Raycast(target.position, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(), out var hitInfo, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude * 0.35f / eid.totalSpeedModifier, 4096, QueryTriggerInteraction.Collide) && hitInfo.collider == enemyCollider)
		{
			projectedPlayerPos = target.position;
		}
		else if (Physics.Raycast(target.position, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(), out hitInfo, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude * 0.35f / eid.totalSpeedModifier, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), QueryTriggerInteraction.Collide))
		{
			projectedPlayerPos = hitInfo.point;
		}
		else
		{
			projectedPlayerPos = target.position + MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity() * 0.35f / eid.totalSpeedModifier;
			projectedPlayerPos = new Vector3(projectedPlayerPos.x, target.transform.position.y + (target.transform.position.y - projectedPlayerPos.y) * 0.5f, projectedPlayerPos.z);
		}
		base.transform.LookAt(new Vector3(projectedPlayerPos.x, base.transform.position.y, projectedPlayerPos.z));
	}

	public void Dash()
	{
		rb.velocity = Vector3.zero;
		dashPower = 200f;
		rb.isKinematic = false;
		damaging = true;
		part.Play();
		partAud.Play();
		StartDamage();
	}

	public void StopDash()
	{
		rb.velocity = Vector3.zero;
		dashPower = 0f;
		if (gc.onGround)
		{
			rb.isKinematic = true;
		}
		damaging = false;
		partAud.Stop();
		StopDamage();
	}

	public void ForceStopDashSound()
	{
		partAud.Stop();
	}

	public void StartDamage()
	{
		damaging = true;
		if (swingCheck == null)
		{
			swingCheck = GetComponentInChildren<SwingCheck2>();
		}
		swingCheck.damage = damage;
		swingCheck.DamageStart();
	}

	public void StopDamage()
	{
		damaging = false;
		if (swingCheck == null)
		{
			swingCheck = GetComponentInChildren<SwingCheck2>();
		}
		swingCheck.DamageStop();
	}

	public void Step()
	{
		Object.Instantiate(stepSound, base.transform.position, Quaternion.identity).GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);
	}

	public void Enrage()
	{
		if (!enraged)
		{
			enraged = true;
			Invoke("EnrageNow", 1f / (eid ? eid.totalSpeedModifier : 1f));
		}
	}

	public void EnrageNow()
	{
		if (!eid.dead)
		{
			GameObject obj = Object.Instantiate(statueChargeSound2, base.transform.position, Quaternion.identity);
			obj.GetComponent<AudioSource>().pitch = 0.3f;
			obj.GetComponent<AudioDistortionFilter>().distortionLevel = 0.5f;
			if (difficulty <= 2)
			{
				anim.speed *= 1.2f;
			}
			else if (difficulty > 3)
			{
				anim.speed = 1.5f * eid.totalSpeedModifier;
			}
			else
			{
				anim.speed = 1.25f * eid.totalSpeedModifier;
			}
			orbLight.range *= 2f;
			origRange *= 2f;
			nma.speed = 25f;
			nma.acceleration = 120f;
			nma.angularSpeed = 6000f;
			anim.SetFloat("WalkSpeed", 1.5f);
			currentEnrageEffect = Object.Instantiate(enrageEffect, st.chest.transform);
			currentEnrageEffect.transform.localScale = Vector3.one * 0.4f;
			currentEnrageEffect.transform.localPosition = new Vector3(-0.25f, 0f, 0f);
			if (ensims == null || ensims.Length == 0)
			{
				ensims = GetComponentsInChildren<EnemySimplifier>();
			}
			EnemySimplifier[] array = ensims;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enraged = true;
			}
		}
	}
}
