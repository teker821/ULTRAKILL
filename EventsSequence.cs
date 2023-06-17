using UnityEngine;

public class EventsSequence : MonoBehaviour
{
	public float delay = 1f;

	public bool loop;

	public bool startOverOnEnable;

	public UltrakillEvent[] events;

	private bool active;

	private int currentEvent;

	private void Start()
	{
		StartEvents();
	}

	private void OnEnable()
	{
		StartEvents();
	}

	private void OnDisable()
	{
		CancelInvoke("ActivateEvent");
		active = false;
		if (startOverOnEnable)
		{
			currentEvent = 0;
		}
	}

	public void StartEvents()
	{
		if (events.Length == 0)
		{
			Debug.LogError("No events set in EventsSequence on " + base.gameObject.name, base.gameObject);
		}
		else if (!active)
		{
			active = true;
			Invoke("ActivateEvent", delay);
		}
	}

	private void ActivateEvent()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (currentEvent < events.Length)
		{
			events[currentEvent].Invoke();
		}
		if (currentEvent < events.Length - 1)
		{
			currentEvent++;
			Invoke("ActivateEvent", delay);
		}
		else if (loop)
		{
			currentEvent = 0;
			if (delay != 0f)
			{
				Invoke("ActivateEvent", delay);
			}
			else
			{
				Invoke("ActivateEvent", Time.deltaTime);
			}
		}
	}

	public void ChangeDelay(float newDelay)
	{
		delay = newDelay;
	}
}
