using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
internal class ControllerPointer : MonoBehaviour
{
	private static RaycastResult? bestResult;

	private PointerEventData eventData;

	private static int ignoreFrame;

	[SerializeField]
	private UnityEvent onPressed;

	[SerializeField]
	private UnityEvent onReleased;

	[SerializeField]
	private UnityEvent onEnter;

	[SerializeField]
	private UnityEvent onExit;

	[SerializeField]
	private float dragThreshold;

	private bool entered;

	private bool pointerDown;

	private bool scrollState;

	public static GraphicRaycaster raycaster;

	private List<RaycastResult> results;

	private Vector2? dragPoint;

	private bool dragging;

	public UnityEvent OnPressed => onPressed;

	public UnityEvent OnReleased => onReleased;

	public UnityEvent OnEnter => onEnter;

	public UnityEvent OnExit => onExit;

	private void Awake()
	{
		if (onPressed == null)
		{
			onPressed = new UnityEvent();
		}
		if (onReleased == null)
		{
			onReleased = new UnityEvent();
		}
		if (onEnter == null)
		{
			onEnter = new UnityEvent();
		}
		if (onExit == null)
		{
			onExit = new UnityEvent();
		}
		results = new List<RaycastResult>();
	}

	private void UpdateSlider()
	{
		if (!TryGetComponent<Slider>(out var component))
		{
			return;
		}
		if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed)
		{
			RectTransform component2 = component.GetComponent<RectTransform>();
			Vector2 screenPoint = new Vector2(Screen.width, Screen.height) / 2f;
			Rect rect = component2.rect;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(component2, screenPoint, raycaster.eventCamera, out var localPoint))
			{
				if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && rect.Contains(localPoint))
				{
					scrollState = true;
				}
				else if (!scrollState)
				{
					return;
				}
				float num = Mathf.InverseLerp(rect.x, rect.x + rect.width, localPoint.x);
				component.value = component.minValue + num * (component.maxValue - component.minValue);
			}
		}
		else
		{
			scrollState = false;
		}
	}

	private void UpdateScrollbars()
	{
		if (!TryGetComponent<ScrollRect>(out var component))
		{
			return;
		}
		if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed)
		{
			if (component.horizontal)
			{
				UpdateScrollbar(component.horizontalScrollbar);
			}
			if (component.vertical)
			{
				UpdateScrollbar(component.verticalScrollbar);
			}
		}
		else
		{
			scrollState = false;
		}
		RectTransform content = component.content;
		Vector2 screenPoint = new Vector2(Screen.width, Screen.height) / 2f;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(content, screenPoint, raycaster.eventCamera, out var localPoint) && content.rect.Contains(localPoint))
		{
			if (component.horizontal)
			{
				component.horizontalScrollbar.value += Mouse.current.scroll.x.ReadValue() / 2f / content.rect.height;
			}
			if (component.vertical)
			{
				component.verticalScrollbar.value += Mouse.current.scroll.y.ReadValue() / 2f / content.rect.height;
			}
		}
	}

	private void UpdateScrollbar(Scrollbar scroll)
	{
		RectTransform component = scroll.GetComponent<RectTransform>();
		Vector2 screenPoint = new Vector2(Screen.width, Screen.height) / 2f;
		Rect rect = component.rect;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(component, screenPoint, raycaster.eventCamera, out var localPoint))
		{
			if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && rect.Contains(localPoint))
			{
				scrollState = true;
			}
			else if (!scrollState)
			{
				return;
			}
			switch (scroll.direction)
			{
			case Scrollbar.Direction.BottomToTop:
				scroll.value = Mathf.InverseLerp(rect.y, rect.y + rect.height, localPoint.y);
				break;
			case Scrollbar.Direction.LeftToRight:
				scroll.value = Mathf.InverseLerp(rect.x, rect.x + rect.width, localPoint.x);
				break;
			case Scrollbar.Direction.TopToBottom:
				scroll.value = Mathf.InverseLerp(rect.y + rect.height, rect.y, localPoint.y);
				break;
			case Scrollbar.Direction.RightToLeft:
				scroll.value = Mathf.InverseLerp(rect.x + rect.width, rect.x, localPoint.x);
				break;
			}
		}
	}

	private void Update()
	{
		if (!EventSystem.current || !raycaster || !raycaster.eventCamera || ((bool)MonoSingleton<OptionsManager>.Instance && MonoSingleton<OptionsManager>.Instance.paused))
		{
			return;
		}
		eventData = new PointerEventData(EventSystem.current)
		{
			button = PointerEventData.InputButton.Left,
			position = new Vector2((float)(raycaster ? raycaster.eventCamera.pixelWidth : Screen.width) / 2f, (float)(raycaster ? raycaster.eventCamera.pixelHeight : Screen.height) / 2f)
		};
		if ((bool)raycaster && ignoreFrame != Time.frameCount)
		{
			ignoreFrame = Time.frameCount;
			bestResult = null;
			results.Clear();
			raycaster.Raycast(eventData, results);
			foreach (RaycastResult result in results)
			{
				if (!result.gameObject.TryGetComponent<Text>(out var _) && (!bestResult.HasValue || bestResult.Value.depth <= result.depth))
				{
					bestResult = result;
				}
			}
		}
		UpdateEvents();
		UpdateSlider();
		UpdateScrollbars();
	}

	private void UpdateEvents()
	{
		if (!bestResult.HasValue)
		{
			return;
		}
		bool flag = entered;
		entered = bestResult.Value.gameObject == base.gameObject;
		if (entered && !flag)
		{
			ExecuteEvents.Execute(base.gameObject, eventData, ExecuteEvents.pointerEnterHandler);
			onEnter?.Invoke();
		}
		if (entered && !MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame)
		{
			pointerDown = true;
			ExecuteEvents.Execute(base.gameObject, eventData, ExecuteEvents.pointerDownHandler);
			ExecuteEvents.Execute(base.gameObject, eventData, ExecuteEvents.pointerClickHandler);
			onPressed?.Invoke();
			dragPoint = eventData.position;
		}
		if (pointerDown && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasCanceledThisFrame)
		{
			pointerDown = false;
			ExecuteEvents.Execute(base.gameObject, eventData, ExecuteEvents.pointerUpHandler);
			onReleased?.Invoke();
		}
		if (flag && !entered)
		{
			ExecuteEvents.Execute(base.gameObject, eventData, ExecuteEvents.pointerExitHandler);
			onExit?.Invoke();
		}
		if (dragPoint.HasValue)
		{
			Vector2 delta = eventData.position - dragPoint.Value;
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
			{
				button = PointerEventData.InputButton.Left,
				position = eventData.position,
				pressPosition = dragPoint.Value,
				delta = delta
			};
			if (pointerDown && entered && delta.sqrMagnitude >= dragThreshold * dragThreshold)
			{
				ExecuteEvents.Execute(base.gameObject, pointerEventData, ExecuteEvents.beginDragHandler);
				dragging = true;
			}
			if (dragging)
			{
				ExecuteEvents.Execute(base.gameObject, pointerEventData, ExecuteEvents.dragHandler);
			}
			if (!pointerDown | !entered)
			{
				dragging = false;
				dragPoint = null;
				ExecuteEvents.Execute(base.gameObject, pointerEventData, ExecuteEvents.endDragHandler);
			}
		}
	}
}
