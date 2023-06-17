using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForceLayoutRebuilds : MonoBehaviour
{
	public int iterations = 3;

	public bool onEnable = true;

	public bool allChildLayoutElements = true;

	public ScrollRect scrollRectToReset;

	private RectTransform rectTransform;

	private void Awake()
	{
		rectTransform = (RectTransform)base.transform;
	}

	private void OnEnable()
	{
		if (onEnable)
		{
			ForceRebuild();
			StartCoroutine(DelayedRebuild());
		}
	}

	public void ForceRebuild()
	{
		List<RectTransform> list = new List<RectTransform> { rectTransform };
		if (allChildLayoutElements)
		{
			LayoutGroup[] componentsInChildren = rectTransform.GetComponentsInChildren<LayoutGroup>(includeInactive: true);
			foreach (LayoutGroup layoutGroup in componentsInChildren)
			{
				list.Add((RectTransform)layoutGroup.transform);
			}
		}
		for (int j = 0; j < iterations; j++)
		{
			foreach (RectTransform item in list)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(item);
			}
		}
		scrollRectToReset.verticalNormalizedPosition = 1f;
	}

	private IEnumerator DelayedRebuild()
	{
		yield return new WaitForEndOfFrame();
		ForceRebuild();
	}
}
