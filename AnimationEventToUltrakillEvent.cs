using UnityEngine;

public class AnimationEventToUltrakillEvent : MonoBehaviour
{
	public int eventNumber;

	public UltrakillEvent onEvent;

	public void ToUltrakillEvent(int num)
	{
		if (num == eventNumber)
		{
			onEvent.Invoke();
		}
	}
}
