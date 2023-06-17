using UnityEngine;

namespace Logic;

[DefaultExecutionOrder(10)]
public class MapIntWatcher : MapVarWatcher<int?>
{
	[SerializeField]
	private IntWatchMode watchMode;

	[SerializeField]
	private UnityEventInt onConditionMetWithValue;

	[SerializeField]
	private int targetValue;

	private int? lastValue;

	private void OnEnable()
	{
		if (!registered)
		{
			if (MonoSingleton<MapVarManager>.Instance == null)
			{
				Debug.LogError("Unable to register MapIntWatcher. Missing map variable manager.");
				return;
			}
			MonoSingleton<MapVarManager>.Instance.RegisterIntWatcher(variableName, delegate(int val)
			{
				ProcessEvent(val);
			});
			registered = true;
		}
		if (evaluateOnEnable)
		{
			ProcessEvent(MonoSingleton<MapVarManager>.Instance.GetInt(variableName));
		}
	}

	private void Update()
	{
		if (continuouslyActivateOnSuccess && lastState)
		{
			CallEvents();
		}
	}

	protected override void ProcessEvent(int? value)
	{
		base.ProcessEvent(value);
		if (lastValue != value)
		{
			lastValue = value;
			bool flag = EvaluateState(value);
			if (watchMode == IntWatchMode.AnyChange || flag != lastState)
			{
				lastState = flag;
				CallEvents();
			}
		}
	}

	protected override bool EvaluateState(int? newValue)
	{
		return watchMode switch
		{
			IntWatchMode.AnyChange => newValue.HasValue, 
			IntWatchMode.EqualTo => newValue == targetValue, 
			IntWatchMode.GreaterThan => newValue > targetValue, 
			IntWatchMode.LessThan => newValue < targetValue, 
			IntWatchMode.NotEqualTo => newValue != targetValue, 
			_ => false, 
		};
	}

	protected override void CallEvents()
	{
		base.CallEvents();
		onConditionMetWithValue.Invoke(lastValue ?? (-1));
	}
}
