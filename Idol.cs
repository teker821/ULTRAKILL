using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;

public class Idol : MonoBehaviour
{
	public EnemyIdentifier overrideTarget;

	public bool activeWhileWaitingForOverride;

	[HideInInspector]
	public EnemyIdentifier target;

	private int difficulty;

	[SerializeField]
	private LineRenderer beam;

	private Vector3 beamOffset;

	[SerializeField]
	private GameObject deathParticle;

	private bool dead;

	private EnemyIdentifier eid;

	[HideInInspector]
	public bool damageBuffing;

	[HideInInspector]
	public bool speedBuffing;

	private void Start()
	{
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		if ((bool)overrideTarget && overrideTarget.gameObject.activeInHierarchy)
		{
			ChangeTarget(overrideTarget);
		}
		eid = GetComponent<EnemyIdentifier>();
		SlowUpdate();
	}

	private void UpdateBuff()
	{
		if (damageBuffing != eid.damageBuff)
		{
			if ((bool)target)
			{
				if (damageBuffing)
				{
					target.DamageUnbuff();
				}
				else
				{
					target.DamageBuff();
				}
			}
			damageBuffing = eid.damageBuff;
		}
		if (speedBuffing == eid.speedBuff)
		{
			return;
		}
		if ((bool)target)
		{
			if (speedBuffing)
			{
				target.SpeedUnbuff();
			}
			else
			{
				target.SpeedBuff();
			}
		}
		speedBuffing = eid.speedBuff;
	}

	private void OnDisable()
	{
		CancelInvoke("SlowUpdate");
		if ((bool)target)
		{
			ChangeTarget(null);
		}
	}

	private void OnEnable()
	{
		CancelInvoke("SlowUpdate");
		SlowUpdate();
	}

	private void Update()
	{
		if ((bool)overrideTarget && target != overrideTarget && !overrideTarget.dead && overrideTarget.gameObject.activeInHierarchy)
		{
			ChangeTarget(overrideTarget);
		}
		if (beam.enabled != (bool)target)
		{
			beam.enabled = target;
		}
		if ((bool)target)
		{
			beam.SetPosition(0, beam.transform.position);
			beam.SetPosition(1, target.transform.position + beamOffset);
		}
	}

	private void SlowUpdate()
	{
		if (BlindEnemies.Blind)
		{
			if ((bool)target && (!overrideTarget || target != overrideTarget || overrideTarget.dead))
			{
				ChangeTarget(null);
			}
			Invoke("SlowUpdate", 0.2f);
			return;
		}
		if ((bool)overrideTarget)
		{
			if ((bool)overrideTarget && !overrideTarget.dead && (overrideTarget.gameObject.activeInHierarchy || !activeWhileWaitingForOverride))
			{
				if (target != overrideTarget && overrideTarget.gameObject.activeInHierarchy)
				{
					ChangeTarget(overrideTarget);
				}
				Invoke("SlowUpdate", 0.2f);
				return;
			}
			overrideTarget = null;
			ChangeTarget(null);
		}
		List<EnemyIdentifier> currentEnemies = MonoSingleton<EnemyTracker>.Instance.GetCurrentEnemies();
		if (currentEnemies != null && currentEnemies.Count > 0)
		{
			bool flag = false;
			float num = float.PositiveInfinity;
			EnemyIdentifier newTarget = null;
			int num2 = 1;
			if ((bool)target && !target.dead)
			{
				num2 = Mathf.Max(MonoSingleton<EnemyTracker>.Instance.GetEnemyRank(target), 2);
			}
			for (int num3 = 6; num3 > num2; num3--)
			{
				for (int i = 0; i < currentEnemies.Count; i++)
				{
					if (((!currentEnemies[i].blessed && currentEnemies[i].enemyType != EnemyType.Idol) || currentEnemies[i] == target) && (MonoSingleton<EnemyTracker>.Instance.GetEnemyRank(currentEnemies[i]) == num3 || (MonoSingleton<EnemyTracker>.Instance.GetEnemyRank(currentEnemies[i]) <= 2 && num3 == 2)))
					{
						float num4 = Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, currentEnemies[i].transform.position);
						if (num4 < num)
						{
							newTarget = currentEnemies[i];
							flag = true;
							num = num4;
						}
					}
				}
				if (flag)
				{
					ChangeTarget(newTarget);
					break;
				}
			}
		}
		Invoke("SlowUpdate", 0.2f);
	}

	public void Death()
	{
		if (dead)
		{
			return;
		}
		dead = true;
		GoreZone goreZone = GoreZone.ResolveGoreZone(base.transform);
		if (TryGetComponent<EnemyIdentifier>(out var component))
		{
			component.Death();
		}
		if ((bool)deathParticle)
		{
			Object.Instantiate(deathParticle, beam.transform.position, Quaternion.identity, goreZone.gibZone);
		}
		GameObject gameObject = null;
		for (int i = 0; i < 3; i++)
		{
			gameObject = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Head, component.underwater, component.sandified, component.blessed);
			if (!gameObject)
			{
				break;
			}
			gameObject.transform.position = beam.transform.position;
			gameObject.transform.SetParent(goreZone.goreZone, worldPositionStays: true);
			gameObject.SetActive(value: true);
			if (gameObject.TryGetComponent<Bloodsplatter>(out var component2))
			{
				component2.GetReady();
			}
		}
		if (!component.dontCountAsKills)
		{
			ActivateNextWave componentInParent = GetComponentInParent<ActivateNextWave>();
			if (componentInParent != null)
			{
				componentInParent.AddDeadEnemy();
			}
		}
		base.gameObject.SetActive(value: false);
		Object.Destroy(base.gameObject);
	}

	private void ChangeTarget(EnemyIdentifier newTarget)
	{
		if ((bool)target)
		{
			target.Unbless();
			if (speedBuffing)
			{
				target.SpeedUnbuff();
			}
			if (damageBuffing)
			{
				target.DamageUnbuff();
			}
		}
		if (!newTarget)
		{
			target = null;
			return;
		}
		target = newTarget;
		target.Bless();
		if (speedBuffing)
		{
			target.SpeedBuff();
		}
		if (damageBuffing)
		{
			target.DamageBuff();
		}
		if (target.TryGetComponent<Collider>(out var component))
		{
			beamOffset = component.bounds.center - target.transform.position;
		}
	}

	public void ChangeOverrideTarget(EnemyIdentifier eid)
	{
		overrideTarget = eid;
		ChangeTarget(eid);
	}
}
