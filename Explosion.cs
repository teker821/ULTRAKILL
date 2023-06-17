using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	public static float globalSizeMulti = 1f;

	public GameObject sourceWeapon;

	public bool enemy;

	public bool harmless;

	public bool lowQuality;

	private CameraController cc;

	private Light light;

	private MeshRenderer mr;

	private Color materialColor;

	private bool fading;

	public float speed;

	public float maxSize;

	private LayerMask lmask;

	public int damage;

	public float enemyDamageMultiplier;

	[HideInInspector]
	public int playerDamageOverride = -1;

	public GameObject explosionChunk;

	public bool ignite;

	public bool friendlyFire;

	private List<Collider> hitColliders = new List<Collider>();

	public string hitterWeapon;

	public bool halved;

	private SphereCollider scol;

	public AffectedSubjects canHit;

	private bool hasHitPlayer;

	public bool rocketExplosion;

	public List<EnemyType> toIgnore;

	[HideInInspector]
	public EnemyIdentifier interruptedEnemy;

	private RaycastHit neoRhit;

	[HideInInspector]
	public bool ultrabooster;

	public bool unblockable;

	public bool electric;

	private void Start()
	{
		cc = MonoSingleton<CameraController>.Instance;
		float num = Vector3.Distance(base.transform.position, cc.transform.position);
		float num2 = 1f;
		if (damage == 0)
		{
			num2 = 0.25f;
		}
		if (num < 3f * maxSize)
		{
			cc.CameraShake(1.5f * num2);
		}
		else if (num < 85f)
		{
			cc.CameraShake((1.5f - (num - 20f) / 65f * 1.5f) / 6f * maxSize * num2);
		}
		scol = GetComponent<SphereCollider>();
		if ((bool)scol)
		{
			scol.enabled = true;
		}
		if (speed == 0f)
		{
			speed = 1f;
		}
		if (!lowQuality && MonoSingleton<PrefsManager>.Instance.GetBoolLocal("simpleExplosions"))
		{
			lowQuality = true;
		}
		if ((bool)MonoSingleton<ComponentsDatabase>.Instance && MonoSingleton<ComponentsDatabase>.Instance.scrollers.Count > 0)
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, 1f, LayerMaskDefaults.Get(LMD.Environment));
			if (array.Length != 0)
			{
				Collider[] array2 = array;
				foreach (Collider collider in array2)
				{
					if (MonoSingleton<ComponentsDatabase>.Instance.scrollers.Contains(collider.transform) && collider.transform.TryGetComponent<ScrollingTexture>(out var component))
					{
						component.attachedObjects.Add(base.transform);
					}
				}
			}
		}
		if (!lowQuality)
		{
			light = GetComponentInChildren<Light>();
			light.enabled = true;
			if (explosionChunk != null)
			{
				for (int j = 0; j < Random.Range(24, 30); j++)
				{
					GameObject obj = Object.Instantiate(explosionChunk, base.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), Random.rotation);
					Vector3 vector = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 2f), Random.Range(-1f, 1f));
					obj.GetComponent<Rigidbody>().AddForce(vector * 100f, ForceMode.VelocityChange);
					Physics.IgnoreCollision(obj.GetComponent<Collider>(), scol);
				}
			}
		}
		lmask = (int)lmask | 0x100;
		lmask = (int)lmask | 0x800;
		lmask = (int)lmask | 0x1000000;
		lmask = (int)lmask | 0x4000000;
		speed *= globalSizeMulti;
		maxSize *= globalSizeMulti;
	}

	private void FixedUpdate()
	{
		base.transform.localScale += Vector3.one * 0.05f * speed;
		if (light != null)
		{
			light.range += 0.05f * speed;
		}
		if (!fading && base.transform.lossyScale.x * scol.radius > maxSize)
		{
			Fade();
		}
		if (!halved && base.transform.lossyScale.x * scol.radius > maxSize / 2f)
		{
			halved = true;
			damage = Mathf.RoundToInt((float)damage / 1.5f);
		}
		if (fading)
		{
			materialColor.a -= 0.02f;
			if (light != null)
			{
				light.intensity -= 0.65f;
			}
			mr.material.SetColor("_Color", materialColor);
			if (materialColor.a <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Collide(other);
	}

	private void Collide(Collider other)
	{
		if (!harmless && !hitColliders.Contains(other))
		{
			bool flag = false;
			if (Physics.Raycast(base.transform.position + (other.transform.position - base.transform.position).normalized * 0.1f, other.transform.position - base.transform.position, out var hitInfo, Vector3.Distance(base.transform.position + (other.transform.position - base.transform.position).normalized * 0.1f, other.transform.position), lmask, QueryTriggerInteraction.Ignore))
			{
				RaycastHit[] rhits = Physics.RaycastAll(base.transform.position + (other.transform.position - base.transform.position).normalized * 0.1f, other.transform.position - base.transform.position, Vector3.Distance(base.transform.position + (other.transform.position - base.transform.position).normalized * 0.1f, other.transform.position), lmask, QueryTriggerInteraction.Ignore);
				if (CheckRaycasts(other, rhits))
				{
					hitInfo = neoRhit;
				}
				else
				{
					flag = false;
				}
			}
			Breakable component8;
			Glass component9;
			Flammable component10;
			if (!flag || other.isTrigger || hitInfo.transform.gameObject == other.gameObject || other.gameObject.layer == 11 || ((bool)hitInfo.collider.attachedRigidbody && (bool)other.attachedRigidbody && hitInfo.collider.attachedRigidbody == other.attachedRigidbody))
			{
				if (enemy && Physics.Raycast(other.transform.position, base.transform.position - other.transform.position, Vector3.Distance(base.transform.position, other.transform.position) - 0.1f, lmask, QueryTriggerInteraction.Ignore))
				{
					RaycastHit[] rhits2 = Physics.RaycastAll(other.transform.position, base.transform.position - other.transform.position, Vector3.Distance(base.transform.position, other.transform.position) - 0.1f, lmask, QueryTriggerInteraction.Ignore);
					if (CheckRaycasts(other, rhits2))
					{
						return;
					}
				}
				if (flag && other.gameObject.layer == 11)
				{
					Collider[] componentsInChildren = hitInfo.transform.GetComponentsInChildren<Collider>();
					bool flag2 = false;
					Collider[] array = componentsInChildren;
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i].transform == hitInfo.transform)
						{
							flag2 = true;
						}
					}
					if (!flag2)
					{
						return;
					}
				}
				Breakable component4;
				Bleeder component5;
				Glass component6;
				Flammable component7;
				if (other.gameObject.tag == "Player" && !hasHitPlayer)
				{
					hasHitPlayer = true;
					hitColliders.Add(other);
					if (canHit != AffectedSubjects.EnemiesOnly)
					{
						if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer && !harmless && damage > 0)
						{
							MonoSingleton<PlatformerMovement>.Instance.Burn();
							return;
						}
						if (!MonoSingleton<NewMovement>.Instance.exploded)
						{
							int num = 200;
							if (rocketExplosion && damage == 0)
							{
								num = Mathf.RoundToInt(100f / ((float)(MonoSingleton<NewMovement>.Instance.rocketJumps + 3) / 3f));
								MonoSingleton<NewMovement>.Instance.rocketJumps++;
							}
							if (Mathf.Abs(base.transform.position.x - other.transform.position.x) < 0.25f && Mathf.Abs(base.transform.position.z - other.transform.position.z) < 0.25f)
							{
								MonoSingleton<NewMovement>.Instance.LaunchFromPoint(other.transform.position, num, maxSize);
								if (ultrabooster && Vector3.Distance(base.transform.position, other.transform.position) < 12f)
								{
									MonoSingleton<NewMovement>.Instance.LaunchFromPoint(other.transform.position, num, maxSize);
								}
							}
							else
							{
								MonoSingleton<NewMovement>.Instance.LaunchFromPoint(base.transform.position, num, maxSize);
								if (ultrabooster && Vector3.Distance(base.transform.position, other.transform.position) < 12f)
								{
									MonoSingleton<NewMovement>.Instance.LaunchFromPoint(base.transform.position, num, maxSize);
								}
							}
						}
						if (damage > 0)
						{
							int num2 = damage;
							if (ultrabooster)
							{
								num2 = ((!(Vector3.Distance(base.transform.position, other.transform.position) < 3f)) ? 50 : 35);
							}
							else if (playerDamageOverride >= 0)
							{
								num2 = playerDamageOverride;
							}
							if (enemy)
							{
								MonoSingleton<NewMovement>.Instance.GetHurt(num2, invincible: true, 1f, explosion: true);
							}
							else
							{
								MonoSingleton<NewMovement>.Instance.GetHurt(num2, invincible: true, 0f, explosion: true);
							}
						}
					}
				}
				else if ((other.gameObject.layer == 10 || other.gameObject.layer == 11) && canHit != AffectedSubjects.PlayerOnly)
				{
					EnemyIdentifierIdentifier componentInParent = other.GetComponentInParent<EnemyIdentifierIdentifier>();
					if (componentInParent != null && componentInParent.eid != null)
					{
						Collider component = componentInParent.eid.GetComponent<Collider>();
						if (component != null && !hitColliders.Contains(component) && !componentInParent.eid.dead)
						{
							hitColliders.Add(component);
							bool flag3 = Physics.Linecast(base.transform.position, component.bounds.center, LayerMaskDefaults.Get(LMD.Environment));
							if (componentInParent.eid.enemyType == EnemyType.Idol && !flag3)
							{
								componentInParent.eid.hitter = hitterWeapon;
								componentInParent.eid.DeliverDamage(other.gameObject, Vector3.zero, other.transform.position, 1f, tryForExplode: false, 0f, sourceWeapon);
							}
							else if (componentInParent.eid.enemyType != EnemyType.MaliciousFace && (!enemy || (componentInParent.eid.enemyType != EnemyType.HideousMass && componentInParent.eid.enemyType != EnemyType.Sisyphus)) && !toIgnore.Contains(componentInParent.eid.enemyType))
							{
								if (friendlyFire)
								{
									componentInParent.eid.hitter = "ffexplosion";
								}
								else if (enemy)
								{
									componentInParent.eid.hitter = "enemy";
								}
								else
								{
									componentInParent.eid.hitter = "explosion";
								}
								if (!componentInParent.eid.hitterWeapons.Contains(hitterWeapon))
								{
									componentInParent.eid.hitterWeapons.Add(hitterWeapon);
								}
								Vector3 vector = (other.transform.position - base.transform.position).normalized;
								if (componentInParent.eid.enemyType == EnemyType.Drone && damage == 0)
								{
									vector = Vector3.zero;
								}
								else if (vector.y <= 0.5f)
								{
									vector = new Vector3(vector.x, vector.y + 0.5f, vector.z);
								}
								else if (vector.y < 1f)
								{
									vector = new Vector3(vector.x, 1f, vector.z);
								}
								float num3 = (float)damage / 10f * enemyDamageMultiplier;
								if (rocketExplosion && componentInParent.eid.enemyType == EnemyType.Cerberus)
								{
									num3 *= 1.5f;
								}
								if (componentInParent.eid.enemyType != EnemyType.Soldier || unblockable || BlindEnemies.Blind || !componentInParent.eid.TryGetComponent<Zombie>(out var component2) || !component2.grounded || !component2.zp || component2.zp.difficulty < 2)
								{
									if (electric)
									{
										componentInParent.eid.hitterAttributes.Add(HitterAttribute.Electricity);
									}
									componentInParent.eid.DeliverDamage(componentInParent.gameObject, vector * 50000f, other.transform.position, num3, tryForExplode: false, 0f, sourceWeapon);
									if (ignite)
									{
										Flammable componentInChildren = componentInParent.eid.GetComponentInChildren<Flammable>();
										if (componentInChildren != null)
										{
											componentInChildren.Burn(damage / 10);
										}
									}
								}
								else
								{
									componentInParent.eid.hitter = "blocked";
									if (component2.zp.difficulty <= 3 || electric)
									{
										if (electric)
										{
											componentInParent.eid.hitterAttributes.Add(HitterAttribute.Electricity);
										}
										componentInParent.eid.DeliverDamage(other.gameObject, Vector3.zero, other.transform.position, num3 * 0.25f, tryForExplode: false, 0f, sourceWeapon);
									}
									component2.zp.Block(base.transform.position);
								}
							}
							else if (componentInParent.eid.enemyType == EnemyType.MaliciousFace)
							{
								Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.ineffectiveSound, other.transform.position, Quaternion.identity);
							}
						}
						else if (componentInParent.eid.dead)
						{
							hitColliders.Add(other);
							if (enemy)
							{
								componentInParent.eid.hitter = "enemy";
							}
							else
							{
								componentInParent.eid.hitter = "explosion";
							}
							componentInParent.eid.DeliverDamage(other.gameObject, (other.transform.position - base.transform.position).normalized * 5000f, other.transform.position, (float)damage / 10f * enemyDamageMultiplier, tryForExplode: false, 0f, sourceWeapon);
							if (ignite && componentInParent.TryGetComponent<Flammable>(out var _))
							{
								Flammable componentInChildren2 = componentInParent.eid.GetComponentInChildren<Flammable>();
								if (componentInChildren2 != null)
								{
									componentInChildren2.Burn(damage / 10);
								}
							}
						}
					}
				}
				else if (other.TryGetComponent<Breakable>(out component4))
				{
					if (!component4.accurateExplosionsOnly)
					{
						component4.Break();
					}
					else
					{
						Vector3 vector2 = other.ClosestPoint(base.transform.position);
						if (!Physics.Raycast(vector2 + (vector2 - base.transform.position).normalized * 0.001f, base.transform.position - vector2, Vector3.Distance(base.transform.position, vector2), lmask, QueryTriggerInteraction.Ignore))
						{
							component4.Break();
						}
					}
				}
				else if (other.TryGetComponent<Bleeder>(out component5))
				{
					bool flag4 = false;
					if (toIgnore.Count > 0 && component5.ignoreTypes.Length != 0)
					{
						EnemyType[] ignoreTypes = component5.ignoreTypes;
						foreach (EnemyType enemyType in ignoreTypes)
						{
							for (int j = 0; j < toIgnore.Count; j++)
							{
								if (enemyType == toIgnore[j])
								{
									flag4 = true;
									break;
								}
							}
							if (flag4)
							{
								break;
							}
						}
					}
					if (!flag4)
					{
						component5.GetHit(other.transform.position, GoreType.Head);
					}
				}
				else if (other.TryGetComponent<Glass>(out component6))
				{
					component6.Shatter();
				}
				else if (ignite && other.TryGetComponent<Flammable>(out component7) && (!enemy || !component7.playerOnly))
				{
					component7.Burn(4f);
				}
			}
			else if (other.TryGetComponent<Breakable>(out component8) && !component8.accurateExplosionsOnly)
			{
				component8.Break();
			}
			else if (other.TryGetComponent<Glass>(out component9))
			{
				component9.Shatter();
			}
			else if (ignite && other.TryGetComponent<Flammable>(out component10) && (!enemy || !component10.playerOnly))
			{
				component10.Burn(4f);
			}
		}
		if (harmless || (!(other.gameObject.tag != "Player") && MonoSingleton<PlayerTracker>.Instance.playerType != PlayerType.Platformer))
		{
			return;
		}
		Rigidbody component11 = other.GetComponent<Rigidbody>();
		if ((!component11 || other.gameObject.layer != 14 || !other.gameObject.CompareTag("Metal") || !other.TryGetComponent<Nail>(out var component12) || component12.magnets.Count == 0) && (bool)component11 && (other.gameObject.layer != 14 || component11.useGravity) && other.gameObject.tag != "IgnorePushes")
		{
			if (!hitColliders.Contains(other))
			{
				hitColliders.Add(other);
			}
			Vector3 normalized = (other.transform.position - base.transform.position).normalized;
			normalized = new Vector3(normalized.x * Mathf.Max(5f - Vector3.Distance(other.transform.position, base.transform.position), 0f) * 7500f, normalized.y * Mathf.Max(5f - Vector3.Distance(other.transform.position, base.transform.position), 0f), normalized.z * Mathf.Max(5f - Vector3.Distance(other.transform.position, base.transform.position), 0f) * 7500f);
			if (component11.useGravity)
			{
				normalized = new Vector3(normalized.x, 18750f, normalized.z);
			}
			if (other.gameObject.layer == 27 || other.gameObject.layer == 9)
			{
				normalized = Vector3.ClampMagnitude(normalized, 5000f);
			}
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer && other.gameObject == MonoSingleton<PlatformerMovement>.Instance.gameObject)
			{
				normalized *= 30f;
			}
			component11.AddForce(normalized);
		}
		if (other.gameObject.layer != 14)
		{
			return;
		}
		ThrownSword component13 = other.GetComponent<ThrownSword>();
		Projectile component14 = other.GetComponent<Projectile>();
		if (component13 != null)
		{
			component13.deflected = true;
		}
		if (component14 != null && !component14.ignoreExplosions)
		{
			component14.homingType = HomingType.None;
			other.transform.LookAt(other.transform.position + (other.transform.position - base.transform.position));
			component14.friendly = true;
			component14.target = null;
			component14.turnSpeed = 0f;
			if (component14.speed < 65f)
			{
				component14.speed = 65f;
			}
		}
	}

	private void Fade()
	{
		harmless = true;
		mr = GetComponent<MeshRenderer>();
		materialColor = mr.material.GetColor("_Color");
		fading = true;
		speed /= 4f;
	}

	private void BecomeHarmless()
	{
		harmless = true;
	}

	private bool CheckRaycasts(Collider other, RaycastHit[] rhits)
	{
		bool flag = true;
		if (rhits.Length != 0)
		{
			for (int i = 0; i < rhits.Length; i++)
			{
				flag = true;
				if (rhits[i].collider.gameObject.layer == 11 && rhits[i].collider.TryGetComponent<EnemyIdentifierIdentifier>(out var component) && (bool)component.eid)
				{
					if (toIgnore.Count > 0)
					{
						foreach (EnemyType item in toIgnore)
						{
							if (item == component.eid.enemyType)
							{
								flag = false;
								break;
							}
						}
					}
					if (flag)
					{
						EnemyIdentifierIdentifier componentInParent = other.GetComponentInParent<EnemyIdentifierIdentifier>();
						if ((bool)componentInParent && (bool)componentInParent.eid && componentInParent.eid == component.eid)
						{
							flag = false;
						}
					}
				}
				if (flag)
				{
					neoRhit = rhits[i];
					break;
				}
			}
			return flag;
		}
		return false;
	}
}
