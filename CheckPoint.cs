using System.Collections.Generic;
using Logic;
using Sandbox.Arm;
using UnityEngine;
using UnityEngine.Events;

public class CheckPoint : MonoBehaviour
{
	[HideInInspector]
	public StatsManager sm;

	[HideInInspector]
	public bool activated;

	public bool forceOff;

	private bool notFirstTime;

	public GameObject toActivate;

	public GameObject[] rooms;

	public List<GameObject> roomsToInherit = new List<GameObject>();

	private List<string> inheritNames = new List<string>();

	private List<Transform> inheritParents = new List<Transform>();

	private GameObject[] roomsPriv;

	[HideInInspector]
	public List<GameObject> defaultRooms = new List<GameObject>();

	public Door[] doorsToUnlock;

	[HideInInspector]
	public List<GameObject> newRooms = new List<GameObject>();

	private int i;

	private GameObject player;

	private NewMovement nm;

	private float tempRot;

	public GameObject graphic;

	public GameObject activateEffect;

	[HideInInspector]
	public int restartKills;

	[HideInInspector]
	public int stylePoints;

	private StyleHUD shud;

	public bool multiUse;

	public bool dontAutoReset;

	public bool startOff;

	public bool unteleportable;

	[HideInInspector]
	public List<int> succesfulHitters = new List<int>();

	[Space]
	public UnityEvent onRestart;

	[HideInInspector]
	public float additionalSpawnRotation;

	private void Start()
	{
		GameObject[] array = rooms;
		foreach (GameObject item in array)
		{
			defaultRooms.Add(item);
		}
		for (int j = 0; j < defaultRooms.Count; j++)
		{
			if (!defaultRooms[j].TryGetComponent<GoreZone>(out var component))
			{
				component = defaultRooms[j].AddComponent<GoreZone>();
			}
			component.checkpoint = this;
			newRooms.Add(Object.Instantiate(defaultRooms[j], defaultRooms[j].transform.position, defaultRooms[j].transform.rotation, defaultRooms[j].transform.parent));
			defaultRooms[j].gameObject.SetActive(value: false);
			newRooms[j].gameObject.SetActive(value: true);
			defaultRooms[j].transform.position = new Vector3(defaultRooms[j].transform.position.x + 10000f, defaultRooms[j].transform.position.y, defaultRooms[j].transform.position.z);
		}
		player = MonoSingleton<NewMovement>.Instance.gameObject;
		sm = MonoSingleton<StatsManager>.Instance;
		if (shud == null)
		{
			shud = MonoSingleton<StyleHUD>.Instance;
		}
		for (int k = 0; k < roomsToInherit.Count; k++)
		{
			inheritNames.Add(roomsToInherit[k].name);
			inheritParents.Add(roomsToInherit[k].transform.parent);
		}
		MonoSingleton<CheckPointsController>.Instance.AddCheckpoint(this);
		if (startOff)
		{
			activated = true;
			graphic?.SetActive(value: false);
			if (TryGetComponent<ModifyMaterial>(out var component2))
			{
				component2.ChangeEmissionIntensity(0f);
			}
		}
	}

	private void Update()
	{
		if (multiUse && activated && !dontAutoReset && Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, base.transform.position) > 15f)
		{
			ReactivateCheckpoint();
		}
		if (!activated && (bool)graphic)
		{
			if (forceOff && graphic.activeSelf)
			{
				graphic.SetActive(value: false);
			}
			else if (!forceOff && !graphic.activeSelf)
			{
				graphic.SetActive(value: true);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!activated && !forceOff && other.gameObject.tag == "Player")
		{
			ActivateCheckPoint();
		}
	}

	public void ActivateCheckPoint()
	{
		sm = MonoSingleton<StatsManager>.Instance;
		sm.currentCheckPoint = this;
		activated = true;
		if ((bool)activateEffect)
		{
			Object.Instantiate(activateEffect, MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, Quaternion.identity);
		}
		if (!multiUse || !notFirstTime)
		{
			MonoSingleton<NewMovement>.Instance.sameCheckpointRestarts = 0;
		}
		if ((bool)graphic)
		{
			if (multiUse)
			{
				graphic.SetActive(value: false);
			}
			else
			{
				Object.Destroy(graphic);
			}
		}
		if ((bool)MonoSingleton<PlatformerMovement>.Instance)
		{
			MonoSingleton<CrateCounter>.Instance.SaveStuff();
		}
		if ((bool)MonoSingleton<MapVarManager>.Instance)
		{
			MonoSingleton<MapVarManager>.Instance.StashStore();
		}
		stylePoints = sm.stylePoints;
		restartKills = 0;
		if (notFirstTime)
		{
			defaultRooms.Clear();
			newRooms.Clear();
			if (rooms.Length != 0)
			{
				GameObject[] array = rooms;
				foreach (GameObject gameObject in array)
				{
					roomsToInherit.Add(gameObject);
					inheritNames.Add(gameObject.name);
					inheritParents.Add(gameObject.transform.parent);
				}
				rooms = new GameObject[0];
			}
		}
		if (shud == null)
		{
			shud = MonoSingleton<StyleHUD>.Instance;
		}
		if (roomsToInherit.Count == 0)
		{
			return;
		}
		for (int j = 0; j < roomsToInherit.Count; j++)
		{
			string text = inheritNames[j];
			text = text.Replace("(Clone)", "");
			GameObject gameObject2 = null;
			for (int num = inheritParents[j].childCount - 1; num >= 0; num--)
			{
				GameObject gameObject3 = inheritParents[j].GetChild(num).gameObject;
				if (gameObject3.name.Replace("(Clone)", "") == text)
				{
					if (gameObject2 == null)
					{
						gameObject2 = gameObject3;
					}
					else
					{
						Object.Destroy(gameObject3);
					}
				}
			}
			InheritRoom(gameObject2);
		}
	}

	public void OnRespawn()
	{
		if (player == null)
		{
			player = MonoSingleton<NewMovement>.Instance.gameObject;
		}
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		if (player.GetComponentInParent<GoreZone>() != null)
		{
			player.transform.parent = null;
		}
		player.transform.position = Vector3.one * -1000f;
		if ((bool)MonoSingleton<PlatformerMovement>.Instance)
		{
			if (MonoSingleton<PlatformerMovement>.Instance.GetComponentInParent<GoreZone>() != null)
			{
				MonoSingleton<PlatformerMovement>.Instance.transform.parent = null;
			}
			MonoSingleton<PlatformerMovement>.Instance.transform.position = Vector3.one * -1000f;
		}
		if ((bool)MonoSingleton<MapVarManager>.Instance)
		{
			MonoSingleton<MapVarManager>.Instance.RestoreStashedStore();
		}
		this.i = 0;
		if ((bool)SandboxArm.debugZone && !MapInfoBase.InstanceAnyType.sandboxTools)
		{
			Object.Destroy(SandboxArm.debugZone.gameObject);
		}
		if (!activated && multiUse)
		{
			activated = true;
			graphic?.SetActive(value: false);
		}
		sm.kills -= restartKills;
		restartKills = 0;
		sm.stylePoints = stylePoints;
		if (succesfulHitters.Count > 0)
		{
			KillHitterCache instance = MonoSingleton<KillHitterCache>.Instance;
			if ((bool)instance && !instance.ignoreRestarts)
			{
				foreach (int succesfulHitter in succesfulHitters)
				{
					instance.RemoveId(succesfulHitter);
				}
			}
		}
		if (shud == null)
		{
			shud = MonoSingleton<StyleHUD>.Instance;
		}
		shud.ComboOver();
		shud.ResetAllFreshness();
		if (doorsToUnlock.Length != 0)
		{
			Door[] array = doorsToUnlock;
			foreach (Door door in array)
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
		DestroyOnCheckpointRestart[] array2 = Object.FindObjectsOfType<DestroyOnCheckpointRestart>();
		if (array2 != null && array2.Length != 0)
		{
			DestroyOnCheckpointRestart[] array3 = array2;
			foreach (DestroyOnCheckpointRestart destroyOnCheckpointRestart in array3)
			{
				if (destroyOnCheckpointRestart.gameObject.activeInHierarchy && !destroyOnCheckpointRestart.dontDestroy)
				{
					Object.Destroy(destroyOnCheckpointRestart.gameObject);
				}
			}
		}
		Harpoon[] array4 = Object.FindObjectsOfType<Harpoon>();
		if (array4 != null && array4.Length != 0)
		{
			Harpoon[] array5 = array4;
			foreach (Harpoon harpoon in array5)
			{
				if (harpoon.gameObject.activeInHierarchy)
				{
					TimeBomb componentInChildren = harpoon.GetComponentInChildren<TimeBomb>();
					if ((bool)componentInChildren)
					{
						componentInChildren.dontExplode = true;
					}
					Object.Destroy(harpoon.gameObject);
				}
			}
		}
		DoorController[] array6 = Object.FindObjectsOfType<DoorController>();
		if (array6 != null && array6.Length != 0)
		{
			DoorController[] array7 = array6;
			for (int i = 0; i < array7.Length; i++)
			{
				array7[i].ForcePlayerOut();
			}
		}
		if ((bool)MonoSingleton<CoinList>.Instance && MonoSingleton<CoinList>.Instance.revolverCoinsList.Count > 0)
		{
			for (int num = MonoSingleton<CoinList>.Instance.revolverCoinsList.Count - 1; num >= 0; num--)
			{
				if (!MonoSingleton<CoinList>.Instance.revolverCoinsList[num].dontDestroyOnPlayerRespawn)
				{
					Object.Destroy(MonoSingleton<CoinList>.Instance.revolverCoinsList[num].gameObject);
					MonoSingleton<CoinList>.Instance.revolverCoinsList.RemoveAt(num);
				}
			}
		}
		if (newRooms.Count > 0)
		{
			ResetRoom();
		}
	}

	public void ResetRoom()
	{
		Vector3 position = newRooms[i].transform.position;
		newRooms[i].SetActive(value: false);
		Object.Destroy(newRooms[i]);
		newRooms[i] = Object.Instantiate(defaultRooms[i], position, defaultRooms[i].transform.rotation, defaultRooms[i].transform.parent);
		newRooms[i].SetActive(value: true);
		if (i + 1 < defaultRooms.Count)
		{
			i++;
			ResetRoom();
			return;
		}
		if ((bool)toActivate)
		{
			toActivate.SetActive(value: true);
		}
		onRestart?.Invoke();
		if (!activated && multiUse)
		{
			activated = true;
			if ((bool)graphic)
			{
				graphic.SetActive(value: false);
			}
		}
		player.transform.position = base.transform.position + base.transform.right * 0.1f + Vector3.up * 1.25f;
		player.GetComponent<Rigidbody>().velocity = Vector3.zero;
		if (nm == null)
		{
			nm = MonoSingleton<NewMovement>.Instance;
		}
		float num = base.transform.rotation.eulerAngles.y + 0.1f + additionalSpawnRotation;
		if ((bool)player && (bool)player.transform.parent && player.transform.parent.gameObject.tag == "Moving")
		{
			num -= player.transform.parent.rotation.eulerAngles.y;
		}
		if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
		{
			nm.cc.ResetCamera(num);
		}
		else
		{
			MonoSingleton<PlatformerMovement>.Instance.ResetCamera(num);
		}
		MonoSingleton<CameraController>.Instance.activated = true;
		if (!nm.enabled)
		{
			nm.enabled = true;
		}
		nm.Respawn();
		nm.GetHealth(0, silent: true);
		nm.cc.StopShake();
		nm.ActivatePlayer();
		if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
		{
			MonoSingleton<PlatformerMovement>.Instance.transform.position = base.transform.position;
			MonoSingleton<PlatformerMovement>.Instance.rb.velocity = Vector3.zero;
			MonoSingleton<PlatformerMovement>.Instance.playerModel.rotation = base.transform.rotation;
			if (additionalSpawnRotation != 0f)
			{
				MonoSingleton<PlatformerMovement>.Instance.playerModel.Rotate(Vector3.up, additionalSpawnRotation);
			}
			MonoSingleton<PlatformerMovement>.Instance.gameObject.SetActive(value: true);
			MonoSingleton<PlatformerMovement>.Instance.SnapCamera();
			MonoSingleton<PlatformerMovement>.Instance.Respawn();
			MonoSingleton<CrateCounter>.Instance.ResetUnsavedStuff();
		}
	}

	public void UpdateRooms()
	{
		Vector3 position = newRooms[i].transform.position;
		Object.Destroy(newRooms[i]);
		newRooms[i] = Object.Instantiate(defaultRooms[i], position, defaultRooms[i].transform.rotation, defaultRooms[i].transform.parent);
		newRooms[i].SetActive(value: true);
		if (i + 1 < defaultRooms.Count)
		{
			i++;
			UpdateRooms();
		}
		else
		{
			i = 0;
		}
	}

	public void InheritRoom(GameObject targetRoom)
	{
		new List<GameObject>();
		new List<GameObject>();
		defaultRooms.Add(targetRoom);
		int index = defaultRooms.IndexOf(targetRoom);
		defaultRooms[index].GetComponent<GoreZone>().checkpoint = this;
		RemoveOnTime[] componentsInChildren = defaultRooms[index].GetComponentsInChildren<RemoveOnTime>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.SetActive(value: false);
		}
		newRooms.Add(Object.Instantiate(defaultRooms[index], defaultRooms[index].transform.position, defaultRooms[index].transform.rotation, defaultRooms[index].transform.parent));
		defaultRooms[index].gameObject.SetActive(value: false);
		newRooms[index].gameObject.SetActive(value: true);
		defaultRooms[index].transform.position = new Vector3(defaultRooms[index].transform.position.x + 10000f, defaultRooms[index].transform.position.y, defaultRooms[index].transform.position.z);
	}

	public void ReactivateCheckpoint()
	{
		if (multiUse)
		{
			activated = false;
			notFirstTime = true;
			ReactivationEffect();
		}
	}

	public void ReactivationEffect()
	{
		if (multiUse && !activated && (bool)graphic)
		{
			graphic.SetActive(value: true);
			if (graphic.TryGetComponent<ScaleTransform>(out var _))
			{
				graphic.transform.localScale = new Vector3(graphic.transform.localScale.x, 0f, graphic.transform.localScale.z);
			}
			if (graphic.TryGetComponent<AudioSource>(out var component2))
			{
				component2.Play();
			}
		}
	}

	public void ApplyCurrentStyleAndKills()
	{
		ApplyCurrentKills();
		ApplyCurrentStyle();
	}

	public void ApplyCurrentKills()
	{
		restartKills = 0;
	}

	public void ApplyCurrentStyle()
	{
		stylePoints = sm.stylePoints;
	}

	public void AddCustomKill()
	{
		MonoSingleton<StatsManager>.Instance.kills++;
		restartKills++;
	}

	public void ChangeSpawnRotation(float degrees)
	{
		additionalSpawnRotation = degrees;
	}
}
