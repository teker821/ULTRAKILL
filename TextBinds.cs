using UnityEngine;
using UnityEngine.UI;

public class TextBinds : MonoBehaviour
{
	public string text1;

	public string input;

	public string text2;

	private Text text;

	private void OnEnable()
	{
		if (!this.text)
		{
			this.text = GetComponent<Text>();
		}
		if (input == "")
		{
			this.text.text = text1;
		}
		else
		{
			string text = "";
			KeyCode keyCode = MonoSingleton<InputManager>.Instance.Inputs[input];
			switch (keyCode)
			{
			case KeyCode.Mouse0:
				text = "Left Mouse Button";
				break;
			case KeyCode.Mouse1:
				text = "Right Mouse Button";
				break;
			case KeyCode.Mouse2:
				text = "Middle Mouse Button";
				break;
			case KeyCode.Mouse3:
			case KeyCode.Mouse4:
			case KeyCode.Mouse5:
			case KeyCode.Mouse6:
			{
				text = keyCode.ToString();
				string s = text.Substring(text.Length - 1, 1);
				text = text.Substring(0, text.Length - 1);
				text += int.Parse(s) + 1;
				break;
			}
			default:
				text = keyCode.ToString();
				break;
			}
			text = MonoSingleton<InputManager>.Instance.GetBindingString(input) ?? text;
			this.text.text = text1 + text + text2;
		}
		this.text.text = this.text.text.Replace('$', '\n');
	}
}
