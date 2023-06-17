using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;

public class SpiderBody : MonoBehaviour
{
	private Rigidbody[] rbs;

	public bool limp;

	private GameObject player;

	private Transform cam;

	private NewMovement nmov;

	private NavMeshAgent nma;

	private Quaternion followPlayerRot;

	public GameObject proj;

	private RaycastHit hit;

	private RaycastHit hit2;

	public LayerMask aimlm;

	private bool readyToShoot = true;

	private float burstCharge;

	private int maxBurst;

	private int currentBurst;

	public float health;

	public bool dead;

	public bool stationary;

	private Rigidbody rb;

	private bool falling;

	private Enemy enemy;

	private Transform firstChild;

	private CharacterJoint[] cjs;

	private CharacterJoint cj;

	public GameObject impactParticle;

	public GameObject impactSprite;

	private Quaternion spriteRot;

	private Vector3 spritePos;

	public Transform mouth;

	private GameObject currentProj;

	private bool charging;

	public GameObject chargeEffect;

	[HideInInspector]
	public GameObject currentCE;

	private float beamCharge;

	private AudioSource ceAud;

	private Light ceLight;

	private Vector3 predictedPlayerPos;

	public GameObject spiderBeam;

	private GameObject currentBeam;

	public GameObject beamExplosion;

	private GameObject currentExplosion;

	private float beamProbability;

	private Quaternion predictedRot;

	private bool rotating;

	public GameObject dripBlood;

	private GameObject currentDrip;

	private StyleCalculator scalc;

	private EnemyIdentifier eid;

	public GameObject spark;

	private int difficulty;

	private float coolDownMultiplier = 1f;

	private int beamsAmount = 1;

	private float maxHealth;

	public GameObject enrageEffect;

	[HideInInspector]
	public GameObject currentEnrageEffect;

	private Material origMaterial;

	public Material woundedMaterial;

	public Material woundedEnrageMaterial;

	public GameObject woundedParticle;

	private bool parryable;

	private MusicManager muman;

	private bool requestedMusic;

	private GoreZone gz;

	[SerializeField]
	private Transform headModel;

	public GameObject breakParticle;

	private bool corpseBroken;

	public GameObject shockwave;

	private EnemySimplifier[] ensims;

	public Renderer mainMesh;

	public float targetHeight = 1f;

	private float defaultHeight;

	[SerializeField]
	private Collider headCollider;

	private List<EnemyIdentifier> fallEnemiesHit = new List<EnemyIdentifier>();

	private void Start()
	{
		rbs = GetComponentsInChildren<Rigidbody>();
		nmov = MonoSingleton<NewMovement>.Instance;
		player = MonoSingleton<PlayerTracker>.Instance.GetPlayer().gameObject;
		cam = MonoSingleton<PlayerTracker>.Instance.GetTarget();
		nma = GetComponent<NavMeshAgent>();
		eid = GetComponent<EnemyIdentifier>();
		if (eid.difficultyOverride >= 0)
		{
			difficulty = eid.difficultyOverride;
		}
		else
		{
			difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		}
		maxHealth = health;
		if (difficulty >= 3)
		{
			coolDownMultiplier = 1.25f;
		}
		else if (difficulty == 1)
		{
			coolDownMultiplier = 0.75f;
		}
		else if (difficulty == 0)
		{
			coolDownMultiplier = 0.5f;
		}
		if (difficulty >= 2)
		{
			maxBurst = 5;
		}
		else
		{
			maxBurst = 2;
		}
		burstCharge = maxBurst;
		if (!mainMesh)
		{
			mainMesh = GetComponentInChildren<SkinnedMeshRenderer>();
		}
		origMaterial = mainMesh.material;
		gz = GoreZone.ResolveGoreZone(base.transform.parent ? base.transform.parent : base.transform);
		if ((bool)nma)
		{
			nma.updateRotation = false;
			if (stationary)
			{
				nma.speed = 0f;
			}
		}
		if ((bool)currentCE)
		{
			Object.Destroy(currentCE);
		}
		defaultHeight = targetHeight;
	}

	private void OnDisable()
	{
		if (!dead)
		{
			requestedMusic = false;
			if (muman == null)
			{
				muman = MonoSingleton<MusicManager>.Instance;
			}
			if ((bool)muman)
			{
				muman.PlayCleanMusic();
			}
		}
	}

	private void Update()
	{
		if (BlindEnemies.Blind)
		{
			return;
		}
		followPlayerRot = Quaternion.LookRotation((player.transform.position - base.transform.position).normalized);
		if (dead)
		{
			return;
		}
		if (beamCharge < 1f)
		{
			headModel.transform.rotation = Quaternion.RotateTowards(headModel.transform.rotation, followPlayerRot, (Quaternion.Angle(headModel.transform.rotation, followPlayerRot) + 10f) * Time.deltaTime * 15f * eid.totalSpeedModifier);
		}
		else if (rotating && beamCharge == 1f)
		{
			headModel.transform.rotation = Quaternion.RotateTowards(headModel.transform.rotation, predictedRot, Quaternion.Angle(headModel.transform.rotation, predictedRot) * Time.deltaTime * 20f * eid.totalSpeedModifier);
		}
		else if (!rotating && beamCharge == 1f)
		{
			predictedRot = Quaternion.LookRotation(player.transform.position - base.transform.position);
			headModel.transform.rotation = Quaternion.RotateTowards(headModel.transform.rotation, predictedRot, (Quaternion.Angle(headModel.transform.rotation, predictedRot) + 10f) * Time.deltaTime * 10f * eid.totalSpeedModifier);
		}
		if (difficulty > 2 && currentEnrageEffect == null && health < maxHealth / 2f)
		{
			Enrage();
		}
		if (!requestedMusic)
		{
			requestedMusic = true;
			muman = MonoSingleton<MusicManager>.Instance;
			muman.PlayBattleMusic();
		}
		if (!charging && beamCharge == 0f)
		{
			if (nma != null && !nma.enabled && !stationary)
			{
				nma.enabled = true;
				nma.isStopped = false;
				nma.speed = 3.5f * eid.totalSpeedModifier;
			}
			if (nma != null && nma.isOnNavMesh && !stationary)
			{
				if ((bool)eid.buffTargeter)
				{
					nma.SetDestination(eid.buffTargeter.transform.position);
					if (Vector3.Distance(base.transform.position, eid.buffTargeter.transform.position) < 15f)
					{
						targetHeight = 0.35f;
					}
					else
					{
						targetHeight = defaultHeight;
					}
				}
				else
				{
					nma.SetDestination(player.transform.position);
					targetHeight = defaultHeight;
				}
				nma.baseOffset = Mathf.MoveTowards(nma.baseOffset, targetHeight, Time.deltaTime * defaultHeight / 2f * eid.totalSpeedModifier);
			}
			if (currentBurst > maxBurst && burstCharge == 0f)
			{
				currentBurst = 0;
				if (difficulty > 0)
				{
					burstCharge = 5f;
				}
				else
				{
					burstCharge = 10f;
				}
			}
			if (burstCharge > 0f)
			{
				burstCharge = Mathf.MoveTowards(burstCharge, 0f, Time.deltaTime * coolDownMultiplier * 5f * eid.totalSpeedModifier);
			}
			if (burstCharge < 0f)
			{
				burstCharge = 0f;
			}
			if (!readyToShoot || burstCharge != 0f || (!(Quaternion.Angle(headModel.rotation, followPlayerRot) < 1f) && !(Vector3.Distance(base.transform.position, player.transform.position) < 10f)) || Physics.Raycast(base.transform.position, player.transform.position - base.transform.position, out hit, Vector3.Distance(base.transform.position, player.transform.position), aimlm))
			{
				return;
			}
			if (currentBurst != 0)
			{
				ShootProj();
			}
			else if ((Random.Range(0f, health * 0.4f) >= beamProbability && beamProbability <= 5f) || (Vector3.Distance(base.transform.position, player.transform.position) > 50f && !MonoSingleton<NewMovement>.Instance.ridingRocket))
			{
				ShootProj();
				beamProbability += 1f;
			}
			else if (!eid.buffTargeter || Vector3.Distance(base.transform.position, eid.buffTargeter.transform.position) > 15f)
			{
				ChargeBeam();
				if (difficulty > 2 && health < maxHealth / 2f)
				{
					beamsAmount = 2;
				}
				if (health > 10f)
				{
					beamProbability = 0f;
				}
				else
				{
					beamProbability = 1f;
				}
			}
		}
		else
		{
			if (!charging)
			{
				return;
			}
			if (beamCharge + 0.5f * coolDownMultiplier * Time.deltaTime * eid.totalSpeedModifier < 1f)
			{
				nma.speed = 0f;
				if (nma.isOnNavMesh)
				{
					nma.SetDestination(base.transform.position);
					nma.isStopped = true;
				}
				beamCharge += 0.5f * coolDownMultiplier * Time.deltaTime * eid.totalSpeedModifier;
				currentCE.transform.localScale = Vector3.one * beamCharge * 2.5f;
				ceAud.pitch = beamCharge * 2f;
				ceLight.intensity = beamCharge * 30f;
			}
			else
			{
				beamCharge = 1f;
				charging = false;
				BeamChargeEnd();
			}
		}
	}

	public void GetHurt(GameObject target, Vector3 force, Vector3 hitPoint, float multiplier, GameObject sourceWeapon = null)
	{
		bool flag = false;
		float num = health;
		bool flag2 = true;
		flag2 = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("bloodEnabled");
		if (eid == null)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (!eid.sandified && !eid.blessed)
		{
			GameObject gameObject = Object.Instantiate(MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Small, eid.underwater), hitPoint, Quaternion.identity);
			if ((bool)gameObject)
			{
				gameObject.transform.SetParent(gz.goreZone, worldPositionStays: true);
				if (eid.hitter == "drill")
				{
					gameObject.transform.localScale *= 2f;
				}
				if (health > 0f)
				{
					gameObject.GetComponent<Bloodsplatter>().GetReady();
				}
				if (multiplier >= 1f)
				{
					gameObject.GetComponent<Bloodsplatter>().hpAmount = 30;
				}
				if (flag2)
				{
					gameObject.GetComponent<ParticleSystem>().Play();
				}
			}
			if (eid.hitter != "shotgun" && eid.hitter != "drill")
			{
				currentDrip = Object.Instantiate(dripBlood, hitPoint, Quaternion.identity);
				if ((bool)currentDrip)
				{
					currentDrip.transform.parent = base.transform;
					currentDrip.transform.LookAt(base.transform);
					currentDrip.transform.Rotate(180f, 180f, 180f);
					if (flag2)
					{
						currentDrip.GetComponent<ParticleSystem>().Play();
					}
				}
			}
		}
		else
		{
			Object.Instantiate(MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Small, eid.underwater, eid.sandified, eid.blessed), hitPoint, Quaternion.identity);
		}
		if (!dead)
		{
			if (!eid.blessed && !InvincibleEnemies.Enabled)
			{
				health -= 1f * multiplier;
			}
			if (scalc == null)
			{
				scalc = MonoSingleton<StyleCalculator>.Instance;
			}
			if (health <= 0f)
			{
				flag = true;
			}
			if (parryable && (eid.hitter == "shotgunzone" || eid.hitter == "punch"))
			{
				parryable = false;
				MonoSingleton<FistControl>.Instance.currentPunch.Parry(hook: false, eid);
				currentExplosion = Object.Instantiate(beamExplosion, base.transform.position, Quaternion.identity);
				if (!InvincibleEnemies.Enabled && !eid.blessed)
				{
					health -= 5f / eid.totalHealthModifier;
				}
				Explosion[] componentsInChildren = currentExplosion.GetComponentsInChildren<Explosion>();
				foreach (Explosion obj in componentsInChildren)
				{
					obj.speed *= eid.totalDamageModifier;
					obj.maxSize *= 1.75f * eid.totalDamageModifier;
					obj.damage = Mathf.RoundToInt(50f * eid.totalDamageModifier);
					obj.canHit = AffectedSubjects.EnemiesOnly;
					obj.friendlyFire = true;
				}
				if (currentEnrageEffect == null)
				{
					CancelInvoke("BeamFire");
					Invoke("StopWaiting", 1f);
					Object.Destroy(currentCE);
				}
			}
			if (multiplier != 0f)
			{
				scalc.HitCalculator(eid.hitter, "spider", "", flag, eid, sourceWeapon);
			}
			if (num >= maxHealth / 2f && health < maxHealth / 2f)
			{
				if (ensims == null || ensims.Length == 0)
				{
					ensims = GetComponentsInChildren<EnemySimplifier>();
				}
				Object.Instantiate(woundedParticle, base.transform.position, Quaternion.identity);
				EnemySimplifier[] array = ensims;
				foreach (EnemySimplifier enemySimplifier in array)
				{
					if (!enemySimplifier.ignoreCustomColor)
					{
						enemySimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, woundedMaterial);
						enemySimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.enraged, woundedEnrageMaterial);
					}
				}
			}
			if (health <= 0f && !dead)
			{
				Die();
			}
		}
		else if (eid.hitter == "ground slam")
		{
			BreakCorpse();
		}
	}

	public void Die()
	{
		dead = true;
		rb = GetComponentInChildren<Rigidbody>();
		DoubleRender[] componentsInChildren = GetComponentsInChildren<DoubleRender>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].RemoveEffect();
		}
		falling = true;
		parryable = false;
		rb.isKinematic = false;
		rb.useGravity = true;
		base.gameObject.layer = 11;
		ResolveStuckness();
		for (int j = 1; j < base.transform.parent.childCount - 1; j++)
		{
			Object.Destroy(base.transform.parent.GetChild(j).gameObject);
		}
		if (currentCE != null)
		{
			Object.Destroy(currentCE);
		}
		Object.Destroy(nma);
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
			ActivateNextWave componentInParent = GetComponentInParent<ActivateNextWave>();
			if (componentInParent != null)
			{
				componentInParent.AddDeadEnemy();
			}
		}
		if (muman == null)
		{
			muman = MonoSingleton<MusicManager>.Instance;
		}
		muman.PlayCleanMusic();
		EnemySimplifier[] array;
		if (currentEnrageEffect != null)
		{
			mainMesh.material = origMaterial;
			MeshRenderer[] componentsInChildren2 = GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].material = origMaterial;
			}
			array = ensims;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enraged = false;
			}
			Object.Destroy(currentEnrageEffect);
		}
		if (ensims == null)
		{
			ensims = GetComponentsInChildren<EnemySimplifier>();
		}
		array = ensims;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Begone();
		}
		if (eid.hitter == "ground slam" || eid.hitter == "breaker")
		{
			BreakCorpse();
		}
	}

	private void ShootProj()
	{
		currentBurst++;
		currentProj = Object.Instantiate(proj, mouth.position, headModel.transform.rotation);
		currentProj.transform.rotation = Quaternion.LookRotation(cam.transform.position - mouth.position);
		Projectile component = currentProj.GetComponent<Projectile>();
		component.safeEnemyType = EnemyType.MaliciousFace;
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
		readyToShoot = false;
		if (difficulty > 0)
		{
			Invoke("ReadyToShoot", 0.1f / eid.totalSpeedModifier);
		}
		else
		{
			Invoke("ReadyToShoot", 0.2f / eid.totalSpeedModifier);
		}
	}

	private void ChargeBeam()
	{
		charging = true;
		currentCE = Object.Instantiate(chargeEffect, mouth);
		currentCE.transform.localScale = Vector3.zero;
		ceAud = currentCE.GetComponent<AudioSource>();
		ceLight = currentCE.GetComponent<Light>();
	}

	private void BeamChargeEnd()
	{
		if (beamsAmount <= 1 && (bool)ceAud)
		{
			ceAud.Stop();
		}
		Vector3 vector = new Vector3(MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().x, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().y / (float)(MonoSingleton<NewMovement>.Instance.ridingRocket ? 1 : 2), MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().z);
		predictedPlayerPos = (MonoSingleton<NewMovement>.Instance.ridingRocket ? MonoSingleton<NewMovement>.Instance.ridingRocket.transform.position : player.transform.position) + vector / 2f / eid.totalSpeedModifier;
		if (MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude > 0f && headCollider.Raycast(new Ray(player.transform.position, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity()), out var hitInfo, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude * 0.5f / eid.totalSpeedModifier))
		{
			predictedPlayerPos = player.transform.position;
		}
		else if (Physics.Raycast(player.transform.position, predictedPlayerPos - player.transform.position, out hitInfo, Vector3.Distance(predictedPlayerPos, player.transform.position), LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), QueryTriggerInteraction.Collide))
		{
			predictedPlayerPos = hitInfo.point;
		}
		if ((bool)nma)
		{
			nma.enabled = false;
		}
		predictedRot = Quaternion.LookRotation(predictedPlayerPos - base.transform.position);
		rotating = true;
		Object.Instantiate(spark, mouth.position, mouth.rotation).transform.LookAt(predictedPlayerPos);
		if (difficulty > 1)
		{
			Invoke("BeamFire", 0.5f / eid.totalSpeedModifier);
		}
		else if (difficulty == 1)
		{
			Invoke("BeamFire", 0.75f / eid.totalSpeedModifier);
		}
		else
		{
			Invoke("BeamFire", 1f / eid.totalSpeedModifier);
		}
		parryable = true;
	}

	private void BeamFire()
	{
		parryable = false;
		if (!dead)
		{
			currentBeam = Object.Instantiate(spiderBeam, mouth.position, mouth.rotation);
			rotating = false;
			if (eid.totalDamageModifier != 1f && currentBeam.TryGetComponent<RevolverBeam>(out var component))
			{
				component.damage *= eid.totalDamageModifier;
			}
			if (beamsAmount > 1)
			{
				beamsAmount--;
				ceAud.pitch = 4f;
				ceAud.volume = 1f;
				Invoke("BeamChargeEnd", 0.5f / eid.totalSpeedModifier);
			}
			else
			{
				Object.Destroy(currentCE);
				Invoke("StopWaiting", 1f / eid.totalSpeedModifier);
			}
		}
	}

	private void StopWaiting()
	{
		if (!dead)
		{
			beamCharge = 0f;
		}
	}

	private void ReadyToShoot()
	{
		readyToShoot = true;
	}

	public void TriggerHit(Collider other)
	{
		if (!falling)
		{
			return;
		}
		EnemyIdentifier enemyIdentifier = other.gameObject.GetComponent<EnemyIdentifier>();
		if (enemyIdentifier == null)
		{
			EnemyIdentifierIdentifier component = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
			if (component != null && component.eid != null)
			{
				enemyIdentifier = component.eid;
			}
		}
		if (enemyIdentifier == null && other.gameObject.TryGetComponent<IdolMauricer>(out var _))
		{
			enemyIdentifier = other.gameObject.GetComponentInParent<EnemyIdentifier>();
		}
		if ((bool)enemyIdentifier && enemyIdentifier != eid && !fallEnemiesHit.Contains(enemyIdentifier))
		{
			FallKillEnemy(enemyIdentifier);
		}
	}

	private void FallKillEnemy(EnemyIdentifier targetEid)
	{
		if ((bool)MonoSingleton<StyleHUD>.Instance && !targetEid.dead)
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(80, "ultrakill.mauriced", null, eid);
		}
		targetEid.hitter = "maurice";
		fallEnemiesHit.Add(targetEid);
		if (targetEid.TryGetComponent<Collider>(out var component))
		{
			Physics.IgnoreCollision(headCollider, component, ignore: true);
		}
		EnemyIdentifier.FallOnEnemy(targetEid);
	}

	private void OnCollisionEnter(Collision other)
	{
		if (!falling)
		{
			return;
		}
		if (other.gameObject.CompareTag("Moving"))
		{
			BreakCorpse();
			MonoSingleton<CameraController>.Instance.CameraShake(2f);
		}
		else
		{
			if (other.gameObject.layer != 8 && other.gameObject.layer != 24)
			{
				return;
			}
			Breakable component3;
			if (other.gameObject.CompareTag("Floor"))
			{
				rb.isKinematic = true;
				rb.useGravity = false;
				Transform transform = base.transform;
				Object.Instantiate(impactParticle, transform.position, transform.rotation);
				spriteRot.eulerAngles = new Vector3(other.contacts[0].normal.x + 90f, other.contacts[0].normal.y, other.contacts[0].normal.z);
				spritePos = new Vector3(other.contacts[0].point.x, other.contacts[0].point.y + 0.1f, other.contacts[0].point.z);
				AudioSource componentInChildren = Object.Instantiate(shockwave, spritePos, Quaternion.identity).GetComponentInChildren<AudioSource>();
				if ((bool)componentInChildren)
				{
					Object.Destroy(componentInChildren);
				}
				Object.Instantiate(impactSprite, spritePos, spriteRot).transform.SetParent(gz.goreZone, worldPositionStays: true);
				Transform transform2 = base.transform;
				transform2.position -= transform2.up * 1.5f;
				falling = false;
				if (TryGetComponent<SphereCollider>(out var component))
				{
					Object.Destroy(component);
				}
				SpiderBodyTrigger componentInChildren2 = base.transform.parent.GetComponentInChildren<SpiderBodyTrigger>(includeInactive: true);
				if ((bool)componentInChildren2)
				{
					Object.Destroy(componentInChildren2.gameObject);
				}
				rb.GetComponent<NavMeshObstacle>().enabled = true;
				MonoSingleton<CameraController>.Instance.CameraShake(2f);
				if (fallEnemiesHit.Count <= 0)
				{
					return;
				}
				foreach (EnemyIdentifier item in fallEnemiesHit)
				{
					if (item != null && !item.dead && item.TryGetComponent<Collider>(out var component2))
					{
						Physics.IgnoreCollision(headCollider, component2, ignore: false);
					}
				}
				fallEnemiesHit.Clear();
			}
			else if (other.gameObject.TryGetComponent<Breakable>(out component3) && !component3.playerOnly)
			{
				component3.Break();
			}
		}
	}

	public void Enrage()
	{
		if (!dead)
		{
			if (ensims == null || ensims.Length == 0)
			{
				ensims = GetComponentsInChildren<EnemySimplifier>();
			}
			EnemySimplifier[] array = ensims;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enraged = true;
			}
			currentEnrageEffect = Object.Instantiate(enrageEffect, base.transform);
			currentEnrageEffect.transform.localScale = Vector3.one * 0.2f;
		}
	}

	public void BreakCorpse()
	{
		if (!corpseBroken)
		{
			corpseBroken = true;
			if (breakParticle != null)
			{
				Transform transform = base.transform;
				Object.Instantiate(breakParticle, transform.position, transform.rotation).transform.SetParent(gz.gibZone);
			}
			Object.Destroy(base.gameObject);
		}
	}

	private void ResolveStuckness()
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, 2f, LayerMaskDefaults.Get(LMD.Environment));
		if (array != null && array.Length != 0)
		{
			SphereCollider component = GetComponent<SphereCollider>();
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				Physics.ComputePenetration(component, base.transform.position, base.transform.rotation, collider, collider.transform.position, collider.transform.rotation, out var direction, out var distance);
				base.transform.position = base.transform.position + direction * (distance + 0.5f);
			}
		}
		array = Physics.OverlapSphere(base.transform.position, 2f, LayerMaskDefaults.Get(LMD.Environment));
		if (array != null && array.Length != 0)
		{
			BreakCorpse();
		}
	}
}
