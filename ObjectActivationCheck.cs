using UnityEngine;

public class ObjectActivationCheck : MonoBehaviour
{
	public bool readyToActivate;

	public void StateChange(bool state)
	{
		readyToActivate = state;
	}
}
