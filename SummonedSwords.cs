using UnityEngine;

public class SummonedSwords : MonoBehaviour
{
	public Transform target;

	private bool inFormation;

	private SummonedSwordFormation formation;

	private Projectile[] swords;

	public float speed = 1f;

	private int difficulty;

	private bool spinning;

	private void Start()
	{
		swords = GetComponentsInChildren<Projectile>();
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		Invoke("Begin", 5f * speed);
	}

	private void FixedUpdate()
	{
		if (!inFormation)
		{
			if ((bool)target)
			{
				base.transform.position = target.position;
			}
			base.transform.Rotate(Vector3.up, Time.deltaTime * 720f * speed, Space.Self);
		}
		else if (formation == SummonedSwordFormation.Spiral)
		{
			base.transform.position = target.position;
			if (spinning)
			{
				base.transform.Rotate(Vector3.up, Time.deltaTime * 720f * speed, Space.Self);
			}
		}
	}

	private void Begin()
	{
		Projectile[] array;
		if (difficulty > 3)
		{
			inFormation = true;
			target = MonoSingleton<NewMovement>.Instance.transform;
			base.transform.position = target.position;
			array = swords;
			foreach (Projectile projectile in array)
			{
				if ((bool)projectile)
				{
					projectile.transform.localPosition += projectile.transform.forward * 5f;
					projectile.transform.Rotate(Vector3.up, 180f, Space.World);
				}
			}
			spinning = true;
			Invoke("StopSpinning", 0.75f * speed);
			Invoke("SpiralStab", 1f * speed);
			return;
		}
		array = swords;
		foreach (Projectile projectile2 in array)
		{
			if ((bool)projectile2)
			{
				Object.Instantiate(projectile2.explosionEffect, projectile2.transform.position, Quaternion.identity);
			}
		}
		Object.Destroy(base.gameObject);
	}

	private void SpiralStab()
	{
		Projectile[] array = swords;
		foreach (Projectile projectile in array)
		{
			if ((bool)projectile)
			{
				projectile.transform.SetParent(null, worldPositionStays: true);
				projectile.speed = 150f * speed;
			}
		}
		Object.Destroy(base.gameObject);
	}

	private void StopSpinning()
	{
		spinning = false;
	}
}
