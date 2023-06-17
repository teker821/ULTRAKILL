using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class PlayerMovementParenting : MonoSingleton<PlayerMovementParenting>
{
	public Transform deltaReceiver;

	private Vector3 lastTrackedPos;

	private float lastAngle;

	private Transform playerTracker;

	private List<Transform> trackedObjects = new List<Transform>();

	public Vector3 currentDelta { get; private set; }

	public List<Transform> TrackedObjects => trackedObjects;

	private new void Awake()
	{
		if (deltaReceiver == null)
		{
			deltaReceiver = base.transform;
		}
	}

	private void FixedUpdate()
	{
		currentDelta = Vector3.zero;
		if (playerTracker == null)
		{
			return;
		}
		Vector3 position = playerTracker.transform.position;
		float y = playerTracker.transform.eulerAngles.y;
		Vector3 vector = position - lastTrackedPos;
		lastTrackedPos = position;
		bool flag = true;
		if ((bool)MonoSingleton<NewMovement>.Instance && (bool)MonoSingleton<NewMovement>.Instance.groundProperties && MonoSingleton<NewMovement>.Instance.groundProperties.dontRotateCamera)
		{
			flag = false;
		}
		float num = y - lastAngle;
		lastAngle = y;
		float num2 = Mathf.Abs(num);
		if (num2 > 180f)
		{
			num2 = 360f - num2;
		}
		if (num2 > 5f)
		{
			DetachPlayer();
			return;
		}
		if (vector.magnitude > 2f)
		{
			DetachPlayer();
			return;
		}
		deltaReceiver.position += vector;
		playerTracker.transform.position = deltaReceiver.position;
		lastTrackedPos = playerTracker.transform.position;
		currentDelta = vector;
		if (flag)
		{
			MonoSingleton<CameraController>.Instance.rotationY += num;
		}
	}

	public bool IsPlayerTracking()
	{
		return playerTracker != null;
	}

	public bool IsObjectTracked(Transform other)
	{
		return trackedObjects.Contains(other);
	}

	public void AttachPlayer(Transform other)
	{
		trackedObjects.Add(other);
		GameObject obj = new GameObject("Player Position Proxy");
		obj.transform.parent = other;
		obj.transform.position = deltaReceiver.position;
		obj.transform.rotation = deltaReceiver.rotation;
		GameObject gameObject = obj;
		lastTrackedPos = gameObject.transform.position;
		lastAngle = gameObject.transform.eulerAngles.y;
		if (playerTracker != null)
		{
			Object.Destroy(playerTracker.gameObject);
		}
		playerTracker = gameObject.transform;
		ClearNulls();
	}

	public void DetachPlayer([CanBeNull] Transform other = null)
	{
		if (other == null)
		{
			trackedObjects.Clear();
		}
		else
		{
			trackedObjects.Remove(other);
		}
		if (trackedObjects.Count == 0)
		{
			Object.Destroy(playerTracker.gameObject);
			playerTracker = null;
		}
		else
		{
			playerTracker.SetParent(trackedObjects.First());
		}
		ClearNulls();
	}

	private void ClearNulls()
	{
		for (int num = trackedObjects.Count - 1; num >= 0; num--)
		{
			if (trackedObjects[num] == null)
			{
				trackedObjects.RemoveAt(num);
			}
		}
	}
}
