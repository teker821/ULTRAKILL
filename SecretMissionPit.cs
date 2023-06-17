using UnityEngine;

public class SecretMissionPit : MonoBehaviour
{
	public int missionNumber;

	public bool halfUnlock;

	public bool primeMission;

	private bool done;

	private void OnTriggerEnter(Collider other)
	{
		if (done || !(other.gameObject.tag == "Player"))
		{
			return;
		}
		done = true;
		if (primeMission)
		{
			if (halfUnlock)
			{
				GameProgressSaver.SetPrime(missionNumber, 1);
			}
			else
			{
				GameProgressSaver.SetPrime(missionNumber, 2);
			}
		}
		else if (halfUnlock)
		{
			GameProgressSaver.FoundSecretMission(missionNumber);
		}
		else
		{
			GameProgressSaver.SetSecretMission(missionNumber);
		}
	}
}
