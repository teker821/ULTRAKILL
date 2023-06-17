using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeleportCheat : MonoBehaviour
{
	private class TeleportTarget
	{
		public string overrideName;

		public CheckPoint checkpoint;

		public FirstRoomPrefab firstRoom;

		public Transform target;
	}

	[SerializeField]
	private GameObject buttonTemplate;

	[SerializeField]
	private Color checkpointColor;

	[SerializeField]
	private Color roomColor;

	private void Start()
	{
		GenerateList();
	}

	private void GenerateList()
	{
		List<TeleportTarget> list = new List<TeleportTarget>();
		FirstRoomPrefab firstRoom = Object.FindObjectOfType<FirstRoomPrefab>();
		if ((bool)firstRoom)
		{
			list.Add(new TeleportTarget
			{
				overrideName = "First Room",
				target = firstRoom.transform,
				firstRoom = firstRoom
			});
		}
		CheckPoint[] array = Object.FindObjectsOfType<CheckPoint>();
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].unteleportable)
			{
				list.Add(new TeleportTarget
				{
					target = array[i].transform,
					checkpoint = array[i]
				});
			}
		}
		foreach (TeleportTarget point in list)
		{
			GameObject obj = Object.Instantiate(buttonTemplate, buttonTemplate.transform.parent);
			obj.GetComponentInChildren<Text>().text = ((!string.IsNullOrEmpty(point.overrideName)) ? point.overrideName : (point.checkpoint ? (point.checkpoint.toActivate ? ImproveCheckpointName(point.checkpoint.toActivate.name) : "<color=red>Missing toActivate</color>") : point.target.name));
			obj.GetComponentInChildren<Text>().color = (point.checkpoint ? checkpointColor : roomColor);
			obj.GetComponentInChildren<Button>().onClick.AddListener(delegate
			{
				Teleport(point.target);
				if ((bool)point.checkpoint)
				{
					point.checkpoint.toActivate.SetActive(value: true);
					if (point.checkpoint.doorsToUnlock.Length != 0)
					{
						Door[] doorsToUnlock = point.checkpoint.doorsToUnlock;
						foreach (Door door in doorsToUnlock)
						{
							if (door.locked)
							{
								door.Unlock();
							}
							if (door.startOpen)
							{
								door.Open();
							}
						}
					}
					if (point.checkpoint.newRooms.Count > 0 && (MonoSingleton<StatsManager>.Instance.currentCheckPoint == point.checkpoint || (!point.checkpoint.multiUse && !point.checkpoint.activated)))
					{
						point.checkpoint.ResetRoom();
					}
					point.checkpoint.onRestart?.Invoke();
				}
				if ((bool)firstRoom)
				{
					GameObject[] activatedRooms = firstRoom.mainDoor.activatedRooms;
					foreach (GameObject gameObject in activatedRooms)
					{
						if (gameObject != null)
						{
							gameObject.SetActive(value: true);
						}
					}
				}
			});
			obj.SetActive(value: true);
		}
		buttonTemplate.SetActive(value: false);
	}

	private void Update()
	{
		if (MonoSingleton<InputManager>.Instance.InputSource.Pause.WasPerformedThisFrame)
		{
			base.gameObject.SetActive(value: false);
			MonoSingleton<OptionsManager>.Instance.UnFreeze();
		}
	}

	private string ImproveCheckpointName(string original)
	{
		if (!original.Contains("- "))
		{
			return original;
		}
		return original.Split('-')[^1];
	}

	private void Teleport(Transform target)
	{
		MonoSingleton<NewMovement>.Instance.transform.position = target.position + target.right * 0.1f + Vector3.up * 1.25f;
		float num = target.rotation.eulerAngles.y + 0.1f;
		if ((bool)MonoSingleton<NewMovement>.Instance.transform.parent && MonoSingleton<NewMovement>.Instance.transform.parent.gameObject.tag == "Moving")
		{
			num -= MonoSingleton<NewMovement>.Instance.transform.parent.rotation.eulerAngles.y;
		}
		MonoSingleton<CameraController>.Instance.ResetCamera(num);
		if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
		{
			MonoSingleton<PlatformerMovement>.Instance.transform.position = target.position;
			MonoSingleton<PlatformerMovement>.Instance.rb.velocity = Vector3.zero;
			MonoSingleton<PlatformerMovement>.Instance.playerModel.rotation = target.rotation;
			MonoSingleton<PlatformerMovement>.Instance.SnapCamera();
		}
		base.gameObject.SetActive(value: false);
		MonoSingleton<OptionsManager>.Instance.UnFreeze();
		MonoSingleton<OutdoorLightMaster>.Instance?.FirstDoorOpen();
		MonoSingleton<PlayerTracker>.Instance.LevelStart();
	}
}
