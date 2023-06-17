using UnityEngine;

public class GlassBreaker : MonoBehaviour
{
	private void Start()
	{
		GetComponentInParent<Glass>().Shatter();
		Object.Destroy(this);
	}
}
