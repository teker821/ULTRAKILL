using UnityEngine;

public class CheckForScroller : MonoBehaviour
{
	public bool checkOnStart = true;

	public bool checkOnCollision = true;

	private ScrollingTexture scroller;

	private void Start()
	{
		if (!checkOnStart)
		{
			return;
		}
		if ((bool)MonoSingleton<ComponentsDatabase>.Instance && MonoSingleton<ComponentsDatabase>.Instance.scrollers.Count > 0)
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, 1f, LayerMaskDefaults.Get(LMD.Environment));
			if (array.Length != 0)
			{
				Collider[] array2 = array;
				foreach (Collider collider in array2)
				{
					if (MonoSingleton<ComponentsDatabase>.Instance.scrollers.Contains(collider.transform) && collider.transform.TryGetComponent<ScrollingTexture>(out var component))
					{
						component.attachedObjects.Add(base.transform);
					}
				}
			}
		}
		if (!checkOnCollision)
		{
			Object.Destroy(this);
		}
	}

	private void OnCollisionEnter(Collision col)
	{
		if (checkOnCollision && (bool)MonoSingleton<ComponentsDatabase>.Instance && MonoSingleton<ComponentsDatabase>.Instance.scrollers.Count > 0 && MonoSingleton<ComponentsDatabase>.Instance.scrollers.Contains(col.transform) && col.transform.TryGetComponent<ScrollingTexture>(out var component))
		{
			scroller = component;
			component.attachedObjects.Add(base.transform);
		}
	}

	private void OnCollisionExit(Collision col)
	{
		if (checkOnCollision && (bool)scroller && col.transform == scroller.transform)
		{
			scroller.attachedObjects.Remove(base.transform);
		}
	}
}
