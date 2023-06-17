using UnityEngine;

public class BreakZone : MonoBehaviour
{
	public bool weakOnly;

	public bool countsAsPrecise;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Breakable") && other.TryGetComponent<Breakable>(out var component) && (!weakOnly || component.weak) && (countsAsPrecise || !component.precisionOnly))
		{
			component.Break();
		}
	}
}
