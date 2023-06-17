using System.Collections.Generic;
using Sandbox;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.Events;

public class EnemyIdentifier : MonoBehaviour, IAlter, IAlterOptions<bool>
{
	[HideInInspector]
	public Zombie zombie;

	[HideInInspector]
	public SpiderBody spider;

	[HideInInspector]
	public Machine machine;

	[HideInInspector]
	public Statue statue;

	[HideInInspector]
	public Wicked wicked;

	[HideInInspector]
	public Drone drone;

	[HideInInspector]
	public Idol idol;

	public EnemyClass enemyClass;

	public EnemyType enemyType;

	public bool spawnIn;

	public GameObject spawnEffect;

	public float health;

	[HideInInspector]
	public string hitter;

	[HideInInspector]
	public List<HitterAttribute> hitterAttributes = new List<HitterAttribute>();

	[HideInInspector]
	public List<string> hitterWeapons = new List<string>();

	public string[] weaknesses;

	public float[] weaknessMultipliers;

	public float totalDamageTakenMultiplier = 1f;

	public GameObject weakPoint;

	[HideInInspector]
	public bool exploded;

	public bool dead;

	[HideInInspector]
	public DoorController usingDoor;

	public bool ignoredByEnemies;

	private EnemyIdentifierIdentifier[] limbs;

	[HideInInspector]
	public int nailsAmount;

	[HideInInspector]
	public List<Nail> nails = new List<Nail>();

	public bool useBrakes;

	public bool bigEnemy;

	public bool unbounceable;

	public bool poise;

	private bool beingZapped;

	[HideInInspector]
	public bool pulledByMagnet;

	[HideInInspector]
	public List<Magnet> stuckMagnets = new List<Magnet>();

	[HideInInspector]
	public List<Harpoon> drillers = new List<Harpoon>();

	[HideInInspector]
	public bool underwater;

	[HideInInspector]
	public bool checkingSpawnStatus = true;

	public bool flying;

	public bool dontCountAsKills;

	public bool dontUnlockBestiary;

	public bool specialOob;

	public GameObject[] activateOnDeath;

	public UnityEvent onDeath;

	private BloodsplatterManager bsm;

	[HideInInspector]
	public GroundCheckEnemy gce;

	private GoreZone gz;

	private List<GameObject> sandifiedParticles = new List<GameObject>();

	[HideInInspector]
	public List<GameObject> blessingGlows = new List<GameObject>();

	[HideInInspector]
	public EnemyIdentifier buffTargeter;

	public int difficultyOverride = -1;

	[HideInInspector]
	public bool hooked;

	[HideInInspector]
	public List<Flammable> burners;

	[HideInInspector]
	public bool harpooned;

	[Header("Modifiers")]
	public bool hookIgnore;

	public bool sandified;

	public bool blessed;

	public float radianceTier = 1f;

	public bool healthBuff;

	public float healthBuffModifier = 1.5f;

	[HideInInspector]
	public int healthBuffRequests;

	public bool speedBuff;

	public float speedBuffModifier = 1.5f;

	[HideInInspector]
	public int speedBuffRequests;

	public bool damageBuff;

	public float damageBuffModifier = 1.5f;

	[HideInInspector]
	public int damageBuffRequests;

	[HideInInspector]
	public float totalSpeedModifier = 1f;

	[HideInInspector]
	public float totalDamageModifier = 1f;

	[HideInInspector]
	public float totalHealthModifier = 1f;

	[Space(10f)]
	public List<Renderer> buffUnaffectedRenderers = new List<Renderer>();

	[SerializeField]
	private string overrideFullName;

	public string fullName
	{
		get
		{
			if (!string.IsNullOrEmpty(overrideFullName))
			{
				return overrideFullName;
			}
			return EnemyTypes.GetEnemyName(enemyType);
		}
	}

	public string alterKey => "enemy-identifier";

	public string alterCategoryName => "enemy";

	public bool allowOnlyOne => true;

	public AlterOption<bool>[] options => new AlterOption<bool>[1]
	{
		new AlterOption<bool>
		{
			name = "Boss Health Bar",
			key = "health-bar",
			callback = delegate(bool value)
			{
				BossHealthBar component = GetComponent<BossHealthBar>();
				if (value)
				{
					if (component == null)
					{
						component = base.gameObject.AddComponent<BossHealthBar>();
					}
				}
				else if (component != null)
				{
					Object.Destroy(component);
				}
			},
			value = (GetComponent<BossHealthBar>() != null)
		}
	};

	private void Awake()
	{
		health = 999f;
		ForceGetHealth();
		UpdateModifiers();
	}

	private GoreZone GetGoreZone()
	{
		if ((bool)gz)
		{
			return gz;
		}
		gz = GoreZone.ResolveGoreZone(base.transform);
		return gz;
	}

	public void ForceGetHealth()
	{
		if (enemyType == EnemyType.Drone || enemyType == EnemyType.Virtue)
		{
			if (!drone)
			{
				drone = GetComponent<Drone>();
			}
			if ((bool)drone)
			{
				health = drone.health;
			}
			return;
		}
		if (enemyType == EnemyType.MaliciousFace)
		{
			if (!spider)
			{
				spider = GetComponent<SpiderBody>();
			}
			if ((bool)spider)
			{
				health = spider.health;
			}
			return;
		}
		switch (enemyClass)
		{
		case EnemyClass.Husk:
			if (!zombie)
			{
				zombie = GetComponent<Zombie>();
			}
			if ((bool)zombie)
			{
				health = zombie.health;
			}
			break;
		case EnemyClass.Demon:
			if (!statue)
			{
				statue = GetComponent<Statue>();
			}
			if ((bool)statue)
			{
				health = statue.health;
			}
			break;
		case EnemyClass.Machine:
			if (!machine)
			{
				machine = GetComponent<Machine>();
			}
			if ((bool)machine)
			{
				health = machine.health;
			}
			break;
		}
	}

	private void Start()
	{
		if (spawnIn && (bool)spawnEffect)
		{
			Collider component = GetComponent<Collider>();
			if ((bool)component)
			{
				Object.Instantiate(spawnEffect, component.bounds.center, base.transform.rotation);
			}
			else
			{
				Object.Instantiate(spawnEffect, base.transform.position + Vector3.up * 1.5f, base.transform.rotation);
			}
			spawnIn = false;
		}
		if (!dontUnlockBestiary)
		{
			MonoSingleton<BestiaryData>.Instance.SetEnemy(enemyType, 1);
		}
		bsm = MonoSingleton<BloodsplatterManager>.Instance;
		if (checkingSpawnStatus)
		{
			if (!dead)
			{
				if (sandified && enemyType != EnemyType.Stalker)
				{
					Sandify(ignorePrevious: true);
				}
				if (blessed)
				{
					Bless(ignorePrevious: true);
				}
				if (speedBuff || damageBuff || healthBuff || OptionsManager.forceRadiance)
				{
					if (speedBuff)
					{
						speedBuffRequests++;
					}
					if (damageBuff)
					{
						damageBuffRequests++;
					}
					if (healthBuff)
					{
						healthBuffRequests++;
					}
					UpdateBuffs();
				}
			}
			checkingSpawnStatus = false;
		}
		if (!MonoSingleton<EnemyTracker>.Instance.GetCurrentEnemies().Contains(this))
		{
			MonoSingleton<EnemyTracker>.Instance.AddEnemy(this);
		}
		gce = GetComponentInChildren<GroundCheckEnemy>(includeInactive: true);
		SlowUpdate();
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", 1f);
		if (drillers.Count <= 0)
		{
			return;
		}
		for (int num = drillers.Count - 1; num >= 0; num--)
		{
			if (drillers[num] == null || !drillers[num].gameObject.activeInHierarchy)
			{
				drillers.RemoveAt(num);
			}
		}
	}

	private void Update()
	{
		ForceGetHealth();
		UpdateModifiers();
		CheckBurners();
	}

	private void UpdateModifiers()
	{
		totalSpeedModifier = 1f;
		totalHealthModifier = 1f;
		totalDamageModifier = 1f;
		float num = Mathf.Max(OptionsManager.radianceTier, radianceTier);
		if (speedBuff || OptionsManager.forceRadiance)
		{
			totalSpeedModifier *= speedBuffModifier * ((num > 1f) ? (0.75f + num / 4f) : num);
		}
		if (healthBuff || OptionsManager.forceRadiance)
		{
			totalHealthModifier *= healthBuffModifier * ((num > 1f) ? (0.75f + num / 4f) : num);
		}
		if (damageBuff || OptionsManager.forceRadiance)
		{
			totalDamageModifier *= damageBuffModifier;
		}
	}

	private void CheckBurners()
	{
		if (burners == null || burners.Count <= 0)
		{
			return;
		}
		for (int num = burners.Count - 1; num >= 0; num--)
		{
			if (burners[num] == null || burners[num].secondary || !burners[num].burning)
			{
				burners.RemoveAt(num);
			}
		}
	}

	public void DeliverDamage(GameObject target, Vector3 force, Vector3 hitPoint, float multiplier, bool tryForExplode, float critMultiplier = 0f, GameObject sourceWeapon = null, bool ignoreTotalDamageTakenMultiplier = false)
	{
		if (target == base.gameObject)
		{
			EnemyIdentifierIdentifier componentInChildren = GetComponentInChildren<EnemyIdentifierIdentifier>();
			if (componentInChildren != null)
			{
				target = componentInChildren.gameObject;
			}
		}
		if (!ignoreTotalDamageTakenMultiplier)
		{
			multiplier *= totalDamageTakenMultiplier;
		}
		multiplier /= totalHealthModifier;
		if (weaknesses.Length != 0)
		{
			for (int i = 0; i < weaknesses.Length; i++)
			{
				if (hitter == weaknesses[i])
				{
					multiplier *= weaknessMultipliers[i];
				}
			}
		}
		CheckBurners();
		if (burners.Count > 0 && hitter != "fire" && hitter != "explosion" && hitter != "ffexplosion")
		{
			multiplier *= 1.5f;
		}
		if (nails.Count > 10)
		{
			for (int j = 0; j < nails.Count - 10; j++)
			{
				if (nails[j] != null)
				{
					Object.Destroy(nails[j].gameObject);
				}
				nails.RemoveAt(j);
			}
		}
		if (!beingZapped && hitterAttributes.Contains(HitterAttribute.Electricity) && nailsAmount > 0)
		{
			beingZapped = true;
			foreach (Nail nail in nails)
			{
				if (nail != null)
				{
					nail.Zap();
				}
			}
			Invoke("AfterShock", 1f);
		}
		if (pulledByMagnet && hitter != "deathzone")
		{
			pulledByMagnet = false;
		}
		bool flag = false;
		switch (enemyType)
		{
		case EnemyType.MaliciousFace:
			if (spider == null)
			{
				spider = GetComponent<SpiderBody>();
			}
			if (spider == null)
			{
				return;
			}
			if (hitter != "explosion" && hitter != "ffexplosion")
			{
				spider.GetHurt(target, force, hitPoint, multiplier, sourceWeapon);
			}
			if (spider.health <= 0f)
			{
				Death();
			}
			health = spider.health;
			flag = true;
			break;
		case EnemyType.Wicked:
			if (wicked == null)
			{
				wicked = GetComponent<Wicked>();
			}
			if (wicked == null)
			{
				return;
			}
			wicked.GetHit();
			flag = true;
			break;
		case EnemyType.Drone:
		case EnemyType.Virtue:
			if (drone == null)
			{
				drone = GetComponent<Drone>();
			}
			if (drone == null)
			{
				return;
			}
			drone.GetHurt(force, multiplier, sourceWeapon);
			health = drone.health;
			if (health <= 0f)
			{
				Death();
			}
			flag = true;
			break;
		case EnemyType.Idol:
			idol = idol ?? GetComponent<Idol>();
			if (hitter == "punch" || hitter == "heavypunch" || hitter == "ground slam")
			{
				idol?.Death();
			}
			break;
		}
		if (!flag)
		{
			switch (enemyClass)
			{
			case EnemyClass.Husk:
				if (zombie == null)
				{
					zombie = GetComponent<Zombie>();
				}
				if (zombie == null)
				{
					return;
				}
				zombie.GetHurt(target, force, multiplier, critMultiplier, sourceWeapon);
				if (tryForExplode && zombie.health <= 0f && !exploded)
				{
					Explode();
				}
				if (zombie.health <= 0f)
				{
					Death();
				}
				health = zombie.health;
				break;
			case EnemyClass.Machine:
				if (machine == null)
				{
					machine = GetComponent<Machine>();
				}
				if (machine == null)
				{
					return;
				}
				machine.GetHurt(target, force, multiplier, critMultiplier, sourceWeapon);
				if (tryForExplode && machine.health <= 0f && (machine.symbiote == null || machine.symbiote.health <= 0f) && !machine.dontDie && !exploded)
				{
					Explode();
				}
				if (machine.health <= 0f && (machine.symbiote == null || machine.symbiote.health <= 0f))
				{
					Death();
				}
				health = machine.health;
				break;
			case EnemyClass.Demon:
				if (statue == null)
				{
					statue = GetComponent<Statue>();
				}
				if (statue == null)
				{
					return;
				}
				statue.GetHurt(target, force, multiplier, critMultiplier, hitPoint, sourceWeapon);
				if (tryForExplode && statue.health <= 0f && !exploded)
				{
					Explode();
				}
				if (statue.health <= 0f)
				{
					Death();
				}
				health = statue.health;
				break;
			}
		}
		hitterAttributes.Clear();
	}

	private void AfterShock()
	{
		float num = nailsAmount / 15;
		if (num > 6f)
		{
			num = 6f;
		}
		GoreZone goreZone = GetGoreZone();
		foreach (Nail nail in nails)
		{
			if (nail == null)
			{
				continue;
			}
			GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Small, underwater, sandified, blessed);
			if ((bool)gore && (bool)goreZone)
			{
				gore.transform.position = nail.transform.position;
				gore.SetActive(value: true);
				Bloodsplatter component = gore.GetComponent<Bloodsplatter>();
				gore.transform.SetParent(goreZone.goreZone, worldPositionStays: true);
				if (component != null && !dead)
				{
					component.GetReady();
				}
			}
			Object.Destroy(nail.gameObject);
		}
		nails.Clear();
		nailsAmount = 0;
		EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
		if (!dead)
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(Mathf.RoundToInt(num * 15f), "<color=cyan>CONDUCTOR</color>", null, this);
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].gameObject != base.gameObject)
			{
				hitter = "aftershock";
				hitterAttributes.Add(HitterAttribute.Electricity);
				DeliverDamage(componentsInChildren[i].gameObject, Vector3.zero, base.transform.position, num, tryForExplode: true);
				break;
			}
		}
		beingZapped = false;
		MonoSingleton<CameraController>.Instance.CameraShake(1f);
	}

	public void Death()
	{
		if (dead)
		{
			return;
		}
		dead = true;
		if (hitterWeapons.Count > 1)
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(50, "ultrakill.arsenal", null, this);
		}
		GameObject[] array = activateOnDeath;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
		onDeath?.Invoke();
		if (!dontUnlockBestiary)
		{
			MonoSingleton<BestiaryData>.Instance.SetEnemy(enemyType);
			if (TryGetComponent<UnlockBestiary>(out var component))
			{
				MonoSingleton<BestiaryData>.Instance.SetEnemy(component.enemy);
			}
		}
		if (health > 0f)
		{
			health = 0f;
		}
		DestroyMagnets();
		if (drillers.Count > 0 && enemyType != EnemyType.MaliciousFace)
		{
			foreach (Harpoon driller in drillers)
			{
				driller.DelayedDestroyIfOnCorpse();
			}
		}
		if (usingDoor != null)
		{
			usingDoor.Close();
			usingDoor = null;
		}
		Desandify(visualOnly: true);
		Unbless();
	}

	public void DestroyMagnets()
	{
		if (stuckMagnets.Count <= 0)
		{
			return;
		}
		for (int num = stuckMagnets.Count - 1; num >= 0; num--)
		{
			if (stuckMagnets[num] != null)
			{
				Object.Destroy(stuckMagnets[num].gameObject);
			}
		}
	}

	public void InstaKill()
	{
		if (dead)
		{
			return;
		}
		Death();
		if (pulledByMagnet)
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(240, "ultrakill.catapulted", null, this);
		}
		dead = true;
		bool flag = false;
		switch (enemyType)
		{
		case EnemyType.Drone:
		case EnemyType.Virtue:
			if (drone == null)
			{
				drone = GetComponent<Drone>();
			}
			drone.GetHurt(Vector3.zero, 999f);
			drone.Explode();
			flag = true;
			break;
		case EnemyType.MaliciousFace:
			if (spider == null)
			{
				spider = GetComponent<SpiderBody>();
			}
			if (!spider.dead)
			{
				spider.Die();
			}
			flag = true;
			break;
		case EnemyType.Idol:
		{
			if (TryGetComponent<Idol>(out var component))
			{
				component.Death();
			}
			break;
		}
		}
		if (!flag)
		{
			switch (enemyClass)
			{
			case EnemyClass.Husk:
				if (zombie == null)
				{
					zombie = GetComponent<Zombie>();
				}
				if (!zombie.limp)
				{
					zombie.GoLimp();
				}
				break;
			case EnemyClass.Machine:
				if (machine == null)
				{
					machine = GetComponent<Machine>();
				}
				if (!machine.limp)
				{
					machine.GoLimp();
				}
				break;
			case EnemyClass.Demon:
				if (statue == null)
				{
					statue = GetComponent<Statue>();
				}
				if (!statue.limp)
				{
					statue.GoLimp();
				}
				break;
			}
		}
		if (usingDoor != null)
		{
			usingDoor.Close();
			usingDoor = null;
		}
	}

	public void Explode()
	{
		bool flag = dead;
		if (!dead)
		{
			Death();
		}
		if (enemyType == EnemyType.MaliciousFace)
		{
			if (spider == null)
			{
				spider = GetComponent<SpiderBody>();
			}
			if (!spider.dead)
			{
				hitter = "breaker";
				spider.Die();
			}
			else
			{
				spider.BreakCorpse();
			}
		}
		else if (enemyType == EnemyType.Drone || enemyType == EnemyType.Virtue)
		{
			if (drone == null)
			{
				drone = GetComponent<Drone>();
			}
			drone.Explode();
		}
		else if (enemyClass == EnemyClass.Husk)
		{
			if (zombie == null)
			{
				zombie = GetComponent<Zombie>();
			}
			if (exploded || !zombie || zombie.chestExploding)
			{
				return;
			}
			exploded = true;
			if (zombie.chestExploding)
			{
				zombie.ChestExplodeEnd();
			}
			if (!flag && pulledByMagnet)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(240, "ultrakill.catapulted", null, this);
			}
			Transform[] componentsInChildren = zombie.GetComponentsInChildren<Transform>(includeInactive: true);
			GoreZone goreZone = GetGoreZone();
			bool flag2 = false;
			Transform[] array = componentsInChildren;
			foreach (Transform transform in array)
			{
				if (transform.gameObject.tag == "Limb")
				{
					Object.Destroy(transform.GetComponent<CharacterJoint>());
					transform.transform.SetParent(goreZone.gibZone, worldPositionStays: true);
					if (!flag2)
					{
						zombie.GetHurt(transform.gameObject, (base.transform.position - transform.position).normalized * 1000f, 1E+09f, 1f);
					}
				}
				else if (transform.gameObject.tag == "Head" || transform.gameObject.tag == "EndLimb")
				{
					flag2 = true;
					zombie.GetHurt(transform.gameObject, (base.transform.position - transform.position).normalized * 1000f, 1E+09f, 1f);
				}
			}
			if (!flag2)
			{
				zombie.GoLimp();
			}
			health = zombie.health;
			if (usingDoor != null)
			{
				usingDoor.Close();
				usingDoor = null;
			}
		}
		else if (enemyClass == EnemyClass.Machine && enemyType != EnemyType.Drone)
		{
			if (machine == null)
			{
				machine = GetComponent<Machine>();
			}
			if (exploded || !machine)
			{
				return;
			}
			exploded = true;
			bool flag3 = false;
			if (machine.dismemberment)
			{
				Collider[] componentsInChildren2 = machine.GetComponentsInChildren<Collider>();
				List<EnemyIdentifierIdentifier> list = new List<EnemyIdentifierIdentifier>();
				Collider[] array2 = componentsInChildren2;
				for (int i = 0; i < array2.Length; i++)
				{
					EnemyIdentifierIdentifier component = array2[i].GetComponent<EnemyIdentifierIdentifier>();
					if (component != null)
					{
						list.Add(component);
					}
				}
				GoreZone goreZone2 = GetGoreZone();
				foreach (EnemyIdentifierIdentifier item in list)
				{
					if (item.gameObject.tag == "Limb")
					{
						CharacterJoint component2 = item.GetComponent<CharacterJoint>();
						if (component2 != null)
						{
							Object.Destroy(component2);
						}
						item.transform.SetParent(goreZone2.gibZone, worldPositionStays: true);
					}
					else if (item.gameObject.tag == "Head" || item.gameObject.tag == "EndLimb")
					{
						flag3 = true;
						machine.GetHurt(item.gameObject, (base.transform.position - item.transform.position).normalized * 1000f, 999f, 1f);
					}
				}
			}
			if (!flag3)
			{
				machine.GoLimp();
			}
			health = machine.health;
			if (usingDoor != null)
			{
				usingDoor.Close();
				usingDoor = null;
			}
		}
		else
		{
			if (enemyClass != EnemyClass.Demon)
			{
				return;
			}
			if (statue == null)
			{
				statue = GetComponent<Statue>();
			}
			if (!exploded)
			{
				exploded = true;
				if (!statue.limp)
				{
					statue.GoLimp();
				}
				health = statue.health;
			}
		}
	}

	public void Splatter()
	{
		if (InvincibleEnemies.Enabled || blessed)
		{
			return;
		}
		if (enemyType == EnemyType.MaliciousFace)
		{
			if (spider == null)
			{
				spider = GetComponent<SpiderBody>();
			}
			spider.Die();
			spider.BreakCorpse();
		}
		else
		{
			if (enemyType == EnemyType.Drone || enemyType == EnemyType.Virtue)
			{
				if (drone == null)
				{
					drone = GetComponent<Drone>();
				}
				drone.GetHurt(Vector3.zero, 999f);
				if (enemyType == EnemyType.Virtue)
				{
					drone.Explode();
				}
				Death();
				return;
			}
			switch (enemyClass)
			{
			case EnemyClass.Husk:
				if (zombie == null)
				{
					zombie = GetComponent<Zombie>();
				}
				break;
			case EnemyClass.Demon:
				if (statue == null)
				{
					statue = GetComponent<Statue>();
				}
				break;
			case EnemyClass.Machine:
				if (machine == null)
				{
					machine = GetComponent<Machine>();
				}
				break;
			}
		}
		bool flag = dead;
		if (enemyClass == EnemyClass.Machine && (bool)machine && !machine.dismemberment)
		{
			InstaKill();
		}
		else if (!exploded && (enemyClass != 0 || !zombie.chestExploding))
		{
			exploded = true;
			limbs = GetComponentsInChildren<EnemyIdentifierIdentifier>();
			if (!flag)
			{
				SendMessage("GoLimp", SendMessageOptions.DontRequireReceiver);
				StyleHUD instance = MonoSingleton<StyleHUD>.Instance;
				if (pulledByMagnet)
				{
					instance.AddPoints(120, "ultrakill.catapulted", null, this);
				}
				instance.AddPoints(100, "ultrakill.splattered", null, this);
				base.transform.Rotate(new Vector3(90f, 0f, 0f));
			}
			GameObject gore = bsm.GetGore(GoreType.Splatter, underwater, sandified, blessed);
			gore.transform.position = base.transform.position + Vector3.up;
			GoreZone goreZone = GetGoreZone();
			if (goreZone != null && goreZone.goreZone != null)
			{
				gore.transform.SetParent(goreZone.goreZone, worldPositionStays: true);
			}
			gore.GetComponent<Bloodsplatter>()?.GetReady();
			EnemyIdentifierIdentifier[] array = limbs;
			foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in array)
			{
				if (enemyIdentifierIdentifier.gameObject.tag == "Body" || enemyIdentifierIdentifier.gameObject.tag == "Limb" || enemyIdentifierIdentifier.gameObject.tag == "Head" || enemyIdentifierIdentifier.gameObject.tag == "EndLimb")
				{
					Object.Destroy(enemyIdentifierIdentifier.GetComponent<CharacterJoint>());
					enemyIdentifierIdentifier.transform.SetParent(GetGoreZone().gibZone, worldPositionStays: true);
					Rigidbody component = enemyIdentifierIdentifier.GetComponent<Rigidbody>();
					if (component != null)
					{
						component.velocity = Vector3.zero;
						enemyIdentifierIdentifier.transform.position = new Vector3(enemyIdentifierIdentifier.transform.position.x, base.transform.position.y + 0.1f, enemyIdentifierIdentifier.transform.position.z);
						Vector3 vector = new Vector3(base.transform.position.x - enemyIdentifierIdentifier.transform.position.x, 0f, base.transform.position.z - enemyIdentifierIdentifier.transform.position.z);
						component.AddForce(vector * 15f, ForceMode.VelocityChange);
						component.constraints = RigidbodyConstraints.FreezePositionY;
					}
				}
			}
			if ((bool)machine && enemyType == EnemyType.Streetcleaner)
			{
				machine.CanisterExplosion();
			}
			Invoke("StopSplatter", 1f);
			if (usingDoor != null)
			{
				usingDoor.Close();
				usingDoor = null;
			}
		}
		Death();
	}

	public void StopSplatter()
	{
		EnemyIdentifierIdentifier[] array = limbs;
		foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in array)
		{
			if (enemyIdentifierIdentifier != null)
			{
				Rigidbody component = enemyIdentifierIdentifier.GetComponent<Rigidbody>();
				if (component != null)
				{
					component.constraints = RigidbodyConstraints.None;
				}
			}
		}
	}

	public void Sandify(bool ignorePrevious = false)
	{
		if (!ignorePrevious && sandified)
		{
			return;
		}
		sandified = true;
		EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
		foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in componentsInChildren)
		{
			GameObject gameObject = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.sandDrip, enemyIdentifierIdentifier.transform.position, enemyIdentifierIdentifier.transform.rotation);
			Collider component = enemyIdentifierIdentifier.GetComponent<Collider>();
			if ((bool)component)
			{
				gameObject.transform.localScale = component.bounds.size;
			}
			gameObject.transform.SetParent(enemyIdentifierIdentifier.transform, worldPositionStays: true);
			sandifiedParticles.Add(gameObject);
		}
		Renderer[] componentsInChildren2 = GetComponentsInChildren<Renderer>();
		Collider component2 = GetComponent<Collider>();
		if ((bool)component2)
		{
			Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.sandificationEffect, component2.bounds.center, Quaternion.identity);
		}
		else
		{
			Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.sandificationEffect, base.transform.position, Quaternion.identity);
		}
		Renderer[] array = componentsInChildren2;
		foreach (Renderer renderer in array)
		{
			if (!buffUnaffectedRenderers.Contains(renderer))
			{
				Material[] materials = renderer.materials;
				for (int j = 0; j < materials.Length; j++)
				{
					materials[j].SetFloat("_HasSandBuff", 1f);
				}
			}
		}
	}

	public void Desandify(bool visualOnly = false)
	{
		if (!visualOnly)
		{
			sandified = false;
		}
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (!buffUnaffectedRenderers.Contains(renderer))
			{
				Material[] materials = renderer.materials;
				for (int j = 0; j < materials.Length; j++)
				{
					materials[j].SetFloat("_HasSandBuff", 0f);
				}
			}
		}
		foreach (GameObject sandifiedParticle in sandifiedParticles)
		{
			Object.Destroy(sandifiedParticle);
		}
		sandifiedParticles.Clear();
	}

	public void Bless(bool ignorePrevious = false)
	{
		if (!ignorePrevious && blessed)
		{
			return;
		}
		blessed = true;
		EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
		foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in componentsInChildren)
		{
			GameObject gameObject = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.blessingGlow, enemyIdentifierIdentifier.transform.position, enemyIdentifierIdentifier.transform.rotation);
			Collider component = enemyIdentifierIdentifier.GetComponent<Collider>();
			if ((bool)component)
			{
				gameObject.transform.localScale = component.bounds.size;
			}
			gameObject.transform.SetParent(enemyIdentifierIdentifier.transform, worldPositionStays: true);
			blessingGlows.Add(gameObject);
		}
	}

	public void Unbless(bool visualOnly = false)
	{
		if (!visualOnly)
		{
			blessed = false;
		}
		foreach (GameObject blessingGlow in blessingGlows)
		{
			Object.Destroy(blessingGlow);
		}
		blessingGlows.Clear();
	}

	public void BuffAll()
	{
		damageBuffRequests++;
		speedBuffRequests++;
		healthBuffRequests++;
		UpdateBuffs();
	}

	public void UnbuffAll()
	{
		speedBuffRequests--;
		healthBuffRequests--;
		damageBuffRequests--;
		UpdateBuffs();
	}

	public void DamageBuff(float modifier = -999f)
	{
		if (modifier == -999f)
		{
			modifier = damageBuffModifier;
		}
		damageBuffRequests++;
		damageBuffModifier = modifier;
		UpdateBuffs();
	}

	public void DamageUnbuff()
	{
		damageBuffRequests--;
		UpdateBuffs();
	}

	public void SpeedBuff(float modifier = -999f)
	{
		if (modifier == -999f)
		{
			modifier = speedBuffModifier;
		}
		speedBuffRequests++;
		speedBuffModifier = modifier;
		UpdateBuffs();
	}

	public void SpeedUnbuff()
	{
		speedBuffRequests--;
		UpdateBuffs();
	}

	public void HealthBuff(float modifier = -999f)
	{
		if (modifier == -999f)
		{
			modifier = healthBuffModifier;
		}
		healthBuffRequests++;
		healthBuffModifier = modifier;
		UpdateBuffs();
	}

	public void HealthUnbuff()
	{
		healthBuffRequests--;
		UpdateBuffs();
	}

	public void UpdateBuffs(bool visualsOnly = false)
	{
		speedBuff = speedBuffRequests > 0;
		healthBuff = healthBuffRequests > 0;
		damageBuff = damageBuffRequests > 0;
		if (!healthBuff && !speedBuff && !damageBuff && !OptionsManager.forceRadiance)
		{
			speedBuffRequests = 0;
			healthBuffRequests = 0;
			damageBuffRequests = 0;
		}
		if (!visualsOnly)
		{
			UpdateModifiers();
			SendMessage("UpdateBuff", SendMessageOptions.DontRequireReceiver);
		}
	}

	public static bool CheckHurtException(EnemyType attacker, EnemyType receiver)
	{
		if ((attacker == EnemyType.Stalker && receiver != EnemyType.Swordsmachine) || (receiver == EnemyType.Stalker && attacker != EnemyType.Swordsmachine))
		{
			return true;
		}
		if ((attacker == EnemyType.Filth || attacker == EnemyType.Stray || attacker == EnemyType.Schism || attacker == EnemyType.Soldier) && (receiver == EnemyType.Filth || receiver == EnemyType.Stray || receiver == EnemyType.Schism || receiver == EnemyType.Soldier))
		{
			return true;
		}
		if (((attacker == EnemyType.Drone || attacker == EnemyType.Virtue) && receiver == EnemyType.FleshPrison) || ((receiver == EnemyType.Drone || receiver == EnemyType.Virtue) && attacker == EnemyType.FleshPrison))
		{
			return true;
		}
		switch (receiver)
		{
		case EnemyType.Sisyphus:
			return true;
		case EnemyType.Ferryman:
			return true;
		default:
			if ((attacker == EnemyType.Gabriel || attacker == EnemyType.GabrielSecond) && (receiver == EnemyType.Gabriel || receiver == EnemyType.GabrielSecond))
			{
				return true;
			}
			return false;
		}
	}

	public static void FallOnEnemy(EnemyIdentifier eid)
	{
		if (eid.dead)
		{
			eid.Explode();
			return;
		}
		switch (eid.enemyType)
		{
		case EnemyType.Idol:
			eid.InstaKill();
			break;
		case EnemyType.Sisyphus:
		{
			if (eid.TryGetComponent<Sisyphus>(out var component5))
			{
				eid.DeliverDamage(eid.gameObject, Vector3.zero, eid.transform.position, 22f, tryForExplode: true);
				component5.Knockdown(component5.transform.position + component5.transform.forward);
			}
			break;
		}
		case EnemyType.Mindflayer:
		{
			if (eid.TryGetComponent<Mindflayer>(out var component2))
			{
				component2.Teleport();
			}
			break;
		}
		case EnemyType.Gabriel:
		{
			if (eid.TryGetComponent<Gabriel>(out var component4))
			{
				component4.Teleport();
			}
			break;
		}
		case EnemyType.GabrielSecond:
		{
			if (eid.TryGetComponent<GabrielSecond>(out var component3))
			{
				component3.Teleport();
			}
			break;
		}
		case EnemyType.Ferryman:
		{
			if (eid.TryGetComponent<Ferryman>(out var component))
			{
				component.Roll();
			}
			break;
		}
		default:
			eid.Explode();
			break;
		}
	}

	public void ChangeDamageTakenMultiplier(float newMultiplier)
	{
		totalDamageTakenMultiplier = newMultiplier;
	}

	public void SimpleDamage(float amount)
	{
		DeliverDamage(base.gameObject, Vector3.zero, base.transform.position, amount, tryForExplode: false);
	}

	public void SimpleDamageIgnoreMultiplier(float amount)
	{
		if (totalDamageTakenMultiplier != 0f)
		{
			DeliverDamage(base.gameObject, Vector3.zero, base.transform.position, amount / totalDamageTakenMultiplier, tryForExplode: false);
		}
		else
		{
			DeliverDamage(base.gameObject, Vector3.zero, base.transform.position, amount, tryForExplode: false, 0f, null, ignoreTotalDamageTakenMultiplier: true);
		}
	}
}
