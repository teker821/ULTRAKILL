using UnityEngine;

public class DoorUnlocker : MonoBehaviour
{
	public Door door;

	public bool open;

	private void OnEnable()
	{
		door.Unlock();
		if (open)
		{
			door.Open(enemy: false, skull: true);
		}
	}
}
