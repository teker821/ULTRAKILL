using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionSpawn : MonoBehaviour
{
	private ParticleSystem part;

	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

	public GameObject toSpawn;

	private void Start()
	{
		part = GetComponent<ParticleSystem>();
	}

	private void OnParticleCollision(GameObject other)
	{
		part.GetCollisionEvents(other, collisionEvents);
		if (collisionEvents.Count > 0)
		{
			Object.Instantiate(toSpawn, collisionEvents[0].intersection, Quaternion.LookRotation(collisionEvents[0].normal));
		}
	}
}
