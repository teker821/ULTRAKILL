using UnityEngine;

public class AnimationSpeedRandomizer : MonoBehaviour
{
	private Animator anim;

	public float speed = 1f;

	public float maxRandomness = 0.1f;

	public bool randomizePlaybackPosition;

	private void Start()
	{
		anim = GetComponent<Animator>();
		anim.speed = speed + Random.Range(0f - maxRandomness, maxRandomness);
		if (randomizePlaybackPosition)
		{
			anim.Play(0, -1, Random.Range(0f, 1f));
		}
	}
}
