using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class CanvasController : MonoSingleton<CanvasController>
{
	protected override void Awake()
	{
		if ((bool)MonoSingleton<CanvasController>.Instance && MonoSingleton<CanvasController>.Instance != this)
		{
			Object.DestroyImmediate(base.gameObject);
		}
		else
		{
			base.Awake();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.transform.SetParent(null);
	}
}
