using UnityEngine;

public class StaminaFail : MonoBehaviour
{
	private void Start()
	{
		StaminaMeter[] array = Object.FindObjectsOfType<StaminaMeter>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Flash(red: true);
		}
	}
}
