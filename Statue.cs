using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;

public class Statue : MonoBehaviour
{
	public float health;

	[HideInInspector]
	public float originalHealth;

	private BloodsplatterManager bsm;

	private GameObject player;

	public bool limp;

	private EnemyIdentifier eid;

	public GameObject chest;

	private float chestHP;

	private AudioSource aud;

	public AudioClip[] hurtSounds;

	private StyleCalculator scalc;

	private GoreZone gz;

	public Material deadMaterial;

	public Material woundedMaterial;

	public Material woundedEnrageMaterial;

	public GameObject woundedParticle;

	private Material originalMaterial;

	public SkinnedMeshRenderer smr;

	private NavMeshAgent nma;

	private Rigidbody rb;

	private Rigidbody[] rbs;

	private Animator anim;

	public AudioClip deathSound;

	private bool noheal;

	public List<GameObject> extraDamageZones = new List<GameObject>();

	public float extraDamageMultiplier;

	private StatueBoss sb;

	private Mass mass;

	private Vector3 origPos;

	private List<Transform> transforms = new List<Transform>();

	public bool grounded;

	private GroundCheckEnemy gc;

	public bool knockedBack;

	private float knockBackCharge;

	public float brakes;

	public float juggleWeight;

	public bool falling;

	private float fallSpeed;

	private float fallTime;

	private bool affectedByGravity = true;

	[HideInInspector]
	public bool musicRequested;

	public bool bigBlood;

	public bool massDeath;

	private bool massDying;

	public bool specialDeath;

	public bool parryable;

	public bool partiallyParryable;

	[HideInInspector]
	public List<Transform> parryables = new List<Transform>();

	private void Start()
	{
		player = MonoSingleton<NewMovement>.Instance.gameObject;
		if (!limp)
		{
			nma = GetComponent<NavMeshAgent>();
			bsm = MonoSingleton<BloodsplatterManager>.Instance;
			rbs = GetComponentsInChildren<Rigidbody>();
			anim = GetComponentInChildren<Animator>();
			if (smr != null)
			{
				originalMaterial = smr.material;
			}
			mass = GetComponent<Mass>();
			gc = GetComponentInChildren<GroundCheckEnemy>();
			if (gc == null)
			{
				affectedByGravity = false;
			}
			rb = GetComponent<Rigidbody>();
			eid = GetComponent<EnemyIdentifier>();
			if (!musicRequested)
			{
				musicRequested = true;
				MonoSingleton<MusicManager>.Instance.PlayBattleMusic();
			}
			if (originalHealth == 0f)
			{
				originalHealth = health;
			}
		}
		if (gz == null)
		{
			gz = GoreZone.ResolveGoreZone(base.transform);
		}
	}

	private void OnDestroy()
	{
		if (massDying)
		{
			DeathEnd();
		}
	}

	private void Update()
	{
		if (!massDying)
		{
			return;
		}
		base.transform.position = new Vector3(origPos.x + Random.Range(-0.5f, 0.5f), origPos.y + Random.Range(-0.5f, 0.5f), origPos.z + Random.Range(-0.5f, 0.5f));
		if (!(Random.Range(0f, 1f) < Time.deltaTime * 5f))
		{
			return;
		}
		int index = Random.Range(0, transforms.Count);
		if (transforms[index] != null)
		{
			GameObject gore = bsm.GetGore(GoreType.Head, eid.underwater, eid.sandified, eid.blessed);
			if ((bool)gore && (bool)gz && (bool)gz.goreZone)
			{
				ReadyGib(gore, transforms[index].gameObject);
			}
			if ((bool)gore && gore.TryGetComponent<Bloodsplatter>(out var component))
			{
				component.GetReady();
			}
		}
		else
		{
			transforms.RemoveAt(index);
		}
	}

	private void FixedUpdate()
	{
		if (!affectedByGravity || limp)
		{
			return;
		}
		if (knockedBack && knockBackCharge <= 0f && rb.velocity.magnitude < 1f && gc.onGround)
		{
			StopKnockBack();
		}
		else if (knockedBack)
		{
			if (knockBackCharge <= 0f)
			{
				brakes = Mathf.MoveTowards(brakes, 0f, 0.0005f * brakes);
			}
			if (rb.velocity.y > 0f)
			{
				rb.velocity = new Vector3(rb.velocity.x * 0.95f * brakes, (rb.velocity.y - juggleWeight) * brakes, rb.velocity.z * 0.95f * brakes);
			}
			else
			{
				rb.velocity = new Vector3(rb.velocity.x * 0.95f * brakes, rb.velocity.y - juggleWeight, rb.velocity.z * 0.95f * brakes);
			}
			juggleWeight += 0.00025f;
			nma.updatePosition = false;
			nma.updateRotation = false;
			nma.enabled = false;
			rb.isKinematic = false;
			rb.useGravity = true;
		}
		else if (!grounded && gc.onGround)
		{
			grounded = true;
		}
		else if (grounded && !gc.onGround)
		{
			grounded = false;
		}
		if (!gc.onGround && !falling && !nma.isOnOffMeshLink)
		{
			rb.isKinematic = false;
			rb.useGravity = true;
			nma.enabled = false;
			falling = true;
			anim.SetBool("Falling", value: true);
		}
		else if (gc.onGround && falling)
		{
			if (fallSpeed <= -50f && !InvincibleEnemies.Enabled && !eid.blessed)
			{
				eid.Splatter();
				return;
			}
			fallSpeed = 0f;
			nma.updatePosition = true;
			nma.updateRotation = true;
			rb.isKinematic = true;
			rb.useGravity = false;
			nma.enabled = true;
			nma.Warp(base.transform.position);
			falling = false;
			anim.SetBool("Falling", value: false);
		}
	}

	public void KnockBack(Vector3 force)
	{
		if (affectedByGravity && sb != null && !sb.inAction)
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
		if (Physics.Raycast(base.transform.position + Vector3.up * 0.1f, Vector3.down, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment)))
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

	public void GetHurt(GameObject target, Vector3 force, float multiplier, float critMultiplier, Vector3 hurtPos, GameObject sourceWeapon = null)
	{
		string hitLimb = "";
		bool dead = false;
		bool flag = false;
		bool flag2 = false;
		float num = 0f;
		GameObject gameObject = null;
		float num2 = health;
		if (massDying || eid == null)
		{
			return;
		}
		if (target.gameObject.tag == "Head")
		{
			num = 1f * multiplier + multiplier * critMultiplier;
			if (extraDamageZones.Count > 0 && extraDamageZones.Contains(target))
			{
				num *= extraDamageMultiplier;
				flag2 = true;
			}
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
				flag = true;
				hitLimb = "head";
			}
			if (health <= 0f && !limp)
			{
				GoLimp();
			}
		}
		else if (target.gameObject.tag == "Limb" || target.gameObject.tag == "EndLimb")
		{
			num = 1f * multiplier + 0.5f * multiplier * critMultiplier;
			if (extraDamageZones.Count > 0 && extraDamageZones.Contains(target))
			{
				num *= extraDamageMultiplier;
				flag2 = true;
			}
			if (!eid.blessed && !InvincibleEnemies.Enabled)
			{
				health -= num;
			}
			if (eid.hitter != "fire" && num > 0f)
			{
				if ((num >= 1f && health > 0f) || (health <= 0f && eid.hitter != "explosion") || (eid.hitter == "explosion" && target.gameObject.tag == "EndLimb"))
				{
					gameObject = bsm.GetGore(GoreType.Limb, eid.underwater, eid.sandified, eid.blessed);
				}
				else if (eid.hitter != "explosion")
				{
					gameObject = bsm.GetGore(GoreType.Small, eid.underwater, eid.sandified, eid.blessed);
				}
			}
			if (!limp)
			{
				flag = true;
				hitLimb = "limb";
			}
			if (health <= 0f && !limp)
			{
				GoLimp();
			}
		}
		else if (target.gameObject.tag == "Body")
		{
			num = 1f * multiplier;
			if (eid.hitter == "shotgunzone")
			{
				if (!parryable && (!partiallyParryable || parryables == null || !parryables.Contains(target.transform)) && (target.gameObject != chest || health - num > 0f))
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
					SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
				}
			}
			if (extraDamageZones.Count > 0 && extraDamageZones.Contains(target))
			{
				num *= extraDamageMultiplier;
				flag2 = true;
			}
			if (!eid.blessed && !InvincibleEnemies.Enabled)
			{
				health -= num;
			}
			if (eid.hitter != "fire" && num > 0f)
			{
				if ((num >= 1f && health > 0f) || (health <= 0f && eid.hitter != "explosion") || (eid.hitter == "explosion" && target.gameObject.tag == "EndLimb"))
				{
					gameObject = bsm.GetGore(GoreType.Body, eid.underwater, eid.sandified, eid.blessed);
				}
				else if (eid.hitter != "explosion")
				{
					gameObject = bsm.GetGore(GoreType.Small, eid.underwater, eid.sandified, eid.blessed);
				}
			}
			_ = (player.transform.position - base.transform.position).normalized;
			if (!limp)
			{
				flag = true;
				hitLimb = "body";
			}
			if (health <= 0f)
			{
				if (!limp)
				{
					GoLimp();
				}
				if (target.GetComponentInParent<Rigidbody>() != null)
				{
					target.GetComponentInParent<Rigidbody>().AddForce(force);
				}
			}
		}
		if (mass != null && mass.spearShot && mass.tailHitboxes.Contains(target))
		{
			MassSpear component = mass.tempSpear.GetComponent<MassSpear>();
			if (component != null && component.hitPlayer)
			{
				if (num >= 1f || component.spearHealth - num <= 0f)
				{
					GameObject gore = bsm.GetGore(GoreType.Head, eid.underwater, eid.sandified, eid.blessed);
					ReadyGib(gore, mass.tailEnd.GetChild(0).gameObject);
				}
				component.spearHealth -= num;
			}
		}
		if (gameObject != null)
		{
			if (gz == null)
			{
				gz = GoreZone.ResolveGoreZone(base.transform);
			}
			if (hurtPos != Vector3.zero)
			{
				gameObject.transform.position = hurtPos;
			}
			else
			{
				gameObject.transform.position = target.transform.position;
			}
			if (eid.hitter == "drill")
			{
				gameObject.transform.localScale *= 2f;
			}
			if (bigBlood)
			{
				gameObject.transform.localScale *= 2f;
			}
			if (gz != null && gz.goreZone != null)
			{
				gameObject.transform.SetParent(gz.goreZone, worldPositionStays: true);
			}
			Bloodsplatter component2 = gameObject.GetComponent<Bloodsplatter>();
			if ((bool)component2)
			{
				ParticleSystem.CollisionModule collision = component2.GetComponent<ParticleSystem>().collision;
				if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone" || eid.hitter == "explosion")
				{
					if (Random.Range(0f, 1f) > 0.5f)
					{
						collision.enabled = false;
					}
					component2.hpAmount = 3;
				}
				else if (eid.hitter == "nail")
				{
					component2.hpAmount = 1;
					component2.GetComponent<AudioSource>().volume *= 0.8f;
				}
				if (!noheal)
				{
					component2.GetReady();
				}
			}
		}
		if ((bool)eid && eid.hitter == "punch" && (parryable || (partiallyParryable && parryables != null && parryables.Count > 0 && parryables.Contains(target.transform))))
		{
			parryable = false;
			partiallyParryable = false;
			parryables.Clear();
			if (!InvincibleEnemies.Enabled && !eid.blessed)
			{
				num = 5f;
			}
			if (!eid.blessed && !InvincibleEnemies.Enabled)
			{
				health -= num;
			}
			MonoSingleton<FistControl>.Instance.currentPunch.Parry(hook: true, eid);
			SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
		}
		if (flag2 && (num >= 1f || (eid.hitter == "shotgun" && Random.Range(0f, 1f) > 0.5f) || (eid.hitter == "nail" && Random.Range(0f, 1f) > 0.85f)))
		{
			gameObject = ((!(extraDamageMultiplier >= 2f)) ? bsm.GetGore(GoreType.Limb, eid.underwater, eid.sandified, eid.blessed) : bsm.GetGore(GoreType.Head, eid.underwater, eid.sandified, eid.blessed));
			if ((bool)gameObject)
			{
				gameObject.transform.position = target.transform.position;
				if (gz != null && gz.goreZone != null)
				{
					gameObject.transform.SetParent(gz.goreZone, worldPositionStays: true);
				}
				Bloodsplatter component3 = gameObject.GetComponent<Bloodsplatter>();
				if ((bool)component3)
				{
					ParticleSystem.CollisionModule collision2 = component3.GetComponent<ParticleSystem>().collision;
					if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone" || eid.hitter == "explosion")
					{
						if (Random.Range(0f, 1f) > 0.5f)
						{
							collision2.enabled = false;
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
		}
		if (health > 0f && hurtSounds.Length != 0 && !eid.blessed)
		{
			if (aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
			aud.volume = 0.75f;
			aud.pitch = Random.Range(0.85f, 1.35f);
			aud.priority = 12;
			aud.Play();
		}
		if (multiplier == 0f)
		{
			flag = false;
		}
		if (flag && eid.hitter != "enemy")
		{
			if (scalc == null)
			{
				scalc = MonoSingleton<StyleCalculator>.Instance;
			}
			MinosArm component4 = GetComponent<MinosArm>();
			if (health <= 0f && !component4)
			{
				dead = true;
				if ((bool)gc && !gc.onGround && !eid.flying)
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
				scalc.HitCalculator(eid.hitter, "spider", hitLimb, dead, eid, sourceWeapon);
			}
		}
		if ((bool)woundedMaterial && num2 >= originalHealth / 2f && health < originalHealth / 2f)
		{
			smr.material = woundedMaterial;
			if ((bool)woundedParticle)
			{
				Object.Instantiate(woundedParticle, chest.transform.position, Quaternion.identity);
			}
			if (smr.TryGetComponent<EnemySimplifier>(out var component5))
			{
				component5.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, woundedMaterial);
				component5.ChangeMaterialNew(EnemySimplifier.MaterialState.enraged, woundedEnrageMaterial);
			}
		}
	}

	public void GoLimp()
	{
		if (limp)
		{
			return;
		}
		if (health > 0f)
		{
			health = 0f;
		}
		gz = GetComponentInParent<GoreZone>();
		Invoke("StopHealing", 1f);
		StatueBoss component = GetComponent<StatueBoss>();
		SwingCheck2[] componentsInChildren = GetComponentsInChildren<SwingCheck2>();
		MinosArm component2 = GetComponent<MinosArm>();
		if (component2 != null)
		{
			component2.Retreat();
			limp = true;
			return;
		}
		if (component != null)
		{
			anim.StopPlayback();
			SwingCheck2[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				Object.Destroy(array[i]);
			}
			StatueBoss[] componentsInChildren2 = GoreZone.ResolveGoreZone(base.transform).GetComponentsInChildren<StatueBoss>();
			if (componentsInChildren2.Length != 0)
			{
				StatueBoss[] array2 = componentsInChildren2;
				foreach (StatueBoss statueBoss in array2)
				{
					if (!(statueBoss == component))
					{
						statueBoss.Enrage();
					}
				}
			}
			component.ForceStopDashSound();
			if (component.currentEnrageEffect != null)
			{
				Object.Destroy(component.currentEnrageEffect);
			}
			Object.Destroy(component);
		}
		else if ((mass != null || massDeath) && !massDying)
		{
			if (mass != null)
			{
				mass.dead = true;
				mass.enabled = false;
				anim.speed = 0f;
				SwingCheck2[] array = componentsInChildren;
				for (int i = 0; i < array.Length; i++)
				{
					Object.Destroy(array[i]);
				}
			}
			origPos = base.transform.position;
			transforms.AddRange(GetComponentsInChildren<Transform>());
			massDying = true;
			Invoke("BloodExplosion", 3f);
			if (mass != null && mass.currentEnrageEffect != null)
			{
				Object.Destroy(mass.currentEnrageEffect);
			}
		}
		if (!eid.dontCountAsKills)
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
		EnemySimplifier[] componentsInChildren3 = GetComponentsInChildren<EnemySimplifier>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].Begone();
		}
		if (smr != null)
		{
			if (deadMaterial != null)
			{
				smr.sharedMaterial = deadMaterial;
			}
			else if (woundedMaterial != null)
			{
				smr.sharedMaterial = woundedMaterial;
			}
			else
			{
				smr.sharedMaterial = originalMaterial;
			}
		}
		if (specialDeath)
		{
			SendMessage("SpecialDeath", SendMessageOptions.DontRequireReceiver);
		}
		else if (!massDying)
		{
			Object.Destroy(nma);
			nma = null;
			Object.Destroy(anim);
			Object.Destroy(base.gameObject.GetComponent<Collider>());
			if (rb == null)
			{
				rb = GetComponent<Rigidbody>();
			}
			Object.Destroy(rb);
			if (!limp)
			{
				rbs = GetComponentsInChildren<Rigidbody>();
				Rigidbody[] array3 = rbs;
				foreach (Rigidbody rigidbody in array3)
				{
					if (rigidbody != null)
					{
						rigidbody.isKinematic = false;
						rigidbody.useGravity = true;
						rigidbody.AddForce(Random.onUnitSphere * 2.5f, ForceMode.VelocityChange);
					}
				}
			}
			if (!limp && !eid.dontCountAsKills)
			{
				ActivateNextWave componentInParent = GetComponentInParent<ActivateNextWave>();
				if (componentInParent != null)
				{
					componentInParent.AddDeadEnemy();
				}
			}
			if (musicRequested)
			{
				musicRequested = false;
				MonoSingleton<MusicManager>.Instance.PlayCleanMusic();
			}
		}
		if (deathSound != null)
		{
			if (aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.clip = deathSound;
			aud.volume = 1f;
			aud.pitch = Random.Range(0.85f, 1.35f);
			aud.priority = 11;
			aud.Play();
		}
		limp = true;
		EnemyScanner componentInChildren = GetComponentInChildren<EnemyScanner>();
		if (componentInChildren != null)
		{
			Object.Destroy(componentInChildren.gameObject);
		}
	}

	private void StopHealing()
	{
		noheal = true;
	}

	private void BloodExplosion()
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform transform in transforms)
		{
			if (transform != null && Random.Range(0f, 1f) < 0.33f)
			{
				GameObject gore = bsm.GetGore(GoreType.Head, eid.underwater, eid.sandified, eid.blessed);
				if ((bool)gore)
				{
					gore.transform.position = transform.position;
					if (gz != null && gz.goreZone != null)
					{
						gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
					}
					gore.GetComponent<Bloodsplatter>()?.GetReady();
				}
			}
			else if (transform == null)
			{
				list.Add(transform);
			}
		}
		if (list.Count > 0)
		{
			foreach (Transform item in list)
			{
				transforms.Remove(item);
			}
			list.Clear();
		}
		if (OptionsMenuToManager.bloodEnabled && base.gameObject.activeInHierarchy)
		{
			for (int i = 0; i < 40; i++)
			{
				GameObject gib;
				if (i < 30)
				{
					gib = bsm.GetGib(GibType.Gib);
					if ((bool)gib)
					{
						if ((bool)gz && (bool)gz.gibZone)
						{
							ReadyGib(gib, transforms[Random.Range(0, transforms.Count)].gameObject);
						}
						gib.transform.localScale *= Random.Range(4f, 7f);
					}
					else
					{
						i = 30;
					}
					continue;
				}
				if (i < 35)
				{
					gib = bsm.GetGib(GibType.Eye);
					if ((bool)gib)
					{
						if ((bool)gz && (bool)gz.gibZone)
						{
							ReadyGib(gib, transforms[Random.Range(0, transforms.Count)].gameObject);
						}
						gib.transform.localScale *= Random.Range(3f, 6f);
					}
					else
					{
						i = 35;
					}
					continue;
				}
				gib = bsm.GetGib(GibType.Brain);
				if (!gib)
				{
					break;
				}
				if ((bool)gz && (bool)gz.gibZone)
				{
					ReadyGib(gib, transforms[Random.Range(0, transforms.Count)].gameObject);
				}
				gib.transform.localScale *= Random.Range(3f, 4f);
			}
		}
		massDying = false;
		DeathEnd();
	}

	private void DeathEnd()
	{
		if (!eid.dontCountAsKills)
		{
			ActivateNextWave componentInParent = GetComponentInParent<ActivateNextWave>();
			if (componentInParent != null)
			{
				componentInParent.AddDeadEnemy();
			}
		}
		if (musicRequested)
		{
			MonoSingleton<MusicManager>.Instance.PlayCleanMusic();
		}
		if ((bool)base.gameObject)
		{
			Object.Destroy(base.gameObject);
		}
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
