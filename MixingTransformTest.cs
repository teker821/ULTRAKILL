using UnityEngine;

public class MixingTransformTest : MonoBehaviour
{
	private Animator anim;

	private void Start()
	{
		anim = GetComponent<Animator>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F10))
		{
			anim.SetLayerWeight(1, 1f);
			anim.SetTrigger("Shoot");
		}
	}
}
