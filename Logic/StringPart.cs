using System;

namespace Logic;

[Serializable]
public struct StringPart
{
	public StringPartType type;

	public VariableType variableType;

	public string value;

	public string GetString()
	{
		switch (type)
		{
		case StringPartType.Variable:
			switch (variableType)
			{
			case VariableType.Bool:
				return MonoSingleton<MapVarManager>.Instance.GetBool(value).ToString();
			case VariableType.Int:
				return MonoSingleton<MapVarManager>.Instance.GetInt(value).ToString();
			case VariableType.String:
				return MonoSingleton<MapVarManager>.Instance.GetString(value);
			case VariableType.Float:
				return MonoSingleton<MapVarManager>.Instance.GetFloat(value).ToString();
			}
			break;
		case StringPartType.NormalText:
			return value;
		case StringPartType.NewLine:
			return "\n";
		}
		return string.Empty;
	}
}
