using UnityEngine;

public class PlayerPosInfo : MonoBehaviour
{
	public bool noPosition;

	public Vector3 velocity;

	public Vector3 position;

	public float wooshTime;

	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
