using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectMouseControl : MonoBehaviour
{
	private ScrollRect m_ScrollRect;

	private void OnEnable()
	{
		m_ScrollRect = GetComponent<ScrollRect>();
	}

	private void Update()
	{
		m_ScrollRect.verticalNormalizedPosition += Mouse.current.scroll.y.ReadValue() / m_ScrollRect.content.sizeDelta.y;
	}
}
