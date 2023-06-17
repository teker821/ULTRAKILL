using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Logic;

public class MapVarStringBuilder : MonoBehaviour
{
	[Header("Input")]
	public StringPart[] stringParts;

	[Header("Output")]
	[SerializeField]
	private string stringVariableName;

	[SerializeField]
	private TextSetMethod textMethod;

	[SerializeField]
	private Text textTarget;

	[Header("Events")]
	[SerializeField]
	private bool buildOnEnable;

	[SerializeField]
	private bool buildOnUpdate;

	private void OnEnable()
	{
		if (buildOnEnable)
		{
			BuildString();
		}
	}

	private void Update()
	{
		if (buildOnUpdate)
		{
			BuildString();
		}
	}

	public void BuildString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringPart[] array = stringParts;
		foreach (StringPart stringPart in array)
		{
			stringBuilder.Append(stringPart.GetString());
		}
		if (textTarget != null)
		{
			if (textMethod == TextSetMethod.SetText)
			{
				textTarget.text = stringBuilder.ToString();
			}
			else if (textMethod == TextSetMethod.AppendText)
			{
				textTarget.text += stringBuilder.ToString();
			}
			else if (textMethod == TextSetMethod.PrependText)
			{
				textTarget.text = stringBuilder.ToString() + textTarget.text;
			}
		}
		if (!string.IsNullOrEmpty(stringVariableName))
		{
			MonoSingleton<MapVarManager>.Instance.SetString(stringVariableName, stringBuilder.ToString());
		}
	}
}
