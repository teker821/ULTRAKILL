using UnityEngine;

public class CyberGrindSettingsNavigator : MonoBehaviour
{
	public GameObject menu;

	public GameObject tipPanel;

	public GameObject[] allPanels;

	public void GoTo(GameObject panel)
	{
		GameObject[] array = allPanels;
		foreach (GameObject obj in array)
		{
			obj.SetActive(obj == panel);
		}
		tipPanel.SetActive(value: false);
		menu.SetActive(value: true);
		panel.SetActive(value: true);
	}

	public void GoToNoMenu(GameObject panel)
	{
		GoTo(panel);
		menu.SetActive(value: false);
	}
}
