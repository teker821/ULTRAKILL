using System.Collections.Generic;
using UnityEngine;

public class FirstRoomPrefab : MonoBehaviour, IPlaceholdableComponent
{
	[HideInInspector]
	public GameObject[] activateOnFirstRoomDoorOpen;

	[HideInInspector]
	public bool levelNameOnOpen = true;

	public Door mainDoor;

	public FinalDoor finalDoor;

	public void WillReplace(GameObject oldObject, GameObject newObject, bool isSelfBeingReplaced)
	{
		if (isSelfBeingReplaced)
		{
			newObject.GetComponent<FirstRoomPrefab>().SwapData(this);
		}
	}

	private void SwapData(FirstRoomPrefab source)
	{
		List<GameObject> list = new List<GameObject>();
		list.AddRange(mainDoor.activatedRooms);
		list.AddRange(source.activateOnFirstRoomDoorOpen);
		mainDoor.activatedRooms = list.ToArray();
		finalDoor.levelNameOnOpen = source.levelNameOnOpen;
	}
}
