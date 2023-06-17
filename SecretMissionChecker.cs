using UnityEngine;

public class SecretMissionChecker : MonoBehaviour
{
	public bool primeMission;

	public int secretMission;

	public UltrakillEvent onMissionGet;

	private void Start()
	{
		if (primeMission && GameProgressSaver.GetPrime(MonoSingleton<PrefsManager>.Instance.GetInt("difficulty"), secretMission) == 2)
		{
			onMissionGet.Invoke();
		}
		else if (!primeMission && GameProgressSaver.GetSecretMission(secretMission) == 2)
		{
			onMissionGet.Invoke();
		}
	}
}
