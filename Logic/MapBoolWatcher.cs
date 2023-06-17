using UnityEngine;

namespace Logic;

[DefaultExecutionOrder(10)]
public class MapBoolWatcher : MapVarWatcher<bool?>
{
	[SerializeField]
	private BoolWatchMode watchMode;

	[SerializeField]
	private UnityEventBool onConditionMetWithValue;

	private bool? lastValue;

	private void OnEnable()
	{
		if (!registered)
		{
			if (MonoSingleton<MapVarManager>.Instance == null)
			{
				Debug.LogError("Unable to register MapBoolWatcher. Missing map variable manager.");
				return;
			}
			MonoSingleton<MapVarManager>.Instance.RegisterBoolWatcher(variableName, delegate(bool val)
			{
				ProcessEvent(val);
			});
			registered = true;
		}
		if (evaluateOnEnable)
		{
			ProcessEvent(MonoSingleton<MapVarManager>.Instance.GetBool(variableName));
		}
	}

	private void Update()
	{
		if (continuouslyActivateOnSuccess && lastState)
		{
			CallEvents();
		}
	}

	protected override void ProcessEvent(bool? value)
	{
		base.ProcessEvent(value);
		if (watchMode == BoolWatchMode.IsFalseOrNull || lastValue != value)
		{
			lastValue = value;
			bool flag = EvaluateState(value);
			if (flag != lastState)
			{
				lastState = flag;
				CallEvents();
			}
		}
	}

	protected override bool EvaluateState(bool? newValue)
	{
		switch (watchMode)
		{
		case BoolWatchMode.IsTrue:
			if (newValue.HasValue)
			{
				return newValue.Value;
			}
			return false;
		case BoolWatchMode.IsFalse:
			if (newValue.HasValue)
			{
				return !newValue.Value;
			}
			return false;
		case BoolWatchMode.IsFalseOrNull:
			if (newValue.HasValue)
			{
				return !newValue.Value;
			}
			return true;
		case BoolWatchMode.AnyValue:
			return newValue.HasValue;
		default:
			return false;
		}
	}

	protected override void CallEvents()
	{
		base.CallEvents();
		onConditionMetWithValue.Invoke(lastValue ?? false);
	}
}
