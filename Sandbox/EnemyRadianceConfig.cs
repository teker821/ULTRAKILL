using System;

namespace Sandbox;

[Serializable]
public class EnemyRadianceConfig
{
	public bool enabled;

	public float tier = 1f;

	public float damageBuff;

	public float speedBuff;

	public float healthBuff;

	public bool damageEnabled
	{
		get
		{
			if (enabled && tier > 0f)
			{
				return damageBuff > 0f;
			}
			return false;
		}
	}

	public bool speedEnabled
	{
		get
		{
			if (enabled && tier > 0f)
			{
				return speedBuff > 0f;
			}
			return false;
		}
	}

	public bool healthEnabled
	{
		get
		{
			if (enabled && tier > 0f)
			{
				return healthBuff > 0f;
			}
			return false;
		}
	}

	public EnemyRadianceConfig()
	{
	}

	public EnemyRadianceConfig(EnemyIdentifier enemyId)
	{
		damageBuff = enemyId.damageBuffModifier;
		speedBuff = enemyId.speedBuffModifier;
		healthBuff = enemyId.healthBuffModifier;
	}
}
