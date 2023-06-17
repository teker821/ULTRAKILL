using UnityEngine;

public class ScriptActivator : MonoBehaviour
{
	public Piston[] pistons;

	public LightPillar[] lightpillars;

	private void OnTriggerEnter(Collider other)
	{
		if (!(other.gameObject.tag == "Player"))
		{
			return;
		}
		if (pistons.Length != 0)
		{
			Piston[] array = pistons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].off = false;
			}
		}
		if (lightpillars.Length != 0)
		{
			LightPillar[] array2 = lightpillars;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].ActivatePillar();
			}
		}
		Object.Destroy(this);
	}
}
