using ULTRAKILL.Cheats;
using UnityEngine;

public class CancerousRodent : MonoBehaviour
{
	private Rigidbody rb;

	private Transform player;

	private Machine mach;

	private Statue stat;

	private EnemyIdentifier eid;

	public bool harmless;

	public GameObject[] activateOnDeath;

	public Transform shootPoint;

	public GameObject projectile;

	private float coolDown = 2f;

	public int projectileAmount;

	private int currentProjectiles;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		player = MonoSingleton<PlayerTracker>.Instance.GetPlayer();
		GetComponent<Collider>();
		if (harmless)
		{
			mach = GetComponent<Machine>();
		}
		else
		{
			stat = GetComponent<Statue>();
		}
		eid = GetComponent<EnemyIdentifier>();
	}

	private void OnDisable()
	{
		if (harmless || ((bool)stat && !(stat.health <= 0f)))
		{
			return;
		}
		GameObject[] array = activateOnDeath;
		foreach (GameObject gameObject in array)
		{
			if ((bool)gameObject)
			{
				gameObject.SetActive(value: true);
			}
		}
	}

	private void Update()
	{
		if (rb != null)
		{
			if (BlindEnemies.Blind)
			{
				rb.velocity = Vector3.zero;
			}
			else
			{
				base.transform.LookAt(new Vector3(player.position.x, base.transform.position.y, player.position.z));
				rb.velocity = base.transform.forward * Time.deltaTime * 100f * eid.totalSpeedModifier;
			}
		}
		if (harmless)
		{
			if (!(mach.health <= 0f))
			{
				return;
			}
			GameObject[] array = activateOnDeath;
			foreach (GameObject gameObject in array)
			{
				if ((bool)gameObject)
				{
					gameObject.SetActive(value: true);
				}
			}
			Object.Destroy(GetComponentInChildren<Light>().gameObject);
			Object.Destroy(this);
		}
		else if (stat.health > 0f && !BlindEnemies.Blind)
		{
			if (coolDown != 0f)
			{
				coolDown = Mathf.MoveTowards(coolDown, 0f, Time.deltaTime * eid.totalSpeedModifier);
			}
			else if (!Physics.Raycast(shootPoint.position, player.transform.position - shootPoint.position, Vector3.Distance(player.transform.position, shootPoint.position), LayerMaskDefaults.Get(LMD.Environment)))
			{
				coolDown = 3f;
				currentProjectiles = projectileAmount;
				FireBurst();
			}
		}
	}

	private void FireBurst()
	{
		GameObject obj = Object.Instantiate(projectile, shootPoint.position, shootPoint.rotation);
		obj.GetComponent<Rigidbody>().AddForce(shootPoint.forward * 2f * eid.totalSpeedModifier, ForceMode.VelocityChange);
		if (obj.TryGetComponent<Projectile>(out var component))
		{
			component.target = player;
			component.speed *= eid.totalSpeedModifier;
			component.damage *= eid.totalDamageModifier;
		}
		currentProjectiles--;
		if (currentProjectiles > 0)
		{
			Invoke("FireBurst", 0.1f * eid.totalSpeedModifier);
		}
	}
}
