using UnityEngine;

public class EnemyRevolver : MonoBehaviour
{
	public EnemyType safeEnemyType;

	public int variation;

	public GameObject bullet;

	public GameObject altBullet;

	public GameObject primaryPrepare;

	private GameObject currentpp;

	private GameObject altCharge;

	private AudioSource altChargeAud;

	private float chargeAmount;

	private bool charging;

	public Transform shootPoint;

	public GameObject muzzleFlash;

	public GameObject muzzleFlashAlt;

	private int difficulty;

	private EnemyIdentifier eid;

	private float speedMultiplier = 1f;

	private float damageMultiplier = 1f;

	private void Start()
	{
		altCharge = shootPoint.GetChild(0).gameObject;
		altChargeAud = altCharge.GetComponent<AudioSource>();
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		eid = GetComponentInParent<EnemyIdentifier>();
	}

	private void Update()
	{
		if (charging)
		{
			float num = 2f;
			if (difficulty == 1)
			{
				num = 1.5f;
			}
			if (difficulty == 0)
			{
				num = 1f;
			}
			chargeAmount = Mathf.MoveTowards(chargeAmount, 1f, Time.deltaTime * num * speedMultiplier);
			altChargeAud.pitch = chargeAmount / 1.75f;
			altCharge.transform.localScale = Vector3.one * chargeAmount * 10f;
		}
	}

	public void Fire()
	{
		if (currentpp != null)
		{
			Object.Destroy(currentpp);
		}
		Vector3 position = shootPoint.position;
		if (Vector3.Distance(base.transform.position, eid.transform.position) > Vector3.Distance(MonoSingleton<NewMovement>.Instance.transform.position, eid.transform.position))
		{
			position = new Vector3(eid.transform.position.x, base.transform.position.y, eid.transform.position.z);
		}
		GameObject obj = Object.Instantiate(bullet, position, shootPoint.rotation);
		Object.Instantiate(muzzleFlash, shootPoint.position, shootPoint.rotation);
		Projectile component = obj.GetComponent<Projectile>();
		if ((bool)component)
		{
			component.safeEnemyType = safeEnemyType;
			if (difficulty == 1)
			{
				component.speed *= 0.75f;
			}
			if (difficulty == 0)
			{
				component.speed *= 0.5f;
			}
			component.speed *= speedMultiplier;
			component.damage *= damageMultiplier;
		}
	}

	public void AltFire()
	{
		CancelAltCharge();
		Vector3 position = shootPoint.position;
		if (Vector3.Distance(base.transform.position, eid.transform.position) > Vector3.Distance(MonoSingleton<NewMovement>.Instance.transform.position, eid.transform.position))
		{
			position = new Vector3(eid.transform.position.x, base.transform.position.y, eid.transform.position.z);
		}
		GameObject obj = Object.Instantiate(altBullet, position, shootPoint.rotation);
		Object.Instantiate(muzzleFlashAlt, shootPoint.position, shootPoint.rotation);
		Projectile component = obj.GetComponent<Projectile>();
		if ((bool)component)
		{
			component.safeEnemyType = safeEnemyType;
			if (difficulty == 1)
			{
				component.speed *= 0.75f;
			}
			if (difficulty == 0)
			{
				component.speed *= 0.5f;
			}
			component.speed *= speedMultiplier;
			component.damage *= damageMultiplier;
		}
	}

	public void PrepareFire()
	{
		if (currentpp != null)
		{
			Object.Destroy(currentpp);
		}
		currentpp = Object.Instantiate(primaryPrepare, shootPoint);
		currentpp.transform.Rotate(Vector3.up * 90f);
	}

	public void PrepareAltFire()
	{
		if ((bool)altCharge)
		{
			charging = true;
			altCharge.SetActive(value: true);
		}
	}

	public void CancelAltCharge()
	{
		if ((bool)altChargeAud)
		{
			charging = false;
			chargeAmount = 0f;
			altChargeAud.pitch = 0f;
			altCharge.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		if (currentpp != null)
		{
			Object.Destroy(currentpp);
		}
	}

	private void UpdateBuffs(EnemyIdentifier eid)
	{
		speedMultiplier = eid.totalSpeedModifier;
		damageMultiplier = eid.totalDamageModifier;
	}
}
