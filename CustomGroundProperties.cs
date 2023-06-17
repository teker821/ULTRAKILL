using UnityEngine;

public class CustomGroundProperties : MonoBehaviour
{
	public float friction = 1f;

	public float speedMultiplier = 1f;

	public bool push;

	public Vector3 pushForce;

	public bool pushDirectionRelative;

	public bool canJump = true;

	public bool silentJumpFail;

	public float jumpForceMultiplier = 1f;

	public bool canSlide = true;

	public bool silentSlideFail;

	public bool canDash = true;

	public bool silentDashFail;

	public bool launchable = true;

	public bool forceCrouch;

	public bool overrideFootsteps;

	public AudioClip newFootstepSound;

	public bool dontRotateCamera;
}
