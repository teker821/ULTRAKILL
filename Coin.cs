using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
	public GameObject sourceWeapon;

	private Rigidbody rb;

	private bool checkingSpeed;

	private float timeToDelete = 1f;

	public LayerMask lmask;

	public GameObject refBeam;

	public Vector3 hitPoint = Vector3.zero;

	private Collider[] cols;

	public bool shot;

	[HideInInspector]
	public bool shotByEnemy;

	private bool wasShotByEnemy;

	public GameObject coinBreak;

	public float power;

	private EnemyIdentifier eid;

	public bool quickDraw;

	public Material uselessMaterial;

	private GameObject altBeam;

	public GameObject coinHitSound;

	private int hitTimes = 1;

	public bool doubled;

	public GameObject flash;

	public GameObject enemyFlash;

	public GameObject chargeEffect;

	private GameObject currentCharge;

	private StyleHUD shud;

	public CoinChainCache ccc;

	public int ricochets;

	[HideInInspector]
	public int difficulty = -1;

	public bool dontDestroyOnPlayerRespawn;

	public bool ignoreBlessedEnemies;

	private void Start()
	{
		MonoSingleton<CoinList>.Instance.AddCoin(this);
		shud = MonoSingleton<StyleHUD>.Instance;
		doubled = false;
		Invoke("GetDeleted", 5f);
		Invoke("StartCheckingSpeed", 0.1f);
		Invoke("TripleTime", 0.35f);
		Invoke("TripleTimeEnd", 0.417f);
		Invoke("DoubleTime", 1f);
		rb = GetComponent<Rigidbody>();
		cols = GetComponents<Collider>();
		Collider[] array = cols;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
	}

	private void Update()
	{
		if (!shot)
		{
			if (checkingSpeed && rb.velocity.magnitude < 1f)
			{
				timeToDelete -= Time.deltaTime * 10f;
			}
			else
			{
				timeToDelete = 1f;
			}
			if (timeToDelete <= 0f)
			{
				GetDeleted();
			}
		}
	}

	private void OnDestroy()
	{
		MonoSingleton<CoinList>.Instance.RemoveCoin(this);
	}

	private void TripleTime()
	{
		if (!shot)
		{
			hitTimes = 2;
			doubled = true;
			if ((bool)currentCharge)
			{
				Object.Destroy(currentCharge);
			}
			if ((bool)flash)
			{
				currentCharge = Object.Instantiate(flash, base.transform.position, Quaternion.identity);
				currentCharge.transform.SetParent(base.transform, worldPositionStays: true);
			}
		}
	}

	private void TripleTimeEnd()
	{
		if (!shot)
		{
			hitTimes = 1;
			doubled = true;
		}
	}

	private void DoubleTime()
	{
		if (!shot)
		{
			hitTimes = 2;
			doubled = true;
			if ((bool)currentCharge)
			{
				Object.Destroy(currentCharge);
			}
			if ((bool)chargeEffect)
			{
				currentCharge = Object.Instantiate(chargeEffect, base.transform.position, base.transform.rotation);
				currentCharge.transform.SetParent(base.transform, worldPositionStays: true);
			}
		}
	}

	public void DelayedReflectRevolver(Vector3 hitp, GameObject beam = null)
	{
		if (checkingSpeed)
		{
			if (shotByEnemy)
			{
				CancelInvoke("EnemyReflect");
				CancelInvoke("ShootAtPlayer");
				shotByEnemy = false;
			}
			ricochets++;
			CancelInvoke("TripleTime");
			CancelInvoke("TripleTimeEnd");
			CancelInvoke("DoubleTime");
			if (!ccc && altBeam == null)
			{
				GameObject gameObject = new GameObject();
				ccc = gameObject.AddComponent<CoinChainCache>();
				gameObject.AddComponent<RemoveOnTime>().time = 5f;
			}
			rb.isKinematic = true;
			shot = true;
			hitPoint = hitp;
			altBeam = beam;
			Invoke("ReflectRevolver", 0.1f);
		}
	}

	public void ReflectRevolver()
	{
		GameObject gameObject = null;
		float num = float.PositiveInfinity;
		Vector3 position = base.transform.position;
		GetComponent<SphereCollider>().enabled = false;
		bool flag = false;
		bool flag2 = false;
		if (MonoSingleton<CoinList>.Instance.revolverCoinsList.Count > 1)
		{
			foreach (Coin revolverCoins in MonoSingleton<CoinList>.Instance.revolverCoinsList)
			{
				if (revolverCoins != this && (!revolverCoins.shot || revolverCoins.shotByEnemy))
				{
					float sqrMagnitude = (revolverCoins.transform.position - position).sqrMagnitude;
					if (sqrMagnitude < num && !Physics.Raycast(base.transform.position, revolverCoins.transform.position - base.transform.position, out var _, Vector3.Distance(base.transform.position, revolverCoins.transform.position) - 0.5f, lmask))
					{
						gameObject = revolverCoins.gameObject;
						num = sqrMagnitude;
					}
				}
			}
			if (gameObject != null)
			{
				flag = true;
				Coin component = gameObject.GetComponent<Coin>();
				component.power = power + 1f;
				component.ricochets += ricochets;
				if (quickDraw)
				{
					component.quickDraw = true;
				}
				if (component.shotByEnemy)
				{
					component.CancelInvoke("EnemyReflect");
					component.CancelInvoke("ShootAtPlayer");
					component.shotByEnemy = false;
				}
				AudioSource[] array = null;
				if (altBeam == null)
				{
					if ((bool)ccc)
					{
						component.ccc = ccc;
					}
					else
					{
						GameObject gameObject2 = new GameObject();
						ccc = gameObject2.AddComponent<CoinChainCache>();
						component.ccc = ccc;
						gameObject2.AddComponent<RemoveOnTime>().time = 5f;
					}
					component.DelayedReflectRevolver(gameObject.transform.position);
					LineRenderer component2 = SpawnBeam().GetComponent<LineRenderer>();
					array = component2.GetComponents<AudioSource>();
					if (hitPoint == Vector3.zero)
					{
						component2.SetPosition(0, base.transform.position);
					}
					else
					{
						component2.SetPosition(0, hitPoint);
					}
					component2.SetPosition(1, gameObject.transform.position);
					if (power > 2f)
					{
						AudioSource[] array2 = array;
						foreach (AudioSource obj in array2)
						{
							obj.pitch = 1f + (power - 2f) / 5f;
							obj.Play();
						}
					}
				}
			}
		}
		if (!flag)
		{
			List<Transform> list = new List<Transform>();
			foreach (Grenade grenade in MonoSingleton<GrenadeList>.Instance.grenadeList)
			{
				if (!grenade.playerRiding)
				{
					list.Add(grenade.transform);
				}
			}
			foreach (Cannonball cannonball in MonoSingleton<GrenadeList>.Instance.cannonballList)
			{
				list.Add(cannonball.transform);
			}
			Transform transform = null;
			gameObject = null;
			num = float.PositiveInfinity;
			position = base.transform.position;
			foreach (Transform item3 in list)
			{
				float magnitude = (item3.transform.position - position).magnitude;
				if (magnitude < num && Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.GetPlayer().transform.position, item3.transform.position) < 100f && !Physics.Raycast(base.transform.position, item3.transform.position - base.transform.position, out var _, Vector3.Distance(base.transform.position, item3.transform.position) - 0.5f, lmask))
				{
					gameObject = item3.gameObject;
					transform = item3;
					num = magnitude;
				}
			}
			if (gameObject != null && transform != null && !altBeam)
			{
				LineRenderer component3 = SpawnBeam().GetComponent<LineRenderer>();
				component3.GetComponents<AudioSource>();
				if (hitPoint == Vector3.zero)
				{
					component3.SetPosition(0, base.transform.position);
				}
				else
				{
					component3.SetPosition(0, hitPoint);
				}
				component3.SetPosition(1, gameObject.transform.position);
				Cannonball component5;
				if (transform.TryGetComponent<Grenade>(out var component4))
				{
					component4.Explode(component4.rocket, harmless: false, !component4.rocket);
				}
				else if (transform.TryGetComponent<Cannonball>(out component5))
				{
					component5.Explode();
				}
			}
			if (gameObject == null)
			{
				GameObject[] array3 = GameObject.FindGameObjectsWithTag("Enemy");
				foreach (GameObject gameObject3 in array3)
				{
					float sqrMagnitude2 = (gameObject3.transform.position - position).sqrMagnitude;
					if (!(sqrMagnitude2 < num))
					{
						continue;
					}
					eid = gameObject3.GetComponent<EnemyIdentifier>();
					if (eid != null && !eid.dead && (!ccc || !ccc.beenHit.Contains(eid.gameObject)) && (!eid.blessed || !ignoreBlessedEnemies))
					{
						Transform transform2;
						if (eid.weakPoint != null && eid.weakPoint.activeInHierarchy)
						{
							transform2 = eid.weakPoint.transform;
						}
						else
						{
							EnemyIdentifierIdentifier componentInChildren = eid.GetComponentInChildren<EnemyIdentifierIdentifier>();
							transform2 = ((!componentInChildren) ? eid.transform : componentInChildren.transform);
						}
						if (!Physics.Raycast(base.transform.position, transform2.position - base.transform.position, out var _, Vector3.Distance(base.transform.position, transform2.position) - 0.5f, lmask))
						{
							gameObject = gameObject3;
							num = sqrMagnitude2;
						}
						else
						{
							eid = null;
						}
					}
					else
					{
						eid = null;
					}
				}
				if (gameObject != null)
				{
					if (eid == null)
					{
						eid = gameObject.GetComponent<EnemyIdentifier>();
					}
					flag2 = true;
					if (altBeam == null)
					{
						if ((bool)ccc)
						{
							ccc.beenHit.Add(eid.gameObject);
						}
						LineRenderer component6 = SpawnBeam().GetComponent<LineRenderer>();
						AudioSource[] components = component6.GetComponents<AudioSource>();
						if (hitPoint == Vector3.zero)
						{
							component6.SetPosition(0, base.transform.position);
						}
						else
						{
							component6.SetPosition(0, hitPoint);
						}
						Vector3 zero = Vector3.zero;
						zero = ((!(eid.weakPoint != null) || !eid.weakPoint.activeInHierarchy) ? eid.GetComponentInChildren<EnemyIdentifierIdentifier>().transform.position : eid.weakPoint.transform.position);
						component6.SetPosition(1, zero);
						if (eid.weakPoint != null && eid.weakPoint.activeInHierarchy && eid.weakPoint.GetComponent<EnemyIdentifierIdentifier>() != null)
						{
							bool flag3 = false;
							if (eid.enemyType == EnemyType.Streetcleaner && Physics.Raycast(base.transform.position, eid.weakPoint.transform.position - base.transform.position, out var hitInfo4, Vector3.Distance(base.transform.position, eid.weakPoint.transform.position), LayerMaskDefaults.Get(LMD.Enemies)) && hitInfo4.transform != eid.weakPoint.transform)
							{
								EnemyIdentifierIdentifier component7 = hitInfo4.transform.GetComponent<EnemyIdentifierIdentifier>();
								if ((bool)component7 && (bool)component7.eid && component7.eid == eid)
								{
									eid.DeliverDamage(hitInfo4.transform.gameObject, (hitInfo4.transform.position - base.transform.position).normalized * 10000f, hitInfo4.transform.position, power, tryForExplode: false, 1f, sourceWeapon);
								}
								flag3 = true;
							}
							if (!eid.blessed)
							{
								RicoshotPointsCheck();
								if (quickDraw)
								{
									shud.AddPoints(50, "ultrakill.quickdraw", sourceWeapon, eid);
								}
							}
							eid.hitter = "revolver";
							if (!eid.hitterWeapons.Contains("revolver1"))
							{
								eid.hitterWeapons.Add("revolver1");
							}
							if (!flag3)
							{
								eid.DeliverDamage(eid.weakPoint, (eid.weakPoint.transform.position - base.transform.position).normalized * 10000f, zero, power, tryForExplode: false, 1f, sourceWeapon);
							}
						}
						else if (eid.weakPoint != null && eid.weakPoint.activeInHierarchy && eid.weakPoint.GetComponentInChildren<Breakable>() != null)
						{
							Breakable componentInChildren2 = eid.weakPoint.GetComponentInChildren<Breakable>();
							RicoshotPointsCheck();
							if (componentInChildren2.precisionOnly)
							{
								shud.AddPoints(100, "ultrakill.interruption", sourceWeapon, eid);
								MonoSingleton<TimeController>.Instance.ParryFlash();
								if ((bool)componentInChildren2.interruptEnemy && !componentInChildren2.interruptEnemy.blessed)
								{
									componentInChildren2.interruptEnemy.Explode();
								}
							}
							componentInChildren2.Break();
						}
						else
						{
							RicoshotPointsCheck();
							eid.hitter = "revolver";
							eid.DeliverDamage(eid.GetComponentInChildren<EnemyIdentifierIdentifier>().gameObject, (eid.GetComponentInChildren<EnemyIdentifierIdentifier>().transform.position - base.transform.position).normalized * 10000f, zero, power, tryForExplode: false, 1f, sourceWeapon);
						}
						if (power > 2f)
						{
							AudioSource[] array2 = components;
							foreach (AudioSource obj2 in array2)
							{
								obj2.pitch = 1f + (power - 2f) / 5f;
								obj2.Play();
							}
						}
						eid = null;
					}
				}
				else
				{
					gameObject = null;
					List<GameObject> list2 = new List<GameObject>();
					array3 = GameObject.FindGameObjectsWithTag("Glass");
					foreach (GameObject item in array3)
					{
						list2.Add(item);
					}
					array3 = GameObject.FindGameObjectsWithTag("GlassFloor");
					foreach (GameObject item2 in array3)
					{
						list2.Add(item2);
					}
					if (list2.Count > 0)
					{
						gameObject = null;
						num = float.PositiveInfinity;
						position = base.transform.position;
						foreach (GameObject item4 in list2)
						{
							float sqrMagnitude3 = (item4.transform.position - position).sqrMagnitude;
							if (!(sqrMagnitude3 < num))
							{
								continue;
							}
							Glass componentInChildren3 = item4.GetComponentInChildren<Glass>();
							if (componentInChildren3 != null && !componentInChildren3.broken && (!ccc || !ccc.beenHit.Contains(componentInChildren3.gameObject)))
							{
								Transform transform3 = item4.transform;
								if (!Physics.Raycast(base.transform.position, transform3.position - base.transform.position, out var hitInfo5, Vector3.Distance(base.transform.position, transform3.position) - 0.5f, lmask) || hitInfo5.transform.gameObject.tag == "Glass" || hitInfo5.transform.gameObject.tag == "GlassFloor")
								{
									gameObject = item4;
									num = sqrMagnitude3;
								}
							}
						}
						if (gameObject != null && altBeam == null)
						{
							gameObject.GetComponentInChildren<Glass>().Shatter();
							if ((bool)ccc)
							{
								ccc.beenHit.Add(gameObject);
							}
							LineRenderer component8 = SpawnBeam().GetComponent<LineRenderer>();
							if (power > 2f)
							{
								AudioSource[] array2 = component8.GetComponents<AudioSource>();
								foreach (AudioSource obj3 in array2)
								{
									obj3.pitch = 1f + (power - 2f) / 5f;
									obj3.Play();
								}
							}
							if (hitPoint == Vector3.zero)
							{
								component8.SetPosition(0, base.transform.position);
							}
							else
							{
								component8.SetPosition(0, hitPoint);
							}
							component8.SetPosition(1, gameObject.transform.position);
						}
					}
					if ((list2.Count == 0 || gameObject == null) && altBeam == null)
					{
						Vector3 normalized = Random.insideUnitSphere.normalized;
						LineRenderer component9 = SpawnBeam().GetComponent<LineRenderer>();
						if (power > 2f)
						{
							AudioSource[] array2 = component9.GetComponents<AudioSource>();
							foreach (AudioSource obj4 in array2)
							{
								obj4.pitch = 1f + (power - 2f) / 5f;
								obj4.Play();
							}
						}
						if (hitPoint == Vector3.zero)
						{
							component9.SetPosition(0, base.transform.position);
						}
						else
						{
							component9.SetPosition(0, hitPoint);
						}
						if (Physics.Raycast(base.transform.position, normalized, out var hitInfo6, float.PositiveInfinity, lmask))
						{
							component9.SetPosition(1, hitInfo6.point);
						}
						else
						{
							component9.SetPosition(1, base.transform.position + normalized * 1000f);
						}
					}
				}
			}
		}
		if (altBeam != null)
		{
			AudioSource[] components2 = Object.Instantiate(coinHitSound, base.transform.position, Quaternion.identity).GetComponents<AudioSource>();
			RevolverBeam component10 = altBeam.GetComponent<RevolverBeam>();
			altBeam.transform.position = base.transform.position;
			if (component10.beamType == BeamType.Revolver && hitTimes > 1 && component10.strongAlt && component10.hitAmount < 99)
			{
				component10.hitAmount++;
				component10.maxHitsPerTarget = component10.hitAmount;
			}
			if (flag2)
			{
				if (eid.weakPoint != null && eid.weakPoint.activeInHierarchy)
				{
					altBeam.transform.LookAt(eid.weakPoint.transform.position);
				}
				else
				{
					altBeam.transform.LookAt(eid.GetComponentInChildren<EnemyIdentifierIdentifier>().transform.position);
				}
				if (!eid.blessed)
				{
					RicoshotPointsCheck();
					if (quickDraw)
					{
						shud.AddPoints(50, "ultrakill.quickdraw", sourceWeapon, eid);
					}
				}
				if (component10.beamType == BeamType.Revolver)
				{
					eid.hitter = "revolver";
					if (!eid.hitterWeapons.Contains("revolver" + component10.gunVariation))
					{
						eid.hitterWeapons.Add("revolver" + component10.gunVariation);
					}
				}
				else
				{
					eid.hitter = "railcannon";
					if (!eid.hitterWeapons.Contains("railcannon0"))
					{
						eid.hitterWeapons.Add("railcannon0");
					}
				}
			}
			else if (gameObject != null)
			{
				altBeam.transform.LookAt(gameObject.transform.position);
			}
			else
			{
				altBeam.transform.forward = Random.insideUnitSphere.normalized;
			}
			if (!flag)
			{
				if (component10.beamType == BeamType.Revolver && component10.hasBeenRicocheter)
				{
					if (component10.maxHitsPerTarget < (component10.strongAlt ? 4 : 3))
					{
						component10.maxHitsPerTarget = Mathf.Min(component10.maxHitsPerTarget + 2, component10.strongAlt ? 4 : 3);
					}
				}
				else
				{
					component10.damage += power / 4f;
				}
			}
			if (power > 2f)
			{
				AudioSource[] array2 = components2;
				foreach (AudioSource obj5 in array2)
				{
					obj5.pitch = 1f + (power - 2f) / 5f;
					obj5.Play();
				}
			}
			altBeam.SetActive(value: true);
		}
		hitTimes--;
		if (hitTimes > 0 && altBeam == null)
		{
			Invoke("ReflectRevolver", 0.05f);
			return;
		}
		base.gameObject.SetActive(value: false);
		new GameObject().AddComponent<CoinCollector>().coin = base.gameObject;
		CancelInvoke("GetDeleted");
	}

	public void DelayedPunchflection()
	{
		if (checkingSpeed && (!shot || shotByEnemy))
		{
			if (shotByEnemy)
			{
				CancelInvoke("EnemyReflect");
				CancelInvoke("ShootAtPlayer");
				shotByEnemy = false;
			}
			CancelInvoke("TripleTime");
			CancelInvoke("TripleTimeEnd");
			CancelInvoke("DoubleTime");
			ricochets++;
			if ((bool)currentCharge)
			{
				Object.Destroy(currentCharge);
			}
			rb.isKinematic = true;
			shot = true;
			Punchflection();
		}
	}

	public async void Punchflection()
	{
		bool flag = false;
		bool flag2 = false;
		GameObject gameObject = null;
		float num = float.PositiveInfinity;
		Vector3 position = base.transform.position;
		GameObject gameObject2 = Object.Instantiate(base.gameObject, base.transform.position, Quaternion.identity);
		gameObject2.SetActive(value: false);
		Vector3 position2 = base.transform.position;
		GetComponent<SphereCollider>().enabled = false;
		GameObject[] array = GameObject.FindGameObjectsWithTag("Enemy");
		foreach (GameObject gameObject3 in array)
		{
			float sqrMagnitude = (gameObject3.transform.position - position).sqrMagnitude;
			if (!(sqrMagnitude < num))
			{
				continue;
			}
			eid = gameObject3.GetComponent<EnemyIdentifier>();
			if (eid != null && !eid.dead)
			{
				Transform transform = ((!(eid.weakPoint != null) || !eid.weakPoint.activeInHierarchy) ? eid.GetComponentInChildren<EnemyIdentifierIdentifier>().transform : eid.weakPoint.transform);
				if (!Physics.Raycast(base.transform.position, transform.position - base.transform.position, out var _, Vector3.Distance(base.transform.position, transform.position) - 0.5f, lmask))
				{
					gameObject = gameObject3;
					num = sqrMagnitude;
				}
				else
				{
					eid = null;
				}
			}
			else
			{
				eid = null;
			}
		}
		if (gameObject != null)
		{
			if (eid == null)
			{
				eid = gameObject.GetComponent<EnemyIdentifier>();
			}
			LineRenderer component = SpawnBeam().GetComponent<LineRenderer>();
			AudioSource[] components = component.GetComponents<AudioSource>();
			if (hitPoint == Vector3.zero)
			{
				component.SetPosition(0, base.transform.position);
			}
			else
			{
				component.SetPosition(0, hitPoint);
			}
			_ = Vector3.zero;
			Vector3 vector = ((!(eid.weakPoint != null) || !eid.weakPoint.activeInHierarchy) ? eid.GetComponentInChildren<EnemyIdentifierIdentifier>().transform.position : eid.weakPoint.transform.position);
			if (eid.blessed)
			{
				flag2 = true;
			}
			component.SetPosition(1, vector);
			position2 = vector;
			shud.AddPoints(50, "ultrakill.fistfullofdollar", sourceWeapon, eid);
			if (eid.weakPoint != null && eid.weakPoint.activeInHierarchy && eid.weakPoint.GetComponent<EnemyIdentifierIdentifier>() != null)
			{
				eid.hitter = "coin";
				if (!eid.hitterWeapons.Contains("coin"))
				{
					eid.hitterWeapons.Add("coin");
				}
				eid.DeliverDamage(eid.weakPoint, (eid.weakPoint.transform.position - base.transform.position).normalized * 10000f, vector, power, tryForExplode: false, 1f, sourceWeapon);
			}
			else if (eid.weakPoint != null && eid.weakPoint.activeInHierarchy)
			{
				Breakable componentInChildren = eid.weakPoint.GetComponentInChildren<Breakable>();
				if (componentInChildren.precisionOnly)
				{
					shud.AddPoints(100, "ultrakill.interruption", sourceWeapon, eid);
					MonoSingleton<TimeController>.Instance.ParryFlash();
					if ((bool)componentInChildren.interruptEnemy && !componentInChildren.interruptEnemy.blessed)
					{
						componentInChildren.interruptEnemy.Explode();
					}
				}
				componentInChildren.Break();
			}
			else
			{
				eid.hitter = "coin";
				eid.DeliverDamage(eid.GetComponentInChildren<EnemyIdentifierIdentifier>().gameObject, (eid.GetComponentInChildren<EnemyIdentifierIdentifier>().transform.position - base.transform.position).normalized * 10000f, hitPoint, power, tryForExplode: false, 1f, sourceWeapon);
			}
			if (power > 2f)
			{
				AudioSource[] array2 = components;
				foreach (AudioSource obj in array2)
				{
					obj.pitch = 1f + (power - 2f) / 5f;
					obj.Play();
				}
				eid = null;
			}
		}
		else
		{
			flag = true;
			Vector3 forward = MonoSingleton<CameraController>.Instance.transform.forward;
			LineRenderer component2 = SpawnBeam().GetComponent<LineRenderer>();
			if (power > 2f)
			{
				AudioSource[] array2 = component2.GetComponents<AudioSource>();
				foreach (AudioSource obj2 in array2)
				{
					obj2.pitch = 1f + (power - 2f) / 5f;
					obj2.Play();
				}
			}
			if (hitPoint == Vector3.zero)
			{
				component2.SetPosition(0, base.transform.position);
			}
			else
			{
				component2.SetPosition(0, hitPoint);
			}
			if (Physics.Raycast(MonoSingleton<CameraController>.Instance.transform.position, forward, out var hitInfo2, float.PositiveInfinity, lmask))
			{
				component2.SetPosition(1, hitInfo2.point);
				position2 = hitInfo2.point - forward;
			}
			else
			{
				component2.SetPosition(1, MonoSingleton<CameraController>.Instance.transform.position + forward * 1000f);
				Object.Destroy(gameObject2);
			}
		}
		if ((bool)gameObject2)
		{
			gameObject2.transform.position = position2;
			gameObject2.SetActive(value: true);
			Coin component3 = gameObject2.GetComponent<Coin>();
			if ((bool)component3)
			{
				component3.shot = false;
				if (component3.power < 5f || (!flag && !flag2))
				{
					component3.power += 1f;
				}
				gameObject2.name = "NewCoin+" + (component3.power - 2f);
			}
			Rigidbody component4 = gameObject2.GetComponent<Rigidbody>();
			if ((bool)component4)
			{
				component4.isKinematic = false;
				component4.velocity = Vector3.zero;
				component4.AddForce(Vector3.up * 25f, ForceMode.VelocityChange);
			}
		}
		base.gameObject.SetActive(value: false);
		new GameObject().AddComponent<CoinCollector>().coin = base.gameObject;
		CancelInvoke("GetDeleted");
	}

	public void Bounce()
	{
		if (!shot)
		{
			if ((bool)currentCharge)
			{
				Object.Destroy(currentCharge);
			}
			GameObject obj = Object.Instantiate(base.gameObject, base.transform.position, Quaternion.identity);
			obj.name = "NewCoin+" + (power - 2f);
			obj.SetActive(value: false);
			Vector3 position = base.transform.position;
			obj.transform.position = position;
			obj.SetActive(value: true);
			GetComponent<SphereCollider>().enabled = false;
			shot = true;
			Coin component = obj.GetComponent<Coin>();
			if ((bool)component)
			{
				component.shot = false;
			}
			Rigidbody component2 = obj.GetComponent<Rigidbody>();
			if ((bool)component2)
			{
				component2.isKinematic = false;
				component2.velocity = Vector3.zero;
				component2.AddForce(Vector3.up * 25f, ForceMode.VelocityChange);
			}
			base.gameObject.SetActive(value: false);
			new GameObject().AddComponent<CoinCollector>().coin = base.gameObject;
			CancelInvoke("GetDeleted");
		}
	}

	public void DelayedEnemyReflect()
	{
		if (!shot)
		{
			shotByEnemy = true;
			wasShotByEnemy = true;
			CancelInvoke("TripleTime");
			CancelInvoke("TripleTimeEnd");
			CancelInvoke("DoubleTime");
			ricochets++;
			if (!ccc)
			{
				GameObject gameObject = new GameObject();
				ccc = gameObject.AddComponent<CoinChainCache>();
				gameObject.AddComponent<RemoveOnTime>().time = 5f;
			}
			rb.isKinematic = true;
			shot = true;
			Invoke("EnemyReflect", 0.1f);
		}
	}

	public void EnemyReflect()
	{
		bool flag = false;
		if (MonoSingleton<CoinList>.Instance.revolverCoinsList.Count > 1)
		{
			GameObject gameObject = null;
			float num = float.PositiveInfinity;
			Vector3 position = base.transform.position;
			foreach (Coin revolverCoins in MonoSingleton<CoinList>.Instance.revolverCoinsList)
			{
				if (revolverCoins != this && !revolverCoins.shot)
				{
					float sqrMagnitude = (revolverCoins.transform.position - position).sqrMagnitude;
					if (sqrMagnitude < num && !Physics.Raycast(base.transform.position, revolverCoins.transform.position - base.transform.position, out var _, Vector3.Distance(base.transform.position, revolverCoins.transform.position) - 0.5f, lmask))
					{
						gameObject = revolverCoins.gameObject;
						num = sqrMagnitude;
					}
				}
			}
			if (gameObject != null)
			{
				flag = true;
				Coin component = gameObject.GetComponent<Coin>();
				component.power = power + 1f;
				component.ricochets += ricochets;
				if (quickDraw)
				{
					component.quickDraw = true;
				}
				AudioSource[] array = null;
				if ((bool)ccc)
				{
					component.ccc = ccc;
				}
				else
				{
					GameObject gameObject2 = new GameObject();
					ccc = gameObject2.AddComponent<CoinChainCache>();
					component.ccc = ccc;
					gameObject2.AddComponent<RemoveOnTime>().time = 5f;
				}
				component.DelayedEnemyReflect();
				LineRenderer component2 = SpawnBeam().GetComponent<LineRenderer>();
				array = component2.GetComponents<AudioSource>();
				if (hitPoint == Vector3.zero)
				{
					component2.SetPosition(0, base.transform.position);
				}
				else
				{
					component2.SetPosition(0, hitPoint);
				}
				component2.SetPosition(1, gameObject.transform.position);
				Gradient gradient = new Gradient();
				gradient.SetKeys(new GradientColorKey[2]
				{
					new GradientColorKey(Color.red, 0f),
					new GradientColorKey(Color.red, 1f)
				}, new GradientAlphaKey[2]
				{
					new GradientAlphaKey(1f, 0f),
					new GradientAlphaKey(1f, 1f)
				});
				component2.colorGradient = gradient;
				if (power > 2f)
				{
					AudioSource[] array2 = array;
					foreach (AudioSource obj in array2)
					{
						obj.pitch = 1f + (power - 2f) / 5f;
						obj.Play();
					}
				}
			}
		}
		if (!flag)
		{
			Invoke("ShootAtPlayer", 0.5f);
			if (TryGetComponent<SphereCollider>(out var component3))
			{
				component3.radius = 20f;
			}
			if ((bool)enemyFlash)
			{
				Object.Instantiate(enemyFlash, base.transform.position, Quaternion.identity).transform.SetParent(base.transform, worldPositionStays: true);
			}
		}
		else
		{
			shotByEnemy = false;
			base.gameObject.SetActive(value: false);
			new GameObject().AddComponent<CoinCollector>().coin = base.gameObject;
			CancelInvoke("GetDeleted");
			GetComponent<SphereCollider>().enabled = false;
		}
	}

	private void ShootAtPlayer()
	{
		GetComponent<SphereCollider>().enabled = false;
		Vector3 zero = Vector3.zero;
		Vector3 defaultPos = MonoSingleton<CameraController>.Instance.GetDefaultPos();
		if (difficulty < 0)
		{
			difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		}
		if (difficulty <= 2)
		{
			Vector3 normalized = MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().normalized;
			defaultPos -= normalized * ((float)(3 - difficulty) / 1.5f);
		}
		zero = ((!Physics.Raycast(base.transform.position, defaultPos - base.transform.position, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment))) ? (base.transform.position + (defaultPos - base.transform.position) * 999f) : hitInfo.point);
		if (MonoSingleton<NewMovement>.Instance.gameObject.layer != 15 && Physics.Raycast(base.transform.position, defaultPos - base.transform.position, hitInfo.distance, 4))
		{
			MonoSingleton<NewMovement>.Instance.GetHurt(Mathf.RoundToInt(7.5f * power), invincible: true);
		}
		LineRenderer component = SpawnBeam().GetComponent<LineRenderer>();
		AudioSource[] components = component.GetComponents<AudioSource>();
		if (hitPoint == Vector3.zero)
		{
			component.SetPosition(0, base.transform.position);
		}
		else
		{
			component.SetPosition(0, hitPoint);
		}
		component.SetPosition(1, zero);
		Gradient gradient = new Gradient();
		gradient.SetKeys(new GradientColorKey[2]
		{
			new GradientColorKey(Color.red, 0f),
			new GradientColorKey(Color.red, 1f)
		}, new GradientAlphaKey[2]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 1f)
		});
		component.colorGradient = gradient;
		component.widthMultiplier *= 2f;
		if (power > 2f)
		{
			AudioSource[] array = components;
			foreach (AudioSource obj in array)
			{
				obj.pitch = 1f + (power - 2f) / 5f;
				obj.Play();
			}
		}
		base.gameObject.SetActive(value: false);
		new GameObject().AddComponent<CoinCollector>().coin = base.gameObject;
		CancelInvoke("GetDeleted");
		shotByEnemy = false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.layer == 8 || collision.gameObject.layer == 24)
		{
			GoreZone componentInParent = collision.transform.GetComponentInParent<GoreZone>();
			if (componentInParent != null)
			{
				base.transform.SetParent(componentInParent.gibZone, worldPositionStays: true);
			}
			GetDeleted();
		}
	}

	public void GetDeleted()
	{
		if (base.gameObject.activeInHierarchy)
		{
			Object.Instantiate(coinBreak, base.transform.position, Quaternion.identity);
		}
		GetComponent<MeshRenderer>().material = uselessMaterial;
		AudioLowPassFilter[] componentsInChildren = GetComponentsInChildren<AudioLowPassFilter>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			AudioLowPassFilter[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				Object.Destroy(array[i]);
			}
		}
		Object.Destroy(GetComponent<AudioSource>());
		Object.Destroy(base.transform.GetChild(0).GetComponent<AudioSource>());
		Object.Destroy(GetComponent<TrailRenderer>());
		Object.Destroy(GetComponent<SphereCollider>());
		base.gameObject.AddComponent<RemoveOnTime>().time = 5f;
		if ((bool)currentCharge)
		{
			Object.Destroy(currentCharge);
		}
		Object.Destroy(this);
	}

	private void StartCheckingSpeed()
	{
		Collider[] array = cols;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
		checkingSpeed = true;
	}

	private GameObject SpawnBeam()
	{
		GameObject obj = Object.Instantiate(refBeam, base.transform.position, Quaternion.identity);
		obj.GetComponent<RevolverBeam>().sourceWeapon = sourceWeapon;
		return obj;
	}

	public void RicoshotPointsCheck()
	{
		string text = "";
		int num = 50;
		if (altBeam != null && altBeam.TryGetComponent<RevolverBeam>(out var component) && component.ultraRicocheter)
		{
			text = "<color=orange>ULTRA</color>";
			num += 50;
		}
		if (wasShotByEnemy)
		{
			text += "<color=red>COUNTER</color>";
			num += 50;
		}
		if (ricochets > 1)
		{
			num += ricochets * 15;
		}
		StyleHUD styleHUD = shud;
		int points = num;
		string prefix = text;
		styleHUD.AddPoints(points, "ultrakill.ricoshot", sourceWeapon, eid, ricochets, prefix);
	}
}
