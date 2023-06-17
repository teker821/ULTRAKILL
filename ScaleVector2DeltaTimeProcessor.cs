using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ScaleVector2DeltaTimeProcessor : InputProcessor<Vector2>
{
	static ScaleVector2DeltaTimeProcessor()
	{
		Initialize();
	}

	public override Vector2 Process(Vector2 value, InputControl control)
	{
		return value * Time.deltaTime;
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Initialize()
	{
		InputSystem.RegisterProcessor<ScaleVector2DeltaTimeProcessor>();
	}
}
