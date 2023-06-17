using ULTRAKILL.Cheats;
using UnityEngine;

public class DroneFlesh : MonoBehaviour
{
	public GameObject beam;

	public GameObject warningBeam;

	public GameObject chargeEffect;

	private GameObject currentWarningBeam;

	private GameObject currentChargeEffect;

	private AudioSource ceAud;

	private Light ceLight;

	private float cooldown = 3f;

	private bool inAction;

	private Drone drn;

	private EnemyIdentifier eid;

	private Transform target;

	private bool tracking;

	private void Start()
	{
		cooldown = Random.Range(2f, 3f);
		drn = GetComponent<Drone>();
		drn.fleshDrone = true;
		eid = GetComponent<EnemyIdentifier>();
		target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
	}

	private void Update()
	{
		if (eid.enemyType == EnemyType.Virtue)
		{
			return;
		}
		if (drn.crashing)
		{
			drn.Explode();
		}
		else
		{
			if (BlindEnemies.Blind)
			{
				return;
			}
			if (tracking)
			{
				base.transform.LookAt(target.position);
			}
			if (!drn.playerSpotted || inAction)
			{
				return;
			}
			if (cooldown > 0f)
			{
				cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
				if (cooldown <= 1f && (bool)chargeEffect)
				{
					if (!currentChargeEffect)
					{
						currentChargeEffect = Object.Instantiate(chargeEffect, base.transform.position + base.transform.forward * 1.5f, base.transform.rotation);
						currentChargeEffect.transform.SetParent(base.transform);
						currentChargeEffect.transform.localScale = Vector3.zero;
						ceAud = currentChargeEffect.GetComponent<AudioSource>();
						ceLight = currentChargeEffect.GetComponent<Light>();
					}
					currentChargeEffect.transform.localScale = Vector3.one * (1f - cooldown) * 2.5f;
					if ((bool)ceAud)
					{
						ceAud.pitch = (1f - cooldown) * 2f;
					}
					if ((bool)ceLight)
					{
						ceLight.intensity = (1f - cooldown) * 30f;
					}
				}
			}
			else
			{
				inAction = true;
				cooldown = Random.Range(1f, 3f);
				PrepareBeam();
			}
		}
	}

	private void PrepareBeam()
	{
		drn.lockPosition = true;
		drn.lockRotation = true;
		base.transform.LookAt(target.position);
		currentWarningBeam = Object.Instantiate(warningBeam, base.transform);
		currentWarningBeam.transform.position += base.transform.forward * 1.5f;
		Invoke("ShootBeam", 0.5f / eid.totalSpeedModifier);
	}

	private void StopTracking()
	{
		tracking = false;
	}

	private void ShootBeam()
	{
		if ((bool)currentWarningBeam)
		{
			Object.Destroy(currentWarningBeam);
		}
		if ((bool)currentChargeEffect)
		{
			Object.Destroy(currentChargeEffect);
		}
		GameObject gameObject = Object.Instantiate(beam, base.transform.position, base.transform.rotation);
		if (eid.totalDamageModifier != 1f && gameObject.TryGetComponent<RevolverBeam>(out var component))
		{
			component.damage *= eid.totalDamageModifier;
		}
		drn.lockPosition = false;
		drn.lockRotation = false;
		inAction = false;
	}

	public void Explode()
	{
		drn.Explode();
	}
}
