using UnityEngine;
using UnityEngine.UI;

public class GearCheckText : MonoBehaviour
{
	public string gearName;

	private Text target;

	private string originalName;

	private void OnEnable()
	{
		if (!target)
		{
			target = GetComponent<Text>();
			originalName = target.text;
		}
		if (GameProgressSaver.CheckGear(gearName) == 0)
		{
			target.text = "???";
		}
		else
		{
			target.text = originalName;
		}
	}
}
