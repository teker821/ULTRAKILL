using UnityEngine;

public class ArenaStatus : MonoBehaviour
{
	public int currentStatus;

	public void SetStatus(int i)
	{
		currentStatus = i;
	}

	public void AddToStatus(int i)
	{
		currentStatus += i;
	}
}
