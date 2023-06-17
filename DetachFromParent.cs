using UnityEngine;

public class DetachFromParent : MonoBehaviour
{
	public bool detachOnStart;

	private void Start()
	{
		if (detachOnStart)
		{
			Detach();
		}
	}

	public void Detach()
	{
		base.transform.SetParent(null, worldPositionStays: true);
	}
}
