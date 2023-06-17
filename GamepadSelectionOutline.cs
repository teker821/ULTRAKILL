using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GamepadSelectionOutline : MonoBehaviour
{
	private static readonly Vector3[] s_Corners = new Vector3[4];

	[SerializeField]
	private Image image;

	[SerializeField]
	private float scrollSpeedPixelsPerSecond = 800f;

	[SerializeField]
	private Vector2 outlineSize = new Vector2(4f, 4f);

	private ScrollRect lastScrollRect;

	private void Update()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		Canvas canvas = (currentSelectedGameObject ? currentSelectedGameObject.GetComponentInParent<Canvas>() : null);
		if (currentSelectedGameObject == null || !currentSelectedGameObject.activeInHierarchy || !currentSelectedGameObject.TryGetComponent<Selectable>(out var _) || MonoSingleton<InputManager>.Instance == null || !(MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad) || canvas.renderMode != 0)
		{
			image.enabled = false;
			return;
		}
		image.enabled = true;
		RectTransform component2 = currentSelectedGameObject.GetComponent<RectTransform>();
		RectTransform rect;
		Bounds selectedBounds = GetSelectedBounds(component2, out rect);
		image.rectTransform.anchoredPosition = selectedBounds.center;
		image.rectTransform.sizeDelta = selectedBounds.size + (Vector3)outlineSize;
		ScrollRect componentInParent = component2.GetComponentInParent<ScrollRect>();
		if (componentInParent != null && !currentSelectedGameObject.TryGetComponent<Scrollbar>(out var _))
		{
			EnsureVisibility(componentInParent, component2, componentInParent != lastScrollRect);
		}
		lastScrollRect = componentInParent;
	}

	private Bounds GetSelectedBounds(RectTransform selected, out RectTransform rect)
	{
		if (selected.TryGetComponent<Selectable>(out var component) && (bool)component.targetGraphic)
		{
			rect = component.targetGraphic.rectTransform;
			return GetRelativeBounds(image.transform.parent, rect);
		}
		rect = selected;
		return GetRelativeBounds(image.transform.parent, selected);
	}

	private Bounds GetRelativeBounds(Transform root, RectTransform child)
	{
		child.GetWorldCorners(s_Corners);
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		for (int i = 0; i < 4; i++)
		{
			Vector3 lhs = root.InverseTransformPoint(s_Corners[i]);
			vector = Vector3.Min(lhs, vector);
			vector2 = Vector3.Max(lhs, vector2);
		}
		Bounds result = new Bounds(vector, Vector3.zero);
		result.Encapsulate(vector2);
		return result;
	}

	private void EnsureVisibility(ScrollRect scrollRect, RectTransform child, bool instantScroll = false)
	{
		Bounds relativeBounds = GetRelativeBounds(scrollRect.content, child);
		if (child.TryGetComponent<GamepadSelectionBoundsExtension>(out var component) && component.Transforms != null)
		{
			RectTransform[] transforms = component.Transforms;
			foreach (RectTransform child2 in transforms)
			{
				relativeBounds.Encapsulate(GetRelativeBounds(scrollRect.content, child2));
			}
		}
		relativeBounds.min -= (Vector3)scrollRect.content.rect.min;
		relativeBounds.max -= (Vector3)scrollRect.content.rect.min;
		float num = scrollRect.content.rect.height - scrollRect.content.rect.height * scrollRect.verticalNormalizedPosition;
		float num2 = scrollRect.content.rect.height - relativeBounds.min.y;
		RectTransform component2;
		float num3 = ((scrollRect.TryGetComponent<RectTransform>(out component2) && num2 < component2.rect.height * 0.75f) ? 1f : ((!(relativeBounds.min.y < num)) ? (relativeBounds.max.y / scrollRect.content.rect.height) : (relativeBounds.min.y / scrollRect.content.rect.height)));
		if (instantScroll)
		{
			scrollRect.verticalNormalizedPosition = num3;
			return;
		}
		float num4 = scrollSpeedPixelsPerSecond / scrollRect.content.rect.height * Time.unscaledDeltaTime;
		if (scrollRect.verticalNormalizedPosition < num3)
		{
			scrollRect.verticalNormalizedPosition = Mathf.Min(scrollRect.verticalNormalizedPosition + num4, num3);
		}
		else if (scrollRect.verticalNormalizedPosition > num3)
		{
			scrollRect.verticalNormalizedPosition = Mathf.Max(scrollRect.verticalNormalizedPosition - num4, num3);
		}
	}
}
