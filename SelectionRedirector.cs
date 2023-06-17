using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal sealed class SelectionRedirector : Selectable
{
	public Selectable[] Selectables;

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (Selectables == null)
		{
			return;
		}
		Selectable[] selectables = Selectables;
		foreach (Selectable selectable in selectables)
		{
			if (selectable != null && selectable.isActiveAndEnabled)
			{
				StartCoroutine(SelectAtEndOfFrame(selectable));
				break;
			}
		}
	}

	private IEnumerator SelectAtEndOfFrame(Selectable selectable)
	{
		yield return new WaitForEndOfFrame();
		selectable.Select();
	}
}
