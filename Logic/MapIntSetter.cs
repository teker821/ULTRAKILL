using UnityEngine;

namespace Logic;

public class MapIntSetter : MapVarSetter
{
	[SerializeField]
	private IntInputType inputType;

	[SerializeField]
	private string sourceVariableName;

	[SerializeField]
	private int min;

	[SerializeField]
	private int max = 1;

	[SerializeField]
	private int[] list;

	[SerializeField]
	private int number;

	public override void SetVar()
	{
		base.SetVar();
		switch (inputType)
		{
		case IntInputType.SetToNumber:
			MonoSingleton<MapVarManager>.Instance.SetInt(variableName, number);
			break;
		case IntInputType.RandomRange:
			MonoSingleton<MapVarManager>.Instance.SetInt(variableName, Random.Range(min, max));
			break;
		case IntInputType.RandomFromList:
			MonoSingleton<MapVarManager>.Instance.SetInt(variableName, list[Random.Range(0, list.Length)]);
			break;
		case IntInputType.AddNumber:
			MonoSingleton<MapVarManager>.Instance.SetInt(variableName, number + (MonoSingleton<MapVarManager>.Instance.GetInt(variableName) ?? 0));
			break;
		case IntInputType.CopyDifferentVariable:
			MonoSingleton<MapVarManager>.Instance.SetInt(variableName, MonoSingleton<MapVarManager>.Instance.GetInt(sourceVariableName) ?? (-1));
			break;
		}
	}
}
