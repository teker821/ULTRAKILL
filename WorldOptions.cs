using UnityEngine;
using UnityEngine.UI;

public class WorldOptions : MonoBehaviour
{
	[SerializeField]
	private Image borderIcon;

	[SerializeField]
	private Text borderStatus;

	[SerializeField]
	private Text buttonText;

	[Space]
	[SerializeField]
	private GameObject border;

	private bool isBorderOn = true;

	public void ToggleBorder()
	{
		isBorderOn = !isBorderOn;
		border.SetActive(isBorderOn);
		borderIcon.color = (isBorderOn ? Color.white : new Color(1f, 1f, 1f, 0.3f));
		borderStatus.text = (isBorderOn ? "ENABLED" : "DISABLED");
		buttonText.text = (isBorderOn ? "DISABLE" : "ENABLE");
	}
}
