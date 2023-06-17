using System.Collections.Generic;
using UnityEngine;

public class ItemPlaceZone : MonoBehaviour
{
	private bool acceptedItemPlaced;

	public ItemType acceptedItemType;

	public GameObject[] activateOnSuccess;

	public GameObject[] deactivateOnSuccess;

	public GameObject[] activateOnFailure;

	public Door[] doors;

	public Door[] reverseDoors;

	public ArenaStatus[] arenaStatuses;

	public ArenaStatus[] reverseArenaStatuses;

	private Collider col;

	private List<Bounds> doorsBounds = new List<Bounds>();

	private List<Bounds> reverseDoorsBounds = new List<Bounds>();

	public GameObject boundsError;

	private void Start()
	{
		col = GetComponent<Collider>();
		ColorDoors(doors);
		ColorDoors(reverseDoors);
		CheckItem(prelim: true);
	}

	private void Awake()
	{
		GetDoorBounds(doors, doorsBounds);
		GetDoorBounds(reverseDoors, reverseDoorsBounds);
	}

	private void GetDoorBounds(Door[] doors, List<Bounds> boundies)
	{
		if (doors.Length == 0)
		{
			return;
		}
		for (int i = 0; i < doors.Length; i++)
		{
			if (doors[i].ignoreHookCheck)
			{
				continue;
			}
			List<Collider> list = new List<Collider>();
			if (doors[i].doorType == DoorType.Normal)
			{
				Collider[] componentsInChildren = doors[i].GetComponentsInChildren<Collider>();
				foreach (Collider item in componentsInChildren)
				{
					list.Add(item);
				}
			}
			else if (doors[i].doorType == DoorType.BigDoorController)
			{
				BigDoor[] componentsInChildren2 = doors[i].GetComponentsInChildren<BigDoor>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					Collider[] componentsInChildren = componentsInChildren2[j].GetComponentsInChildren<Collider>();
					foreach (Collider item2 in componentsInChildren)
					{
						list.Add(item2);
					}
				}
			}
			else if (doors[i].doorType == DoorType.SubDoorController)
			{
				SubDoor[] componentsInChildren3 = doors[i].GetComponentsInChildren<SubDoor>();
				for (int j = 0; j < componentsInChildren3.Length; j++)
				{
					Collider[] componentsInChildren = componentsInChildren3[j].GetComponentsInChildren<Collider>();
					foreach (Collider item3 in componentsInChildren)
					{
						list.Add(item3);
					}
				}
			}
			if (list.Count > 0)
			{
				Bounds bounds = list[0].bounds;
				if (list.Count > 1)
				{
					for (int l = 1; l < list.Count; l++)
					{
						bounds.Encapsulate(list[l].bounds);
					}
				}
				boundies.Add(bounds);
			}
			else
			{
				boundies.Add(new Bounds(Vector3.zero, Vector3.zero));
			}
		}
	}

	public bool CheckDoorBounds(Vector3 origin, Vector3 end, bool reverseBounds)
	{
		bool result = true;
		foreach (Bounds item in reverseBounds ? reverseDoorsBounds : doorsBounds)
		{
			if (item.IntersectRay(new Ray(origin, end - origin), out var distance) && distance < Vector3.Distance(origin, end) + 1f)
			{
				Object.Instantiate(boundsError, item.center, Quaternion.identity).transform.localScale = item.size * 1.1f;
				result = false;
			}
		}
		return result;
	}

	private void ColorDoors(Door[] drs)
	{
		foreach (Door door in drs)
		{
			switch (acceptedItemType)
			{
			case ItemType.SkullBlue:
				door.defaultLightsColor = new Color(0f, 0.75f, 1f);
				break;
			case ItemType.SkullRed:
				door.defaultLightsColor = new Color(1f, 0.2f, 0.2f);
				break;
			case ItemType.SkullGreen:
				door.defaultLightsColor = new Color(0.25f, 1f, 0.25f);
				break;
			}
			if (!door.noPass || !door.noPass.activeSelf)
			{
				door.ChangeColor(door.defaultLightsColor);
			}
			door.AltarControlled();
		}
	}

	public void CheckItem(bool prelim = false)
	{
		ItemIdentifier componentInChildren = GetComponentInChildren<ItemIdentifier>();
		GameObject[] array;
		Door[] array2;
		if (componentInChildren != null)
		{
			if (componentInChildren.itemType == acceptedItemType)
			{
				acceptedItemPlaced = true;
				componentInChildren.ipz = this;
				array = activateOnSuccess;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: true);
				}
				array = deactivateOnSuccess;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: false);
				}
				array2 = doors;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].Open(enemy: false, skull: true);
				}
				array2 = reverseDoors;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].Close();
				}
				if (!prelim)
				{
					ArenaStatus[] array3 = arenaStatuses;
					for (int i = 0; i < array3.Length; i++)
					{
						array3[i].currentStatus++;
					}
					array3 = reverseArenaStatuses;
					for (int i = 0; i < array3.Length; i++)
					{
						array3[i].currentStatus--;
					}
				}
			}
			else
			{
				array = activateOnFailure;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: true);
				}
			}
			if ((bool)col)
			{
				col.enabled = false;
			}
			return;
		}
		if ((bool)col)
		{
			col.enabled = true;
		}
		if (!prelim && !acceptedItemPlaced)
		{
			return;
		}
		array = activateOnSuccess;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		array = activateOnFailure;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		array = deactivateOnSuccess;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
		array2 = doors;
		foreach (Door door in array2)
		{
			if (door.doorType != 0 || door.transform.localPosition != door.closedPos)
			{
				door.Close();
			}
		}
		array2 = reverseDoors;
		foreach (Door door2 in array2)
		{
			if (door2.doorType != 0 || door2.transform.localPosition != door2.closedPos + door2.openPos)
			{
				door2.Open(enemy: false, skull: true);
			}
		}
		if (!prelim)
		{
			acceptedItemPlaced = false;
			ArenaStatus[] array3 = arenaStatuses;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].currentStatus--;
			}
			array3 = reverseArenaStatuses;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].currentStatus++;
			}
		}
	}
}
