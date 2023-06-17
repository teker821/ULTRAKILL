using UnityEngine;

namespace Logic;

[DefaultExecutionOrder(10)]
public class MapFloatWatcher : MapVarWatcher<float?>
{
	[SerializeField]
	private FloatWatchMode watchMode;

	[SerializeField]
	private UnityEventFloat onConditionMetWithValue;

	[SerializeField]
	private float targetValue = 3f;

	private float? lastValue;

	private void OnEnable()
	{
		if (!registered)
		{
			if (MonoSingleton<MapVarManager>.Instance == null)
			{
				Debug.LogError("Unable to register MapFloatWatcher. Missing map variable manager.");
				return;
			}
			MonoSingleton<MapVarManager>.Instance.RegisterFloatWatcher(variableName, delegate(float val)
			{
				ProcessEvent(val);
			});
			registered = true;
		}
		if (evaluateOnEnable)
		{
			ProcessEvent(MonoSingleton<MapVarManager>.Instance.GetFloat(variableName));
		}
	}

	private void Update()
	{
		if (continuouslyActivateOnSuccess && lastState)
		{
			CallEvents();
		}
	}

	protected override void ProcessEvent(float? value)
	{
		base.ProcessEvent(value);
		if (lastValue != value)
		{
			lastValue = value;
			bool flag = EvaluateState(value);
			if (watchMode == FloatWatchMode.AnyChange || flag != lastState)
			{
				lastState = flag;
				CallEvents();
			}
		}
	}

	protected override bool EvaluateState(float? newValue)
	{
		return watchMode switch
		{
			FloatWatchMode.AnyChange => newValue.HasValue, 
			FloatWatchMode.EqualTo => newValue == targetValue, 
			FloatWatchMode.GreaterThan => newValue > targetValue, 
			FloatWatchMode.LessThan => newValue < targetValue, 
			FloatWatchMode.NotEqualTo => newValue != targetValue, 
			_ => false, 
		};
	}

	protected override void CallEvents()
	{
		base.CallEvents();
		onConditionMetWithValue.Invoke(lastValue ?? (-1f));
	}
}
