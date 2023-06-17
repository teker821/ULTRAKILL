using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour
{
	public List<int> visitedRooms = new List<int>();

	private int nextRoom;

	private int newRoomChance;

	private int newRoomMinChance = 4;

	public int totalLevels;

	public int rooms;

	public int clearedHallways;

	public int clearedRooms;

	public bool allClear;

	private Text roomAmount;

	private RandomSoundPlayer rsp;

	private bool fadeToFin;

	private void Awake()
	{
		roomAmount = GetComponentInChildren<Text>();
		rsp = GetComponentInChildren<RandomSoundPlayer>();
	}

	private void Update()
	{
		if (fadeToFin)
		{
			AudioSource component = GameObject.FindWithTag("EndingSong").GetComponent<AudioSource>();
			AudioSource component2 = GameObject.FindWithTag("EndingSongReverb").GetComponent<AudioSource>();
			Time.timeScale = 0.1f;
			Time.fixedDeltaTime = 0.002f;
			if (component.volume < 1f)
			{
				component.volume += 10f * Time.deltaTime;
			}
			component2.volume -= 10f * Time.deltaTime;
		}
	}

	public void SwitchRooms(string roomType)
	{
		if (roomType == "Hallway")
		{
			newRoomChance = Random.Range(0, newRoomMinChance);
			if (clearedRooms < rooms && totalLevels >= 4 && newRoomChance == 0)
			{
				int num = 0;
				while (num == 0)
				{
					nextRoom = Random.Range(1, rooms + 1);
					num++;
					foreach (int visitedRoom in visitedRooms)
					{
						if (nextRoom == visitedRoom)
						{
							num = 0;
						}
					}
				}
			}
		}
		else if (roomType == "Room")
		{
			newRoomChance = Random.Range(0, newRoomMinChance);
			if (clearedHallways < rooms && totalLevels >= 4 && newRoomChance == 0)
			{
				int num2 = 0;
				while (num2 == 0)
				{
					nextRoom = Random.Range(rooms + 1, rooms * 2 + 1);
					num2++;
					foreach (int visitedRoom2 in visitedRooms)
					{
						if (nextRoom == visitedRoom2)
						{
							num2 = 0;
						}
					}
				}
			}
		}
		if (clearedRooms == rooms && clearedHallways == rooms && !allClear)
		{
			Application.LoadLevel(rooms * 2 + 1);
			allClear = true;
			rsp.playing = false;
			return;
		}
		if (allClear)
		{
			fadeToFin = true;
			Invoke("EndingStart", 0.1f);
			return;
		}
		if ((newRoomChance > 0 || totalLevels < 4 || clearedRooms == rooms) && roomType == "Hallway")
		{
			Application.LoadLevel(1);
			Invoke("RoomSwitched", 0.1f);
			if (newRoomChance > 0)
			{
				newRoomMinChance--;
			}
			return;
		}
		if ((newRoomChance > 0 || totalLevels < 4 || clearedHallways == rooms) && roomType == "Room")
		{
			Application.LoadLevel(rooms + 1);
			Invoke("RoomSwitched", 0.1f);
			if (newRoomChance > 0)
			{
				newRoomMinChance--;
			}
			return;
		}
		if (roomType == "Hallway")
		{
			clearedRooms++;
		}
		else if (roomType == "Room")
		{
			clearedHallways++;
		}
		visitedRooms.Add(nextRoom);
		Application.LoadLevel(nextRoom);
		Invoke("RoomSwitched", 0.1f);
		newRoomMinChance = Random.Range(3, 6);
	}

	private void RoomSwitched()
	{
		totalLevels++;
		if (totalLevels < 10)
		{
			roomAmount.text = "00" + totalLevels;
		}
		else if (totalLevels < 100)
		{
			roomAmount.text = "0" + totalLevels;
		}
		else if (totalLevels < 1000)
		{
			roomAmount.text = string.Concat(totalLevels);
		}
		else
		{
			roomAmount.text = "???";
		}
		rsp.RollForPlay();
	}

	private void EndingStart()
	{
		Application.LoadLevel(rooms * 2 + 2);
		Object.Destroy(base.gameObject);
	}
}
