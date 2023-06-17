using UnityEngine;

public class ColorBlindActivator : MonoBehaviour
{
	public Transform parentOfSetters;

	private ColorBlindSetter[] cbss;

	private void Start()
	{
		if (cbss == null || cbss.Length == 0)
		{
			cbss = GetComponentsInChildren<ColorBlindSetter>(includeInactive: true);
		}
		ColorBlindSetter[] array = cbss;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Prepare();
		}
	}

	public void ResetToDefault()
	{
		if (cbss == null || cbss.Length == 0)
		{
			cbss = GetComponentsInChildren<ColorBlindSetter>(includeInactive: true);
		}
		ColorBlindSetter[] array = cbss;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ResetToDefault();
		}
	}
}
