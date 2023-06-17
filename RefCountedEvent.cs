using System;
using UnityEngine;
using UnityEngine.Events;

public sealed class RefCountedEvent : MonoBehaviour
{
	private int m_RefCount;

	[SerializeField]
	private UnityEvent m_Activate;

	[SerializeField]
	private UnityEvent m_Deactivate;

	public void AddRef()
	{
		if (m_RefCount == 0)
		{
			m_Activate?.Invoke();
		}
		m_RefCount++;
	}

	public void Release()
	{
		if (m_RefCount == 0)
		{
			throw new InvalidOperationException();
		}
		if (m_RefCount == 1)
		{
			m_Deactivate?.Invoke();
		}
		m_RefCount--;
	}
}
