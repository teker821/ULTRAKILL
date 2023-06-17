using System;
using UnityEngine;
using UnityEngine.UI;

public class Tram : MonoBehaviour
{
	private int currentSpeedStep;

	private float currentSpeed;

	private Rigidbody rb;

	private AudioSource aud;

	public int maxSpeedStep;

	public int minSpeedStep;

	public float speedMultiplier;

	public Image[] speedIndicators;

	public float maxPlayerDistance;

	[Space]
	public TrainTrackPoint currentPoint;

	public TrainTrackPoint nextPoint;

	public TrainTrackPoint previousPoint;

	private Transform player;

	private Bounds tramBounds;

	public GameObject bonkSound;

	private Transform curveTarget;

	private Vector3 noTurnCachedForward;

	private ScreenZone[] screenActivators;

	private bool turnedOn = true;

	private bool isTrackDirty;

	private void Start()
	{
		screenActivators = GetComponentsInChildren<ScreenZone>();
		if ((bool)currentPoint)
		{
			ApplyPointRotation(currentPoint, currentPoint.next);
			base.transform.position = currentPoint.transform.position;
			nextPoint = currentPoint.next;
			previousPoint = currentPoint.previous;
		}
	}

	public void ApplyPointRotation(TrainTrackPoint point, TrainTrackPoint next)
	{
		switch (point.turn)
		{
		case TrainTrackPoint.TurningMethod.None:
			if (currentSpeed > 0f)
			{
				noTurnCachedForward = PointFromTram(next.transform.position) * Vector3.forward;
			}
			else
			{
				noTurnCachedForward = PointFromTram(next.transform.position) * -Vector3.forward;
			}
			Debug.Log("cache set to:");
			Debug.Log(noTurnCachedForward);
			break;
		case TrainTrackPoint.TurningMethod.TurnInstantly:
		{
			Quaternion rotation = Quaternion.LookRotation(next.transform.position - point.transform.position, Vector3.up);
			base.transform.rotation = rotation;
			break;
		}
		case TrainTrackPoint.TurningMethod.CurveToNext:
		{
			if (next.turn == TrainTrackPoint.TurningMethod.CurveToNext)
			{
				curveTarget = next.transform;
				break;
			}
			curveTarget = null;
			Quaternion rotation = Quaternion.LookRotation(next.transform.position - point.transform.position, Vector3.up);
			base.transform.rotation = rotation;
			break;
		}
		}
	}

	private void StopTram(TrainTrackPoint point)
	{
		switch (point.ifLast)
		{
		case TrainTrackPoint.StoppingMethod.StopInstantly:
			if (Mathf.Abs(currentSpeed * speedMultiplier) > 0.1f)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(bonkSound, base.transform.position, Quaternion.identity);
				if ((bool)gameObject)
				{
					AudioSource component = gameObject.GetComponent<AudioSource>();
					if ((bool)component)
					{
						component.volume = Mathf.Abs(currentSpeed * speedMultiplier * 2f);
					}
				}
				CameraController instance = MonoSingleton<CameraController>.Instance;
				if ((bool)instance)
				{
					if (Mathf.Abs(currentSpeed * speedMultiplier * 1.5f) > 2f)
					{
						instance.CameraShake(2f);
					}
					else
					{
						instance.CameraShake(Mathf.Abs(currentSpeed * speedMultiplier * 1.5f));
					}
				}
			}
			currentSpeed = 0f;
			break;
		case TrainTrackPoint.StoppingMethod.StopSlowly:
			currentSpeed = 0f;
			break;
		}
	}

	private Quaternion PointFromTram(Vector3 point)
	{
		return Quaternion.LookRotation(point - base.transform.position, Vector3.up);
	}

	private Quaternion Point(Vector3 point1, Vector3 point2)
	{
		return Quaternion.LookRotation(point2 - point1, Vector3.up);
	}

	private void CheckIfArrived()
	{
		if (currentSpeed > 0f)
		{
			if (PassCheck(forward: true))
			{
				TrainTrackPoint trainTrackPoint = nextPoint;
				base.transform.position = trainTrackPoint.transform.position;
				Debug.Log($"Point {trainTrackPoint.name} has been passed. Next: {(bool)trainTrackPoint.next && trainTrackPoint.next.isAllowed}");
				if ((bool)trainTrackPoint.next && trainTrackPoint.next.isAllowed)
				{
					nextPoint = trainTrackPoint.next;
					previousPoint = trainTrackPoint;
					isTrackDirty = false;
					ApplyPointRotation(trainTrackPoint, trainTrackPoint.next);
					currentPoint = null;
				}
				else
				{
					currentPoint = trainTrackPoint;
					previousPoint = trainTrackPoint.previous;
					nextPoint = trainTrackPoint.next;
					isTrackDirty = false;
					currentSpeedStep = 0;
					UpdateSpeedIndicators();
					StopTram(currentPoint);
				}
			}
		}
		else if (PassCheck(forward: false))
		{
			TrainTrackPoint trainTrackPoint2 = previousPoint;
			base.transform.position = trainTrackPoint2.transform.position;
			Debug.Log("Point " + trainTrackPoint2.name + " has been passed");
			if ((bool)trainTrackPoint2.previous && trainTrackPoint2.previous.isAllowed)
			{
				previousPoint = trainTrackPoint2.previous;
				nextPoint = trainTrackPoint2;
				ApplyPointRotation(trainTrackPoint2.previous, trainTrackPoint2);
				currentPoint = null;
			}
			else
			{
				previousPoint = trainTrackPoint2.previous;
				nextPoint = trainTrackPoint2.next;
				currentPoint = trainTrackPoint2;
				currentSpeedStep = 0;
				UpdateSpeedIndicators();
				StopTram(currentPoint);
			}
		}
	}

	private bool PassCheck(bool forward)
	{
		if (forward)
		{
			if (previousPoint.turn == TrainTrackPoint.TurningMethod.None)
			{
				return DidPassAPoint(nextPoint, -noTurnCachedForward);
			}
			return DidPassAPoint(nextPoint, base.transform.forward);
		}
		if (previousPoint.turn == TrainTrackPoint.TurningMethod.None)
		{
			return DidPassAPoint(previousPoint, noTurnCachedForward);
		}
		return DidPassAPoint(previousPoint, -base.transform.forward);
	}

	private bool DidPassAPoint(TrainTrackPoint point, Vector3 direction)
	{
		Vector3 position = point.transform.position;
		Vector3 position2 = base.transform.position;
		if ((position2.x == position.x || ((direction.x > 0f) ? (position2.x > position.x) : (position2.x < position.x))) && (position2.y == position.y || ((direction.y > 0f) ? (position2.y > position.y) : (position2.y < position.y))))
		{
			if (position2.z != position.z)
			{
				if (!(direction.z > 0f))
				{
					return position2.z < position.z;
				}
				return position2.z > position.z;
			}
			return true;
		}
		return false;
	}

	private void MoveTram()
	{
		if (!((double)Math.Abs(currentSpeed) < 0.0001))
		{
			switch (((!(currentSpeed > 0f)) ? (nextPoint ? nextPoint : currentPoint) : (previousPoint ? previousPoint : currentPoint)).turn)
			{
			case TrainTrackPoint.TurningMethod.None:
				base.transform.position = base.transform.position + -noTurnCachedForward * currentSpeed;
				break;
			case TrainTrackPoint.TurningMethod.TurnInstantly:
				base.transform.position = base.transform.position + base.transform.forward * currentSpeed;
				break;
			case TrainTrackPoint.TurningMethod.CurveToNext:
				base.transform.position = base.transform.position + base.transform.forward * currentSpeed;
				break;
			}
		}
	}

	private void FixedUpdate()
	{
		if (!rb)
		{
			rb = GetComponent<Rigidbody>();
		}
		if (!aud)
		{
			aud = GetComponent<AudioSource>();
		}
		if (!rb)
		{
			return;
		}
		if (currentSpeed > 0f && currentSpeedStep > 0 && (!nextPoint.next || !nextPoint.next.isAllowed) && nextPoint.ifLast == TrainTrackPoint.StoppingMethod.StopSlowly && Vector3.Distance(base.transform.position, nextPoint.transform.position) * Time.fixedDeltaTime < currentSpeed * speedMultiplier)
		{
			currentSpeed = Vector3.Distance(base.transform.position, nextPoint.transform.position) * Time.fixedDeltaTime / speedMultiplier + Time.fixedDeltaTime / 2f;
		}
		if (currentSpeed < 0f && currentSpeedStep < 0 && (!previousPoint.previous || !previousPoint.previous.isAllowed) && previousPoint.ifLast == TrainTrackPoint.StoppingMethod.StopSlowly && Vector3.Distance(base.transform.position, previousPoint.transform.position) * Time.fixedDeltaTime * -1f > currentSpeed * speedMultiplier)
		{
			currentSpeed = Vector3.Distance(base.transform.position, previousPoint.transform.position) * -1f * Time.fixedDeltaTime / speedMultiplier - Time.fixedDeltaTime / 2f;
		}
		else
		{
			currentSpeed = Mathf.MoveTowards(currentSpeed, (float)currentSpeedStep * (speedMultiplier / 10f), speedMultiplier / 10f * Time.fixedDeltaTime);
			if (((currentSpeed > 0f && currentSpeedStep > 0) || (currentSpeed < 0f && currentSpeedStep < 0) || (currentSpeed == 0f && currentSpeedStep == 0)) && isTrackDirty)
			{
				if (currentSpeed == 0f && currentSpeedStep == 0 && (bool)previousPoint)
				{
					currentPoint = previousPoint;
				}
				else if (currentSpeed > 0f)
				{
					currentPoint = previousPoint;
				}
				else if (currentSpeed < 0f)
				{
					currentPoint = nextPoint;
				}
				isTrackDirty = false;
			}
		}
		MoveTram();
		if (currentSpeed != 0f)
		{
			CheckIfArrived();
			if ((bool)previousPoint && previousPoint.turn == TrainTrackPoint.TurningMethod.CurveToNext && (bool)nextPoint && nextPoint.turn == TrainTrackPoint.TurningMethod.CurveToNext)
			{
				Transform transform = null;
				if (currentSpeed < 0f && (bool)previousPoint)
				{
					transform = previousPoint.transform;
				}
				else if (currentSpeed > 0f && (bool)nextPoint)
				{
					transform = nextPoint.transform;
				}
				if (transform != null)
				{
					float num = Vector3.Distance(previousPoint.transform.position, nextPoint.transform.position);
					Quaternion to = Quaternion.LookRotation(transform.position - base.transform.position);
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, currentSpeed / num * 81f);
				}
			}
		}
		if ((bool)aud)
		{
			if (currentSpeed != 0f && !aud.isPlaying)
			{
				aud.Play();
			}
			else if (currentSpeed == 0f && aud.isPlaying)
			{
				aud.Stop();
			}
			float num2 = 0f;
			num2 = ((!(Mathf.Abs(currentSpeed) >= 0.5f)) ? (Mathf.Abs(currentSpeed) * 2f) : 1f);
			aud.volume = num2;
			aud.pitch = num2 * 2f;
		}
		if (!player)
		{
			player = MonoSingleton<NewMovement>.Instance.transform;
		}
		if (maxPlayerDistance != 0f && (bool)player && Vector3.Distance(base.transform.position, player.position) > maxPlayerDistance)
		{
			currentSpeedStep = 0;
			UpdateSpeedIndicators();
		}
	}

	private void OnGUI()
	{
		_ = Debug.isDebugBuild;
	}

	public bool SpeedUp(int amount)
	{
		if (!turnedOn)
		{
			return false;
		}
		if (currentSpeed >= 0f)
		{
			if (!nextPoint)
			{
				return false;
			}
			if (!nextPoint.isAllowed)
			{
				return false;
			}
		}
		else
		{
			if (!previousPoint)
			{
				return false;
			}
			if (!previousPoint.isAllowed)
			{
				return false;
			}
		}
		if (currentSpeedStep < maxSpeedStep)
		{
			if (currentSpeedStep == 0 && currentSpeed == 0f)
			{
				if ((bool)currentPoint && currentPoint != nextPoint)
				{
					previousPoint = currentPoint;
				}
			}
			else
			{
				isTrackDirty = true;
			}
			if (currentSpeedStep + amount <= maxSpeedStep)
			{
				currentSpeedStep += amount;
			}
			else
			{
				currentSpeedStep = maxSpeedStep;
			}
			UpdateSpeedIndicators();
			return true;
		}
		return false;
	}

	public bool SpeedDown(int amount)
	{
		if (!turnedOn)
		{
			return false;
		}
		if (currentSpeed <= 0f)
		{
			if (!previousPoint)
			{
				return false;
			}
			if (!previousPoint.isAllowed)
			{
				return false;
			}
		}
		else
		{
			if (!nextPoint)
			{
				return false;
			}
			if (!nextPoint.isAllowed)
			{
				return false;
			}
		}
		if (currentSpeedStep > minSpeedStep)
		{
			if (currentSpeedStep == 0 && currentSpeed == 0f)
			{
				if ((bool)currentPoint && currentPoint != previousPoint)
				{
					nextPoint = currentPoint;
				}
			}
			else
			{
				isTrackDirty = true;
			}
			if (currentSpeedStep - amount >= minSpeedStep)
			{
				currentSpeedStep -= amount;
			}
			else
			{
				currentSpeedStep = minSpeedStep;
			}
			UpdateSpeedIndicators();
			return true;
		}
		return false;
	}

	public void UpdateSpeedIndicators()
	{
		for (int i = 0; i < speedIndicators.Length; i++)
		{
			if (i == currentSpeedStep - minSpeedStep)
			{
				speedIndicators[i].fillCenter = true;
			}
			else
			{
				speedIndicators[i].fillCenter = false;
			}
		}
	}

	public void InstantStop()
	{
		currentSpeedStep = 0;
		currentSpeed = 0f;
		UpdateSpeedIndicators();
	}

	public void TeleportTramToPoint(TrainTrackPoint point)
	{
		currentPoint = point;
		nextPoint = point.next;
		previousPoint = point.previous;
		base.transform.position = point.transform.position;
		if ((bool)point.next)
		{
			ApplyPointRotation(point, point.next);
		}
		else if ((bool)point.previous)
		{
			ApplyPointRotation(point.previous, point);
		}
	}

	public void ShutDown()
	{
		if (screenActivators != null && screenActivators.Length != 0)
		{
			ScreenZone[] array = screenActivators;
			foreach (ScreenZone screenZone in array)
			{
				ObjectActivator[] components = screenZone.GetComponents<ObjectActivator>();
				if (components != null && components.Length != 0)
				{
					ObjectActivator[] array2 = components;
					foreach (ObjectActivator objectActivator in array2)
					{
						if (objectActivator.events.toActivateObjects != null && objectActivator.events.toActivateObjects.Length != 0)
						{
							GameObject[] toActivateObjects = objectActivator.events.toActivateObjects;
							for (int k = 0; k < toActivateObjects.Length; k++)
							{
								toActivateObjects[k].SetActive(value: false);
							}
						}
					}
				}
				screenZone.gameObject.SetActive(value: false);
			}
		}
		InstantStop();
		turnedOn = false;
	}

	public void TurnOn()
	{
		if (screenActivators != null && screenActivators.Length != 0)
		{
			ScreenZone[] array = screenActivators;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: true);
			}
		}
		turnedOn = true;
	}
}
