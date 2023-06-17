using UnityEngine;

public class SetPlayerPref : MonoBehaviour
{
	public string playerPref;

	public int intValue;

	private void Start()
	{
		PlayerPrefs.SetInt(playerPref, intValue);
	}
}
