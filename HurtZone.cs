using System.Collections.Generic;
using Sandbox;
using UnityEngine;
using UnityEngine.Serialization;

public class HurtZone : MonoBehaviour, IAlter, IAlterOptions<float>
{
	public EnviroDamageType damageType;

	public bool trigger;

	public float hurtCooldown = 1f;

	[FormerlySerializedAs("damage")]
	public float setDamage;

	public float enemyDamageOverride;

	private int hurtingPlayer;

	private float playerHurtCooldown;

	private List<EnemyIdentifier> hurtList = new List<EnemyIdentifier>();

	private List<int> limbsAmount = new List<int>();

	private List<float> hurtTimes = new List<float>();

	private List<EnemyIdentifier> toRemove = new List<EnemyIdentifier>();

	public GameObject hurtParticle;

	private int difficulty;

	private float damageMultiplier = 1f;

	private float damage => setDamage * damageMultiplier;

	public bool allowOnlyOne => true;

	public string alterKey => "hurt_zone";

	public string alterCategoryName => "Hurt Zone";

	public AlterOption<float>[] options => new AlterOption<float>[2]
	{
		new AlterOption<float>
		{
			key = "damage",
			name = "Damage",
			value = setDamage,
			callback = delegate(float f)
			{
				setDamage = f;
			},
			constraints = new SliderConstraints
			{
				min = 0f,
				max = 200f
			}
		},
		new AlterOption<float>
		{
			key = "hurt_cooldown",
			name = "Hurt Cooldown",
			value = hurtCooldown,
			callback = delegate(float f)
			{
				hurtCooldown = f;
			},
			constraints = new SliderConstraints
			{
				min = 0f,
				max = 10f,
				step = 0.1f
			}
		}
	};

	private void Start()
	{
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		if (difficulty < 2 && damage < 100f)
		{
			if (difficulty == 1)
			{
				damageMultiplier = 0.5f;
			}
			else if (difficulty == 0)
			{
				damageMultiplier = 0.25f;
			}
		}
	}

	private void OnDisable()
	{
		hurtingPlayer = 0;
	}

	private void FixedUpdate()
	{
		if (!base.enabled)
		{
			return;
		}
		if (hurtingPlayer > 0 && playerHurtCooldown <= 0f && damage > 0f)
		{
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
			{
				if (!MonoSingleton<NewMovement>.Instance.dead && MonoSingleton<NewMovement>.Instance.gameObject.activeInHierarchy)
				{
					MonoSingleton<NewMovement>.Instance.GetHurt((int)damage, invincible: false);
					if ((bool)hurtParticle)
					{
						Object.Instantiate(hurtParticle, MonoSingleton<NewMovement>.Instance.transform.position, Quaternion.identity);
					}
				}
				else
				{
					hurtingPlayer = 0;
				}
			}
			else if (!MonoSingleton<PlatformerMovement>.Instance.dead && MonoSingleton<PlatformerMovement>.Instance.gameObject.activeInHierarchy)
			{
				if (damageType == EnviroDamageType.WeakBurn || damageType == EnviroDamageType.Burn || damageType == EnviroDamageType.Acid)
				{
					MonoSingleton<PlatformerMovement>.Instance.Burn();
				}
				else
				{
					MonoSingleton<PlatformerMovement>.Instance.Explode();
					if ((bool)hurtParticle)
					{
						Object.Instantiate(hurtParticle, MonoSingleton<PlatformerMovement>.Instance.transform.position, Quaternion.identity);
					}
				}
			}
			else
			{
				hurtingPlayer = 0;
			}
			playerHurtCooldown = hurtCooldown;
		}
		else if (playerHurtCooldown > 0f)
		{
			playerHurtCooldown -= Time.deltaTime;
		}
		if (hurtList.Count <= 0)
		{
			return;
		}
		foreach (EnemyIdentifier hurt in hurtList)
		{
			if (hurt != null)
			{
				float num = hurtTimes[hurtList.IndexOf(hurt)];
				num -= Time.deltaTime;
				if (num <= 0f)
				{
					if (damageType == EnviroDamageType.Burn || damageType == EnviroDamageType.WeakBurn)
					{
						hurt.hitter = "fire";
					}
					else if (damageType == EnviroDamageType.Acid)
					{
						hurt.hitter = "acid";
					}
					else
					{
						hurt.hitter = "environment";
					}
					GameObject gameObject = hurt.gameObject;
					EnemyIdentifierIdentifier[] componentsInChildren = hurt.GetComponentsInChildren<EnemyIdentifierIdentifier>();
					if (componentsInChildren != null && componentsInChildren.Length != 0)
					{
						for (int num2 = componentsInChildren.Length - 1; num2 >= 0; num2--)
						{
							if (componentsInChildren[num2].gameObject.tag != "Body" && componentsInChildren[num2].transform.localScale != Vector3.zero)
							{
								gameObject = componentsInChildren[num2].gameObject;
								break;
							}
						}
					}
					if (hurt.dead && (gameObject == hurt.gameObject || hurt.enemyClass == EnemyClass.Demon || (hurt.enemyClass == EnemyClass.Machine && (bool)hurt.machine && !hurt.machine.dismemberment)))
					{
						int index = hurtList.IndexOf(hurt);
						hurtTimes.RemoveAt(index);
						limbsAmount.RemoveAt(index);
						toRemove.Add(hurt);
						break;
					}
					if (enemyDamageOverride == 0f)
					{
						hurt.DeliverDamage(gameObject, Vector3.zero, hurt.transform.position, damage / 2f, tryForExplode: false);
					}
					else
					{
						hurt.DeliverDamage(gameObject, Vector3.zero, hurt.transform.position, enemyDamageOverride, tryForExplode: false);
					}
					if ((bool)hurtParticle && !hurt.dead)
					{
						Object.Instantiate(hurtParticle, hurt.transform.position, Quaternion.identity);
					}
					if ((damageType == EnviroDamageType.Burn || damageType == EnviroDamageType.WeakBurn) && !hurt.dead)
					{
						Flammable componentInChildren = hurt.GetComponentInChildren<Flammable>();
						if (componentInChildren != null)
						{
							componentInChildren.Burn(4f);
						}
					}
					num = ((!hurt.dead || damageType != EnviroDamageType.Acid) ? 1f : 0.1f);
				}
				hurtTimes[hurtList.IndexOf(hurt)] = num;
			}
			else
			{
				toRemove.Add(hurt);
			}
		}
		if (toRemove.Count <= 0)
		{
			return;
		}
		foreach (EnemyIdentifier item in toRemove)
		{
			hurtList.Remove(item);
		}
		toRemove.Clear();
	}

	private void Enter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			hurtingPlayer++;
		}
		else
		{
			if (other.gameObject.layer != 10 && other.gameObject.layer != 11 && other.gameObject.layer != 12 && other.gameObject.layer != 20)
			{
				return;
			}
			EnemyIdentifierIdentifier enemyIdentifierIdentifier = ((other.gameObject.layer == 12) ? other.gameObject.GetComponentInChildren<EnemyIdentifierIdentifier>() : ((other.gameObject.layer != 20 || !other.transform.parent) ? other.gameObject.GetComponent<EnemyIdentifierIdentifier>() : other.transform.parent.GetComponentInChildren<EnemyIdentifierIdentifier>()));
			if (!(enemyIdentifierIdentifier != null) || !(enemyIdentifierIdentifier.eid != null) || (enemyIdentifierIdentifier.eid.dead && enemyIdentifierIdentifier.eid.enemyClass == EnemyClass.Demon) || !(enemyIdentifierIdentifier.transform.localScale != Vector3.zero) || (damageType == EnviroDamageType.WeakBurn && (enemyIdentifierIdentifier.eid.enemyType == EnemyType.Streetcleaner || enemyIdentifierIdentifier.eid.enemyType == EnemyType.Sisyphus)))
			{
				return;
			}
			if (!hurtList.Contains(enemyIdentifierIdentifier.eid))
			{
				hurtList.Add(enemyIdentifierIdentifier.eid);
				hurtTimes.Add(1f);
				limbsAmount.Add(1);
				if (!base.enabled)
				{
					return;
				}
				if (damageType == EnviroDamageType.Burn || damageType == EnviroDamageType.WeakBurn)
				{
					enemyIdentifierIdentifier.eid.hitter = "fire";
				}
				else if (damageType == EnviroDamageType.Acid)
				{
					enemyIdentifierIdentifier.eid.hitter = "acid";
				}
				else
				{
					enemyIdentifierIdentifier.eid.hitter = "environment";
				}
				if ((bool)hurtParticle && !enemyIdentifierIdentifier.eid.dead)
				{
					Object.Instantiate(hurtParticle, enemyIdentifierIdentifier.eid.transform.position, Quaternion.identity);
				}
				if (enemyDamageOverride == 0f)
				{
					enemyIdentifierIdentifier.eid.DeliverDamage(enemyIdentifierIdentifier.eid.gameObject, Vector3.zero, enemyIdentifierIdentifier.eid.transform.position, damage / 2f, tryForExplode: false);
				}
				else
				{
					enemyIdentifierIdentifier.eid.DeliverDamage(enemyIdentifierIdentifier.eid.gameObject, Vector3.zero, enemyIdentifierIdentifier.eid.transform.position, enemyDamageOverride, tryForExplode: false);
				}
				if (damageType == EnviroDamageType.Burn || damageType == EnviroDamageType.WeakBurn)
				{
					Flammable componentInChildren = enemyIdentifierIdentifier.eid.GetComponentInChildren<Flammable>();
					if (componentInChildren != null)
					{
						componentInChildren.Burn(4f);
					}
				}
			}
			else
			{
				limbsAmount[hurtList.IndexOf(enemyIdentifierIdentifier.eid)]++;
			}
		}
	}

	private void Exit(Collider other)
	{
		if (other.gameObject.tag == "Player" && hurtingPlayer > 0)
		{
			hurtingPlayer--;
		}
		else
		{
			if (other.gameObject.layer != 10 && other.gameObject.layer != 11 && other.gameObject.layer != 12 && other.gameObject.layer != 20)
			{
				return;
			}
			EnemyIdentifierIdentifier enemyIdentifierIdentifier = ((other.gameObject.layer == 12) ? other.gameObject.GetComponentInChildren<EnemyIdentifierIdentifier>() : ((other.gameObject.layer != 20 || !other.transform.parent) ? other.gameObject.GetComponent<EnemyIdentifierIdentifier>() : other.transform.parent.GetComponentInChildren<EnemyIdentifierIdentifier>()));
			if (enemyIdentifierIdentifier != null && enemyIdentifierIdentifier.eid != null && hurtList.Contains(enemyIdentifierIdentifier.eid))
			{
				int index = hurtList.IndexOf(enemyIdentifierIdentifier.eid);
				limbsAmount[index]--;
				if (limbsAmount[index] <= 0)
				{
					hurtTimes.RemoveAt(index);
					limbsAmount.RemoveAt(index);
					hurtList.Remove(enemyIdentifierIdentifier.eid);
				}
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (trigger || other.gameObject.layer == 20)
		{
			Enter(other);
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if (!trigger)
		{
			Enter(other.collider);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (trigger || other.gameObject.layer == 20)
		{
			Exit(other);
		}
	}

	private void OnCollisionExit(Collision other)
	{
		if (!trigger)
		{
			Exit(other.collider);
		}
	}
}
