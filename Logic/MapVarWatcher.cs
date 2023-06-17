using UnityEngine;

namespace Logic;

public abstract class MapVarWatcher<T> : MonoBehaviour
{
	[SerializeField]
	protected string variableName;

	[Tooltip("If true, the watcher will check its state immediately after being enabled or spawned.")]
	[SerializeField]
	protected bool evaluateOnEnable = true;

	[Tooltip("The component will be disabled after the event is executed")]
	[SerializeField]
	protected bool onlyActivateOnce;

	[Tooltip("Call the event every frame if the conditions are met")]
	[SerializeField]
	protected bool continuouslyActivateOnSuccess;

	protected bool lastState;

	protected bool registered;

	[SerializeField]
	protected UltrakillEvent onConditionMet;

	protected virtual void ProcessEvent(T value)
	{
	}

	protected virtual bool EvaluateState(T newValue)
	{
		return false;
	}

	protected virtual void CallEvents()
	{
		if (lastState)
		{
			onConditionMet.Invoke();
		}
		else
		{
			onConditionMet.Revert();
		}
	}
}
