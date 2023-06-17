using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;

public class FleshPrison : MonoBehaviour
{
	public Transform rotationBone;

	private Collider col;

	public Transform target;

	private Animator anim;

	public bool altVersion;

	private Texture currentIdleTexture;

	private Texture defaultTexture;

	public Texture[] idleTextures;

	private float idleTimer = 0.5f;

	public Texture hurtTexture;

	public Texture attackTexture;

	[SerializeField]
	private Renderer mainRenderer;

	private AudioSource aud;

	private BossHealthBar bossHealth;

	private float secondaryBarValue;

	private bool started;

	private bool inAction;

	private float health;

	private EnemyIdentifier eid;

	private Statue stat;

	private bool hurting;

	private bool shakingCamera;

	private Vector3 origPos;

	public GameObject fleshDrone;

	public GameObject skullDrone;

	private float fleshDroneCooldown = 3f;

	private int droneAmount = 10;

	private int currentDrone;

	private GameObject targeter;

	private bool healing;

	public List<DroneFlesh> currentDrones = new List<DroneFlesh>();

	public GameObject healingTargetEffect;

	public GameObject healingEffect;

	private float rotationSpeed = 45f;

	private float rotationSpeedTarget = 45f;

	private float attackCooldown = 5f;

	private int previousAttack = 666;

	public GameObject insignia;

	private float maxHealth;

	public GameObject homingProjectile;

	private int projectileAmount = 40;

	private int currentProjectile = 40;

	private float homingProjectileCooldown;

	public GameObject attackWindUp;

	public GameObject blackHole;

	private BlackHoleProjectile currentBlackHole;

	private int difficulty;

	public UltrakillEvent onFirstHeal;

	private int timesHealed;

	private bool noDrones;

	private MaterialPropertyBlock texOverride;

	private float maxDroneCooldown => (!started) ? 3 : ((difficulty == 2) ? 25 : 30);

	private void Awake()
	{
		if (!mainRenderer)
		{
			mainRenderer = GetComponentInChildren<Renderer>();
		}
		if ((bool)mainRenderer)
		{
			texOverride = new MaterialPropertyBlock();
			mainRenderer.GetPropertyBlock(texOverride);
			defaultTexture = mainRenderer.material.mainTexture;
			texOverride.SetTexture("_MainTex", defaultTexture);
			mainRenderer.SetPropertyBlock(texOverride);
			currentIdleTexture = defaultTexture;
		}
	}

	private void Start()
	{
		target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
		eid = GetComponent<EnemyIdentifier>();
		stat = GetComponent<Statue>();
		maxHealth = stat.health;
		health = stat.health;
		origPos = rotationBone.localPosition;
		aud = GetComponent<AudioSource>();
		anim = GetComponentInChildren<Animator>();
		if (eid.difficultyOverride >= 0)
		{
			difficulty = eid.difficultyOverride;
		}
		else
		{
			difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		}
		col = rotationBone.GetComponentInChildren<EnemyIdentifierIdentifier>().GetComponent<Collider>();
		bossHealth = GetComponent<BossHealthBar>();
	}

	private void Update()
	{
		float num = Mathf.Abs(rotationSpeed);
		if (num < 45f)
		{
			num = 45f;
		}
		if (rotationSpeed != rotationSpeedTarget)
		{
			rotationSpeed = Mathf.MoveTowards(rotationSpeed, rotationSpeedTarget, Time.deltaTime * (num / 2f + 5f));
		}
		rotationBone.Rotate(Vector3.forward, Time.deltaTime * rotationSpeed * eid.totalSpeedModifier, Space.Self);
		if (BlindEnemies.Blind)
		{
			return;
		}
		if (health > stat.health)
		{
			float num2 = health - stat.health;
			if (currentDrones.Count > 0)
			{
				for (int num3 = currentDrones.Count - 1; num3 >= 0; num3--)
				{
					if (currentDrones[num3] == null)
					{
						currentDrones.RemoveAt(num3);
					}
				}
			}
			if (currentDrones.Count > 8)
			{
				fleshDroneCooldown -= num2 * 1.5f;
			}
			else if (currentDrones.Count > 5)
			{
				fleshDroneCooldown -= num2 / 2.5f;
			}
			else if (currentDrones.Count > 0)
			{
				fleshDroneCooldown -= num2 / 5f;
			}
			else
			{
				fleshDroneCooldown -= num2 / 7.5f;
			}
			health = stat.health;
		}
		else if (health < stat.health)
		{
			health = stat.health;
		}
		if ((bool)bossHealth)
		{
			if (!healing)
			{
				secondaryBarValue = Mathf.MoveTowards(secondaryBarValue, fleshDroneCooldown / maxDroneCooldown, (Mathf.Abs(secondaryBarValue - fleshDroneCooldown / maxDroneCooldown) + 1f) * Time.deltaTime);
			}
			bossHealth.UpdateSecondaryBar(secondaryBarValue);
		}
		if ((bool)anim && !noDrones)
		{
			anim.speed = ((!inAction) ? 1 : 5);
		}
		if (!inAction)
		{
			if (health > stat.health)
			{
				idleTimer = 0.15f;
				texOverride.SetTexture("_MainTex", hurtTexture);
				mainRenderer.SetPropertyBlock(texOverride);
				hurting = true;
			}
			else
			{
				idleTimer = Mathf.MoveTowards(idleTimer, 0f, Time.deltaTime * eid.totalSpeedModifier);
				if (hurting)
				{
					if (idleTimer > 0f)
					{
						rotationBone.transform.localPosition = new Vector3(origPos.x + Random.Range(0f - idleTimer, idleTimer), origPos.y, origPos.z + Random.Range(0f - idleTimer, idleTimer));
					}
					else
					{
						rotationBone.transform.localPosition = origPos;
						hurting = false;
					}
				}
				if (idleTimer == 0f)
				{
					if (currentIdleTexture == defaultTexture)
					{
						idleTimer = 0.25f;
						texOverride.SetTexture("_MainTex", idleTextures[Random.Range(0, idleTextures.Length)]);
						mainRenderer.SetPropertyBlock(texOverride);
					}
					else
					{
						idleTimer = Random.Range(0.5f, 1f);
						texOverride.SetTexture("_MainTex", defaultTexture);
						mainRenderer.SetPropertyBlock(texOverride);
					}
					texOverride.SetTexture("_MainTex", defaultTexture);
					mainRenderer.SetPropertyBlock(texOverride);
				}
				if (fleshDroneCooldown <= 0f)
				{
					started = true;
					for (int num4 = currentDrones.Count - 1; num4 >= 0; num4--)
					{
						if (currentDrones[num4] == null)
						{
							currentDrones.RemoveAt(num4);
						}
					}
					texOverride.SetTexture("_MainTex", attackTexture);
					mainRenderer.SetPropertyBlock(texOverride);
					idleTimer = 0f;
					fleshDroneCooldown = maxDroneCooldown;
					attackCooldown = 3f;
					inAction = true;
					rotationSpeed = 0f;
					rotationSpeedTarget = 0f;
					if (stat.health > maxHealth / 2f)
					{
						droneAmount = 10;
					}
					else
					{
						droneAmount = 12;
					}
					if (difficulty == 1)
					{
						droneAmount = Mathf.RoundToInt((float)droneAmount / 1.5f);
					}
					else if (difficulty == 0)
					{
						droneAmount /= 2;
					}
					if (altVersion)
					{
						droneAmount /= 2;
					}
					if (droneAmount < 3)
					{
						droneAmount = 3;
					}
					if (timesHealed == 1)
					{
						onFirstHeal?.Invoke();
					}
					timesHealed++;
					if (currentDrones.Count <= 0)
					{
						healing = true;
						secondaryBarValue = 0f;
						Invoke("SpawnFleshDrones", 1f / eid.totalSpeedModifier);
					}
					else
					{
						StartHealing();
					}
					shakingCamera = true;
					aud.Play();
				}
				else if (fleshDroneCooldown > 3f)
				{
					if (attackCooldown > 0f)
					{
						float num5 = 1f;
						if (difficulty == 1)
						{
							num5 = 0.9f;
						}
						else if (difficulty == 0)
						{
							num5 = 0.75f;
						}
						attackCooldown = Mathf.MoveTowards(attackCooldown, 0f, Time.deltaTime * num5 * eid.totalSpeedModifier);
					}
					else
					{
						int num6 = 2;
						if (!currentBlackHole && difficulty > 0)
						{
							num6 = 3;
						}
						int num7 = Random.Range(0, num6);
						if (num7 == previousAttack)
						{
							num7++;
						}
						if (num7 >= num6)
						{
							num7 = 0;
						}
						inAction = true;
						Color color = Color.white;
						float time = 1f / eid.totalSpeedModifier;
						switch (num7)
						{
						case 0:
							Invoke("SpawnInsignia", time);
							attackCooldown = 4f;
							break;
						case 1:
							Invoke("HomingProjectileAttack", time);
							color = ((!altVersion) ? new Color(0f, 1f, 0.9f) : new Color(1f, 0.75f, 0f));
							attackCooldown = 1f;
							break;
						case 2:
							Invoke("SpawnBlackHole", time);
							color = new Color(1f, 0f, 1f);
							attackCooldown = 2f;
							break;
						}
						GameObject obj = Object.Instantiate(attackWindUp, rotationBone.position, Quaternion.LookRotation(target.position - rotationBone.position));
						if (obj.TryGetComponent<Light>(out var component))
						{
							component.color = color;
						}
						if (obj.TryGetComponent<SpriteRenderer>(out var component2))
						{
							component2.color = color;
						}
						previousAttack = num7;
					}
				}
			}
		}
		else
		{
			texOverride.SetTexture("_MainTex", attackTexture);
			mainRenderer.SetPropertyBlock(texOverride);
			idleTimer = 0f;
			if (currentProjectile < projectileAmount)
			{
				homingProjectileCooldown = Mathf.MoveTowards(homingProjectileCooldown, 0f, Time.deltaTime * (Mathf.Abs(rotationSpeed) / 10f) * eid.totalSpeedModifier);
				if (homingProjectileCooldown <= 0f)
				{
					GameObject gameObject = Object.Instantiate(homingProjectile, rotationBone.position + rotationBone.up * 8f, rotationBone.rotation);
					Projectile component3 = gameObject.GetComponent<Projectile>();
					component3.target = target;
					component3.safeEnemyType = EnemyType.FleshPrison;
					if (difficulty >= 2)
					{
						component3.turningSpeedMultiplier = 0.5f;
					}
					else if (difficulty == 1)
					{
						component3.turningSpeedMultiplier = 0.45f;
					}
					else
					{
						component3.turningSpeedMultiplier = 0.4f;
					}
					if (altVersion)
					{
						component3.turnSpeed *= 4f;
						component3.turningSpeedMultiplier *= 4f;
						component3.predictiveHomingMultiplier = 1.25f;
					}
					component3.speed *= eid.totalSpeedModifier;
					component3.damage *= eid.totalDamageModifier;
					homingProjectileCooldown = 1f;
					currentProjectile++;
					gameObject.transform.SetParent(base.transform, worldPositionStays: true);
					if (altVersion && gameObject.TryGetComponent<Rigidbody>(out var component4))
					{
						component4.AddForce(Vector3.up * 50f, ForceMode.VelocityChange);
					}
				}
				if (currentProjectile >= projectileAmount)
				{
					inAction = false;
					anim?.SetBool("Shooting", value: false);
					if (rotationSpeed >= 0f)
					{
						rotationSpeedTarget = 45f;
					}
					else
					{
						rotationSpeedTarget = -45f;
					}
					if (fleshDroneCooldown < 1f)
					{
						fleshDroneCooldown = 1f;
					}
				}
			}
		}
		if (fleshDroneCooldown > 0f)
		{
			fleshDroneCooldown = Mathf.MoveTowards(fleshDroneCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (shakingCamera)
		{
			MonoSingleton<CameraController>.Instance.CameraShake(0.25f);
		}
	}

	private void SpawnFleshDrones()
	{
		if (currentDrone == 0)
		{
			targeter = new GameObject();
			targeter.transform.position = rotationBone.position;
			if (altVersion)
			{
				targeter.transform.rotation = Quaternion.LookRotation(Vector3.up);
			}
			else
			{
				targeter.transform.rotation = Quaternion.LookRotation(new Vector3(target.position.x, targeter.transform.position.y, target.position.z) - targeter.transform.position);
			}
		}
		if (currentDrone < droneAmount)
		{
			secondaryBarValue = (float)currentDrone / (float)droneAmount;
			GameObject gameObject = (((difficulty != 3 || currentDrone % 5 != 0) && (difficulty != 4 || currentDrone % 3 != 0) && difficulty != 5) ? Object.Instantiate(fleshDrone, targeter.transform.position + targeter.transform.up * (altVersion ? 50 : 20), targeter.transform.rotation) : Object.Instantiate(skullDrone, targeter.transform.position + targeter.transform.up * (altVersion ? 50 : 20), targeter.transform.rotation));
			gameObject.transform.SetParent(base.transform, worldPositionStays: true);
			if (gameObject.TryGetComponent<EnemyIdentifier>(out var component))
			{
				component.dontCountAsKills = true;
				component.damageBuff = eid.damageBuff;
				component.healthBuff = eid.healthBuff;
				component.speedBuff = eid.speedBuff;
			}
			targeter.transform.Rotate((altVersion ? Vector3.forward : Vector3.forward) * (360 / droneAmount));
			if (gameObject.TryGetComponent<DroneFlesh>(out var component2))
			{
				currentDrones.Add(component2);
			}
			currentDrone++;
			Invoke("SpawnFleshDrones", 0.1f / eid.totalSpeedModifier);
		}
		else
		{
			inAction = false;
			if (Random.Range(0, 2) == 0)
			{
				rotationSpeedTarget = 45f;
			}
			else
			{
				rotationSpeedTarget = -45f;
			}
			aud.Stop();
			shakingCamera = false;
			currentDrone = 0;
			Object.Destroy(targeter);
			fleshDroneCooldown = (altVersion ? 30 : 25);
			healing = false;
		}
	}

	private void StartHealing()
	{
		healing = true;
		secondaryBarValue = 0f;
		for (int i = 0; i < currentDrones.Count; i++)
		{
			if (currentDrones[i] == null)
			{
				currentDrones.RemoveAt(i);
				continue;
			}
			if (Object.Instantiate(healingTargetEffect, currentDrones[i].transform).TryGetComponent<LineToPoint>(out var component))
			{
				component.targets[1] = rotationBone;
			}
			if (currentDrones[i].TryGetComponent<Rigidbody>(out var component2))
			{
				component2.isKinematic = true;
			}
		}
		if (difficulty >= 1)
		{
			eid.totalDamageTakenMultiplier = 0.1f;
		}
		if (currentDrones.Count > 0)
		{
			Invoke("HealFromDrone", 5f / eid.totalSpeedModifier);
		}
		else
		{
			Invoke("SpawnFleshDrones", 1f / eid.totalSpeedModifier);
		}
	}

	private void HealFromDrone()
	{
		if (stat.health <= 0f)
		{
			return;
		}
		if (currentDrones.Count > 0)
		{
			if (currentDrones[0] != null)
			{
				float num = 1f;
				if (difficulty == 1)
				{
					num = 0.75f;
				}
				else if (difficulty == 0)
				{
					num = 0.35f;
				}
				num /= eid.totalHealthModifier;
				if (altVersion)
				{
					num *= 2f;
				}
				if (!Physics.Raycast(rotationBone.position, currentDrones[0].transform.position - rotationBone.position, Vector3.Distance(rotationBone.position, currentDrones[0].transform.position), LayerMaskDefaults.Get(LMD.Environment)))
				{
					if (stat.health + 10f * num <= maxHealth)
					{
						stat.health += 10f * num;
					}
					else
					{
						stat.health = maxHealth;
					}
					eid.health = stat.health;
					Object.Instantiate(healingEffect, rotationBone);
				}
				currentDrones[0].Explode();
				currentDrones.RemoveAt(0);
				Invoke("HealFromDrone", 0.25f / eid.totalSpeedModifier);
			}
			else
			{
				currentDrones.RemoveAt(0);
				HealFromDrone();
			}
		}
		else
		{
			eid.totalDamageTakenMultiplier = 1f;
			SpawnFleshDrones();
		}
	}

	private void HomingProjectileAttack()
	{
		inAction = true;
		texOverride.SetTexture("_MainTex", attackTexture);
		mainRenderer.SetPropertyBlock(texOverride);
		idleTimer = 0f;
		homingProjectileCooldown = 1f;
		currentProjectile = 0;
		anim?.SetBool("Shooting", value: true);
		if (Random.Range(0, 2) == 0)
		{
			rotationSpeedTarget = 360f;
		}
		else
		{
			rotationSpeedTarget = -360f;
		}
		if (altVersion)
		{
			rotationSpeedTarget /= 8f;
		}
		if ((rotationSpeedTarget > 0f && rotationSpeed < 0f) || (rotationSpeedTarget < 0f && rotationSpeed > 0f))
		{
			rotationSpeed = 0f;
		}
		if (stat.health > maxHealth / 2f)
		{
			if (difficulty >= 2)
			{
				projectileAmount = 50;
			}
			else
			{
				projectileAmount = 35;
			}
		}
		else if (difficulty >= 2)
		{
			projectileAmount = 75;
		}
		else
		{
			projectileAmount = 50;
		}
		if (altVersion)
		{
			projectileAmount /= 3;
		}
	}

	private void SpawnInsignia()
	{
		inAction = false;
		GameObject gameObject = Object.Instantiate(insignia, MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, Quaternion.identity);
		if (altVersion)
		{
			Vector3 playerVelocity = MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity();
			playerVelocity.y = 0f;
			if (playerVelocity.magnitude > 0f)
			{
				gameObject.transform.LookAt(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position + playerVelocity);
			}
			else
			{
				gameObject.transform.Rotate(Vector3.up * Random.Range(0f, 360f), Space.Self);
			}
			gameObject.transform.Rotate(Vector3.right * 90f, Space.Self);
		}
		if (gameObject.TryGetComponent<VirtueInsignia>(out var component))
		{
			component.predictive = true;
			component.noTracking = true;
			component.otherParent = base.transform;
			if (stat.health > maxHealth / 2f)
			{
				component.charges = 2;
			}
			else
			{
				component.charges = 3;
			}
			if (difficulty == 3)
			{
				component.charges++;
			}
			component.windUpSpeedMultiplier = 0.5f;
			component.windUpSpeedMultiplier *= eid.totalSpeedModifier;
			component.damage = Mathf.RoundToInt((float)component.damage * eid.totalDamageModifier);
			component.target = MonoSingleton<PlayerTracker>.Instance.GetPlayer();
			component.predictiveVersion = null;
			Light light = gameObject.AddComponent<Light>();
			light.range = 30f;
			light.intensity = 50f;
		}
		if (difficulty >= 2)
		{
			gameObject.transform.localScale = new Vector3(8f, 2f, 8f);
		}
		else if (difficulty == 1)
		{
			gameObject.transform.localScale = new Vector3(7f, 2f, 7f);
		}
		else
		{
			gameObject.transform.localScale = new Vector3(5f, 2f, 5f);
		}
		GoreZone componentInParent = GetComponentInParent<GoreZone>();
		if ((bool)componentInParent)
		{
			gameObject.transform.SetParent(componentInParent.transform, worldPositionStays: true);
		}
		else
		{
			gameObject.transform.SetParent(base.transform, worldPositionStays: true);
		}
		if (fleshDroneCooldown < 1f)
		{
			fleshDroneCooldown = 1f;
		}
	}

	private void SpawnBlackHole()
	{
		GameObject gameObject = Object.Instantiate(blackHole, base.transform);
		gameObject.transform.position = rotationBone.position;
		currentBlackHole = gameObject.GetComponent<BlackHoleProjectile>();
		if ((bool)currentBlackHole)
		{
			currentBlackHole.safeType = EnemyType.FleshPrison;
			currentBlackHole.Activate();
			currentBlackHole.speed *= eid.totalSpeedModifier;
		}
		inAction = false;
		texOverride.SetTexture("_MainTex", attackTexture);
		mainRenderer.SetPropertyBlock(texOverride);
		idleTimer = 0.5f;
		if (fleshDroneCooldown < 1f)
		{
			fleshDroneCooldown = 1f;
		}
	}

	public void ForceDronesOff()
	{
		noDrones = true;
		CancelInvoke("HealFromDrone");
		CancelInvoke("SpawnFleshDrones");
		anim?.SetBool("Shooting", value: false);
		if (currentDrones.Count > 0)
		{
			foreach (DroneFlesh currentDrone in currentDrones)
			{
				currentDrone.Explode();
			}
		}
		if ((bool)currentBlackHole)
		{
			currentBlackHole.Explode();
		}
		VirtueInsignia[] array = Object.FindObjectsOfType<VirtueInsignia>();
		foreach (VirtueInsignia virtueInsignia in array)
		{
			if (virtueInsignia.otherParent == base.transform)
			{
				Object.Destroy(virtueInsignia.gameObject);
			}
		}
		Projectile[] componentsInChildren = GetComponentsInChildren<Projectile>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i].gameObject);
		}
		if ((bool)anim)
		{
			anim.speed = 20f;
		}
	}
}
