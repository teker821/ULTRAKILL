using UnityEngine;

public class RoomArea : MonoBehaviour
{
	public State roomType;

	private RoomManager rm;

	private string roomName;

	private void Awake()
	{
		rm = GameObject.FindWithTag("RoomManager").GetComponent<RoomManager>();
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			ChangeLevel();
		}
	}

	private void ChangeLevel()
	{
		if (roomType == State.Hallway)
		{
			rm.SwitchRooms("Hallway");
		}
		else if (roomType == State.Room)
		{
			rm.SwitchRooms("Room");
		}
	}
}
