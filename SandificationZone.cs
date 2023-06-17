using UnityEngine;

public class SandificationZone : MonoBehaviour
{
	private int difficulty;

	[HideInInspector]
	public bool buffHealth;

	[HideInInspector]
	public float healthBuff = 1f;

	[HideInInspector]
	public bool buffDamage;

	[HideInInspector]
	public float damageBuff = 1f;

	public bool buffSpeed;

	public float speedBuff = 1f;

	private void Start()
	{
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
	}

	private void Enter(Collider other)
	{
		if (other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			if (difficulty >= 3)
			{
				if (MonoSingleton<NewMovement>.Instance.hp > 10)
				{
					MonoSingleton<NewMovement>.Instance.ForceAntiHP(100 - MonoSingleton<NewMovement>.Instance.hp + 10);
				}
				else
				{
					MonoSingleton<NewMovement>.Instance.ForceAntiHP(99f);
				}
			}
			else if (difficulty == 2)
			{
				if (MonoSingleton<NewMovement>.Instance.hp > 10)
				{
					MonoSingleton<NewMovement>.Instance.ForceAntiHP(Mathf.RoundToInt(MonoSingleton<NewMovement>.Instance.antiHp) + 10);
				}
				else
				{
					MonoSingleton<NewMovement>.Instance.ForceAntiHP(99f);
				}
			}
		}
		else
		{
			if (other.gameObject.layer != 10 && other.gameObject.layer != 11)
			{
				return;
			}
			EnemyIdentifierIdentifier component = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
			if ((bool)component && (bool)component.eid && !component.eid.dead)
			{
				component.eid.Sandify();
				if (buffHealth)
				{
					component.eid.HealthBuff(healthBuff);
				}
				if (buffDamage)
				{
					component.eid.DamageBuff(damageBuff);
				}
				if (buffSpeed)
				{
					component.eid.SpeedBuff(speedBuff);
				}
				component.eid.UpdateBuffs();
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Enter(other);
	}

	private void OnCollisionEnter(Collision collision)
	{
		Enter(collision.collider);
	}
}
