using System;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
	private NewMovement pm;

	public GameObject sawSound;

	public string deathType;

	public bool dontExplode;

	public bool aliveOnly;

	public AffectedSubjects affected;

	private bool playerAffected;

	private bool enemyAffected;

	public bool checkForPlayerOutsideTrigger;

	[Space(10f)]
	public bool notInstakill;

	public Vector3 respawnTarget;

	public bool dontChangeRespawnTarget;

	public int damage = 50;

	private Transform player;

	public EnemyType[] unaffectedEnemyTypes;

	private void Start()
	{
		if (unaffectedEnemyTypes == null)
		{
			unaffectedEnemyTypes = Array.Empty<EnemyType>();
		}
		player = MonoSingleton<NewMovement>.Instance.transform;
		switch (affected)
		{
		case AffectedSubjects.All:
			enemyAffected = true;
			playerAffected = true;
			break;
		case AffectedSubjects.EnemiesOnly:
			enemyAffected = true;
			playerAffected = false;
			break;
		case AffectedSubjects.PlayerOnly:
			enemyAffected = false;
			playerAffected = true;
			break;
		}
		Invoke("SlowUpdate", 1f);
	}

	private void SlowUpdate()
	{
		if (base.gameObject.activeInHierarchy && checkForPlayerOutsideTrigger && player.transform.position.y < base.transform.position.y)
		{
			GotHit(player.GetComponent<Collider>());
		}
		Invoke("SlowUpdate", 1f);
	}

	private void OnTriggerEnter(Collider other)
	{
		GotHit(other);
	}

	private void GotHit(Collider other)
	{
		if (other.gameObject.tag == "Player" && playerAffected)
		{
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
			{
				if (dontExplode || deathType.ToLower() == "fall")
				{
					MonoSingleton<PlatformerMovement>.Instance.Fall();
				}
				else
				{
					MonoSingleton<PlatformerMovement>.Instance.Explode(ignoreInvincible: true);
				}
				return;
			}
			if (!notInstakill)
			{
				if (pm == null)
				{
					pm = other.GetComponent<NewMovement>();
				}
				pm.GetHurt(999999, invincible: false, 1f, explosion: false, instablack: true);
				if (sawSound != null)
				{
					UnityEngine.Object.Instantiate(sawSound, other.transform.position, Quaternion.identity);
				}
				base.enabled = false;
				return;
			}
			if (pm == null)
			{
				pm = other.GetComponent<NewMovement>();
			}
			if (pm.hp > 0)
			{
				if (pm.hp > damage)
				{
					pm.GetHurt(damage, invincible: true);
				}
				else if (pm.hp > 1)
				{
					pm.GetHurt(pm.hp - 1, invincible: true);
				}
				else
				{
					pm.FakeHurt();
				}
				if (sawSound != null)
				{
					UnityEngine.Object.Instantiate(sawSound, other.transform.position, Quaternion.identity);
				}
				other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
				StatsManager instance = MonoSingleton<StatsManager>.Instance;
				_ = Vector3.zero;
				if (respawnTarget != Vector3.zero)
				{
					other.transform.position = respawnTarget + Vector3.up * 1.25f;
				}
				else if (instance.currentCheckPoint != null)
				{
					other.transform.position = instance.currentCheckPoint.transform.position + Vector3.up * 1.25f;
				}
				else
				{
					other.transform.position = instance.spawnPos;
				}
			}
		}
		else if ((other.gameObject.tag == "Enemy" || other.gameObject.layer == 10) && enemyAffected)
		{
			EnemyIdentifier enemyIdentifier = other.gameObject.GetComponentInParent<EnemyIdentifier>();
			if (enemyIdentifier == null)
			{
				EnemyIdentifierIdentifier component = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
				if (component != null && component.eid != null)
				{
					enemyIdentifier = component.eid;
				}
			}
			if (!(enemyIdentifier != null))
			{
				return;
			}
			if (unaffectedEnemyTypes.Length != 0)
			{
				for (int i = 0; i < unaffectedEnemyTypes.Length; i++)
				{
					if (unaffectedEnemyTypes[i] == enemyIdentifier.enemyType)
					{
						return;
					}
				}
			}
			if ((dontExplode || aliveOnly || enemyIdentifier.exploded) && enemyIdentifier.dead)
			{
				return;
			}
			if (sawSound != null)
			{
				UnityEngine.Object.Instantiate(sawSound, other.transform.position, Quaternion.identity);
			}
			enemyIdentifier.hitter = "deathzone";
			StyleHUD instance2 = MonoSingleton<StyleHUD>.Instance;
			if ((bool)instance2)
			{
				if (!enemyIdentifier.dead)
				{
					instance2.AddPoints(80, deathType, null, enemyIdentifier);
				}
				else
				{
					instance2.AddPoints(20, "<color=grey>" + deathType + "</color>", null, enemyIdentifier);
				}
			}
			if (!dontExplode)
			{
				enemyIdentifier.Explode();
			}
			else
			{
				enemyIdentifier.InstaKill();
			}
		}
		else if (other.gameObject.tag == "Coin")
		{
			other.GetComponent<Coin>()?.GetDeleted();
		}
	}
}
