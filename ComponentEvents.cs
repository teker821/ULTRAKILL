using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public sealed class ComponentEvents : MonoBehaviour
{
	[SerializeField]
	private UnityEvent onEnable;

	[SerializeField]
	private UnityEvent onDisable;

	private void OnEnable()
	{
		onEnable?.Invoke();
	}

	private void OnDisable()
	{
		onDisable?.Invoke();
	}
}
