using UnityEngine;
using UnityEngine.Events;

internal class BackSelectEvent : MonoBehaviour
{
	[SerializeField]
	private UnityEvent m_OnBack;

	public void InvokeOnBack()
	{
		m_OnBack?.Invoke();
	}
}
