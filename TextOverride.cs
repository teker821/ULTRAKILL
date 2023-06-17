using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.UI;

internal sealed class TextOverride : MonoBehaviour
{
	private Component m_TextComponent;

	[TextArea]
	[SerializeField]
	private string m_KeyboardText;

	[TextArea]
	[SerializeField]
	private string m_GenericText;

	[TextArea]
	[SerializeField]
	private string m_DualShockText;

	private void Awake()
	{
		TMP_Text component2;
		if (TryGetComponent<Text>(out var component))
		{
			m_TextComponent = component;
		}
		else if (TryGetComponent<TMP_Text>(out component2))
		{
			m_TextComponent = component2;
		}
		if (string.IsNullOrEmpty(m_KeyboardText))
		{
			m_KeyboardText = GetText();
		}
	}

	private void Update()
	{
		if (!string.IsNullOrEmpty(m_DualShockText) && MonoSingleton<InputManager>.Instance.LastButtonDevice is DualShockGamepad)
		{
			SetText(m_DualShockText);
		}
		else if (MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad)
		{
			SetText(m_GenericText);
		}
		else
		{
			SetText(m_KeyboardText);
		}
	}

	private void SetText(string value)
	{
		if (m_TextComponent is Text)
		{
			((Text)m_TextComponent).text = value;
		}
		else if (m_TextComponent is TMP_Text)
		{
			((Text)m_TextComponent).text = value;
		}
	}

	private string GetText()
	{
		if (m_TextComponent is Text)
		{
			return ((Text)m_TextComponent).text;
		}
		if (m_TextComponent is TMP_Text)
		{
			return ((Text)m_TextComponent).text;
		}
		return null;
	}
}
