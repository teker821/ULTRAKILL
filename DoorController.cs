using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
	public int type;

	private Door dc;

	private bool open;

	private bool playerIn;

	public bool enemyIn;

	public bool reverseDirection;

	public bool dontDeactivateOnAltarControl;

	public List<EnemyIdentifier> doorUsers = new List<EnemyIdentifier>();

	private List<EnemyIdentifier> doorUsersToDelete = new List<EnemyIdentifier>();

	private void Start()
	{
		dc = base.transform.parent.GetComponentInChildren<Door>();
	}

	private void OnDrawGizmos()
	{
		Collider component = GetComponent<Collider>();
		if ((bool)component)
		{
			Bounds bounds = component.bounds;
			Gizmos.color = new Color(0.2f, 0.2f, 1f, 1f);
			Gizmos.DrawWireCube(bounds.center, bounds.size);
			Gizmos.color = new Color(0.2f, 0.2f, 1f, 0.15f);
			Gizmos.DrawCube(bounds.center, bounds.size);
		}
	}

	private void OnDisable()
	{
		if (playerIn && open && !dc.locked)
		{
			Close();
		}
	}

	private void Update()
	{
		if ((playerIn || enemyIn) && !open && !dc.locked)
		{
			open = true;
			if (reverseDirection)
			{
				dc.reverseDirection = true;
			}
			else
			{
				dc.reverseDirection = false;
			}
			if (playerIn)
			{
				dc.Optimize();
			}
			if (type == 0)
			{
				if (!playerIn)
				{
					dc.Open(enemy: true);
				}
				else
				{
					dc.Open();
				}
			}
			else if (type == 1)
			{
				if (!playerIn)
				{
					dc.Open(enemy: true);
				}
				else
				{
					dc.Open();
				}
				Object.Destroy(this);
			}
			else if (type == 2)
			{
				dc.Close();
				Object.Destroy(this);
			}
		}
		else if (open && !dc.locked && !playerIn && !enemyIn)
		{
			Close();
		}
		if (enemyIn && doorUsers.Count > 0)
		{
			foreach (EnemyIdentifier doorUser in doorUsers)
			{
				if (doorUser == null || !doorUser.gameObject.activeInHierarchy || doorUser.dead)
				{
					doorUsersToDelete.Add(doorUser);
				}
			}
			if (doorUsersToDelete.Count > 0)
			{
				foreach (EnemyIdentifier item in doorUsersToDelete)
				{
					doorUsers.Remove(item);
				}
				doorUsersToDelete.Clear();
				if (doorUsers.Count <= 0)
				{
					enemyIn = false;
					if (!playerIn)
					{
						Close();
					}
				}
			}
		}
		if (!playerIn && !enemyIn && dc.transform.localPosition == dc.closedPos)
		{
			open = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			playerIn = true;
		}
		else if (other.gameObject.tag == "Enemy" && !open)
		{
			EnemyIdentifier component = other.gameObject.GetComponent<EnemyIdentifier>();
			if (component != null && !component.dead && component.enemyClass == EnemyClass.Machine && component.enemyType != EnemyType.Drone && !doorUsers.Contains(component))
			{
				enemyIn = true;
				doorUsers.Add(component);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			playerIn = false;
		}
		else
		{
			if (!(other.gameObject.tag == "Enemy"))
			{
				return;
			}
			EnemyIdentifier component = other.gameObject.GetComponent<EnemyIdentifier>();
			if (component != null && component.enemyClass == EnemyClass.Machine && component.enemyType != EnemyType.Drone)
			{
				if (doorUsers.Contains(component))
				{
					doorUsers.Remove(component);
				}
				if (doorUsers.Count <= 0)
				{
					enemyIn = false;
				}
			}
		}
	}

	public void Close()
	{
		open = false;
		dc.Close();
	}

	public void ForcePlayerOut()
	{
		playerIn = false;
	}
}
