using UnityEngine;

namespace Logic;

public class MapFloatSetter : MapVarSetter
{
	[SerializeField]
	private FloatInputType inputType;

	[SerializeField]
	private string sourceVariableName;

	[SerializeField]
	private float min;

	[SerializeField]
	private float max = 1f;

	[SerializeField]
	private float[] list;

	[SerializeField]
	private float number;

	public override void SetVar()
	{
		base.SetVar();
		switch (inputType)
		{
		case FloatInputType.SetToNumber:
			MonoSingleton<MapVarManager>.Instance.SetFloat(variableName, number);
			break;
		case FloatInputType.RandomRange:
			MonoSingleton<MapVarManager>.Instance.SetFloat(variableName, Random.Range(min, max));
			break;
		case FloatInputType.RandomFromList:
			MonoSingleton<MapVarManager>.Instance.SetFloat(variableName, list[Random.Range(0, list.Length)]);
			break;
		case FloatInputType.AddNumber:
			MonoSingleton<MapVarManager>.Instance.SetFloat(variableName, (MonoSingleton<MapVarManager>.Instance.GetFloat(variableName) ?? 0f) + number);
			break;
		case FloatInputType.CopyDifferentVariable:
			MonoSingleton<MapVarManager>.Instance.SetFloat(variableName, MonoSingleton<MapVarManager>.Instance.GetFloat(sourceVariableName) ?? 0f);
			break;
		case FloatInputType.MultiplyByNumber:
			MonoSingleton<MapVarManager>.Instance.SetFloat(variableName, (MonoSingleton<MapVarManager>.Instance.GetFloat(variableName) ?? 1f) * number);
			break;
		case FloatInputType.MultiplyByVariable:
			MonoSingleton<MapVarManager>.Instance.SetFloat(variableName, (MonoSingleton<MapVarManager>.Instance.GetFloat(variableName) ?? 1f) * (MonoSingleton<MapVarManager>.Instance.GetFloat(sourceVariableName) ?? 1f));
			break;
		}
	}
}
