using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class VirtueController : MonoSingleton<VirtueController>
{
	public float cooldown;

	public int currentVirtues;

	private void Update()
	{
		if (cooldown > 0f)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
		}
	}
}
