using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
	public float health;

	private int difficulty = -1;

	private Rigidbody[] rbs;

	public bool limp;

	public GameObject player;

	public NavMeshAgent nma;

	public Animator anim;

	private float currentSpeed;

	private Rigidbody rb;

	private ZombieMelee zm;

	[HideInInspector]
	public ZombieProjectiles zp;

	private AudioSource aud;

	public AudioClip[] hurtSounds;

	public float hurtSoundVol;

	public AudioClip deathSound;

	public float deathSoundVol;

	public AudioClip scream;

	private GroundCheckEnemy gc;

	public bool grounded;

	private float defaultSpeed;

	public Vector3 agentVelocity;

	private StyleCalculator scalc;

	private EnemyIdentifier eid;

	private GoreZone gz;

	public Material deadMaterial;

	public Material simplifiedMaterial;

	public Renderer smr;

	private Material originalMaterial;

	public GameObject chest;

	private float chestHP = 3f;

	public bool chestExploding;

	public GameObject chestExplosionStuff;

	public bool attacking;

	public LayerMask lmask;

	private LayerMask lmaskWater;

	public Transform target;

	public List<EnemyIdentifier> enemyTargets = new List<EnemyIdentifier>();

	public bool friendly;

	public EnemyIdentifier targetedEnemy;

	private bool noheal;

	private float speedMultiplier = 1f;

	public bool stopped;

	public bool knockedBack;

	private float knockBackCharge;

	public float brakes;

	public float juggleWeight;

	public bool falling;

	public bool noFallDamage;

	public bool musicRequested;

	private float fallSpeed;

	private float fallTime;

	private float reduceFallTime;

	private BloodsplatterManager bsm;

	public bool variableSpeed;

	private bool blindedByCheat;

	private bool chestExploded;

	private GoreZone GetGoreZone()
	{
		if ((bool)gz)
		{
			return gz;
		}
		gz = GoreZone.ResolveGoreZone(base.transform);
		return gz;
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	private void SetSpeed()
	{
		if (limp)
		{
			return;
		}
		if (!eid)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (!nma)
		{
			nma = GetComponent<NavMeshAgent>();
		}
		if (!anim)
		{
			anim = GetComponent<Animator>();
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
		if (difficulty >= 3)
		{
			nma.acceleration = 60f;
			nma.angularSpeed = 2600f;
			nma.speed = 20f;
			speedMultiplier = 1.25f;
		}
		else if (difficulty == 2)
		{
			nma.acceleration = 30f;
			nma.angularSpeed = 800f;
			nma.speed = 20f;
			speedMultiplier = 1f;
		}
		else if (difficulty == 1)
		{
			nma.acceleration = 30f;
			nma.angularSpeed = 400f;
			nma.speed = 15f;
			speedMultiplier = 0.75f;
		}
		else if (difficulty == 0)
		{
			nma.acceleration = 15f;
			nma.angularSpeed = 400f;
			nma.speed = 10f;
			speedMultiplier = 0.5f;
		}
		nma.acceleration *= eid.totalSpeedModifier;
		nma.angularSpeed *= eid.totalSpeedModifier;
		nma.speed *= eid.totalSpeedModifier;
		speedMultiplier *= eid.totalSpeedModifier;
		if ((bool)nma)
		{
			defaultSpeed = nma.speed;
		}
		if ((bool)anim && !variableSpeed)
		{
			if (difficulty >= 2)
			{
				anim.SetFloat("Speed", 1f * eid.totalSpeedModifier);
			}
			else if (difficulty == 1)
			{
				anim.SetFloat("Speed", 0.875f * eid.totalSpeedModifier);
			}
			else if (difficulty == 0)
			{
				anim.SetFloat("Speed", 0.75f * eid.totalSpeedModifier);
			}
		}
	}

	private void Start()
	{
		rbs = GetComponentsInChildren<Rigidbody>();
		player = MonoSingleton<PlayerTracker>.Instance.GetPlayer().gameObject;
		rb = GetComponent<Rigidbody>();
		zm = GetComponent<ZombieMelee>();
		zp = GetComponent<ZombieProjectiles>();
		gc = GetComponentInChildren<GroundCheckEnemy>();
		if (!smr)
		{
			smr = GetComponentInChildren<SkinnedMeshRenderer>();
		}
		if ((bool)smr)
		{
			originalMaterial = smr.sharedMaterial;
		}
		if (!friendly)
		{
			target = player.transform;
			EnemyScanner componentInChildren = GetComponentInChildren<EnemyScanner>();
			if (componentInChildren != null)
			{
				componentInChildren.gameObject.SetActive(value: false);
			}
		}
		if (limp)
		{
			noheal = true;
		}
		SetSpeed();
		lmaskWater = lmask;
		lmaskWater = (int)lmaskWater | 0x10;
	}

	private void OnEnable()
	{
		attacking = false;
	}

	private void Update()
	{
		if (BlindEnemies.Blind)
		{
			target = null;
			blindedByCheat = true;
		}
		else if (blindedByCheat)
		{
			target = player.transform;
			blindedByCheat = false;
		}
		if ((bool)anim && variableSpeed)
		{
			anim.SetFloat("Speed", anim.speed);
		}
		if (knockBackCharge > 0f)
		{
			knockBackCharge = Mathf.MoveTowards(knockBackCharge, 0f, Time.deltaTime);
		}
		if (!limp && friendly && enemyTargets.Count > 0)
		{
			if (target == null)
			{
				float num = 100f;
				foreach (EnemyIdentifier enemyTarget in enemyTargets)
				{
					if (Vector3.Distance(base.transform.position, enemyTarget.transform.position) < num)
					{
						num = Vector3.Distance(base.transform.position, enemyTarget.transform.position);
						target = enemyTarget.transform;
						targetedEnemy = enemyTarget;
					}
				}
			}
			else if (targetedEnemy.dead)
			{
				enemyTargets.Remove(targetedEnemy);
				if (enemyTargets.Count == 0)
				{
					target = null;
				}
				else
				{
					float num2 = 100f;
					foreach (EnemyIdentifier enemyTarget2 in enemyTargets)
					{
						if (Vector3.Distance(base.transform.position, enemyTarget2.transform.position) < num2)
						{
							num2 = Vector3.Distance(base.transform.position, enemyTarget2.transform.position);
							target = enemyTarget2.transform;
							targetedEnemy = enemyTarget2;
						}
					}
				}
			}
		}
		if (falling && !limp)
		{
			fallTime += Time.deltaTime;
			if (gc.onGround)
			{
				if (fallSpeed <= -50f && !InvincibleEnemies.Enabled && !noFallDamage && !eid.blessed)
				{
					if (eid == null)
					{
						eid = GetComponent<EnemyIdentifier>();
					}
					eid.Splatter();
					return;
				}
				fallSpeed = 0f;
				nma.updatePosition = true;
				nma.updateRotation = true;
				if (aud == null)
				{
					aud = GetComponent<AudioSource>();
				}
				if (aud.clip == scream && aud.isPlaying)
				{
					aud.Stop();
				}
				rb.isKinematic = true;
				rb.useGravity = false;
				nma.enabled = true;
				nma.Warp(base.transform.position);
				falling = false;
				anim.SetBool("Falling", value: false);
			}
			else if (eid.underwater && aud.clip == scream && aud.isPlaying)
			{
				aud.Stop();
			}
			else if (fallTime > 0.05f && rb.velocity.y < fallSpeed)
			{
				fallSpeed = rb.velocity.y;
				reduceFallTime = 0.5f;
				if (aud == null)
				{
					aud = GetComponent<AudioSource>();
				}
				if (!aud.isPlaying && !limp && !eid.underwater && (!Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo, float.PositiveInfinity, lmaskWater, QueryTriggerInteraction.Collide) || ((hitInfo.distance > 32f || rb.velocity.y < -50f) && hitInfo.transform.gameObject.layer != 4)))
				{
					aud.clip = scream;
					aud.volume = 1f;
					aud.priority = 78;
					aud.pitch = Random.Range(0.8f, 1.2f);
					aud.Play();
				}
			}
			else if (fallTime > 0.05f && rb.velocity.y > fallSpeed)
			{
				reduceFallTime = Mathf.MoveTowards(reduceFallTime, 0f, Time.deltaTime);
				if (reduceFallTime <= 0f)
				{
					fallSpeed = rb.velocity.y;
				}
			}
			else if (rb.velocity.y > 0f)
			{
				fallSpeed = 0f;
			}
		}
		else if (fallTime > 0f)
		{
			fallTime = 0f;
		}
	}

	private void FixedUpdate()
	{
		if (limp)
		{
			return;
		}
		if (knockedBack && knockBackCharge <= 0f && rb.velocity.magnitude < 1f && gc.onGround)
		{
			StopKnockBack();
		}
		else if (knockedBack)
		{
			if (eid.useBrakes || gc.onGround)
			{
				if (knockBackCharge <= 0f && gc.onGround)
				{
					brakes = Mathf.MoveTowards(brakes, 0f, 0.0005f * brakes);
				}
				rb.velocity = new Vector3(rb.velocity.x * 0.95f * brakes, rb.velocity.y - juggleWeight, rb.velocity.z * 0.95f * brakes);
			}
			else if (!eid.useBrakes)
			{
				brakes = 1f;
			}
			nma.updatePosition = false;
			nma.updateRotation = false;
			nma.enabled = false;
			rb.isKinematic = false;
			rb.useGravity = true;
		}
		if (grounded && nma != null && nma.enabled && variableSpeed && nma.isOnNavMesh)
		{
			if (nma.isStopped || nma.velocity == Vector3.zero || stopped)
			{
				anim.speed = 1f * speedMultiplier;
			}
			else
			{
				anim.speed = nma.velocity.magnitude / nma.speed * speedMultiplier;
			}
		}
		else if (!grounded && gc.onGround)
		{
			grounded = true;
			nma.speed = defaultSpeed;
		}
		if (!gc.onGround && !falling && !nma.isOnOffMeshLink)
		{
			grounded = false;
			rb.isKinematic = false;
			rb.useGravity = true;
			nma.enabled = false;
			anim.speed = 1f;
			falling = true;
			anim.SetBool("Falling", value: true);
			anim.SetTrigger("StartFalling");
			if (zp != null)
			{
				zp.CancelAttack();
			}
			if (zm != null)
			{
				zm.CancelAttack();
			}
		}
	}

	public void KnockBack(Vector3 force)
	{
		if ((bool)rb)
		{
			nma.enabled = false;
			rb.isKinematic = false;
			rb.useGravity = true;
			if (!knockedBack || (!gc.onGround && rb.velocity.y < 0f))
			{
				rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
			}
			if (!gc.onGround)
			{
				rb.AddForce(Vector3.up, ForceMode.VelocityChange);
			}
			rb.AddForce(force / 10f, ForceMode.VelocityChange);
			knockedBack = true;
			knockBackCharge = Mathf.Min(knockBackCharge + force.magnitude / 1500f, 0.35f);
			brakes = 1f;
		}
	}

	public void StopKnockBack()
	{
		if (!(nma != null))
		{
			return;
		}
		if (Physics.Raycast(base.transform.position + Vector3.up * 0.1f, Vector3.down, out var hitInfo, float.PositiveInfinity, lmask))
		{
			_ = Vector3.zero;
			if (NavMesh.SamplePosition(hitInfo.point, out var hit, 4f, nma.areaMask))
			{
				knockedBack = false;
				nma.updatePosition = true;
				nma.updateRotation = true;
				nma.enabled = true;
				rb.isKinematic = true;
				juggleWeight = 0f;
				eid.pulledByMagnet = false;
				nma.Warp(hit.position);
			}
			else
			{
				knockBackCharge = 0.5f;
			}
		}
		else
		{
			knockBackCharge = 0.5f;
		}
	}

	public void GetHurt(GameObject target, Vector3 force, float multiplier, float critMultiplier, GameObject sourceWeapon = null)
	{
		string hitLimb = "";
		bool flag = false;
		bool flag2 = false;
		bool flag3 = true;
		if (eid == null)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if ((bool)gc && !gc.onGround && eid.hitter != "fire")
		{
			multiplier *= 1.5f;
		}
		if (force != Vector3.zero && !limp)
		{
			KnockBack(force / 100f);
			if (eid.hitter == "heavypunch" || (eid.hitter == "cannonball" && (bool)gc && !gc.onGround))
			{
				eid.useBrakes = false;
			}
			else
			{
				eid.useBrakes = true;
			}
		}
		if (!MonoSingleton<PrefsManager>.Instance.GetBoolLocal("bloodEnabled"))
		{
			flag3 = false;
		}
		if (chestExploding && health <= 0f && (target.gameObject.tag == "Limb" || target.gameObject.tag == "EndLimb") && target.GetComponentInParent<EnemyIdentifier>() != null)
		{
			ChestExplodeEnd();
		}
		GameObject gameObject = null;
		if (bsm == null)
		{
			bsm = MonoSingleton<BloodsplatterManager>.Instance;
		}
		if (eid.hitter == "punch" && attacking)
		{
			if (!InvincibleEnemies.Enabled && !eid.blessed)
			{
				health -= 5f;
			}
			attacking = false;
			MonoSingleton<FistControl>.Instance.currentPunch.Parry(hook: false, eid);
		}
		if (target.gameObject.tag == "Head")
		{
			float num = 1f * multiplier + multiplier * critMultiplier;
			if (!eid.blessed && !InvincibleEnemies.Enabled)
			{
				health -= num;
			}
			if (eid.hitter != "fire" && num > 0f)
			{
				gameObject = ((!(num >= 1f) && !(health <= 0f)) ? bsm.GetGore(GoreType.Small, eid.underwater, eid.sandified, eid.blessed) : bsm.GetGore(GoreType.Head, eid.underwater, eid.sandified, eid.blessed));
			}
			_ = (player.transform.position - base.transform.position).normalized;
			if (!limp)
			{
				flag2 = true;
				hitLimb = "head";
			}
			if (health <= 0f)
			{
				if (!limp)
				{
					GoLimp();
				}
				if (eid.hitter != "fire" && eid.hitter != "sawblade")
				{
					float num2 = 1f;
					if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone")
					{
						num2 = 0.5f;
					}
					else if (eid.hitter == "Explosion")
					{
						num2 = 0.25f;
					}
					if (target.transform.parent != null && target.transform.parent.GetComponentInParent<Rigidbody>() != null)
					{
						target.transform.parent.GetComponentInParent<Rigidbody>().AddForce(force * 10f);
					}
					if (flag3 && eid.hitter != "harpoon")
					{
						GameObject gameObject2 = null;
						GoreZone goreZone = GetGoreZone();
						for (int i = 0; (float)i < 6f * num2; i++)
						{
							gameObject2 = bsm.GetGib(GibType.Skull);
							if ((bool)gameObject2 && (bool)goreZone && (bool)goreZone.gibZone)
							{
								ReadyGib(gameObject2, target);
							}
						}
						for (int j = 0; (float)j < 4f * num2; j++)
						{
							gameObject2 = bsm.GetGib(GibType.Brain);
							if ((bool)gameObject2 && (bool)goreZone && (bool)goreZone.gibZone)
							{
								ReadyGib(gameObject2, target);
							}
						}
						for (int k = 0; (float)k < 2f * num2; k++)
						{
							gameObject2 = bsm.GetGib(GibType.Eye);
							if ((bool)gameObject2 && (bool)goreZone && (bool)goreZone.gibZone)
							{
								ReadyGib(gameObject2, target);
							}
							gameObject2 = bsm.GetGib(GibType.Jaw);
							if ((bool)gameObject2 && (bool)goreZone && (bool)goreZone.gibZone)
							{
								ReadyGib(gameObject2, target);
							}
						}
					}
				}
			}
		}
		else if (target.gameObject.tag == "Limb" || target.gameObject.tag == "EndLimb")
		{
			if (eid == null)
			{
				eid = GetComponent<EnemyIdentifier>();
			}
			float num = 1f * multiplier + 0.5f * multiplier * critMultiplier;
			if (!eid.blessed && !InvincibleEnemies.Enabled)
			{
				health -= num;
			}
			if (eid.hitter != "fire" && num > 0f)
			{
				if (((num >= 1f || health <= 0f) && eid.hitter != "explosion") || (eid.hitter == "explosion" && target.gameObject.tag == "EndLimb"))
				{
					gameObject = bsm.GetGore(GoreType.Limb, eid.underwater, eid.sandified, eid.blessed);
				}
				else if (eid.hitter != "explosion")
				{
					gameObject = bsm.GetGore(GoreType.Small, eid.underwater, eid.sandified, eid.blessed);
				}
			}
			_ = (player.transform.position - base.transform.position).normalized;
			if (!limp)
			{
				flag2 = true;
				hitLimb = "limb";
			}
			if (health <= 0f)
			{
				if (!limp)
				{
					GoLimp();
				}
				if (eid.hitter == "sawblade")
				{
					if (!chestExploded && target.transform.position.y > chest.transform.position.y - 1f)
					{
						ChestExplosion(cut: true);
					}
				}
				else if (eid.hitter != "fire" && eid.hitter != "harpoon")
				{
					if (target.gameObject.tag == "Limb" && flag3 && eid.hitter != "explosion")
					{
						float num3 = 1f;
						GetGoreZone();
						if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone")
						{
							num3 = 0.5f;
						}
						for (int l = 0; (float)l < 4f * num3; l++)
						{
							GameObject gib = bsm.GetGib(GibType.Gib);
							if ((bool)gib && (bool)gz && (bool)gz.gibZone)
							{
								ReadyGib(gib, target);
							}
						}
					}
					else
					{
						target.transform.localScale = Vector3.zero;
					}
				}
			}
		}
		else if (target.gameObject.tag == "Body" || (attacking && eid.hitter == "shotgunzone" && MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude > 18f) || eid.hitter == "sawblade")
		{
			float num = multiplier;
			if (eid == null)
			{
				eid = GetComponent<EnemyIdentifier>();
			}
			if (eid.hitter == "shotgunzone")
			{
				if (!attacking && (target.gameObject != chest || health - num > 0f))
				{
					num = 0f;
				}
				else if (attacking && (target.gameObject == chest || MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude > 18f))
				{
					if (!InvincibleEnemies.Enabled && !eid.blessed)
					{
						num *= 2f;
					}
					MonoSingleton<FistControl>.Instance.currentPunch.Parry(hook: false, eid);
				}
			}
			if (!eid.blessed && !InvincibleEnemies.Enabled)
			{
				health -= num;
			}
			if (eid.hitter != "fire" && num > 0f)
			{
				gameObject = ((!(num >= 1f) && !(health <= 0f)) ? bsm.GetGore(GoreType.Small, eid.underwater, eid.sandified, eid.blessed) : bsm.GetGore(GoreType.Body, eid.underwater, eid.sandified, eid.blessed));
			}
			if (health <= 0f && target.gameObject == chest && eid.hitter != "fire")
			{
				if (eid.hitter == "shotgunzone" || eid.hitter == "sawblade")
				{
					chestHP = 0f;
				}
				else
				{
					chestHP -= num;
				}
				if (chestHP <= 0f && eid.hitter != "harpoon")
				{
					ChestExplosion(eid.hitter == "sawblade");
				}
			}
			if (!limp)
			{
				flag2 = true;
				hitLimb = "body";
			}
			if (health <= 0f)
			{
				if (!limp)
				{
					GoLimp();
				}
				if (eid.hitter != "sawblade" && target.GetComponentInParent<Rigidbody>() != null)
				{
					target.GetComponentInParent<Rigidbody>().AddForce(force * 10f);
				}
			}
		}
		if (gameObject != null)
		{
			GetGoreZone();
			gameObject.transform.position = target.transform.position;
			if (eid.hitter == "drill")
			{
				gameObject.transform.localScale *= 2f;
			}
			if (gz != null && gz.goreZone != null)
			{
				gameObject.transform.SetParent(gz.goreZone, worldPositionStays: true);
			}
			Bloodsplatter component = gameObject.GetComponent<Bloodsplatter>();
			if ((bool)component)
			{
				ParticleSystem.CollisionModule collision = component.GetComponent<ParticleSystem>().collision;
				if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone" || eid.hitter == "explosion")
				{
					if (Random.Range(0f, 1f) > 0.5f)
					{
						collision.enabled = false;
					}
					component.hpAmount = 3;
				}
				else if (eid.hitter == "nail")
				{
					component.hpAmount = 1;
					component.GetComponent<AudioSource>().volume *= 0.8f;
				}
				if (!noheal)
				{
					component.GetReady();
				}
			}
		}
		if (health <= 0f)
		{
			if (eid.hitter == "sawblade")
			{
				Cut(target);
			}
			else if ((target.gameObject.CompareTag("Limb") || target.gameObject.CompareTag("Head")) && eid.hitter != "harpoon" && eid.hitter != "fire")
			{
				if (target.transform.childCount > 0)
				{
					Transform child = target.transform.GetChild(0);
					CharacterJoint[] componentsInChildren = target.GetComponentsInChildren<CharacterJoint>();
					GoreZone goreZone2 = GetGoreZone();
					if (componentsInChildren.Length != 0)
					{
						CharacterJoint[] array = componentsInChildren;
						foreach (CharacterJoint characterJoint in array)
						{
							if (goreZone2 != null && goreZone2.goreZone != null)
							{
								characterJoint.transform.SetParent(goreZone2.goreZone);
							}
							Object.Destroy(characterJoint);
						}
					}
					CharacterJoint component2 = target.GetComponent<CharacterJoint>();
					if (component2 != null)
					{
						component2.connectedBody = null;
						Object.Destroy(component2);
					}
					target.transform.position = child.position;
					target.transform.SetParent(child);
					if (goreZone2 != null && goreZone2.gibZone != null)
					{
						child.SetParent(goreZone2.gibZone, worldPositionStays: true);
					}
					Object.Destroy(target.GetComponent<Rigidbody>());
				}
				Object.Destroy(target.GetComponent<Collider>());
				target.transform.localScale = Vector3.zero;
			}
			else if (target.gameObject.tag == "EndLimb" && eid.hitter != "harpoon" && eid.hitter != "fire")
			{
				target.transform.localScale = Vector3.zero;
			}
		}
		if (health > 0f && !limp && hurtSounds.Length != 0 && !eid.blessed && eid.hitter != "blocked")
		{
			if (aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
			aud.volume = hurtSoundVol;
			aud.pitch = Random.Range(0.85f, 1.35f);
			aud.priority = 12;
			aud.Play();
		}
		if (eid == null)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (multiplier == 0f)
		{
			flag2 = false;
		}
		if (!flag2 || !(eid.hitter != "enemy"))
		{
			return;
		}
		if (scalc == null)
		{
			scalc = MonoSingleton<StyleCalculator>.Instance;
		}
		if (health <= 0f)
		{
			flag = true;
			if ((bool)gc && !gc.onGround)
			{
				if (eid.hitter == "explosion" || eid.hitter == "ffexplosion" || eid.hitter == "railcannon")
				{
					scalc.shud.AddPoints(120, "ultrakill.fireworks", sourceWeapon, eid);
				}
				else if (eid.hitter == "ground slam")
				{
					scalc.shud.AddPoints(160, "ultrakill.airslam", sourceWeapon, eid);
				}
				else if (eid.hitter != "deathzone")
				{
					scalc.shud.AddPoints(50, "ultrakill.airshot", sourceWeapon, eid);
				}
			}
		}
		if (eid.hitter != "secret" && (bool)scalc)
		{
			scalc.HitCalculator(eid.hitter, "zombie", hitLimb, flag, eid, sourceWeapon);
		}
		if (flag && eid.hitter != "fire")
		{
			Flammable componentInChildren = GetComponentInChildren<Flammable>();
			if ((bool)componentInChildren && componentInChildren.burning && (bool)scalc)
			{
				scalc.shud.AddPoints(50, "ultrakill.finishedoff", sourceWeapon, eid);
			}
		}
	}

	public void GoLimp()
	{
		if (limp)
		{
			return;
		}
		gz = GetGoreZone();
		attacking = false;
		Invoke("StopHealing", 1f);
		health = 0f;
		if (eid == null)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (!friendly && !eid.dontCountAsKills)
		{
			if (gz != null && gz.checkpoint != null)
			{
				gz.AddDeath();
				gz.checkpoint.sm.kills++;
			}
			else
			{
				MonoSingleton<StatsManager>.Instance.kills++;
			}
		}
		EnemySimplifier[] componentsInChildren = GetComponentsInChildren<EnemySimplifier>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Begone();
		}
		if (deadMaterial != null)
		{
			if ((bool)smr)
			{
				smr.sharedMaterial = deadMaterial;
			}
			else if ((bool)smr)
			{
				smr.sharedMaterial = originalMaterial;
			}
		}
		if (zm != null)
		{
			zm.track = false;
			if (!chestExploding)
			{
				anim.StopPlayback();
			}
			if (zm.tr != null)
			{
				zm.tr.enabled = false;
			}
			Object.Destroy(base.gameObject.GetComponentInChildren<SwingCheck2>().gameObject);
			Object.Destroy(zm);
		}
		if (zp != null)
		{
			zp.DamageEnd();
			if (!chestExploding)
			{
				anim.StopPlayback();
			}
			if (zp.hasMelee)
			{
				zp.MeleeDamageEnd();
			}
			Object.Destroy(zp);
			Projectile componentInChildren = GetComponentInChildren<Projectile>();
			if (componentInChildren != null)
			{
				Object.Destroy(componentInChildren.gameObject);
			}
		}
		if (nma != null)
		{
			Object.Destroy(nma);
		}
		if (!chestExploding)
		{
			Object.Destroy(anim);
		}
		Object.Destroy(base.gameObject.GetComponent<Collider>());
		if (rb == null)
		{
			rb = GetComponent<Rigidbody>();
		}
		Object.Destroy(rb);
		if (aud == null)
		{
			aud = GetComponent<AudioSource>();
		}
		if (deathSound != null)
		{
			aud.clip = deathSound;
			if (eid.hitter != "fire")
			{
				aud.volume = deathSoundVol;
			}
			else
			{
				aud.volume = 0.5f;
			}
			aud.pitch = Random.Range(0.85f, 1.35f);
			aud.priority = 11;
			aud.Play();
		}
		if (!limp && !chestExploding)
		{
			rbs = GetComponentsInChildren<Rigidbody>();
			Rigidbody[] array = rbs;
			foreach (Rigidbody obj in array)
			{
				obj.isKinematic = false;
				obj.useGravity = true;
			}
		}
		if (!limp)
		{
			if (!eid.dontCountAsKills)
			{
				ActivateNextWave componentInParent = GetComponentInParent<ActivateNextWave>();
				if (componentInParent != null)
				{
					componentInParent.AddDeadEnemy();
				}
			}
			if (gz != null && gz.gibZone != null)
			{
				base.transform.SetParent(gz.gibZone, worldPositionStays: true);
			}
		}
		if (musicRequested)
		{
			musicRequested = false;
			MonoSingleton<MusicManager>.Instance.PlayCleanMusic();
		}
		limp = true;
		EnemyScanner componentInChildren2 = GetComponentInChildren<EnemyScanner>();
		if (componentInChildren2 != null)
		{
			Object.Destroy(componentInChildren2.gameObject);
		}
	}

	public void ChestExplodeEnd()
	{
		anim.enabled = false;
		anim.StopPlayback();
		Object.Destroy(anim);
		rbs = GetComponentsInChildren<Rigidbody>();
		Rigidbody[] array = rbs;
		foreach (Rigidbody rigidbody in array)
		{
			if (rigidbody != null)
			{
				rigidbody.isKinematic = false;
				rigidbody.useGravity = true;
			}
		}
		chestExploding = false;
	}

	public void StopHealing()
	{
		noheal = true;
	}

	private void ReadyGib(GameObject tempGib, GameObject target)
	{
		tempGib.transform.position = target.transform.position;
		tempGib.transform.rotation = Random.rotation;
		GetGoreZone();
		tempGib.transform.SetParent(gz.gibZone);
	}

	public void ChestExplosion(bool cut = false)
	{
		if (chestExploded)
		{
			return;
		}
		GoreZone goreZone = GetGoreZone();
		if (!cut)
		{
			CharacterJoint[] componentsInChildren = chest.GetComponentsInChildren<CharacterJoint>();
			if (componentsInChildren.Length != 0)
			{
				CharacterJoint[] array = componentsInChildren;
				foreach (CharacterJoint characterJoint in array)
				{
					if (characterJoint.transform.parent.parent == chest.transform)
					{
						Rigidbody[] componentsInChildren2 = characterJoint.transform.GetComponentsInChildren<Rigidbody>();
						foreach (Rigidbody obj in componentsInChildren2)
						{
							obj.isKinematic = false;
							obj.useGravity = true;
						}
						Object.Destroy(characterJoint);
					}
					else if (characterJoint.transform == chest.transform)
					{
						if (characterJoint.TryGetComponent<Collider>(out var component))
						{
							Object.Destroy(component);
						}
						Object.Destroy(characterJoint);
					}
				}
			}
			if (chest.TryGetComponent<Rigidbody>(out var component2))
			{
				Object.Destroy(component2);
			}
			if (!limp && !eid.exploded && !eid.dead)
			{
				if (gc.onGround)
				{
					rb.isKinematic = true;
					knockedBack = false;
				}
				anim.Rebind();
				anim.speed = 1f;
				anim.SetTrigger("ChestExplosion");
				chestExploding = true;
			}
		}
		GetGoreZone();
		if (MonoSingleton<PrefsManager>.Instance.GetBoolLocal("bloodEnabled"))
		{
			for (int k = 0; k < 6; k++)
			{
				GameObject gib = bsm.GetGib((k < 2) ? GibType.Jaw : GibType.Gib);
				if ((bool)gib && (bool)goreZone && (bool)goreZone.gibZone)
				{
					ReadyGib(gib, chest);
				}
			}
			if (!eid.sandified)
			{
				Object.Instantiate(chestExplosionStuff, chest.transform.parent.position, chest.transform.parent.rotation).transform.SetParent(chest.transform.parent, worldPositionStays: true);
			}
		}
		EnemyIdentifierIdentifier[] componentsInChildren3 = chest.GetComponentsInChildren<EnemyIdentifierIdentifier>();
		for (int l = 0; l < componentsInChildren3.Length; l++)
		{
			if (!componentsInChildren3[l])
			{
				continue;
			}
			GoreType got;
			switch (componentsInChildren3[l].gameObject.tag)
			{
			case "Head":
				got = GoreType.Head;
				break;
			case "EndLimb":
			case "Limb":
				got = GoreType.Limb;
				break;
			default:
				got = GoreType.Body;
				break;
			}
			GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(got, eid.underwater, eid.sandified, eid.blessed);
			if ((bool)gore)
			{
				gore.transform.position = chest.transform.position;
				Bloodsplatter component3 = gore.GetComponent<Bloodsplatter>();
				if ((bool)component3)
				{
					component3.hpAmount = 10;
				}
				if (goreZone != null && goreZone.goreZone != null)
				{
					gore.transform.SetParent(goreZone.goreZone, worldPositionStays: true);
				}
				if (!noheal && (bool)component3)
				{
					component3.GetReady();
				}
			}
		}
		if (!cut)
		{
			chest.transform.localScale = Vector3.zero;
		}
		else
		{
			if (!limp)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(50, "ultrakill.halfoff", null, eid);
			}
			Cut(chest);
		}
		chestExploded = true;
	}

	public void Cut(GameObject target)
	{
		if (target.TryGetComponent<CharacterJoint>(out var component))
		{
			Object.Destroy(component);
			target.transform.SetParent(gz.gibZone, worldPositionStays: true);
			Rigidbody[] componentsInChildren = target.transform.GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody obj in componentsInChildren)
			{
				obj.isKinematic = false;
				obj.useGravity = true;
				obj.angularDrag = 0.001f;
				obj.maxAngularVelocity = float.PositiveInfinity;
				obj.velocity = Vector3.zero;
				obj.AddForce(Vector3.up * (target.CompareTag("Head") ? 250 : 25), ForceMode.VelocityChange);
				obj.AddTorque(target.transform.right * 1f, ForceMode.VelocityChange);
			}
		}
	}
}
