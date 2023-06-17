using UnityEngine;

public class EnemyNailgun : MonoBehaviour
{
	public GameObject nail;

	public GameObject altNail;

	public Transform shootPoint;

	public GameObject flash;

	public GameObject muzzleFlash;

	[SerializeField]
	private AudioSource chargeSound;

	private bool charging;

	private float chargeAmount;

	private int burstAmount;

	private float cooldown;

	private GameObject currentNail;

	private float currentSpread = 5f;

	private float fireRate = 0.033f;

	public Collider[] toIgnore;

	private int difficulty;

	private EnemyIdentifier eid;

	private float speedMultiplier;

	private float damageMultiplier;

	private void Start()
	{
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		eid = GetComponentInParent<EnemyIdentifier>();
	}

	private void FixedUpdate()
	{
		if (cooldown > 0f)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.fixedDeltaTime * speedMultiplier);
		}
		if (burstAmount > 0 && cooldown <= 0f)
		{
			Vector3 position = shootPoint.position;
			if (Vector3.Distance(base.transform.position, eid.transform.position) > Vector3.Distance(MonoSingleton<NewMovement>.Instance.transform.position, eid.transform.position))
			{
				Vector3 vector = eid.transform.position + base.transform.forward * Vector3.Distance(MonoSingleton<NewMovement>.Instance.transform.position, eid.transform.position);
				position = new Vector3(vector.x, base.transform.position.y, vector.z);
			}
			GameObject gameObject = Object.Instantiate(currentNail, position, shootPoint.rotation);
			gameObject.transform.Rotate(Random.Range((0f - currentSpread) / 3f, currentSpread / 3f), Random.Range((0f - currentSpread) / 3f, currentSpread / 3f), Random.Range((0f - currentSpread) / 3f, currentSpread / 3f));
			gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * 200f * speedMultiplier, ForceMode.VelocityChange);
			if (damageMultiplier != 1f && gameObject.TryGetComponent<Nail>(out var component))
			{
				component.damage *= damageMultiplier;
			}
			Collider component2 = gameObject.GetComponent<CapsuleCollider>();
			Collider[] array = toIgnore;
			foreach (Collider collider in array)
			{
				Physics.IgnoreCollision(component2, collider, ignore: true);
			}
			Object.Instantiate(muzzleFlash, shootPoint);
			cooldown = fireRate;
			burstAmount--;
		}
		if (charging)
		{
			chargeAmount = Mathf.MoveTowards(chargeAmount, 1f, Time.deltaTime);
			chargeSound.pitch = chargeAmount * 2f;
		}
	}

	public void Fire()
	{
		burstAmount = 30;
		if (difficulty > 2)
		{
			currentSpread = 5f;
		}
		else if (difficulty == 2)
		{
			currentSpread = 3f;
		}
		else
		{
			currentSpread = 1.5f;
		}
		fireRate = 0.033f;
		currentNail = nail;
	}

	public void AltFire()
	{
		burstAmount = 100;
		if (difficulty > 2)
		{
			currentSpread = 25f;
		}
		else if (difficulty == 2)
		{
			currentSpread = 15f;
		}
		else
		{
			currentSpread = 7.5f;
		}
		fireRate = 0.01f;
		currentNail = altNail;
		chargeSound.Stop();
		charging = false;
	}

	public void PrepareFire()
	{
		burstAmount = 0;
		Object.Instantiate(flash, shootPoint.position, shootPoint.rotation).transform.localScale *= 4f;
	}

	public void PrepareAltFire()
	{
		burstAmount = 0;
		charging = true;
		chargeAmount = 0f;
		chargeSound.pitch = 0f;
		chargeSound.Play();
	}

	public void CancelAltCharge()
	{
		charging = false;
		chargeAmount = 0f;
		chargeSound.pitch = 0f;
	}

	private void UpdateBuffs(EnemyIdentifier eid)
	{
		speedMultiplier = eid.totalSpeedModifier;
		damageMultiplier = eid.totalDamageModifier;
	}
}
