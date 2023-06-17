using UnityEngine;
using UnityEngine.UI;

public class SecretMissionPanel : MonoBehaviour
{
	public LayerSelect layerSelect;

	public int missionNumber;

	private void Start()
	{
		GotEnabled();
	}

	private void OnEnable()
	{
		GotEnabled();
	}

	public void GotEnabled()
	{
		int secretMission = GameProgressSaver.GetSecretMission(missionNumber);
		Debug.Log("Secret Mission Status: " + secretMission);
		switch (secretMission)
		{
		case 2:
			GetComponent<Image>().fillCenter = true;
			GetComponentInChildren<Text>().color = Color.black;
			GetComponent<Button>().interactable = true;
			layerSelect.SecretMissionDone();
			break;
		case 1:
			GetComponent<Image>().fillCenter = false;
			GetComponentInChildren<Text>().color = Color.white;
			GetComponent<Button>().interactable = true;
			break;
		default:
			GetComponent<Image>().fillCenter = false;
			GetComponentInChildren<Text>().color = new Color(0.5f, 0.5f, 0.5f);
			GetComponent<Button>().interactable = false;
			break;
		}
	}
}
