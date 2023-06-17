using UnityEngine;

public class BlackHoleTrigger : MonoBehaviour
{
	private BlackHoleProjectile bhp;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 12)
		{
			if (!bhp)
			{
				bhp = GetComponentInParent<BlackHoleProjectile>();
			}
			EnemyIdentifier component = other.GetComponent<EnemyIdentifier>();
			if ((bool)component && (!bhp.enemy || (component.enemyType != bhp.safeType && !EnemyIdentifier.CheckHurtException(bhp.safeType, component.enemyType) && !bhp.shootList.Contains(component))))
			{
				bhp.shootList.Add(component);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == 12)
		{
			if (!bhp)
			{
				bhp = GetComponentInParent<BlackHoleProjectile>();
			}
			EnemyIdentifier component = other.GetComponent<EnemyIdentifier>();
			if ((bool)component && (!bhp.enemy || (component.enemyType != bhp.safeType && !EnemyIdentifier.CheckHurtException(bhp.safeType, component.enemyType) && bhp.shootList.Contains(component))))
			{
				bhp.shootList.Remove(component);
			}
		}
	}
}
