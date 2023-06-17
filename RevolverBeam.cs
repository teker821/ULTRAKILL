using System.Collections.Generic;
using CustomRay;
using Sandbox;
using UnityEngine;

public class RevolverBeam : MonoBehaviour
{
	private const float ForceBulletPropMulti = 0.005f;

	public BeamType beamType;

	public HitterAttribute[] attributes;

	private LineRenderer lr;

	private AudioSource aud;

	private Light muzzleLight;

	public Vector3 alternateStartPoint;

	public GameObject sourceWeapon;

	[HideInInspector]
	public int bodiesPierced;

	private int enemiesPierced;

	private RaycastHit[] allHits;

	[HideInInspector]
	public List<RaycastResult> hitList = new List<RaycastResult>();

	private GunControl gc;

	private RaycastHit hit;

	private Vector3 shotHitPoint;

	public CameraController cc;

	private bool maliciousIgnorePlayer;

	public GameObject hitParticle;

	public int bulletForce;

	public bool quickDraw;

	public int gunVariation;

	public float damage;

	public float critDamageOverride;

	public float screenshakeMultiplier = 1f;

	public int hitAmount;

	public int maxHitsPerTarget;

	private int currentHits;

	public bool noMuzzleflash;

	private bool fadeOut;

	private bool didntHit;

	private LayerMask ignoreEnemyTrigger;

	private LayerMask enemyLayerMask;

	private LayerMask pierceLayerMask;

	public int ricochetAmount;

	[HideInInspector]
	public bool hasBeenRicocheter;

	public GameObject ricochetSound;

	public GameObject enemyHitSound;

	public bool fake;

	public EnemyType ignoreEnemyType;

	public bool deflected;

	private bool chargeBacked;

	public bool strongAlt;

	public bool ultraRicocheter = true;

	public bool canHitProjectiles;

	private bool hasHitProjectile;

	[HideInInspector]
	public List<EnemyIdentifier> hitEids = new List<EnemyIdentifier>();

	[HideInInspector]
	public Transform previouslyHitTransform;

	[HideInInspector]
	public bool aimAssist;

	[HideInInspector]
	public bool intentionalRicochet;

	private void Start()
	{
		if (aimAssist)
		{
			RicochetAimAssist(base.gameObject, intentionalRicochet);
		}
		if (ricochetAmount > 0)
		{
			hasBeenRicocheter = true;
		}
		muzzleLight = GetComponent<Light>();
		lr = GetComponent<LineRenderer>();
		cc = MonoSingleton<CameraController>.Instance;
		gc = cc.GetComponentInChildren<GunControl>();
		if (beamType == BeamType.Enemy)
		{
			enemyLayerMask = (int)enemyLayerMask | 4;
		}
		enemyLayerMask = (int)enemyLayerMask | 0x400;
		enemyLayerMask = (int)enemyLayerMask | 0x800;
		if (canHitProjectiles)
		{
			enemyLayerMask = (int)enemyLayerMask | 0x4000;
		}
		pierceLayerMask = (int)pierceLayerMask | 0x100;
		pierceLayerMask = (int)pierceLayerMask | 0x1000000;
		pierceLayerMask = (int)pierceLayerMask | 0x4000000;
		ignoreEnemyTrigger = (int)enemyLayerMask | (int)pierceLayerMask;
		if (!fake)
		{
			Shoot();
		}
		else
		{
			fadeOut = true;
		}
		if (maxHitsPerTarget == 0)
		{
			maxHitsPerTarget = 99;
		}
	}

	private void Update()
	{
		if (fadeOut)
		{
			lr.widthMultiplier -= Time.deltaTime * 1.5f;
			if (muzzleLight != null)
			{
				muzzleLight.intensity -= Time.deltaTime * 100f;
			}
			if (lr.widthMultiplier <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	public void FakeShoot(Vector3 target)
	{
		Vector3 position = base.transform.position;
		if (alternateStartPoint != Vector3.zero)
		{
			position = alternateStartPoint;
		}
		lr.SetPosition(0, position);
		lr.SetPosition(1, target);
		Transform child = base.transform.GetChild(0);
		if (!noMuzzleflash)
		{
			child.position = position;
			child.rotation = base.transform.rotation;
		}
		else
		{
			child.gameObject.SetActive(value: false);
		}
	}

	private void Shoot()
	{
		if (hitAmount == 1)
		{
			fadeOut = true;
			if (beamType == BeamType.Railgun)
			{
				cc.CameraShake(2f * screenshakeMultiplier);
			}
			else if (strongAlt)
			{
				cc.CameraShake(0.25f * screenshakeMultiplier);
			}
			bool flag = Physics.Raycast(base.transform.position, base.transform.forward, out hit, float.PositiveInfinity, ignoreEnemyTrigger);
			bool flag2 = false;
			RaycastHit hitInfo = default(RaycastHit);
			if (flag && (hit.transform.gameObject.layer == 8 || hit.transform.gameObject.layer == 24))
			{
				flag2 = Physics.SphereCast(base.transform.position, 0.4f, base.transform.forward, out hitInfo, Vector3.Distance(base.transform.position, hit.point), enemyLayerMask);
			}
			if (flag2)
			{
				HitSomething(hitInfo);
			}
			else if (flag)
			{
				HitSomething(hit);
			}
			else
			{
				shotHitPoint = base.transform.position + base.transform.forward * 1000f;
			}
		}
		else
		{
			if (Physics.Raycast(base.transform.position, base.transform.forward, out hit, float.PositiveInfinity, pierceLayerMask))
			{
				shotHitPoint = hit.point;
			}
			else
			{
				shotHitPoint = base.transform.position + base.transform.forward * 999f;
				didntHit = true;
			}
			float radius = 0.6f;
			if (beamType == BeamType.Railgun)
			{
				radius = 1.2f;
			}
			else if (beamType == BeamType.Enemy)
			{
				radius = 0.3f;
			}
			allHits = Physics.SphereCastAll(base.transform.position, radius, base.transform.forward, Vector3.Distance(base.transform.position, shotHitPoint), enemyLayerMask, QueryTriggerInteraction.Collide);
		}
		Vector3 position = base.transform.position;
		if (alternateStartPoint != Vector3.zero)
		{
			position = alternateStartPoint;
		}
		lr.SetPosition(0, position);
		lr.SetPosition(1, shotHitPoint);
		if (hitAmount != 1)
		{
			PiercingShotOrder();
		}
		Transform child = base.transform.GetChild(0);
		if (!noMuzzleflash)
		{
			child.position = position;
			child.rotation = base.transform.rotation;
		}
		else
		{
			child.gameObject.SetActive(value: false);
		}
	}

	private void HitSomething(RaycastHit hit)
	{
		bool flag = false;
		if (hit.transform.gameObject.layer == 8 || hit.transform.gameObject.layer == 24)
		{
			ExecuteHits(hit);
		}
		else if (beamType != 0 && hit.transform.gameObject.tag == "Coin")
		{
			flag = true;
			lr.SetPosition(1, hit.transform.position);
			GameObject gameObject = Object.Instantiate(base.gameObject, hit.point, base.transform.rotation);
			gameObject.SetActive(value: false);
			RevolverBeam component = gameObject.GetComponent<RevolverBeam>();
			component.bodiesPierced = 0;
			component.noMuzzleflash = true;
			component.alternateStartPoint = Vector3.zero;
			if (beamType == BeamType.MaliciousFace || beamType == BeamType.Enemy)
			{
				component.deflected = true;
			}
			Coin component2 = hit.transform.gameObject.GetComponent<Coin>();
			if (component2 != null)
			{
				if (component.deflected)
				{
					component2.ignoreBlessedEnemies = true;
				}
				sourceWeapon = component2.sourceWeapon ?? sourceWeapon;
				component2.DelayedReflectRevolver(hit.point, gameObject);
			}
			fadeOut = true;
		}
		else
		{
			ExecuteHits(hit);
		}
		shotHitPoint = hit.point;
		if (!(hit.transform.gameObject.tag != "Armor") || flag)
		{
			return;
		}
		GameObject gameObject2 = Object.Instantiate(hitParticle, shotHitPoint, base.transform.rotation);
		gameObject2.transform.forward = hit.normal;
		if (beamType == BeamType.Railgun)
		{
			Explosion[] componentsInChildren = gameObject2.GetComponentsInChildren<Explosion>();
			foreach (Explosion explosion in componentsInChildren)
			{
				explosion.sourceWeapon = sourceWeapon ?? explosion.sourceWeapon;
			}
		}
		if (beamType != BeamType.MaliciousFace && (beamType != BeamType.Railgun || !maliciousIgnorePlayer))
		{
			return;
		}
		Explosion[] componentsInChildren2 = gameObject2.GetComponentsInChildren<Explosion>();
		if (componentsInChildren2.Length == 0)
		{
			return;
		}
		int @int = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		if (beamType == BeamType.MaliciousFace)
		{
			Explosion[] componentsInChildren = componentsInChildren2;
			foreach (Explosion explosion2 in componentsInChildren)
			{
				if (deflected || maliciousIgnorePlayer)
				{
					explosion2.unblockable = true;
					explosion2.canHit = AffectedSubjects.EnemiesOnly;
				}
				else
				{
					explosion2.enemy = true;
				}
				if (@int < 2)
				{
					explosion2.maxSize *= 0.65f;
					explosion2.speed *= 0.65f;
				}
			}
		}
		else
		{
			Explosion[] componentsInChildren = componentsInChildren2;
			foreach (Explosion explosion3 in componentsInChildren)
			{
				explosion3.sourceWeapon = sourceWeapon ?? explosion3.sourceWeapon;
				explosion3.canHit = AffectedSubjects.EnemiesOnly;
			}
		}
	}

	private void PiercingShotOrder()
	{
		hitList.Clear();
		RaycastHit[] array = allHits;
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			if (raycastHit.transform != previouslyHitTransform)
			{
				hitList.Add(new RaycastResult(raycastHit));
			}
		}
		bool flag = true;
		if (!didntHit && (hit.transform.gameObject.layer == 8 || hit.transform.gameObject.layer == 24))
		{
			if (hit.transform.gameObject.TryGetComponent<SandboxProp>(out var _))
			{
				hit.rigidbody.AddForceAtPosition(base.transform.forward * bulletForce * 0.005f, hit.point, ForceMode.VelocityChange);
			}
			if (hit.transform.GetComponent<Breakable>() != null || hit.transform.gameObject.TryGetComponent<Bleeder>(out var _))
			{
				flag = true;
			}
			else if ((bool)hit.transform.GetComponent<AttributeChecker>())
			{
				flag = true;
			}
		}
		if (!didntHit && (flag || hit.transform.gameObject.tag == "Glass" || hit.transform.gameObject.tag == "GlassFloor" || hit.transform.gameObject.tag == "Armor"))
		{
			hitList.Add(new RaycastResult(hit));
		}
		hitList.Sort();
		PiercingShotCheck();
	}

	private void PiercingShotCheck()
	{
		if (enemiesPierced < hitList.Count)
		{
			if (hitList[enemiesPierced].transform == null)
			{
				enemiesPierced++;
				PiercingShotCheck();
				return;
			}
			if (hitList[enemiesPierced].transform.gameObject.tag == "Armor" || (ricochetAmount > 0 && (hitList[enemiesPierced].transform.gameObject.layer == 8 || hitList[enemiesPierced].transform.gameObject.layer == 24 || hitList[enemiesPierced].transform.gameObject.layer == 0)))
			{
				bool flag = hitList[enemiesPierced].transform.gameObject.tag != "Armor";
				GameObject gameObject = Object.Instantiate(base.gameObject, hitList[enemiesPierced].rrhit.point, base.transform.rotation);
				gameObject.transform.forward = Vector3.Reflect(base.transform.forward, hitList[enemiesPierced].rrhit.normal);
				lr.SetPosition(1, hitList[enemiesPierced].rrhit.point);
				RevolverBeam component = gameObject.GetComponent<RevolverBeam>();
				component.noMuzzleflash = true;
				component.alternateStartPoint = Vector3.zero;
				component.bodiesPierced = bodiesPierced;
				component.previouslyHitTransform = hitList[enemiesPierced].transform;
				component.aimAssist = true;
				component.intentionalRicochet = flag;
				if (flag)
				{
					ricochetAmount--;
					if (beamType != 0 || component.maxHitsPerTarget < 3 || (strongAlt && component.maxHitsPerTarget < 4))
					{
						component.maxHitsPerTarget++;
					}
					component.hitEids.Clear();
				}
				component.ricochetAmount = ricochetAmount;
				GameObject gameObject2 = Object.Instantiate(ricochetSound, hitList[enemiesPierced].rrhit.point, Quaternion.identity);
				gameObject2.SetActive(value: false);
				gameObject.SetActive(value: false);
				MonoSingleton<DelayedActivationManager>.Instance.Add(gameObject, 0.1f);
				MonoSingleton<DelayedActivationManager>.Instance.Add(gameObject2, 0.1f);
				if (hitList[enemiesPierced].transform.gameObject.TryGetComponent<Glass>(out var component2) && !component2.broken)
				{
					component2.Shatter();
				}
				if (hitList[enemiesPierced].transform.gameObject.TryGetComponent<Breakable>(out var component3) && (strongAlt || component3.weak || beamType == BeamType.Railgun))
				{
					component3.Break();
				}
				fadeOut = true;
				enemiesPierced = hitList.Count;
				return;
			}
			if (hitList[enemiesPierced].transform.gameObject.tag == "Coin" && bodiesPierced < hitAmount)
			{
				Coin component4 = hitList[enemiesPierced].transform.gameObject.GetComponent<Coin>();
				if (component4 == null)
				{
					enemiesPierced++;
					PiercingShotCheck();
					return;
				}
				lr.SetPosition(1, hitList[enemiesPierced].transform.position);
				GameObject gameObject3 = Object.Instantiate(base.gameObject, hitList[enemiesPierced].rrhit.point, base.transform.rotation);
				gameObject3.SetActive(value: false);
				RevolverBeam component5 = gameObject3.GetComponent<RevolverBeam>();
				component5.bodiesPierced = 0;
				component5.noMuzzleflash = true;
				component5.alternateStartPoint = Vector3.zero;
				component5.hitEids.Clear();
				if (beamType == BeamType.Enemy)
				{
					component4.ignoreBlessedEnemies = true;
					component5.deflected = true;
				}
				component4.DelayedReflectRevolver(hitList[enemiesPierced].rrhit.point, gameObject3);
				fadeOut = true;
				return;
			}
			if ((hitList[enemiesPierced].transform.gameObject.layer == 10 || hitList[enemiesPierced].transform.gameObject.layer == 11) && hitList[enemiesPierced].transform.gameObject.tag != "Breakable" && bodiesPierced < hitAmount)
			{
				EnemyIdentifierIdentifier componentInParent = hitList[enemiesPierced].transform.gameObject.GetComponentInParent<EnemyIdentifierIdentifier>();
				if (!componentInParent)
				{
					if (attributes.Length != 0)
					{
						AttributeChecker component6 = hitList[enemiesPierced].transform.GetComponent<AttributeChecker>();
						if ((bool)component6)
						{
							HitterAttribute[] array = attributes;
							for (int i = 0; i < array.Length; i++)
							{
								if (array[i] == component6.targetAttribute)
								{
									component6.DelayedActivate();
									break;
								}
							}
						}
					}
					enemiesPierced++;
					currentHits = 0;
					PiercingShotCheck();
					return;
				}
				EnemyIdentifier eid = componentInParent.eid;
				if (eid != null)
				{
					if ((!hitEids.Contains(eid) || (eid.dead && beamType == BeamType.Revolver && enemiesPierced == hitList.Count - 1)) && (beamType != BeamType.Enemy || deflected || (eid.enemyType != ignoreEnemyType && !EnemyIdentifier.CheckHurtException(eid.enemyType, ignoreEnemyType))))
					{
						bool flag2 = false;
						if (eid.dead)
						{
							flag2 = true;
						}
						ExecuteHits(hitList[enemiesPierced].rrhit);
						if (!flag2 || hitList[enemiesPierced].transform.gameObject.layer == 11 || (beamType == BeamType.Revolver && enemiesPierced == hitList.Count - 1))
						{
							currentHits++;
							bodiesPierced++;
							Object.Instantiate(hitParticle, hitList[enemiesPierced].rrhit.point, base.transform.rotation);
							MonoSingleton<TimeController>.Instance.HitStop(0.05f);
						}
						else
						{
							if (beamType == BeamType.Revolver)
							{
								hitEids.Add(eid);
							}
							enemiesPierced++;
							currentHits = 0;
						}
						if (currentHits >= maxHitsPerTarget)
						{
							hitEids.Add(eid);
							currentHits = 0;
							enemiesPierced++;
						}
						if (beamType == BeamType.Revolver && !flag2)
						{
							Invoke("PiercingShotCheck", 0.05f);
						}
						else if (beamType == BeamType.Revolver)
						{
							PiercingShotCheck();
						}
						else if (!flag2)
						{
							Invoke("PiercingShotCheck", 0.025f);
						}
						else
						{
							Invoke("PiercingShotCheck", 0.01f);
						}
					}
					else
					{
						enemiesPierced++;
						currentHits = 0;
						PiercingShotCheck();
					}
				}
				else
				{
					ExecuteHits(hitList[enemiesPierced].rrhit);
					enemiesPierced++;
					PiercingShotCheck();
				}
				return;
			}
			if (canHitProjectiles && hitList[enemiesPierced].transform.gameObject.layer == 14)
			{
				if (!hasHitProjectile)
				{
					Invoke("PiercingShotCheck", 0.01f);
				}
				else
				{
					MonoSingleton<TimeController>.Instance.HitStop(0.05f);
					Invoke("PiercingShotCheck", 0.05f);
				}
				ExecuteHits(hitList[enemiesPierced].rrhit);
				enemiesPierced++;
				return;
			}
			if (hitList[enemiesPierced].transform.gameObject.tag == "Glass" || hitList[enemiesPierced].transform.gameObject.tag == "GlassFloor")
			{
				Glass component7 = hitList[enemiesPierced].transform.gameObject.GetComponent<Glass>();
				if (!component7.broken)
				{
					component7.Shatter();
				}
				enemiesPierced++;
				PiercingShotCheck();
				return;
			}
			if (beamType == BeamType.Enemy && hitList[enemiesPierced].transform.gameObject.CompareTag("Player") && bodiesPierced < hitAmount)
			{
				ExecuteHits(hitList[enemiesPierced].rrhit);
				bodiesPierced++;
				enemiesPierced++;
				PiercingShotCheck();
				return;
			}
			Breakable component8 = hitList[enemiesPierced].transform.GetComponent<Breakable>();
			if (component8 != null && (beamType == BeamType.Railgun || component8.weak))
			{
				if (component8.interrupt)
				{
					MonoSingleton<StyleHUD>.Instance.AddPoints(100, "ultrakill.interruption", sourceWeapon);
					MonoSingleton<TimeController>.Instance.ParryFlash();
					if (canHitProjectiles)
					{
						component8.breakParticle = MonoSingleton<DefaultReferenceManager>.Instance.superExplosion;
					}
					if ((bool)component8.interruptEnemy && !component8.interruptEnemy.blessed)
					{
						component8.interruptEnemy.Explode();
					}
				}
				component8.Break();
			}
			else if (bodiesPierced < hitAmount)
			{
				ExecuteHits(hitList[enemiesPierced].rrhit);
			}
			Object.Instantiate(hitParticle, hitList[enemiesPierced].rrhit.point, Quaternion.LookRotation(hitList[enemiesPierced].rrhit.normal));
			enemiesPierced++;
			PiercingShotCheck();
		}
		else
		{
			enemiesPierced = 0;
			fadeOut = true;
		}
	}

	public void ExecuteHits(RaycastHit currentHit)
	{
		if (!(currentHit.transform != null))
		{
			return;
		}
		Breakable component = currentHit.transform.GetComponent<Breakable>();
		if (component != null && (strongAlt || beamType == BeamType.Railgun || component.weak))
		{
			if (component.interrupt)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(100, "ultrakill.interruption", sourceWeapon);
				MonoSingleton<TimeController>.Instance.ParryFlash();
				if (canHitProjectiles)
				{
					component.breakParticle = MonoSingleton<DefaultReferenceManager>.Instance.superExplosion;
				}
				if ((bool)component.interruptEnemy && !component.interruptEnemy.blessed)
				{
					component.interruptEnemy.Explode();
				}
			}
			component.Break();
		}
		if (canHitProjectiles && currentHit.transform.gameObject.layer == 14 && currentHit.transform.gameObject.TryGetComponent<Projectile>(out var component2) && (component2.speed != 0f || component2.decorative))
		{
			Object.Instantiate((!hasHitProjectile) ? MonoSingleton<DefaultReferenceManager>.Instance.superExplosion : component2.explosionEffect, component2.transform.position, Quaternion.identity);
			Object.Destroy(component2.gameObject);
			if (!hasHitProjectile)
			{
				MonoSingleton<TimeController>.Instance.ParryFlash();
			}
			hasHitProjectile = true;
		}
		if (currentHit.transform.gameObject.TryGetComponent<Bleeder>(out var component3))
		{
			if (beamType == BeamType.Railgun || strongAlt)
			{
				component3.GetHit(currentHit.point, GoreType.Head);
			}
			else
			{
				component3.GetHit(currentHit.point, GoreType.Body);
			}
		}
		if (currentHit.transform.gameObject.TryGetComponent<SandboxProp>(out var _))
		{
			currentHit.rigidbody.AddForceAtPosition(base.transform.forward * bulletForce * 0.005f, hit.point, ForceMode.VelocityChange);
		}
		Coin component5 = currentHit.transform.GetComponent<Coin>();
		if (component5 != null && beamType == BeamType.Revolver)
		{
			if (quickDraw)
			{
				component5.quickDraw = true;
			}
			component5.DelayedReflectRevolver(currentHit.point);
		}
		if (currentHit.transform.gameObject.tag == "Enemy" || currentHit.transform.gameObject.tag == "Body" || currentHit.transform.gameObject.tag == "Limb" || currentHit.transform.gameObject.tag == "EndLimb" || currentHit.transform.gameObject.tag == "Head")
		{
			EnemyIdentifier eid = currentHit.transform.GetComponentInParent<EnemyIdentifierIdentifier>().eid;
			if ((bool)eid && !deflected && (beamType == BeamType.MaliciousFace || beamType == BeamType.Enemy) && (eid.enemyType == ignoreEnemyType || EnemyIdentifier.CheckHurtException(ignoreEnemyType, eid.enemyType)))
			{
				enemiesPierced++;
				return;
			}
			if (hitAmount > 1)
			{
				cc.CameraShake(1f * screenshakeMultiplier);
			}
			else
			{
				cc.CameraShake(0.5f * screenshakeMultiplier);
			}
			if ((bool)eid && !eid.dead && quickDraw && !eid.blessed)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(50, "ultrakill.quickdraw", sourceWeapon, eid);
				quickDraw = false;
			}
			string text = "";
			if (beamType == BeamType.Revolver)
			{
				text = "revolver";
			}
			else if (beamType == BeamType.Railgun)
			{
				text = "railcannon";
			}
			else if (beamType == BeamType.MaliciousFace || beamType == BeamType.Enemy)
			{
				text = "enemy";
			}
			if ((bool)eid)
			{
				eid.hitter = text;
				if (attributes != null && attributes.Length != 0)
				{
					HitterAttribute[] array = attributes;
					foreach (HitterAttribute item in array)
					{
						eid.hitterAttributes.Add(item);
					}
				}
				if (!eid.hitterWeapons.Contains(text + gunVariation))
				{
					eid.hitterWeapons.Add(text + gunVariation);
				}
			}
			float critMultiplier = 1f;
			if (beamType != 0)
			{
				critMultiplier = 0f;
			}
			if (critDamageOverride != 0f || strongAlt)
			{
				critMultiplier = critDamageOverride;
			}
			float num = damage;
			if ((bool)eid && deflected)
			{
				if (beamType == BeamType.MaliciousFace && eid.enemyType == EnemyType.MaliciousFace)
				{
					num = 999f;
				}
				else if (beamType == BeamType.Enemy)
				{
					num *= 2.5f;
				}
				if (!chargeBacked)
				{
					chargeBacked = true;
					if (!eid.blessed)
					{
						MonoSingleton<StyleHUD>.Instance.AddPoints(400, "ultrakill.chargeback", sourceWeapon, eid);
					}
				}
			}
			bool tryForExplode = false;
			if (strongAlt)
			{
				tryForExplode = true;
			}
			if ((bool)eid)
			{
				eid.DeliverDamage(currentHit.transform.gameObject, (currentHit.transform.position - base.transform.position).normalized * bulletForce, currentHit.point, num, tryForExplode, critMultiplier, sourceWeapon);
			}
			if (beamType != BeamType.MaliciousFace && beamType != BeamType.Enemy)
			{
				if ((bool)eid && !eid.dead && currentHit.transform.gameObject.tag == "Head" && beamType == BeamType.Revolver && !eid.blessed)
				{
					gc.headshots++;
					gc.headShotComboTime = 3f;
				}
				else if (currentHit.transform.gameObject.tag != "Head" || beamType == BeamType.Railgun)
				{
					gc.headshots = 0;
					gc.headShotComboTime = 0f;
				}
				if (gc.headshots > 1 && (bool)eid && !eid.blessed)
				{
					MonoSingleton<StyleHUD>.Instance.AddPoints(gc.headshots * 20, "ultrakill.headshotcombo", count: gc.headshots, sourceWeapon: sourceWeapon, eid: eid);
				}
			}
			Object.Instantiate(enemyHitSound, currentHit.point, Quaternion.identity);
		}
		else if (currentHit.transform.gameObject.layer == 10)
		{
			Grenade componentInParent = currentHit.transform.GetComponentInParent<Grenade>();
			if (componentInParent != null)
			{
				MonoSingleton<TimeController>.Instance.ParryFlash();
				if ((beamType == BeamType.Railgun && hitAmount == 1) || beamType == BeamType.MaliciousFace)
				{
					maliciousIgnorePlayer = true;
					componentInParent.Explode(componentInParent.rocket, harmless: false, !componentInParent.rocket, 2f, ultrabooster: true, sourceWeapon);
				}
				else
				{
					componentInParent.Explode(componentInParent.rocket, harmless: false, !componentInParent.rocket, 1f, ultrabooster: false, sourceWeapon);
				}
			}
			else
			{
				Cannonball componentInParent2 = currentHit.transform.GetComponentInParent<Cannonball>();
				if ((bool)componentInParent2)
				{
					MonoSingleton<TimeController>.Instance.ParryFlash();
					componentInParent2.Explode();
				}
			}
		}
		else if (beamType == BeamType.Enemy && currentHit.transform.gameObject.CompareTag("Player"))
		{
			Object.Instantiate(enemyHitSound, currentHit.point, Quaternion.identity);
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
			{
				MonoSingleton<NewMovement>.Instance.GetHurt(Mathf.RoundToInt(damage * 10f), invincible: true);
			}
			else
			{
				MonoSingleton<PlatformerMovement>.Instance.Explode();
			}
		}
		else
		{
			if ((bool)gc)
			{
				gc.headshots = 0;
				gc.headShotComboTime = 0f;
			}
			if (currentHit.transform.gameObject.tag == "Armor")
			{
				GameObject gameObject = Object.Instantiate(base.gameObject, currentHit.point, base.transform.rotation);
				gameObject.transform.forward = Vector3.Reflect(base.transform.forward, currentHit.normal);
				RevolverBeam component6 = gameObject.GetComponent<RevolverBeam>();
				component6.noMuzzleflash = true;
				component6.alternateStartPoint = Vector3.zero;
				component6.aimAssist = true;
				GameObject gameObject2 = Object.Instantiate(ricochetSound, currentHit.point, Quaternion.identity);
				gameObject2.SetActive(value: false);
				gameObject.SetActive(value: false);
				MonoSingleton<DelayedActivationManager>.Instance.Add(gameObject, 0.1f);
				MonoSingleton<DelayedActivationManager>.Instance.Add(gameObject2, 0.1f);
			}
		}
	}

	private void RicochetAimAssist(GameObject beam, bool aimAtHead = false)
	{
		RaycastHit[] array = Physics.SphereCastAll(beam.transform.position, 5f, beam.transform.forward, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Enemies));
		if (array == null || array.Length == 0)
		{
			return;
		}
		Vector3 worldPosition = beam.transform.forward * 1000f;
		float num = float.PositiveInfinity;
		GameObject gameObject = null;
		bool flag = false;
		for (int i = 0; i < array.Length; i++)
		{
			Coin component;
			bool flag2 = MonoSingleton<CoinList>.Instance.revolverCoinsList.Count > 0 && array[i].transform.TryGetComponent<Coin>(out component) && (!component.shot || component.shotByEnemy);
			if ((!flag || flag2) && (!(array[i].distance > num) || (!flag && flag2)) && (!(array[i].distance < 0.1f) || flag2) && !Physics.Raycast(beam.transform.position, array[i].point - beam.transform.position, array[i].distance, LayerMaskDefaults.Get(LMD.Environment)) && (flag2 || (array[i].transform.TryGetComponent<EnemyIdentifierIdentifier>(out var component2) && (bool)component2.eid && !component2.eid.dead)))
			{
				if (flag2)
				{
					flag = true;
				}
				worldPosition = (flag2 ? array[i].transform.position : array[i].point);
				num = array[i].distance;
				gameObject = array[i].transform.gameObject;
			}
		}
		if ((bool)gameObject)
		{
			if (aimAtHead && !flag && (critDamageOverride != 0f || (beamType == BeamType.Revolver && !strongAlt)) && gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out var component3) && (bool)component3.eid && (bool)component3.eid.weakPoint && !Physics.Raycast(beam.transform.position, component3.eid.weakPoint.transform.position - beam.transform.position, Vector3.Distance(component3.eid.weakPoint.transform.position, beam.transform.position), LayerMaskDefaults.Get(LMD.Environment)))
			{
				worldPosition = component3.eid.weakPoint.transform.position;
			}
			beam.transform.LookAt(worldPosition);
		}
	}
}
