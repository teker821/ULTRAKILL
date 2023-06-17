using System.Collections;
using UnityEngine;

public class PlayerMovementLimiter : MonoBehaviour
{
	[SerializeField]
	private float animatorInteractionSpeedCap = 30f;

	private Rigidbody rigidbody;

	private Vector3 lastVel;

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
	}

	private void LateUpdate()
	{
		lastVel = rigidbody.velocity;
	}

	private bool AnimatorCheck(Collision collision)
	{
		if (collision.gameObject.TryGetComponent<Animator>(out var component))
		{
			return true;
		}
		if ((bool)collision.transform.parent && collision.transform.parent.TryGetComponent<Animator>(out component))
		{
			return true;
		}
		return false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (AnimatorCheck(collision) && (lastVel - rigidbody.velocity).magnitude > animatorInteractionSpeedCap)
		{
			StartCoroutine(CancelOnNext(lastVel, lastVel - rigidbody.velocity));
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (AnimatorCheck(collision) && (lastVel - rigidbody.velocity).magnitude > animatorInteractionSpeedCap)
		{
			StartCoroutine(CancelOnNext(lastVel, lastVel - rigidbody.velocity));
		}
	}

	private IEnumerator CancelOnNext(Vector3 lastVelocity, Vector3 newDelta)
	{
		Debug.Log($"Player Animator Push Cancelled. {newDelta.magnitude}");
		rigidbody.velocity = lastVelocity;
		yield return new WaitForFixedUpdate();
		rigidbody.velocity = lastVelocity;
	}
}
