using UnityEngine;
using UnityEngine.AI;

public class Wicked : MonoBehaviour
{
	public Transform[] patrolPoints;

	public Transform targetPoint;

	private GameObject player;

	public LayerMask lmask;

	public float playerSpotTime;

	private AudioSource aud;

	private NavMeshAgent nma;

	public GameObject hitSound;

	private bool lineOfSight;

	private EnemyIdentifier eid;

	private void Start()
	{
		player = MonoSingleton<PlayerTracker>.Instance.GetPlayer().gameObject;
		nma = GetComponent<NavMeshAgent>();
		aud = GetComponent<AudioSource>();
		eid = GetComponent<EnemyIdentifier>();
		if (targetPoint == null)
		{
			targetPoint = patrolPoints[Random.Range(0, patrolPoints.Length)];
		}
	}

	private void Update()
	{
		if (playerSpotTime > 0f)
		{
			nma.speed = 22f * eid.totalSpeedModifier;
		}
		else
		{
			nma.speed = 8f * eid.totalSpeedModifier;
		}
		Debug.DrawRay(base.transform.position + Vector3.up * 2f, player.transform.position + Vector3.up * 2f - base.transform.position + Vector3.up * 2f, Color.red);
		if (!Physics.Raycast(base.transform.position + Vector3.up * 2f, player.transform.position - base.transform.position + Vector3.up * 2f, Vector3.Distance(base.transform.position + Vector3.up * 2f, player.transform.position), lmask))
		{
			if (!lineOfSight && !aud.isPlaying)
			{
				aud.Play();
			}
			lineOfSight = true;
			playerSpotTime = 5f;
		}
		else
		{
			lineOfSight = false;
			if (playerSpotTime != 0f)
			{
				playerSpotTime = Mathf.MoveTowards(playerSpotTime, 0f, Time.deltaTime);
			}
		}
		if (playerSpotTime <= 0f)
		{
			if (Vector3.Distance(base.transform.position, targetPoint.position) < 1f)
			{
				targetPoint = patrolPoints[Random.Range(0, patrolPoints.Length)];
			}
			nma.SetDestination(targetPoint.position);
		}
		else
		{
			nma.SetDestination(player.transform.position);
		}
	}

	public void GetHit()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		Object.Instantiate(hitSound, base.transform.position, Quaternion.identity);
		Vector3 position = patrolPoints[0].position;
		float num = 0f;
		for (int i = 0; i < patrolPoints.Length; i++)
		{
			if (Vector3.Distance(patrolPoints[i].position, player.transform.position) > num)
			{
				num = Vector3.Distance(patrolPoints[i].position, player.transform.position);
				position = patrolPoints[i].position;
			}
		}
		if ((bool)eid && eid.hooked)
		{
			Debug.Log("Hooked");
			MonoSingleton<HookArm>.Instance.StopThrow(1f, sparks: true);
		}
		MonoSingleton<BestiaryData>.Instance.SetEnemy(EnemyType.Wicked);
		if (aud.isPlaying)
		{
			aud.Stop();
		}
		nma.Warp(position);
		playerSpotTime = 0f;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Player")
		{
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
			{
				MonoSingleton<NewMovement>.Instance.GetHurt(Mathf.RoundToInt(999f * eid.totalDamageModifier), invincible: false);
			}
			else
			{
				MonoSingleton<PlatformerMovement>.Instance.Explode();
			}
		}
		GetHit();
	}
}
