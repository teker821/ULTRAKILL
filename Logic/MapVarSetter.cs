using UnityEngine;

namespace Logic;

public abstract class MapVarSetter : MonoBehaviour
{
	public string variableName;

	public VariablePersistence persistence;

	public bool setOnEnable = true;

	public bool setEveryFrame;

	private void OnEnable()
	{
		if (setOnEnable)
		{
			SetVar();
		}
	}

	private void Update()
	{
		if (setEveryFrame)
		{
			SetVar();
		}
	}

	public virtual void SetVar()
	{
	}
}
