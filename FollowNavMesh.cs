using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FollowNavMesh : MonoBehaviour
{
	public Transform target;

	private NavMeshAgent nma;

	public float trackFrequency = 0.1f;

	public bool chaseEnemies;

	public float chaseEnemiesRange = 50f;

	private void Start()
	{
		nma = GetComponent<NavMeshAgent>();
		if (!target)
		{
			target = MonoSingleton<PlayerTracker>.Instance.GetPlayer();
		}
		Invoke("Track", trackFrequency);
	}

	private void Track()
	{
		Invoke("Track", trackFrequency);
		Transform transform = target;
		if (chaseEnemies)
		{
			List<EnemyIdentifier> currentEnemies = MonoSingleton<EnemyTracker>.Instance.GetCurrentEnemies();
			float num = chaseEnemiesRange;
			foreach (EnemyIdentifier item in currentEnemies)
			{
				if (!item.flying && Vector3.Distance(base.transform.position, item.transform.position) < num)
				{
					transform = item.transform;
					num = Vector3.Distance(base.transform.position, item.transform.position);
				}
			}
		}
		if (transform != target)
		{
			nma.stoppingDistance = 0f;
		}
		else
		{
			nma.stoppingDistance = 10f;
		}
		if ((bool)transform && Physics.Raycast(transform.position, Vector3.down, out var _, 50f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			nma.SetDestination(transform.position);
		}
	}
}
