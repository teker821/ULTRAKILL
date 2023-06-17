using UnityEngine;

public class Enemy : MonoBehaviour
{
	private Rigidbody[] rbs;

	public bool limp;

	private Rigidbody rb;

	private Animator anim;

	private float currentSpeed;

	public float coolDown;

	public bool damaging;

	private TrailRenderer tr;

	private bool track;

	private AudioSource aud;

	private GroundCheck gc;

	public bool grounded;

	private float defaultSpeed;

	public Vector3 agentVelocity;

	private void Start()
	{
		rbs = GetComponentsInChildren<Rigidbody>();
		rb = GetComponent<Rigidbody>();
		anim = GetComponent<Animator>();
		gc = GetComponentInChildren<GroundCheck>();
	}
}
