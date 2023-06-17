using UnityEngine;

public class SpinDirectionChanger : MonoBehaviour
{
	public Spin target;

	public Vector3 direction;

	private void Start()
	{
		target.spinDirection = direction;
	}
}
