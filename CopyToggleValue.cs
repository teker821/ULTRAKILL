using UnityEngine;
using UnityEngine.UI;

public class CopyToggleValue : MonoBehaviour
{
	public Toggle target;

	private Toggle currentToggle;

	private void Start()
	{
		currentToggle = GetComponent<Toggle>();
		currentToggle.isOn = target.isOn;
	}
}
