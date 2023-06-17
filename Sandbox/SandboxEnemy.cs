using System;
using UnityEngine;
using UnityEngine.AI;

namespace Sandbox;

public class SandboxEnemy : SandboxSpawnableInstance
{
	public EnemyIdentifier enemyId;

	public EnemyRadianceConfig radiance;

	private bool lastSpeedBuffState;

	private bool lastDamageBuffState;

	private bool lastHealthBuffState;

	public override void Awake()
	{
		base.Awake();
		enemyId = GetComponent<EnemyIdentifier>();
		if (enemyId == null)
		{
			enemyId = GetComponentInChildren<EnemyIdentifier>();
		}
		radiance = new EnemyRadianceConfig(enemyId);
	}

	public void RestoreRadiance(EnemyRadianceConfig config)
	{
		radiance = config;
		UpdateRadiance();
	}

	public void UpdateRadiance()
	{
		enemyId.radianceTier = radiance.tier;
		if (!lastSpeedBuffState && radiance.speedEnabled)
		{
			enemyId.SpeedBuff(radiance.speedBuff);
		}
		else if (lastSpeedBuffState && !radiance.speedEnabled)
		{
			enemyId.SpeedUnbuff();
		}
		enemyId.speedBuffModifier = radiance.speedBuff;
		if (!lastDamageBuffState && radiance.damageEnabled)
		{
			enemyId.DamageBuff(radiance.damageBuff);
		}
		else if (lastDamageBuffState && !radiance.damageEnabled)
		{
			enemyId.DamageUnbuff();
		}
		enemyId.damageBuffModifier = radiance.damageBuff;
		if (!lastHealthBuffState && radiance.healthEnabled)
		{
			enemyId.HealthBuff(radiance.healthBuff);
		}
		else if (lastHealthBuffState && !radiance.healthEnabled)
		{
			enemyId.HealthUnbuff();
		}
		enemyId.healthBuffModifier = radiance.healthBuff;
		lastSpeedBuffState = radiance.speedEnabled;
		lastDamageBuffState = radiance.damageEnabled;
		lastHealthBuffState = radiance.healthEnabled;
		enemyId.UpdateBuffs();
	}

	private void OnEnable()
	{
		enemyId = GetComponent<EnemyIdentifier>();
		if (!enemyId)
		{
			enemyId = GetComponentInChildren<EnemyIdentifier>();
		}
	}

	public SavedEnemy SaveEnemy()
	{
		if (!enemyId || enemyId.health < 0f || enemyId.dead)
		{
			return null;
		}
		SavedEnemy obj = new SavedEnemy
		{
			Radiance = radiance
		};
		SavedGeneric saveObject = obj;
		BaseSave(ref saveObject);
		return obj;
	}

	public override void Pause(bool freeze = true)
	{
		Debug.Log("Pause");
		base.Pause(freeze);
		if (collider.gameObject.TryGetComponent<NavMeshAgent>(out var component))
		{
			component.enabled = false;
		}
		if (collider.gameObject.TryGetComponent<EnemyIdentifier>(out var component2))
		{
			component2.enabled = false;
		}
		if (collider.gameObject.TryGetComponent<Animator>(out var component3))
		{
			component3.enabled = false;
		}
		foreach (Type type in EnemyTypes.types)
		{
			if (collider.gameObject.TryGetComponent(type, out var component4))
			{
				((Behaviour)component4).enabled = false;
			}
		}
	}

	public override void Resume()
	{
		base.Resume();
		if (collider == null)
		{
			return;
		}
		if (collider.gameObject.TryGetComponent<NavMeshAgent>(out var component))
		{
			component.enabled = true;
		}
		if (collider.gameObject.TryGetComponent<EnemyIdentifier>(out var component2))
		{
			component2.enabled = true;
		}
		if (collider.gameObject.TryGetComponent<Animator>(out var component3))
		{
			component3.enabled = true;
		}
		foreach (Type type in EnemyTypes.types)
		{
			if (collider.gameObject.TryGetComponent(type, out var component4))
			{
				((Behaviour)component4).enabled = true;
			}
		}
	}
}
