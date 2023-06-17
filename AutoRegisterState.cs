using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AutoRegisterState : MonoBehaviour
{
	public string stateKey;

	[Space]
	public bool trackSelf = true;

	[Tooltip("If any of the tracked objects remain active, the state will be considered valid")]
	public GameObject[] additionalTrackedObjects;

	[FormerlySerializedAs("playerInputBlocking")]
	[Space]
	public LockMode playerInputLock;

	[FormerlySerializedAs("cameraInputBlocking")]
	public LockMode cameraInputLock;

	public LockMode cursorLock;

	[Space]
	public int priority = 1;

	private GameState ownState;

	private void OnEnable()
	{
		List<GameObject> list = new List<GameObject>();
		if (trackSelf)
		{
			list.Add(base.gameObject);
		}
		if (additionalTrackedObjects != null)
		{
			list.AddRange(additionalTrackedObjects);
		}
		if (ownState == null)
		{
			if (list.Count == 0)
			{
				ownState = new GameState(stateKey);
			}
			else if (list.Count == 1)
			{
				ownState = new GameState(stateKey, list[0]);
			}
			else
			{
				ownState = new GameState(stateKey, list.ToArray());
			}
		}
		ownState.playerInputLock = playerInputLock;
		ownState.cameraInputLock = cameraInputLock;
		ownState.cursorLock = cursorLock;
		ownState.priority = priority;
		GameStateManager.Instance.RegisterState(ownState);
	}

	private void OnDestroy()
	{
		GameStateManager.Instance.PopState(stateKey);
	}
}
