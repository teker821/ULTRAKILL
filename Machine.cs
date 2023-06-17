using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Machine : MonoBehaviour
{
	public float health;

	private BloodsplatterManager bsm;

	private GameObject player;

	public bool limp;

	private EnemyIdentifier eid;

	public GameObject chest;

	private float chestHP = 3f;

	private AudioSource aud;

	public AudioClip[] hurtSounds;

	[HideInInspector]
	public StyleCalculator scalc;

	private GoreZone gz;

	public Material deadMaterial;

	private Material originalMaterial;

	public SkinnedMeshRenderer smr;

	private NavMeshAgent nma;

	private Rigidbody rb;

	private Rigidbody[] rbs;

	private Animator anim;

	public AudioClip deathSound;

	public AudioClip scream;

	private bool noheal;

	public bool bigKill;

	public bool parryable;

	public bool partiallyParryable;

	[HideInInspector]
	public List<Transform> parryables = new List<Transform>();

	private SwordsMachine sm;

	private Streetcleaner sc;

	private V2 v2;

	private Mindflayer mf;

	private Sisyphus sisy;

	private Turret tur;

	private Ferryman fm;

	public GameObject[] destroyOnDeath;

	public Machine symbiote;

	private bool symbiotic;

	private bool healing;

	public bool grounded;

	private GroundCheckEnemy gc;

	public bool knockedBack;

	public bool overrideFalling;

	private float knockBackCharge;

	public float brakes;

	public float juggleWeight;

	public bool falling;

	private LayerMask lmask;

	private LayerMask lmaskWater;

	private float fallSpeed;

	private float fallTime;

	private float reduceFallTime;

	public bool noFallDamage;

	public bool dontDie;

	public bool dismemberment;

	private CameraController cc;

	public bool specialDeath;

	[HideInInspector]
	public bool musicRequested;

	public UnityEvent onDeath;

	private void Start()
	{
		cc = MonoSingleton<CameraController>.Instance;
		player = MonoSingleton<NewMovement>.Instance.gameObject;
		nma = GetComponent<NavMeshAgent>();
		bsm = MonoSingleton<BloodsplatterManager>.Instance;
		rbs = GetComponentsInChildren<Rigidbody>();
		anim = GetComponentInChildren<Animator>();
		eid = GetComponent<EnemyIdentifier>();
		if (smr != null)
		{
			originalMaterial = smr.material;
		}
		switch (eid.enemyType)
		{
		case EnemyType.Swordsmachine:
			sm = GetComponent<SwordsMachine>();
			break;
		case EnemyType.Streetcleaner:
			sc = GetComponent<Streetcleaner>();
			break;
		case EnemyType.V2:
			v2 = GetComponent<V2>();
			break;
		case EnemyType.Mindflayer:
			mf = GetComponent<Mindflayer>();
			break;
		case EnemyType.Sisyphus:
			sisy = GetComponent<Sisyphus>();
			break;
		case EnemyType.Turret:
			tur = GetComponent<Turret>();
			break;
		case EnemyType.Ferryman:
			fm = GetComponent<Ferryman>();
			break;
		}
		if (symbiote != null)
		{
			symbiotic = true;
		}
		gc = GetComponentInChildren<GroundCheckEnemy>();
		rb = GetComponent<Rigidbody>();
		if (!gz)
		{
			gz = GoreZone.ResolveGoreZone(base.transform);
		}
		if (!musicRequested && !eid.dead && (sm == null || !sm.friendly))
		{
			musicRequested = true;
			MonoSingleton<MusicManager>.Instance.PlayBattleMusic();
		}
		lmask = (int)lmask | 0x100;
		lmask = (int)lmask | 0x1000000;
		lmaskWater = lmask;
		lmaskWater = (int)lmaskWater | 0x10;
	}

	private void OnEnable()
	{
		parryable = false;
		partiallyParryable = false;
	}

	private void Update()
	{
		if (knockBackCharge > 0f)
		{
			knockBackCharge = Mathf.MoveTowards(knockBackCharge, 0f, Time.deltaTime);
		}
		if (healing && !limp && (bool)symbiote)
		{
			health = Mathf.MoveTowards(health, symbiote.health, Time.deltaTime * 10f);
			eid.health = health;
			if (health >= symbiote.health)
			{
				healing = false;
				if ((bool)sm)
				{
					sm.downed = false;
				}
				if ((bool)sisy)
				{
					sisy.downed = false;
				}
			}
		}
		if (falling && rb != null && !overrideFalling && (!nma || !nma.isOnOffMeshLink))
		{
			fallTime += Time.deltaTime;
			if (gc.onGround && falling && nma != null)
			{
				if (fallSpeed <= -60f && !noFallDamage && !InvincibleEnemies.Enabled && !eid.blessed)
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
				if (!sm || !sm.moveAtTarget)
				{
					rb.isKinematic = true;
				}
				if (aud == null)
				{
					aud = GetComponent<AudioSource>();
				}
				if ((bool)aud && aud.clip == scream && aud.isPlaying)
				{
					aud.Stop();
				}
				rb.useGravity = false;
				nma.enabled = true;
				nma.Warp(base.transform.position);
				falling = false;
				anim.SetBool("Falling", value: false);
			}
			else if (eid.underwater && (bool)aud && aud.clip == scream && aud.isPlaying)
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
				if ((bool)aud && !aud.isPlaying && !limp && !noFallDamage && !eid.underwater && (!Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo, float.PositiveInfinity, lmaskWater, QueryTriggerInteraction.Collide) || ((hitInfo.distance > 42f || rb.velocity.y < -60f) && hitInfo.transform.gameObject.layer != 4)))
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
		if (limp || !(gc != null) || overrideFalling)
		{
			return;
		}
		if (knockedBack && knockBackCharge <= 0f && (rb.velocity.magnitude < 1f || v2 != null) && gc.onGround)
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
			if (nma != null)
			{
				nma.updatePosition = false;
				nma.updateRotation = false;
				nma.enabled = false;
				rb.isKinematic = false;
				rb.useGravity = true;
			}
		}
		if (!grounded && gc.onGround)
		{
			grounded = true;
		}
		else if (grounded && !gc.onGround)
		{
			grounded = false;
		}
		if (!gc.onGround && !falling && nma != null && !nma.isOnOffMeshLink)
		{
			rb.isKinematic = false;
			rb.useGravity = true;
			nma.enabled = false;
			falling = true;
			anim.SetBool("Falling", value: true);
			if (sc != null)
			{
				sc.StopFire();
			}
			if (tur != null)
			{
				tur.CancelAim(instant: true);
			}
		}
	}

	public void KnockBack(Vector3 force)
	{
		if ((!(sc == null) && sc.dodging) || (!(sm == null) && sm.inAction) || (!(tur == null) && tur.lodged) || eid.poise)
		{
			return;
		}
		if (nma != null)
		{
			nma.enabled = false;
			rb.isKinematic = false;
			rb.useGravity = true;
		}
		if ((bool)gc && !overrideFalling)
		{
			if (!knockedBack || (!gc.onGround && rb.velocity.y < 0f))
			{
				rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
			}
			if (!gc.onGround)
			{
				rb.AddForce(Vector3.up, ForceMode.VelocityChange);
			}
		}
		rb.AddForce(force / 10f, ForceMode.VelocityChange);
		knockedBack = true;
		knockBackCharge = Mathf.Min(knockBackCharge + force.magnitude / 1500f, 0.35f);
		brakes = 1f;
	}

	public void StopKnockBack()
	{
		knockBackCharge = 0f;
		if (nma != null)
		{
			if (Physics.Raycast(base.transform.position + Vector3.up * 0.1f, Vector3.down, out var hitInfo, float.PositiveInfinity, lmask))
			{
				_ = Vector3.zero;
				if (NavMesh.SamplePosition(hitInfo.point, out var hit, 4f, nma.areaMask))
				{
					knockedBack = false;
					nma.updatePosition = true;
					nma.updateRotation = true;
					nma.enabled = true;
					if (!sm || !sm.moveAtTarget)
					{
						rb.isKinematic = true;
					}
					juggleWeight = 0f;
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
		else if (v2 != null)
		{
			knockedBack = false;
			juggleWeight = 0f;
		}
	}

	public void GetHurt(GameObject target, Vector3 force, float multiplier, float critMultiplier, GameObject sourceWeapon = null)
	{
		string hitLimb = "";
		bool dead = false;
		bool flag = false;
		float num = multiplier;
		GameObject gameObject = null;
		if (eid == null)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (force != Vector3.zero && !limp && sm == null && (v2 == null || !v2.inIntro) && (tur == null || !tur.lodged || eid.hitter == "heavypunch" || eid.hitter == "railcannon" || eid.hitter == "cannonball"))
		{
			if ((bool)tur && tur.lodged)
			{
				tur.CancelAim(instant: true);
				tur.Unlodge();
			}
			KnockBack(force / 100f);
			if (eid.hitter == "heavypunch" || ((bool)gc && !gc.onGround && eid.hitter == "cannonball"))
			{
				eid.useBrakes = false;
			}
			else
			{
				eid.useBrakes = true;
			}
		}
		if (v2 != null && v2.secondEncounter && eid.hitter == "heavypunch")
		{
			v2.InstaEnrage();
		}
		if (sc != null && target.gameObject == sc.canister && !sc.canisterHit && eid.hitter == "revolver")
		{
			if (!InvincibleEnemies.Enabled && !eid.blessed)
			{
				sc.canisterHit = true;
			}
			if (!eid.dead && !InvincibleEnemies.Enabled && !eid.blessed)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(200, "ultrakill.instakill", sourceWeapon, eid);
			}
			MonoSingleton<TimeController>.Instance.ParryFlash();
			Invoke("CanisterExplosion", 0.1f);
			return;
		}
		if (tur != null && tur.aiming && (eid.hitter == "revolver" || eid.hitter == "coin") && tur.interruptables.Contains(target.transform))
		{
			tur.Interrupt();
		}
		if (eid.hitter == "punch" && (parryable || (partiallyParryable && parryables != null && parryables.Contains(target.transform))))
		{
			parryable = false;
			partiallyParryable = false;
			parryables.Clear();
			if (!InvincibleEnemies.Enabled && !eid.blessed)
			{
				health -= 5f;
			}
			MonoSingleton<FistControl>.Instance.currentPunch.Parry(hook: false, eid);
			if (sm != null && health > 0f)
			{
				if (!sm.enraged)
				{
					sm.Knockdown(light: true);
				}
				else
				{
					sm.Enrage();
				}
			}
			else
			{
				SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
			}
		}
		if ((bool)sisy && num > 0f)
		{
			if (eid.burners.Count > 0)
			{
				if (eid.hitter != "fire")
				{
					if (num <= 0.5f)
					{
						gameObject = bsm.GetGore(GoreType.Limb, eid.underwater, eid.sandified, eid.blessed);
						sisy.PlayHurtSound(1);
					}
					else
					{
						gameObject = bsm.GetGore(GoreType.Head, eid.underwater, eid.sandified, eid.blessed);
						sisy.PlayHurtSound(2);
					}
				}
				else
				{
					sisy.PlayHurtSound();
				}
			}
			else if (eid.hitter != "fire")
			{
				gameObject = bsm.GetGore(GoreType.Smallest, eid.underwater, eid.sandified, eid.blessed);
			}
		}
		float num2 = 0f;
		if (target.gameObject.CompareTag("Head"))
		{
			num2 = 1f;
		}
		else if (target.gameObject.CompareTag("Limb") || target.gameObject.CompareTag("EndLimb"))
		{
			num2 = 0.5f;
		}
		num = multiplier + num2 * multiplier * critMultiplier;
		if (num2 == 0f && eid.hitter == "shotgunzone")
		{
			if (!parryable && (target.gameObject != chest || health - num > 0f))
			{
				num = 0f;
			}
			else if ((parryable && (target.gameObject == chest || MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude > 18f)) || (partiallyParryable && parryables != null && parryables.Contains(target.transform)))
			{
				num *= 1.5f;
				parryable = false;
				partiallyParryable = false;
				parryables.Clear();
				MonoSingleton<FistControl>.Instance.currentPunch.Parry(hook: false, eid);
				if (sm != null && health - num > 0f)
				{
					if (!sm.enraged)
					{
						sm.Knockdown(light: true);
					}
					else
					{
						sm.Enrage();
					}
				}
				else
				{
					SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		if ((bool)sisy && !limp && eid.hitter == "fire" && health > 0f && health - num < 0.01f)
		{
			num = health - 0.01f;
		}
		if (!eid.blessed && !InvincibleEnemies.Enabled)
		{
			health -= num;
		}
		if (!gameObject && eid.hitter != "fire" && num > 0f)
		{
			if (num2 == 1f && (num >= 1f || health <= 0f))
			{
				gameObject = bsm.GetGore(GoreType.Head, eid.underwater, eid.sandified, eid.blessed);
			}
			else if (((num >= 1f || health <= 0f) && eid.hitter != "explosion") || (eid.hitter == "explosion" && target.gameObject.tag == "EndLimb"))
			{
				gameObject = ((!target.gameObject.CompareTag("Body")) ? bsm.GetGore(GoreType.Limb, eid.underwater, eid.sandified, eid.blessed) : bsm.GetGore(GoreType.Body, eid.underwater, eid.sandified, eid.blessed));
			}
			else if (eid.hitter != "explosion")
			{
				gameObject = bsm.GetGore(GoreType.Small, eid.underwater, eid.sandified, eid.blessed);
			}
		}
		if (!limp)
		{
			flag = true;
			string text = target.gameObject.tag.ToLower();
			if (text == "endlimb")
			{
				text = "limb";
			}
			hitLimb = text;
		}
		if (health <= 0f)
		{
			if (symbiotic)
			{
				if (sm != null && !sm.downed && symbiote.health > 0f)
				{
					sm.downed = true;
					sm.Down();
					Invoke("StartHealing", 3f);
				}
				else if (sisy != null && !sisy.downed && symbiote.health > 0f)
				{
					sisy.downed = true;
					sisy.Knockdown(base.transform.position + base.transform.forward);
					Invoke("StartHealing", 3f);
				}
				else if (symbiote.health <= 0f)
				{
					symbiotic = false;
					if (!limp)
					{
						GoLimp();
					}
				}
			}
			else
			{
				if (!limp)
				{
					GoLimp();
				}
				if (OptionsMenuToManager.bloodEnabled && !target.gameObject.CompareTag("EndLimb"))
				{
					float num3 = 1f;
					if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone" || eid.hitter == "explosion")
					{
						num3 = 0.5f;
					}
					string text2 = target.gameObject.tag;
					if (!(text2 == "Head"))
					{
						if (text2 == "Limb")
						{
							for (int i = 0; (float)i < 4f * num3; i++)
							{
								GameObject gib = bsm.GetGib(GibType.Gib);
								if ((bool)gib && (bool)gz && (bool)gz.gibZone)
								{
									ReadyGib(gib, target);
								}
							}
							if (target.transform.childCount > 0 && dismemberment)
							{
								Transform child = target.transform.GetChild(0);
								CharacterJoint[] componentsInChildren = target.GetComponentsInChildren<CharacterJoint>();
								if (componentsInChildren.Length != 0)
								{
									CharacterJoint[] array = componentsInChildren;
									for (int j = 0; j < array.Length; j++)
									{
										Object.Destroy(array[j]);
									}
								}
								CharacterJoint component = target.GetComponent<CharacterJoint>();
								if (component != null)
								{
									component.connectedBody = null;
									Object.Destroy(component);
								}
								target.transform.position = child.position;
								target.transform.SetParent(child);
								child.SetParent(gz.gibZone);
								Object.Destroy(target.GetComponent<Rigidbody>());
							}
						}
					}
					else
					{
						for (int k = 0; (float)k < 6f * num3; k++)
						{
							GameObject gib = bsm.GetGib(GibType.Skull);
							if ((bool)gib && (bool)gz && (bool)gz.gibZone)
							{
								ReadyGib(gib, target);
							}
						}
						for (int l = 0; (float)l < 4f * num3; l++)
						{
							GameObject gib = bsm.GetGib(GibType.Brain);
							if ((bool)gib && (bool)gz && (bool)gz.gibZone)
							{
								ReadyGib(gib, target);
							}
						}
						for (int m = 0; (float)m < 2f * num3; m++)
						{
							GameObject gib = bsm.GetGib(GibType.Eye);
							if ((bool)gib && (bool)gz && (bool)gz.gibZone)
							{
								ReadyGib(gib, target);
							}
							gib = bsm.GetGib(GibType.Jaw);
							if ((bool)gib && (bool)gz && (bool)gz.gibZone)
							{
								ReadyGib(gib, target);
							}
						}
					}
				}
				if (dismemberment)
				{
					if (!target.gameObject.CompareTag("Body"))
					{
						if (target.TryGetComponent<Collider>(out var component2))
						{
							Object.Destroy(component2);
						}
						target.transform.localScale = Vector3.zero;
					}
					else if (target.gameObject == chest && v2 == null && sc == null)
					{
						chestHP -= num;
						if (chestHP <= 0f || eid.hitter == "shotgunzone")
						{
							CharacterJoint[] componentsInChildren2 = target.GetComponentsInChildren<CharacterJoint>();
							if (componentsInChildren2.Length != 0)
							{
								CharacterJoint[] array = componentsInChildren2;
								foreach (CharacterJoint characterJoint in array)
								{
									if (characterJoint.transform.parent.parent == chest.transform)
									{
										Object.Destroy(characterJoint);
										characterJoint.transform.parent = null;
									}
								}
							}
							if (OptionsMenuToManager.bloodEnabled)
							{
								for (int n = 0; n < 2; n++)
								{
									GameObject gib2 = bsm.GetGib(GibType.Gib);
									if ((bool)gib2 && (bool)gz && (bool)gz.gibZone)
									{
										ReadyGib(gib2, target);
									}
								}
							}
							GameObject gore = bsm.GetGore(GoreType.Head, eid.underwater, eid.sandified, eid.blessed);
							gore.transform.position = target.transform.position;
							gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
							target.transform.localScale = Vector3.zero;
						}
					}
				}
			}
			if (limp)
			{
				Rigidbody componentInParent = target.GetComponentInParent<Rigidbody>();
				if (componentInParent != null)
				{
					componentInParent.AddForce(force);
				}
			}
		}
		if (gameObject != null)
		{
			if (!gz)
			{
				gz = GoreZone.ResolveGoreZone(base.transform);
			}
			gameObject.transform.position = target.transform.position;
			if (eid.hitter == "drill")
			{
				gameObject.transform.localScale *= 2f;
			}
			if (gz != null && gz.goreZone != null)
			{
				gameObject.transform.SetParent(gz.goreZone, worldPositionStays: true);
			}
			Bloodsplatter component3 = gameObject.GetComponent<Bloodsplatter>();
			if ((bool)component3)
			{
				ParticleSystem.CollisionModule collision = component3.GetComponent<ParticleSystem>().collision;
				if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone" || eid.hitter == "explosion")
				{
					if (Random.Range(0f, 1f) > 0.5f)
					{
						collision.enabled = false;
					}
					component3.hpAmount = 3;
				}
				else if (eid.hitter == "nail")
				{
					component3.hpAmount = 1;
					component3.GetComponent<AudioSource>().volume *= 0.8f;
				}
				if (!noheal)
				{
					component3.GetReady();
				}
			}
		}
		if ((health > 0f || symbiotic) && hurtSounds.Length != 0 && !eid.blessed)
		{
			if (aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
			if ((bool)tur)
			{
				aud.volume = 0.85f;
			}
			else
			{
				aud.volume = 0.5f;
			}
			if (sm != null)
			{
				aud.pitch = Random.Range(0.85f, 1.35f);
			}
			else
			{
				aud.pitch = Random.Range(0.9f, 1.1f);
			}
			aud.priority = 12;
			aud.Play();
		}
		if (num == 0f)
		{
			flag = false;
		}
		if (!flag || !(eid.hitter != "enemy"))
		{
			return;
		}
		if (scalc == null)
		{
			scalc = MonoSingleton<StyleCalculator>.Instance;
		}
		if (health <= 0f && !symbiotic && (v2 == null || !v2.dontDie) && (!eid.flying || (bool)mf))
		{
			dead = true;
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
		if (eid.hitter != "secret")
		{
			if (bigKill)
			{
				scalc.HitCalculator(eid.hitter, "spider", hitLimb, dead, eid, sourceWeapon);
			}
			else
			{
				scalc.HitCalculator(eid.hitter, "machine", hitLimb, dead, eid, sourceWeapon);
			}
		}
	}

	public void GoLimp()
	{
		if (limp)
		{
			return;
		}
		if (!gz)
		{
			gz = GoreZone.ResolveGoreZone(base.transform);
		}
		onDeath?.Invoke();
		if (health > 0f)
		{
			health = 0f;
		}
		Invoke("StopHealing", 1f);
		if ((bool)v2)
		{
			v2.active = false;
			v2.Die();
		}
		if ((bool)mf)
		{
			mf.active = false;
		}
		if ((bool)tur)
		{
			tur.OnDeath();
		}
		if ((bool)fm)
		{
			fm.OnDeath();
		}
		SwingCheck2[] componentsInChildren = GetComponentsInChildren<SwingCheck2>();
		if (sm != null)
		{
			anim.StopPlayback();
			SwingCheck2[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				Object.Destroy(array[i]);
			}
			sm.CoolSword();
			if (sm.currentEnrageEffect != null)
			{
				Object.Destroy(sm.currentEnrageEffect);
			}
			Object.Destroy(sm);
		}
		if (sc != null)
		{
			if (anim != null)
			{
				anim.StopPlayback();
			}
			BulletCheck componentInChildren = GetComponentInChildren<BulletCheck>();
			if (componentInChildren != null)
			{
				Object.Destroy(componentInChildren.gameObject);
			}
			sc.hose.SetParent(sc.hoseTarget, worldPositionStays: true);
			sc.hose.transform.localPosition = Vector3.zero;
			sc.hose.transform.localScale = Vector3.zero;
			sc.StopFire();
			sc.dead = true;
			sc.damaging = false;
			FireZone componentInChildren2 = GetComponentInChildren<FireZone>();
			if ((bool)componentInChildren2)
			{
				Object.Destroy(componentInChildren2.gameObject);
			}
			if (sc.canister != null)
			{
				sc.canister.GetComponentInChildren<ParticleSystem>().Stop();
				AudioSource componentInChildren3 = sc.canister.GetComponentInChildren<AudioSource>();
				if (componentInChildren3 != null)
				{
					Object.Destroy(componentInChildren3);
				}
			}
		}
		if (destroyOnDeath.Length != 0)
		{
			GameObject[] array2 = destroyOnDeath;
			foreach (GameObject gameObject in array2)
			{
				if (gameObject.activeInHierarchy)
				{
					Transform transform = gameObject.GetComponentInParent<Rigidbody>().transform;
					if ((bool)transform)
					{
						gameObject.transform.SetParent(transform);
						gameObject.transform.position = transform.position;
						gameObject.transform.localScale = Vector3.zero;
					}
				}
			}
		}
		if (!dontDie && !eid.dontCountAsKills && !limp)
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
			ActivateNextWave componentInParent = GetComponentInParent<ActivateNextWave>();
			if (componentInParent != null)
			{
				componentInParent.AddDeadEnemy();
			}
		}
		EnemySimplifier[] componentsInChildren2 = GetComponentsInChildren<EnemySimplifier>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].Begone();
		}
		if (deadMaterial != null)
		{
			smr.sharedMaterial = deadMaterial;
		}
		else if (smr != null && !mf)
		{
			smr.sharedMaterial = originalMaterial;
		}
		if (nma != null)
		{
			Object.Destroy(nma);
			nma = null;
		}
		if (!v2 && !specialDeath)
		{
			Object.Destroy(anim);
			Object.Destroy(base.gameObject.GetComponent<Collider>());
			if (rb == null)
			{
				rb = GetComponent<Rigidbody>();
			}
			Object.Destroy(rb);
		}
		if (aud == null)
		{
			aud = GetComponent<AudioSource>();
		}
		if (deathSound != null)
		{
			aud.clip = deathSound;
			aud.pitch = Random.Range(0.85f, 1.35f);
			aud.priority = 11;
			aud.Play();
			if ((bool)tur)
			{
				aud.volume = 1f;
			}
		}
		if (!limp)
		{
			if (specialDeath)
			{
				SendMessage("Death", SendMessageOptions.DontRequireReceiver);
			}
			else if (!v2 && !mf)
			{
				rbs = GetComponentsInChildren<Rigidbody>();
				Rigidbody[] array3 = rbs;
				foreach (Rigidbody rigidbody in array3)
				{
					if (rigidbody != null)
					{
						rigidbody.isKinematic = false;
						rigidbody.useGravity = true;
					}
				}
			}
			if (musicRequested)
			{
				MonoSingleton<MusicManager>.Instance.PlayCleanMusic();
			}
		}
		limp = true;
		EnemyScanner componentInChildren4 = GetComponentInChildren<EnemyScanner>();
		if (componentInChildren4 != null)
		{
			Object.Destroy(componentInChildren4.gameObject);
		}
	}

	private void StartHealing()
	{
		if (symbiotic && symbiote != null)
		{
			healing = true;
		}
	}

	private void StopHealing()
	{
		noheal = true;
	}

	public void CanisterExplosion()
	{
		if (InvincibleEnemies.Enabled || eid.blessed)
		{
			if ((bool)sc && sc.canisterHit)
			{
				sc.canisterHit = false;
			}
			return;
		}
		eid.Explode();
		Explosion[] componentsInChildren = Object.Instantiate(sc.explosion, sc.canister.transform.position, Quaternion.identity).GetComponentsInChildren<Explosion>();
		foreach (Explosion obj in componentsInChildren)
		{
			obj.maxSize *= 1.75f;
			obj.damage = 50;
			obj.friendlyFire = true;
		}
		CharacterJoint[] componentsInChildren2 = chest.GetComponentsInChildren<CharacterJoint>();
		if (componentsInChildren2.Length != 0)
		{
			CharacterJoint[] array = componentsInChildren2;
			foreach (CharacterJoint characterJoint in array)
			{
				if (characterJoint.transform.parent.parent == chest.transform)
				{
					Object.Destroy(characterJoint);
					characterJoint.transform.parent = null;
				}
			}
		}
		if (OptionsMenuToManager.bloodEnabled)
		{
			for (int j = 0; j < 2; j++)
			{
				GameObject gib = bsm.GetGib(GibType.Gib);
				if ((bool)gib && (bool)gz && (bool)gz.gibZone)
				{
					ReadyGib(gib, sc.canister);
				}
			}
		}
		GameObject gore = bsm.GetGore(GoreType.Head, eid.underwater, eid.sandified, eid.blessed);
		gore.transform.position = sc.canister.transform.position;
		gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
		chest.transform.localScale = Vector3.zero;
		if (sc.canister.TryGetComponent<Collider>(out var component))
		{
			Object.Destroy(component);
		}
		sc.canister.transform.localScale = Vector3.zero;
		sc.canister.transform.parent = gz.gibZone;
		sc.canister.transform.position = Vector3.one * 9999f;
	}

	private void ReadyGib(GameObject tempGib, GameObject target)
	{
		tempGib.transform.position = target.transform.position;
		tempGib.transform.rotation = Random.rotation;
		if (!gz)
		{
			gz = GetComponentInParent<GoreZone>();
		}
		tempGib.transform.SetParent(gz.gibZone);
		if (!OptionsMenuToManager.bloodEnabled)
		{
			tempGib.SetActive(value: false);
		}
	}
}
