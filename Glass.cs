using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Glass : MonoBehaviour
{
	public bool broken;

	public bool wall;

	private Transform[] glasses;

	public GameObject shatterParticle;

	public AudioClip scream;

	private StyleHUD shud;

	private int kills;

	private bool playerOn;

	private Collider[] cols;

	private List<GameObject> droppedEnemies = new List<GameObject>();

	public void Shatter()
	{
		cols = GetComponentsInChildren<Collider>();
		base.gameObject.layer = 17;
		broken = true;
		Collider[] array;
		if (playerOn)
		{
			if (!wall)
			{
				MonoSingleton<NewMovement>.Instance.GetComponentInChildren<GroundCheck>().onGround = false;
			}
			else
			{
				WallCheck componentInChildren = MonoSingleton<NewMovement>.Instance.GetComponentInChildren<WallCheck>();
				array = cols;
				foreach (Collider item in array)
				{
					if (componentInChildren.cols.Contains(item))
					{
						componentInChildren.cols.Remove(item);
					}
				}
			}
		}
		glasses = base.transform.GetComponentsInChildren<Transform>();
		Transform[] array2 = glasses;
		foreach (Transform transform in array2)
		{
			if (transform.gameObject != base.gameObject)
			{
				Object.Destroy(transform.gameObject);
			}
		}
		array = cols;
		foreach (Collider collider in array)
		{
			if (!collider.isTrigger)
			{
				collider.enabled = false;
			}
		}
		Invoke("BecomeObstacle", 0.5f);
		Object.Instantiate(shatterParticle, base.transform);
	}

	private void OnTriggerStay(Collider other)
	{
		if (broken && !wall && other.gameObject.tag == "Enemy" && !droppedEnemies.Contains(other.gameObject))
		{
			droppedEnemies.Add(other.gameObject);
			EnemyIdentifier component = other.GetComponent<EnemyIdentifier>();
			if (component.enemyClass == EnemyClass.Husk)
			{
				component.GetComponentInChildren<GroundCheckEnemy>().onGround = false;
			}
			kills++;
		}
		if (!broken && other.gameObject.tag == "Player")
		{
			playerOn = true;
		}
	}

	private void OnCollisionStay(Collision other)
	{
		if (!broken && other.gameObject.tag == "Player")
		{
			playerOn = true;
		}
	}

	private void OnCollisionExit(Collision other)
	{
		if (!broken && other.gameObject.tag == "Player")
		{
			playerOn = false;
		}
	}

	private void BecomeObstacle()
	{
		NavMeshObstacle component = GetComponent<NavMeshObstacle>();
		if (wall)
		{
			component.carving = false;
			component.enabled = false;
		}
		else
		{
			component.enabled = true;
			Collider[] array = cols;
			foreach (Collider collider in array)
			{
				if (!collider.isTrigger)
				{
					collider.enabled = false;
				}
			}
		}
		if (kills >= 3)
		{
			Debug.Log("Got Glass kills " + kills);
			StatsManager instance = MonoSingleton<StatsManager>.Instance;
			if (instance.maxGlassKills < kills)
			{
				instance.maxGlassKills = kills;
			}
		}
	}
}
