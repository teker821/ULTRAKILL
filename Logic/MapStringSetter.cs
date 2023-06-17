using UnityEngine;

namespace Logic;

public class MapStringSetter : MapVarSetter
{
	[SerializeField]
	private StringInputType inputType;

	[SerializeField]
	private string sourceVariableName;

	[SerializeField]
	private VariableType sourceVariableType;

	[SerializeField]
	private string textValue;

	public override void SetVar()
	{
		base.SetVar();
		switch (inputType)
		{
		case StringInputType.JustText:
			MonoSingleton<MapVarManager>.Instance.SetString(variableName, textValue);
			break;
		case StringInputType.CopyDifferentVariable:
			if (sourceVariableType == VariableType.String)
			{
				MonoSingleton<MapVarManager>.Instance.SetString(variableName, MonoSingleton<MapVarManager>.Instance.GetString(sourceVariableName));
			}
			else if (sourceVariableType == VariableType.Int)
			{
				MonoSingleton<MapVarManager>.Instance.SetString(variableName, MonoSingleton<MapVarManager>.Instance.GetInt(sourceVariableName).ToString());
			}
			else if (sourceVariableType == VariableType.Float)
			{
				MonoSingleton<MapVarManager>.Instance.SetString(variableName, MonoSingleton<MapVarManager>.Instance.GetFloat(sourceVariableName).ToString());
			}
			else if (sourceVariableType == VariableType.Bool)
			{
				MonoSingleton<MapVarManager>.Instance.SetString(variableName, MonoSingleton<MapVarManager>.Instance.GetBool(sourceVariableName).ToString());
			}
			break;
		}
	}
}
