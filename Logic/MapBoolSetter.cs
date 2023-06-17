namespace Logic;

public class MapBoolSetter : MapVarSetter
{
	public BoolInputType inputType;

	public bool value;

	public override void SetVar()
	{
		base.SetVar();
		switch (inputType)
		{
		case BoolInputType.Set:
			MonoSingleton<MapVarManager>.Instance.SetBool(variableName, value);
			break;
		case BoolInputType.Toggle:
		{
			bool flag = MonoSingleton<MapVarManager>.Instance.GetBool(variableName) ?? false;
			MonoSingleton<MapVarManager>.Instance.SetBool(variableName, !flag);
			break;
		}
		}
	}
}
