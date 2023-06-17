using System;
using System.Collections;
using System.Collections.Generic;
using NewBlood.IK;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Solver3D))]
public class Sisyphus : MonoBehaviour
{
	private enum AttackType
	{
		OverheadSlam,
		HorizontalSwing,
		Stab,
		AirStab
	}

	private static readonly int s_SwingAnimSpeed = Animator.StringToHash("SwingSpeed");

	private float swingArmSpeed;

	private static readonly int s_OverheadSlam = Animator.StringToHash("OverheadSlam");

	private static readonly int s_HorizontalSwing = Animator.StringToHash("HorizontalSwing");

	private static readonly int s_Stab = Animator.StringToHash("Stab");

	private static readonly int s_AirStab = Animator.StringToHash("AirStab");

	private static readonly int s_AirStabCancel = Animator.StringToHash("AirStabCancel");

	private static readonly int s_Stomp = Animator.StringToHash("Stomp");

	[SerializeField]
	private Transform target;

	[SerializeField]
	private Solver3D m_Solver;

	[SerializeField]
	private Animator anim;

	[SerializeField]
	private Transform m_Boulder;

	[SerializeField]
	private Collider boulderCol;

	[SerializeField]
	private PhysicalShockwave m_ShockwavePrefab;

	[SerializeField]
	private GameObject explosion;

	private Pose m_StartPose;

	private AttackType m_AttackType;

	private float[] m_NormalizedDistances;

	private Transform[] m_Transforms;

	private bool didCollide;

	private bool airStabCancelled;

	private bool pullSelfRetract;

	private bool swinging;

	private bool inAction;

	private float stuckInActionTimer;

	private int attacksPerformed;

	private int previousAttack = -1;

	private bool previouslyJumped;

	private float cooldown;

	private NavMeshAgent nma;

	private SwingCheck2 sc;

	private float airStabOvershoot = 2f;

	private float stabOvershoot = 1.1f;

	private GroundCheckEnemy gce;

	private Rigidbody rb;

	private bool jumping;

	private Vector3 jumpTarget;

	private bool superJumping;

	private float trackingX;

	private float trackingY;

	private bool forceCorrectOrientation;

	private Collider col;

	[SerializeField]
	private GameObject rubble;

	[SerializeField]
	private TrailRenderer trail;

	[SerializeField]
	private ParticleSystem swingParticle;

	[SerializeField]
	private AudioSource swingAudio;

	public bool stationary;

	private AudioSource aud;

	[SerializeField]
	private AudioClip[] attackVoices;

	[SerializeField]
	private AudioClip stompVoice;

	[SerializeField]
	private AudioClip deathVoice;

	[SerializeField]
	private GameObject[] hurtSounds;

	private GameObject currentHurtSound;

	[SerializeField]
	private Transform[] legs;

	[SerializeField]
	private Transform armature;

	private int difficulty = -1;

	[SerializeField]
	private GameObject attackFlash;

	private float stuckChecker;

	private EnemyIdentifier eid;

	private GoreZone gz;

	private Machine mach;

	private Coroutine co;

	[SerializeField]
	private Cannonball boulderCb;

	private bool isParried;

	[SerializeField]
	private Transform originalBoulder;

	[HideInInspector]
	public bool knockedDownByCannonball;

	[SerializeField]
	private GameObject fallSound;

	private List<EnemyIdentifier> fallEnemiesHit = new List<EnemyIdentifier>();

	[Header("Animations")]
	[SerializeField]
	private SisyAttackAnimationDetails overheadSlamAnim;

	[SerializeField]
	private SisyAttackAnimationDetails horizontalSwingAnim;

	[SerializeField]
	private SisyAttackAnimationDetails groundStabAnim;

	[SerializeField]
	private SisyAttackAnimationDetails airStabAnim;

	[HideInInspector]
	public bool downed;

	public bool jumpOnSpawn;

	private bool dontFacePlayer;

	private float superKnockdownWindow;

	private SisyAttackAnimationDetails GetAnimationDetails(AttackType type)
	{
		return type switch
		{
			AttackType.OverheadSlam => overheadSlamAnim, 
			AttackType.HorizontalSwing => horizontalSwingAnim, 
			AttackType.Stab => groundStabAnim, 
			AttackType.AirStab => airStabAnim, 
			_ => null, 
		};
	}

	private void Start()
	{
		m_Solver.Initialize();
		IKChain3D chain = m_Solver.GetChain(0);
		m_Transforms = new Transform[chain.transformCount];
		m_Transforms[m_Transforms.Length - 1] = chain.effector;
		for (int num = m_Transforms.Length - 2; num >= 0; num--)
		{
			m_Transforms[num] = m_Transforms[num + 1].parent;
		}
		float num2 = 0f;
		m_NormalizedDistances = new float[m_Transforms.Length - 1];
		for (int i = 0; i < m_NormalizedDistances.Length; i++)
		{
			m_NormalizedDistances[i] = Vector3.Distance(m_Transforms[i].position, m_Transforms[i + 1].position);
			num2 += m_NormalizedDistances[i];
		}
		for (int j = 0; j < m_NormalizedDistances.Length; j++)
		{
			m_NormalizedDistances[j] /= num2;
		}
		m_StartPose = new Pose(m_Boulder.localPosition, m_Boulder.localRotation);
		if (target == null)
		{
			target = MonoSingleton<PlayerTracker>.Instance.GetPlayer();
		}
		cooldown = 3f;
		nma = GetComponent<NavMeshAgent>();
		if ((bool)nma)
		{
			nma.enabled = true;
		}
		sc = GetComponentInChildren<SwingCheck2>();
		rb = GetComponent<Rigidbody>();
		gce = GetComponentInChildren<GroundCheckEnemy>();
		col = GetComponent<Collider>();
		aud = GetComponent<AudioSource>();
		gz = GoreZone.ResolveGoreZone(base.transform);
		mach = GetComponent<Machine>();
		anim.SetFloat(s_SwingAnimSpeed, 1f * eid.totalSpeedModifier);
		Physics.IgnoreCollision(col, boulderCol);
		SetSpeed();
		boulderCb.sisy = this;
		if (jumpOnSpawn)
		{
			Jump(target.position);
		}
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	private void SetSpeed()
	{
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
			anim.SetFloat("StompSpeed", 0.75f * eid.totalSpeedModifier);
		}
		else if (difficulty == 2)
		{
			anim.SetFloat("StompSpeed", 0.875f * eid.totalSpeedModifier);
		}
		else
		{
			anim.SetFloat("StompSpeed", 1f * eid.totalSpeedModifier);
		}
	}

	private void OnDisable()
	{
		if (co != null)
		{
			StopCoroutine(co);
		}
		StopAction();
		ResetBoulderPose();
		SwingStop();
		if ((bool)target)
		{
			swingArmSpeed = Mathf.Max(0.01f, Vector3.Distance(base.transform.position, target.position) / 100f) * eid.totalSpeedModifier;
		}
		if ((bool)gce && !gce.onGround)
		{
			rb.isKinematic = false;
			rb.useGravity = true;
		}
	}

	private void OnEnable()
	{
		SetSpeed();
		anim.SetFloat(s_SwingAnimSpeed, 1f * eid.totalSpeedModifier);
	}

	private void LateUpdate()
	{
		ChangeArmLength(Vector3.Distance(m_Transforms[0].position, m_Boulder.position));
		m_Solver.UpdateIK(1f);
		m_Transforms[m_Transforms.Length - 1].position = m_Boulder.position;
		if (!isParried)
		{
			m_Boulder.rotation = originalBoulder.rotation;
			m_Boulder.Rotate(Vector3.right * -90f, Space.Self);
			m_Boulder.Rotate(Vector3.up * -5f, Space.Self);
		}
		else
		{
			originalBoulder.transform.up = m_Boulder.transform.forward;
		}
	}

	private void ChangeArmLength(float targetLength)
	{
		for (int i = 0; i < m_NormalizedDistances.Length; i++)
		{
			Vector3 vector = Vector3.Normalize(m_Transforms[i + 1].position - m_Transforms[i].position);
			float num = targetLength * m_NormalizedDistances[i];
			m_Transforms[i + 1].position = m_Transforms[i].position + vector * num;
		}
	}

	private void FixedUpdate()
	{
		if (!BlindEnemies.Blind)
		{
			if (inAction && (anim.GetCurrentAnimatorStateInfo(0).IsName("Walking") || anim.GetCurrentAnimatorStateInfo(0).IsName("Idle")))
			{
				stuckInActionTimer = Mathf.MoveTowards(stuckInActionTimer, 2f, Time.fixedDeltaTime);
				if (stuckInActionTimer == 2f)
				{
					inAction = false;
				}
			}
			else
			{
				stuckInActionTimer = 0f;
			}
		}
		if (gce.onGround && !nma.isOnNavMesh && !nma.isOnOffMeshLink && !BlindEnemies.Blind)
		{
			if (gce.onGround && !nma.isOnNavMesh && !nma.isOnOffMeshLink && !inAction)
			{
				stuckChecker = Mathf.MoveTowards(stuckChecker, 3f, Time.fixedDeltaTime);
				if (stuckChecker >= 3f && !jumping)
				{
					stuckChecker = 2f;
					superJumping = true;
					Jump(target.position);
				}
			}
			else
			{
				stuckChecker = 0f;
			}
		}
		if (gce.onGround && !superJumping && !inAction && rb.useGravity && !rb.isKinematic)
		{
			nma.enabled = true;
			rb.isKinematic = true;
			rb.useGravity = false;
			jumping = false;
			inAction = true;
			if (superKnockdownWindow > 0f)
			{
				downed = true;
				Knockdown(base.transform.position + base.transform.forward);
				Invoke("Undown", 4f);
			}
			else
			{
				anim.Play("Landing");
				if (difficulty >= 1)
				{
					RaycastHit[] array = Physics.RaycastAll(base.transform.position + Vector3.up * 4f, Vector3.down, 6f, LayerMaskDefaults.Get(LMD.Environment));
					PhysicalShockwave physicalShockwave = null;
					if (array.Length != 0)
					{
						bool flag = false;
						RaycastHit[] array2 = array;
						for (int i = 0; i < array2.Length; i++)
						{
							RaycastHit raycastHit = array2[i];
							if (raycastHit.collider != boulderCol)
							{
								physicalShockwave = UnityEngine.Object.Instantiate(m_ShockwavePrefab, raycastHit.point, Quaternion.identity);
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							physicalShockwave = UnityEngine.Object.Instantiate(m_ShockwavePrefab, base.transform.position, Quaternion.identity);
						}
					}
					else
					{
						physicalShockwave = UnityEngine.Object.Instantiate(m_ShockwavePrefab, base.transform.position, Quaternion.identity);
					}
					if ((bool)physicalShockwave)
					{
						physicalShockwave.transform.SetParent(gz.transform);
						physicalShockwave.speed *= eid.totalSpeedModifier;
						physicalShockwave.damage = Mathf.RoundToInt((float)physicalShockwave.damage * eid.totalDamageModifier);
					}
				}
			}
			if (fallEnemiesHit.Count > 0)
			{
				foreach (EnemyIdentifier item in fallEnemiesHit)
				{
					if (item != null && !item.dead && item.TryGetComponent<Collider>(out var component))
					{
						Physics.IgnoreCollision(col, component, ignore: false);
					}
				}
				fallEnemiesHit.Clear();
			}
		}
		else if (!gce.onGround && rb.useGravity && !rb.isKinematic)
		{
			RaycastHit[] array2 = Physics.SphereCastAll(col.bounds.center, 2.5f, rb.velocity, rb.velocity.magnitude * Time.fixedDeltaTime + 6f, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment));
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit2 = array2[i];
				EnemyIdentifierIdentifier component4;
				if (raycastHit2.transform.gameObject.layer == 8 || raycastHit2.transform.gameObject.layer == 24)
				{
					Glass component3;
					if (raycastHit2.transform.TryGetComponent<Breakable>(out var component2) && !component2.playerOnly && !component2.precisionOnly)
					{
						component2.Break();
					}
					else if (raycastHit2.transform.TryGetComponent<Glass>(out component3))
					{
						component3.Shatter();
					}
				}
				else if (raycastHit2.transform.TryGetComponent<EnemyIdentifierIdentifier>(out component4) && (bool)component4.eid && component4.eid != eid && !fallEnemiesHit.Contains(component4.eid))
				{
					FallKillEnemy(component4.eid);
				}
			}
			array2 = Physics.SphereCastAll(col.bounds.center, 2.5f, rb.velocity, rb.velocity.magnitude * Time.fixedDeltaTime + 6f, 4096);
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit3 = array2[i];
				if (raycastHit3.transform != base.transform && raycastHit3.transform.TryGetComponent<EnemyIdentifier>(out var component5) && !fallEnemiesHit.Contains(component5))
				{
					FallKillEnemy(component5);
				}
			}
		}
		if (!inAction && !BlindEnemies.Blind && gce.onGround && !jumping)
		{
			if (cooldown > 0f)
			{
				forceCorrectOrientation = false;
				if (Vector3.Distance(base.transform.position, target.position) < 10f)
				{
					cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * 3f * eid.totalSpeedModifier);
				}
				else
				{
					cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
				}
				if (!stationary)
				{
					if (nma.isOnNavMesh && Physics.Raycast(target.position, Vector3.down, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment)))
					{
						if (NavMesh.SamplePosition(target.position, out var hit, 1f, nma.areaMask))
						{
							nma.SetDestination(hit.position);
						}
						else
						{
							nma.SetDestination(hitInfo.point);
						}
					}
					else if (nma.isOnNavMesh)
					{
						nma.SetDestination(target.position);
					}
					if (nma.velocity.magnitude < 1f)
					{
						anim.SetBool("Walking", value: false);
					}
					else
					{
						anim.SetBool("Walking", value: true);
					}
				}
			}
			else if (Vector3.Distance(base.transform.position, target.position) < 8f && difficulty != 0)
			{
				inAction = true;
				aud.pitch = UnityEngine.Random.Range(1.4f, 1.6f);
				aud.PlayOneShot(stompVoice);
				anim.SetTrigger(s_Stomp);
			}
			else
			{
				if ((attacksPerformed >= UnityEngine.Random.Range(2, 4) || Vector3.Distance(base.transform.position, target.position) > 100f) && Physics.Raycast(target.position, Vector3.down, 50f, LayerMaskDefaults.Get(LMD.Environment)))
				{
					Jump(target.position);
					attacksPerformed = 0;
					return;
				}
				int num = UnityEngine.Random.Range(0, 4);
				bool flag2 = false;
				int num2 = 0;
				while ((num == previousAttack || (num == 3 && previouslyJumped)) && num2 < 10)
				{
					num2++;
					num = UnityEngine.Random.Range(0, 4);
					if (num2 == 10)
					{
						Debug.LogError("While method in Sisyphus' attack choosing function hit the failsafe", this);
					}
				}
				if (TestAttack(num))
				{
					flag2 = true;
				}
				else
				{
					int[] array3 = new int[4] { 0, 1, 2, 3 };
					int num3 = 4;
					if (previouslyJumped)
					{
						num3 = 3;
					}
					for (int j = 0; j < num3; j++)
					{
						int num4 = array3[j];
						int num5 = UnityEngine.Random.Range(j, num3);
						array3[j] = array3[num5];
						array3[num5] = num4;
					}
					for (int k = 0; k < 4; k++)
					{
						if (array3[k] != num && TestAttack(array3[k]))
						{
							flag2 = true;
							num = array3[k];
							break;
						}
					}
				}
				forceCorrectOrientation = false;
				if (flag2)
				{
					if (!stationary && nma.isOnNavMesh)
					{
						nma.SetDestination(base.transform.position);
					}
					inAction = true;
					if (difficulty == 2)
					{
						cooldown = 2f;
					}
					else if (difficulty == 1)
					{
						cooldown = 2.5f;
					}
					else if (difficulty == 0)
					{
						cooldown = 3f;
					}
					else
					{
						cooldown = 1.5f;
					}
					previousAttack = num;
					previouslyJumped = false;
					switch (num)
					{
					case 0:
						m_AttackType = AttackType.OverheadSlam;
						base.transform.LookAt(new Vector3(target.position.x, base.transform.position.y, target.position.z));
						anim.SetTrigger(s_OverheadSlam);
						trackingX = 1f;
						trackingY = 0.15f;
						break;
					case 1:
						m_AttackType = AttackType.HorizontalSwing;
						anim.SetTrigger(s_HorizontalSwing);
						trackingX = 0f;
						trackingY = 1f;
						break;
					case 2:
						m_AttackType = AttackType.Stab;
						anim.SetTrigger(s_Stab);
						trackingX = 0.9f;
						trackingY = 0.5f;
						break;
					case 3:
						m_AttackType = AttackType.AirStab;
						StartCoroutine(AirStab());
						Jump(noEnd: true);
						trackingX = 0f;
						trackingY = 0.9f;
						break;
					}
					if (num < attackVoices.Length && num != 3)
					{
						if (num == 1)
						{
							aud.pitch = UnityEngine.Random.Range(1.4f, 1.6f);
						}
						else
						{
							aud.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
						}
						aud.PlayOneShot(attackVoices[num]);
					}
					if (num != 3)
					{
						UnityEngine.Object.Instantiate(attackFlash, m_Boulder);
					}
					attacksPerformed++;
				}
				else
				{
					Jump(target.position);
				}
			}
		}
		else if (inAction)
		{
			if (!gce.onGround)
			{
				rb.useGravity = false;
			}
			if (!dontFacePlayer)
			{
				RotateTowardsTarget();
			}
		}
		else if (jumping)
		{
			RotateTowardsTarget();
		}
		if (jumping)
		{
			Vector3 vector = new Vector3(jumpTarget.x, base.transform.position.y, jumpTarget.z);
			base.transform.position = Vector3.MoveTowards(base.transform.position, vector, Vector3.Distance(base.transform.position, vector) * Time.fixedDeltaTime * 2f);
			if (superJumping)
			{
				RaycastHit hitInfo2;
				bool num6 = Physics.SphereCast(base.transform.position, 3f, rb.velocity.normalized, out hitInfo2, rb.velocity.magnitude * Time.fixedDeltaTime, LayerMaskDefaults.Get(LMD.Environment));
				if (!num6 && didCollide && Physics.SphereCast(base.transform.position, 3f, -rb.velocity.normalized, out hitInfo2, rb.velocity.magnitude * Time.fixedDeltaTime, LayerMaskDefaults.Get(LMD.Environment)))
				{
					UnityEngine.Object.Instantiate(rubble, hitInfo2.point, Quaternion.LookRotation(hitInfo2.normal));
					didCollide = false;
				}
				if (num6 && !didCollide)
				{
					didCollide = true;
					if (Vector3.Distance(base.transform.position + rb.velocity * Time.fixedDeltaTime, jumpTarget) > 3f)
					{
						UnityEngine.Object.Instantiate(rubble, hitInfo2.point, Quaternion.LookRotation(hitInfo2.normal));
					}
				}
				if (rb.velocity.y >= 0f)
				{
					col.isTrigger = Vector3.Distance(base.transform.position, jumpTarget) > 1f;
				}
				else if (jumpTarget.y + 8f > base.transform.position.y + Mathf.Abs(rb.velocity.y) * Time.fixedDeltaTime)
				{
					col.isTrigger = false;
					superJumping = false;
					base.transform.position = vector;
				}
			}
		}
		if (!inAction && !gce.onGround)
		{
			rb.useGravity = true;
		}
		if (!rb.isKinematic && rb.useGravity)
		{
			rb.velocity -= Vector3.up * 200f * Time.fixedDeltaTime;
		}
		if (!jumping && !rb.isKinematic && !inAction)
		{
			anim.Play("Jump", -1, 0.95f);
		}
		else if (gce.onGround && !inAction && !superJumping)
		{
			superJumping = false;
			jumping = false;
		}
	}

	private void Update()
	{
		if (superKnockdownWindow > 0f)
		{
			superKnockdownWindow = Mathf.MoveTowards(superKnockdownWindow, 0f, Time.deltaTime);
		}
	}

	private bool TestAttack(int attack)
	{
		float num = Vector3.Distance(base.transform.position, target.position);
		LayerMask layerMask = LayerMaskDefaults.Get(LMD.Environment);
		switch (attack)
		{
		case 0:
			if (!Physics.Raycast(base.transform.position, Vector3.up, num, layerMask) && !Physics.Raycast(base.transform.position + Vector3.up * num, target.position - base.transform.position, num, layerMask) && !Physics.Raycast(target.position, Vector3.up, num, layerMask))
			{
				return true;
			}
			return false;
		case 1:
		{
			Vector3 position = base.transform.position;
			float num2 = Vector3.Distance(target.position, position);
			float num3 = target.position.y - position.y;
			Vector3 vector2 = position + base.transform.up * 5f + base.transform.right * (0f - num2);
			Vector3 vector3 = position + base.transform.up * 5f + Vector3.up * num3 * 2f + base.transform.right * num2;
			if (!Physics.Raycast(base.transform.position + Vector3.up * 3f, -base.transform.right, num, layerMask) && !Physics.Raycast(vector2, target.position - vector2, Vector3.Distance(vector2, target.position), layerMask) && !Physics.Raycast(base.transform.position + Vector3.up * 3f, base.transform.right, num, layerMask) && !Physics.Raycast(vector3, target.position - vector3, Vector3.Distance(vector3, target.position), layerMask))
			{
				return true;
			}
			return false;
		}
		case 2:
		{
			Vector3 vector4 = target.position + Vector3.up * 3f;
			RaycastHit hitInfo;
			return !Physics.SphereCast(base.transform.position + Vector3.up * 3f, 1.75f, Quaternion.LookRotation(vector4 - base.transform.position, Vector3.up).eulerAngles, out hitInfo, Vector3.Distance(base.transform.position, target.position), layerMask);
		}
		case 3:
		{
			Vector3 vector = base.transform.position + Vector3.up * 73f;
			Vector3 direction = Vector3.Normalize(target.position - vector);
			if (!Physics.Raycast(base.transform.position + Vector3.up * 3f, base.transform.up, 70f, layerMask))
			{
				return !Physics.Raycast(vector, direction, Vector3.Distance(vector, target.position), layerMask);
			}
			return false;
		}
		default:
			return false;
		}
	}

	public bool CanFit(Vector3 point)
	{
		if (Physics.Raycast(point, Vector3.up, 11f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			return false;
		}
		return true;
	}

	private IEnumerator AirStab()
	{
		superJumping = false;
		yield return new WaitForSeconds(1f);
		UnityEngine.Object.Instantiate(attackFlash, m_Boulder).transform.localScale *= 5f;
		aud.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
		aud.PlayOneShot(attackVoices[3]);
		trackingX = 0.9f;
		trackingY = 0.9f;
		Debug.Log("Airstab freeze");
		rb.isKinematic = true;
		anim.SetTrigger(s_AirStab);
	}

	private IEnumerator AirStabAttack(float time)
	{
		airStabCancelled = true;
		rb.isKinematic = true;
		ResetBoulderPose();
		Vector3 start = m_Boulder.position;
		float t = 0f;
		time *= swingArmSpeed * GetAnimationDetails(AttackType.AirStab).finalDurationMulti;
		Vector3 attackTarget = base.transform.position + (base.transform.forward * Vector3.Distance(base.transform.position, target.position) + base.transform.right * 3f) * airStabOvershoot;
		sc.DamageStart();
		while (swinging)
		{
			Vector3 vector = Vector3.LerpUnclamped(start, attackTarget, t / time);
			trail.transform.forward = vector - m_Boulder.position;
			m_Boulder.position = vector;
			yield return new WaitForEndOfFrame();
			t += Time.deltaTime;
			if (Physics.OverlapSphere(m_Boulder.position, 3.75f, LayerMaskDefaults.Get(LMD.Environment)).Length != 0)
			{
				SlamShockwave();
				SwingStop();
				swinging = false;
				trackingX = 0f;
				trackingY = 0f;
			}
		}
		Debug.Log("End reached");
		trackingX = 0.75f;
		trackingY = 0f;
		SwingStop();
	}

	public void ExtendArm(float time)
	{
		SisyAttackAnimationDetails animationDetails = GetAnimationDetails(m_AttackType);
		boulderCol.enabled = false;
		trail.emitting = true;
		swingParticle.Play();
		swinging = true;
		swingAudio.Play();
		boulderCb.launchable = true;
		float num = Vector3.Distance(base.transform.position, GetActualTargetPos());
		if (num < 10f)
		{
			num = 10f;
		}
		num -= 10f;
		swingArmSpeed = Mathf.Clamp(num / animationDetails.boulderDistanceDivide, animationDetails.minBoulderSpeed, animationDetails.maxBoulderSpeed);
		if (m_AttackType == AttackType.AirStab)
		{
			num *= 0.35f;
			swingArmSpeed /= airStabOvershoot;
		}
		float num2 = 1f - num / animationDetails.boulderDistanceDivide;
		num2 *= animationDetails.speedDistanceMulti;
		num2 = Mathf.Clamp(num2, animationDetails.minAnimSpeedCap, animationDetails.maxAnimSpeedCap);
		_ = m_AttackType;
		_ = 2;
		float num3 = 1f;
		if (difficulty >= 4)
		{
			num3 = 1.5f;
		}
		else if (difficulty == 3)
		{
			num3 = 1.25f;
		}
		else if (difficulty == 1)
		{
			num3 = 0.75f;
		}
		else if (difficulty == 0)
		{
			num3 = 0.5f;
		}
		num3 *= eid.totalSpeedModifier;
		num2 *= num3;
		swingArmSpeed /= num3;
		anim.SetFloat(s_SwingAnimSpeed, num2);
		if (m_AttackType == AttackType.OverheadSlam)
		{
			co = StartCoroutine(OverheadSlamAttack(time));
		}
		else if (m_AttackType == AttackType.HorizontalSwing)
		{
			co = StartCoroutine(HorizontalSwingAttack(time));
		}
		else if (m_AttackType == AttackType.Stab)
		{
			co = StartCoroutine(StabAttack(time));
		}
		else if (m_AttackType == AttackType.AirStab)
		{
			co = StartCoroutine(AirStabAttack(time));
		}
	}

	public void RetractArm(float time)
	{
		inAction = false;
		anim.SetFloat(s_SwingAnimSpeed, 1f * eid.totalSpeedModifier);
		swingArmSpeed = Mathf.Max(0.01f, Vector3.Distance(base.transform.position, target.position) / 100f);
		TryToRetractArm(time);
	}

	private Vector3 GetActualTargetPos()
	{
		switch (m_AttackType)
		{
		case AttackType.OverheadSlam:
		{
			Vector3 position = base.transform.position;
			position.y = target.position.y;
			return position + base.transform.forward * (Vector3.Distance(position, target.position) - 0.5f) - base.transform.forward;
		}
		case AttackType.HorizontalSwing:
		{
			Vector3 result = target.position - base.transform.forward * 3f;
			result.y = target.position.y;
			return result;
		}
		case AttackType.AirStab:
			return target.position + base.transform.right * 10f;
		default:
			return target.position;
		}
	}

	private bool SwingCheck(bool noExplosion = false)
	{
		if (Physics.OverlapSphere(m_Boulder.position, 0.75f, LayerMaskDefaults.Get(LMD.Environment)).Length != 0)
		{
			if (!noExplosion)
			{
				GameObject temp = UnityEngine.Object.Instantiate(explosion, m_Boulder.position + m_Boulder.forward, Quaternion.identity);
				SetupExplosion(temp);
			}
			SwingStop();
			return true;
		}
		return false;
	}

	private void SetupExplosion(GameObject temp)
	{
		if (difficulty > 2 && eid.totalDamageModifier == 1f && eid.totalSpeedModifier == 1f)
		{
			return;
		}
		Explosion[] componentsInChildren = temp.GetComponentsInChildren<Explosion>();
		foreach (Explosion explosion in componentsInChildren)
		{
			if (difficulty <= 2)
			{
				explosion.maxSize *= 0.66f;
				explosion.speed /= 0.66f;
			}
			explosion.maxSize *= eid.totalDamageModifier;
			explosion.speed *= eid.totalDamageModifier;
			explosion.damage = Mathf.RoundToInt((float)explosion.damage * eid.totalDamageModifier);
		}
	}

	private IEnumerator HorizontalSwingAttack(float time)
	{
		ResetBoulderPose();
		float t2 = 0f;
		time *= swingArmSpeed * GetAnimationDetails(AttackType.HorizontalSwing).finalDurationMulti;
		Vector3 actualTarget = GetActualTargetPos();
		sc.DamageStart();
		while (t2 < time / 3f && swinging)
		{
			float num = Vector3.Distance(actualTarget, base.transform.position);
			Vector3 vector = base.transform.position + base.transform.up * 5f + base.transform.right * (0f - num);
			Vector3 a = m_Boulder.parent.TransformPoint(m_StartPose.position);
			Debug.DrawLine(base.transform.position, vector, Color.red, 8f);
			Vector3 vector2 = Vector3.Lerp(a, vector, t2 / (time / 2f));
			trail.transform.forward = vector2 - m_Boulder.position;
			m_Boulder.transform.position = vector2;
			yield return new WaitForEndOfFrame();
			t2 += Time.deltaTime;
			if (SwingCheck(noExplosion: true))
			{
				yield return new WaitForSeconds(0.5f);
				RetractArm(0.5f);
				yield break;
			}
		}
		t2 = 0f;
		float progressEnd = time / 1.5f;
		float yPos = actualTarget.y;
		while (t2 < progressEnd && swinging)
		{
			float num2 = t2 / progressEnd;
			if (num2 <= 0.5f)
			{
				actualTarget = GetActualTargetPos();
				_ = 0.12f;
				actualTarget.y = yPos + 2f;
			}
			Vector3 position = base.transform.position;
			float num3 = Vector3.Distance(actualTarget, position);
			float num4 = actualTarget.y - position.y;
			Vector3 vector3 = position + base.transform.up * 5f + base.transform.right * (0f - num3);
			Vector3 vector4 = position + base.transform.up * 5f + Vector3.up * num4 * 2f + base.transform.right * num3;
			trackingY = 1f;
			Quaternion a2 = Quaternion.LookRotation(vector3 - position, Vector3.up);
			Quaternion b = Quaternion.LookRotation(vector4 - position, Vector3.up);
			Quaternion quaternion = Quaternion.LookRotation(actualTarget - position, Vector3.up);
			Quaternion quaternion2 = ((num2 > 0.5f) ? Quaternion.Lerp(quaternion, b, (num2 - 0.5f) * 2f) : Quaternion.Lerp(a2, quaternion, num2 * 2f));
			Vector3 vector5 = position + quaternion2 * Vector3.forward * num3;
			trail.transform.forward = vector5 - m_Boulder.position;
			m_Boulder.position = vector5;
			yield return new WaitForEndOfFrame();
			t2 += Time.deltaTime;
			if (SwingCheck())
			{
				yield return new WaitForSeconds(0.5f);
				RetractArm(0.5f);
				yield break;
			}
		}
		SwingStop();
		TryToRetractArm(2f);
	}

	private IEnumerator OverheadSlamAttack(float time)
	{
		ResetBoulderPose();
		Vector3 start = m_Boulder.position;
		float t2 = 0f;
		time *= swingArmSpeed * GetAnimationDetails(AttackType.OverheadSlam).finalDurationMulti;
		sc.DamageStart();
		Vector3 actualTargetPos = GetActualTargetPos();
		while (t2 < time)
		{
			Vector3 vector = Vector3.Lerp(start, actualTargetPos, t2 / time);
			vector.y += Vector3.Distance(start, actualTargetPos) * Mathf.Sin(Mathf.Clamp01(t2 / time) * (float)Math.PI);
			trail.transform.forward = vector - m_Boulder.position;
			m_Boulder.position = vector;
			yield return new WaitForEndOfFrame();
			t2 += Time.deltaTime;
			actualTargetPos = GetActualTargetPos();
		}
		if (swinging)
		{
			if (Physics.OverlapSphere(m_Boulder.position, 5f, LayerMaskDefaults.Get(LMD.Environment)).Length != 0)
			{
				SlamShockwave();
				SwingStop();
			}
			else
			{
				bool hit = false;
				t2 = 0f;
				while (!hit)
				{
					Vector3 position = m_Boulder.position;
					position.y -= Time.deltaTime * swingArmSpeed * 400f;
					trail.transform.forward = position - m_Boulder.position;
					m_Boulder.position = position;
					if (Physics.OverlapSphere(m_Boulder.position, 5f, LayerMaskDefaults.Get(LMD.Environment)).Length != 0)
					{
						SlamShockwave();
						SwingStop();
						hit = true;
					}
					yield return new WaitForEndOfFrame();
					if (t2 > 1.5f)
					{
						Debug.Log("No ground");
						hit = true;
					}
					t2 += Time.deltaTime;
				}
			}
		}
		trackingY = 0f;
		sc.DamageStop();
		yield return new WaitForSeconds(1f);
		TryToRetractArm(2f);
	}

	private void SlamShockwave()
	{
		Collider[] array = Physics.OverlapSphere(m_Boulder.position, 3.5f, LayerMaskDefaults.Get(LMD.Environment));
		if (array.Length != 0)
		{
			float num = 5f;
			Vector3 vector = m_Boulder.position;
			for (int i = 0; i < array.Length; i++)
			{
				Vector3 vector2 = array[i].ClosestPoint(m_Boulder.position);
				if (Vector3.Distance(m_Boulder.position, vector2) < num)
				{
					vector = vector2;
					num = Vector3.Distance(m_Boulder.position, vector2);
				}
			}
			GameObject temp = UnityEngine.Object.Instantiate(explosion, vector + Vector3.up * 0.1f, Quaternion.identity);
			m_Boulder.position = vector;
			SetupExplosion(temp);
		}
		else
		{
			GameObject temp2 = UnityEngine.Object.Instantiate(explosion, m_Boulder.position, Quaternion.identity);
			m_Boulder.position -= Vector3.up * 2f;
			SetupExplosion(temp2);
		}
	}

	private IEnumerator StabAttack(float time)
	{
		ResetBoulderPose();
		Vector3 start = m_Boulder.position;
		float t = 0f;
		time *= swingArmSpeed * GetAnimationDetails(AttackType.Stab).finalDurationMulti;
		Vector3 b = target.position + Vector3.up * 3f;
		Vector3 attackTarget = base.transform.position + base.transform.forward * Vector3.Distance(base.transform.position, b);
		attackTarget.y = b.y;
		trackingX = 0f;
		trackingY = 0f;
		sc.DamageStart();
		bool canCancel = false;
		while (swinging)
		{
			Vector3 vector = Vector3.LerpUnclamped(start, attackTarget, t / time);
			if (!canCancel && Vector3.Distance(start, vector) >= 20f)
			{
				canCancel = true;
			}
			trail.transform.forward = vector - m_Boulder.position;
			m_Boulder.position = vector;
			yield return new WaitForEndOfFrame();
			t += Time.deltaTime;
			if (canCancel && Physics.OverlapSphere(m_Boulder.position, 2f, LayerMaskDefaults.Get(LMD.Environment)).Length != 0)
			{
				Debug.Log("Cancel stab");
				GameObject temp = UnityEngine.Object.Instantiate(explosion, m_Boulder.position + m_Boulder.forward, Quaternion.identity);
				SetupExplosion(temp);
				anim.Play(s_Stab, -1, 0.73f);
				SwingStop();
				yield return new WaitForSeconds(0.5f);
				RetractArm(0.5f);
			}
		}
		Debug.Log("End reached");
		sc.DamageStop();
	}

	public void TryToRetractArm(float time)
	{
		if (swinging)
		{
			swinging = false;
			boulderCol.enabled = true;
			boulderCb.Unlaunch(relaunchable: false);
			SwingStop();
			co = StartCoroutine(RetractArmAsync(time));
			isParried = false;
		}
	}

	public void SwingStop()
	{
		trail.emitting = false;
		swingParticle?.Stop();
		sc?.DamageStop();
		swingAudio?.Stop();
		boulderCb?.Unlaunch(relaunchable: false);
		isParried = false;
	}

	private IEnumerator RetractArmAsync(float time)
	{
		float t = 0f;
		Vector3 boulderStart = m_Boulder.position;
		Transform oldBoulderParent = m_Boulder.parent;
		Vector3 bossStart = base.transform.position;
		if (pullSelfRetract)
		{
			m_Boulder.SetParent(base.transform.parent ? base.transform.parent : null);
		}
		for (; t < time; t += Time.deltaTime)
		{
			Debug.Log("Retracting " + pullSelfRetract);
			Vector3 b = ((!pullSelfRetract) ? m_Boulder.parent.TransformPoint(m_StartPose.position) : m_Boulder.transform.position);
			(pullSelfRetract ? base.transform : m_Boulder).position = Vector3.Lerp(pullSelfRetract ? bossStart : boulderStart, b, t / time);
			_ = pullSelfRetract;
			yield return new WaitForEndOfFrame();
		}
		if (pullSelfRetract)
		{
			m_Boulder.SetParent(oldBoulderParent);
			rb.isKinematic = false;
			rb.useGravity = true;
			base.transform.rotation = Quaternion.identity;
			rb.AddForce(Vector3.down * 300f, ForceMode.VelocityChange);
			pullSelfRetract = false;
			ResetBoulderPose();
			StopAction();
		}
	}

	private void Jump(bool noEnd = false)
	{
		Jump(base.transform.position, noEnd);
	}

	private void Jump(Vector3 target, bool noEnd = false)
	{
		if (jumping || stationary)
		{
			return;
		}
		previouslyJumped = true;
		if (RaycastHelper.RaycastAndDebugDraw(target, Vector3.up, 50f, LayerMaskDefaults.Get(LMD.Environment)) || RaycastHelper.RaycastAndDebugDraw(col.bounds.center, Vector3.up, 25f, LayerMaskDefaults.Get(LMD.Environment)) || RaycastHelper.RaycastAndDebugDraw(col.bounds.center, target - col.bounds.center, Vector3.Distance(col.bounds.center, target), LayerMaskDefaults.Get(LMD.Environment)))
		{
			superJumping = true;
		}
		didCollide = false;
		jumpTarget = target;
		if (superJumping)
		{
			RaycastHit hitInfo;
			if (NavMesh.SamplePosition(target, out var hit, 2f, nma.areaMask))
			{
				jumpTarget = hit.position;
			}
			else if (Physics.Raycast(target, -Vector3.up, out hitInfo, 3f, LayerMaskDefaults.Get(LMD.Environment)))
			{
				jumpTarget = hitInfo.point;
			}
			if (!CanFit(jumpTarget))
			{
				int num = 60;
				float num2 = UnityEngine.Random.Range(0f, 360f);
				int num3 = 0;
				while (num3 < 360)
				{
					Vector3 vector = jumpTarget + Quaternion.Euler(0f, (float)num3 + num2, 0f) * Vector3.forward * 12f;
					vector += Vector3.up * 2f;
					if (!Physics.Linecast(jumpTarget + Vector3.up * 2f, vector, LayerMaskDefaults.Get(LMD.Environment)))
					{
						Debug.Log($"Successful horizontal cast. Angle {(float)num3 + num2}");
						Debug.DrawRay(vector, Vector3.down * 50f, Color.yellow, 50f);
						if (Physics.Raycast(vector, Vector3.down, out var hitInfo2, 50f, LayerMaskDefaults.Get(LMD.Environment)))
						{
							if (!CanFit(hitInfo2.point))
							{
								num3 += num;
								continue;
							}
							Debug.Log("Successful cast down. Changing target.");
							jumpTarget = hitInfo2.point;
							break;
						}
					}
					num3 += num;
				}
			}
		}
		Debug.Log("Jump start");
		jumping = true;
		anim.Play("Jump");
		rb.isKinematic = false;
		rb.useGravity = true;
		if (superJumping)
		{
			col.isTrigger = true;
		}
		nma.enabled = false;
		UnityEngine.Object.Instantiate(rubble, base.transform.position, base.transform.rotation);
		rb.velocity = Vector3.zero;
		rb.AddForce(Vector3.up * Mathf.Max(50f, 100f + Vector3.Distance(base.transform.position, target)), ForceMode.VelocityChange);
		trackingX = 0f;
		trackingY = 1f;
		inAction = true;
		if (!noEnd)
		{
			Debug.Log("Jump End");
			Invoke("StopAction", 0.5f);
		}
	}

	private void FlyToArm()
	{
		if (!airStabCancelled)
		{
			Debug.Log("FlyToArm");
			inAction = false;
			pullSelfRetract = true;
			forceCorrectOrientation = true;
			trackingX = 0.3f;
			aud.pitch = UnityEngine.Random.Range(1.4f, 1.6f);
			aud.PlayOneShot(attackVoices[3]);
			anim.SetFloat(s_SwingAnimSpeed, 1f);
			swinging = true;
			TryToRetractArm(0.4f);
		}
	}

	private void CancelAirStab()
	{
		Vector3 position = base.transform.position;
		position.y = target.position.y;
		if (Vector3.Distance(position, target.position) > Vector3.Distance(m_Boulder.position, target.position) && !swinging)
		{
			Debug.Log("No Cancel");
			airStabCancelled = false;
			return;
		}
		Debug.Log("Cancel Air Stab");
		inAction = false;
		airStabCancelled = true;
		pullSelfRetract = false;
		swinging = true;
		RetractArm(1f);
		anim.SetTrigger(s_AirStabCancel);
		rb.isKinematic = false;
		rb.useGravity = true;
		nma.enabled = false;
		rb.velocity = Vector3.zero;
		forceCorrectOrientation = true;
		trackingX = 0.3f;
		trackingY = 1f;
	}

	public void Death()
	{
		aud.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
		aud.PlayOneShot(deathVoice);
		GoreZone componentInParent = GetComponentInParent<GoreZone>();
		Transform[] array = legs;
		foreach (Transform obj in array)
		{
			obj.parent = componentInParent.gibZone;
			Rigidbody[] componentsInChildren = obj.GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody obj2 in componentsInChildren)
			{
				obj2.isKinematic = false;
				obj2.useGravity = true;
			}
		}
		EnemyIdentifierIdentifier[] componentsInChildren2 = GetComponentsInChildren<EnemyIdentifierIdentifier>();
		if (OptionsMenuToManager.bloodEnabled)
		{
			EnemyIdentifierIdentifier[] array2 = componentsInChildren2;
			foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in array2)
			{
				GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Head, enemyIdentifierIdentifier.eid.underwater, enemyIdentifierIdentifier.eid.sandified, enemyIdentifierIdentifier.eid.blessed);
				if ((bool)gore)
				{
					gore.transform.position = enemyIdentifierIdentifier.transform.position;
					gore.transform.SetParent(componentInParent.goreZone, worldPositionStays: true);
					gore.SetActive(value: true);
				}
				for (int k = 0; k < 3; k++)
				{
					GameObject gib = MonoSingleton<BloodsplatterManager>.Instance.GetGib(GibType.Gib);
					if ((bool)gib)
					{
						gib.transform.position = enemyIdentifierIdentifier.transform.position;
						gib.transform.parent = componentInParent.gibZone;
						gib.transform.rotation = UnityEngine.Random.rotation;
						gib.transform.localScale *= 4f;
					}
				}
			}
		}
		armature.localScale = Vector3.zero;
		Collider[] componentsInChildren3 = GetComponentsInChildren<Collider>();
		for (int num = componentsInChildren3.Length - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(componentsInChildren3[num]);
		}
		UnityEngine.Object.Destroy(m_Boulder.gameObject);
		UnityEngine.Object.Destroy(anim);
		UnityEngine.Object.Destroy(this);
	}

	private void StopAction()
	{
		inAction = false;
		dontFacePlayer = false;
		knockedDownByCannonball = false;
	}

	private void ResetBoulderPose()
	{
		Debug.Log("Boulder Reset");
		m_Boulder.localPosition = m_StartPose.position;
		m_Boulder.localRotation = m_StartPose.rotation;
		boulderCb.Unlaunch();
		isParried = false;
	}

	private void RotateTowardsTarget()
	{
		Vector3 vector = target.position;
		if (gce.onGround || forceCorrectOrientation)
		{
			vector = new Vector3(target.position.x, base.transform.position.y, target.position.z);
		}
		Quaternion b = Quaternion.LookRotation(vector - base.transform.position);
		float num = (Quaternion.Angle(base.transform.rotation, b) * 10f + 30f) * Time.fixedDeltaTime;
		float num2 = base.transform.rotation.eulerAngles.x;
		float num3 = base.transform.rotation.eulerAngles.y;
		while (num2 - b.eulerAngles.x > 180f)
		{
			num2 -= 360f;
		}
		for (; num2 - b.eulerAngles.x < -180f; num2 += 360f)
		{
		}
		while (num3 - b.eulerAngles.y > 180f)
		{
			num3 -= 360f;
		}
		for (; num3 - b.eulerAngles.y < -180f; num3 += 360f)
		{
		}
		float num4 = 1f;
		if (difficulty == 1)
		{
			num4 = 0.75f;
		}
		else if (difficulty == 0)
		{
			num4 = 0.5f;
		}
		base.transform.rotation = Quaternion.Euler(Mathf.MoveTowards(num2, b.eulerAngles.x, num * trackingX * num4), Mathf.MoveTowards(num3, b.eulerAngles.y, num * trackingY * num4), Mathf.MoveTowards(base.transform.rotation.eulerAngles.z, b.eulerAngles.z, num));
	}

	public void StompExplosion()
	{
		Vector3 vector = base.transform.position + Vector3.up;
		if (Physics.Raycast(vector, target.position - vector, Vector3.Distance(target.position, vector), LayerMaskDefaults.Get(LMD.Environment)))
		{
			vector = base.transform.position + Vector3.up * 5f;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(this.explosion, vector, Quaternion.identity);
		if (difficulty > 2 && eid.totalDamageModifier == 1f && eid.totalSpeedModifier == 1f)
		{
			return;
		}
		Explosion[] componentsInChildren = gameObject.GetComponentsInChildren<Explosion>();
		foreach (Explosion explosion in componentsInChildren)
		{
			if (difficulty >= 3)
			{
				explosion.maxSize *= 1.5f;
				explosion.speed *= 1.5f;
			}
			explosion.maxSize *= eid.totalDamageModifier;
			explosion.speed *= eid.totalDamageModifier;
			explosion.damage = Mathf.RoundToInt((float)explosion.damage * eid.totalDamageModifier);
		}
	}

	public void PlayHurtSound(int type = 0)
	{
		if ((bool)currentHurtSound)
		{
			if (type == 0)
			{
				return;
			}
			UnityEngine.Object.Destroy(currentHurtSound);
		}
		currentHurtSound = UnityEngine.Object.Instantiate(hurtSounds[type], base.transform.position, Quaternion.identity);
	}

	public void GotParried()
	{
		isParried = true;
		if (co != null)
		{
			StopCoroutine(co);
		}
	}

	public void Knockdown(Vector3 boulderPos)
	{
		Debug.Log("Knockdown");
		if (!pullSelfRetract)
		{
			if (co != null)
			{
				StopCoroutine(co);
			}
			if (!knockedDownByCannonball)
			{
				base.transform.LookAt(new Vector3(boulderPos.x, base.transform.position.y, boulderPos.z));
			}
			if (!inAction && gce.onGround)
			{
				inAction = true;
				if (!stationary && nma.isOnNavMesh)
				{
					nma.SetDestination(base.transform.position);
				}
			}
		}
		if (!gce.onGround)
		{
			superKnockdownWindow = 0.25f;
		}
		dontFacePlayer = true;
		if (gce.onGround && !knockedDownByCannonball)
		{
			knockedDownByCannonball = true;
			anim.Play("Knockdown");
		}
		PlayHurtSound(2);
		trackingX = 0f;
		trackingY = 0f;
		if (knockedDownByCannonball)
		{
			Invoke("CheckLoop", 0.85f);
		}
		GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Splatter, eid.underwater, eid.sandified, eid.blessed);
		if ((bool)gore)
		{
			gore.transform.position = boulderPos;
			gore.transform.up = base.transform.forward;
			gore.transform.SetParent(GetComponentInParent<GoreZone>().goreZone, worldPositionStays: true);
			gore.SetActive(value: true);
			if (gore.TryGetComponent<Bloodsplatter>(out var component))
			{
				component.GetReady();
			}
		}
		if (!pullSelfRetract)
		{
			ResetBoulderPose();
			SwingStop();
		}
	}

	public void FallSound()
	{
		MonoSingleton<CameraController>.Instance.CameraShake(0.5f);
		UnityEngine.Object.Instantiate(fallSound, base.transform.position, Quaternion.identity);
	}

	private void FallKillEnemy(EnemyIdentifier eid)
	{
		eid.hitter = "enemy";
		fallEnemiesHit.Add(eid);
		if (eid.TryGetComponent<Collider>(out var component))
		{
			Physics.IgnoreCollision(col, component, ignore: true);
		}
		EnemyIdentifier.FallOnEnemy(eid);
	}

	public void CheckLoop()
	{
		if (downed)
		{
			anim.SetFloat("DownedSpeed", 0f);
			Invoke("CheckLoop", 0.1f);
		}
		else
		{
			anim.SetFloat("DownedSpeed", 1f);
		}
	}

	private void Undown()
	{
		downed = false;
	}
}
