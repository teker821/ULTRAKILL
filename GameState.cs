using JetBrains.Annotations;
using UnityEngine;

public class GameState
{
	public string key;

	private bool tracked;

	public LockMode playerInputLock;

	public LockMode cameraInputLock;

	public LockMode cursorLock;

	public float? timerModifier;

	public int priority = 1;

	[CanBeNull]
	public GameObject trackedObject { get; }

	[CanBeNull]
	public GameObject[] trackedObjects { get; }

	public GameState(string key, GameObject trackedObject)
	{
		this.key = key;
		this.trackedObject = trackedObject;
		tracked = trackedObject != null;
	}

	public GameState(string key, GameObject[] trackedObjects)
	{
		this.key = key;
		this.trackedObjects = trackedObjects;
		tracked = trackedObjects != null;
	}

	public GameState(string key)
	{
		this.key = key;
		tracked = false;
	}

	public bool IsValid()
	{
		if (!tracked)
		{
			return true;
		}
		if (trackedObjects != null)
		{
			GameObject[] array = trackedObjects;
			foreach (GameObject gameObject in array)
			{
				if (gameObject != null && gameObject.activeInHierarchy)
				{
					return true;
				}
			}
			return false;
		}
		if (trackedObject != null)
		{
			return trackedObject.activeInHierarchy;
		}
		return false;
	}
}
