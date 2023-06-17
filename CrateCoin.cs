using UnityEngine;

public class CrateCoin : MonoBehaviour
{
	[SerializeField]
	private GameObject getEffect;

	private bool caught;

	private float speed = 25f;

	private Vector3 startDirection;

	private void Start()
	{
		startDirection = Random.insideUnitSphere;
		speed = Random.Range(20, 35);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			caught = true;
		}
	}

	private void Update()
	{
		if (caught)
		{
			speed += Time.deltaTime * 25f;
			base.transform.position = Vector3.MoveTowards(base.transform.position, MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, Time.deltaTime * speed);
			if (Vector3.Distance(base.transform.position, MonoSingleton<PlayerTracker>.Instance.GetPlayer().position) < 1f)
			{
				caught = false;
				MonoSingleton<CrateCounter>.Instance?.AddCoin();
				if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
				{
					Object.Instantiate(getEffect, base.transform.position, Quaternion.identity);
				}
				else
				{
					MonoSingleton<PlatformerMovement>.Instance.CoinGet();
				}
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			if (speed > 0f)
			{
				speed = Mathf.MoveTowards(speed, 0f, Time.deltaTime * 50f);
			}
			base.transform.position += startDirection * speed * Time.deltaTime;
		}
	}
}
