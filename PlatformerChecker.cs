using UnityEngine;

public class PlatformerChecker : MonoBehaviour
{
	public bool activated;

	public UltrakillEvent onPlatformer;

	private void Update()
	{
		if (!activated && MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
		{
			activated = true;
			onPlatformer.Invoke();
		}
	}
}
