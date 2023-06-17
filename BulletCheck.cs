using UnityEngine;

public class BulletCheck : MonoBehaviour
{
	public CheckerType type;

	private Streetcleaner sc;

	private V2 v2;

	private AudioSource aud;

	private int difficulty;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		switch (type)
		{
		case CheckerType.Streetcleaner:
			sc = GetComponentInParent<Streetcleaner>();
			break;
		case CheckerType.V2:
			v2 = GetComponentInParent<V2>();
			break;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		switch (type)
		{
		case CheckerType.Streetcleaner:
			if (other.gameObject.layer == 14)
			{
				Grenade component2 = other.GetComponent<Grenade>();
				if (component2 != null)
				{
					component2.enemy = true;
					component2.CanCollideWithPlayer();
					sc?.DeflectShot();
					Rigidbody component3 = other.GetComponent<Rigidbody>();
					float magnitude = component3.velocity.magnitude;
					component3.velocity = base.transform.right * magnitude;
					other.transform.forward = base.transform.right;
					aud.Play();
				}
				else
				{
					sc?.Dodge();
				}
			}
			break;
		case CheckerType.V2:
			if (other.gameObject.layer == 14)
			{
				Projectile component = other.GetComponent<Projectile>();
				if (v2 == null)
				{
					v2 = GetComponentInParent<V2>();
				}
				if (component == null || component.safeEnemyType != EnemyType.V2 || difficulty > 2)
				{
					v2?.Dodge(other.transform);
				}
			}
			break;
		}
	}

	public void ForceDodge()
	{
		switch (type)
		{
		case CheckerType.Streetcleaner:
			sc?.Dodge();
			break;
		case CheckerType.V2:
			v2?.Dodge(base.transform);
			break;
		}
	}
}
