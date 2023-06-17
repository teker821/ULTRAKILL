using UnityEngine;
using UnityEngine.Events;

public class AnimationEvent : MonoBehaviour
{
	[SerializeField]
	private UnityEvent onEvent;

	public void TriggerTheEvent()
	{
		onEvent.Invoke();
	}
}
