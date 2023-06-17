using UnityEngine;

public class FireZone : MonoBehaviour
{
	public FlameSource source;

	private Streetcleaner sc;

	private float playerHurtCooldown;

	private NewMovement nmov;

	private void Update()
	{
		if (playerHurtCooldown != 0f)
		{
			playerHurtCooldown = Mathf.MoveTowards(playerHurtCooldown, 0f, Time.deltaTime);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (source != FlameSource.Streetcleaner)
		{
			return;
		}
		if (sc == null)
		{
			sc = GetComponentInParent<Streetcleaner>();
		}
		if (!sc.damaging)
		{
			return;
		}
		if (playerHurtCooldown == 0f && other.gameObject.tag == "Player")
		{
			playerHurtCooldown = 0.5f;
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
			{
				MonoSingleton<PlatformerMovement>.Instance.Burn();
			}
			else
			{
				MonoSingleton<NewMovement>.Instance.GetHurt((int)(20f * sc.eid.totalDamageModifier), invincible: true);
			}
		}
		else
		{
			Flammable component = other.GetComponent<Flammable>();
			if (component != null && !component.playerOnly)
			{
				component.Burn(10f);
			}
		}
	}
}
