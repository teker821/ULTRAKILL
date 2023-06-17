using UnityEngine;

public class FishGhost : MonoBehaviour
{
	private float speed = 1f;

	private float turnSpeed = 1f;

	private float direction = 1f;

	private float directionChangeTendency;

	private float jitter;

	private float indecisiveness;

	private TimeSince timeSinceLogicTick;

	private bool scared;

	private TimeSince timeSinceSpook;

	private TimeSince timeSinceDifficultAction;

	private FishConstraints constraints;

	public float tiredness => 1f - Mathf.Clamp01(timeSinceDifficultAction);

	private void RollTheDice()
	{
		speed = Random.Range(0.1f, 7f);
		turnSpeed = Random.Range(4f, 50f);
		direction = ((Random.value > 0.5f) ? 1f : (-1f));
		directionChangeTendency = Random.Range(0.0001f, 0.02f);
		jitter = Random.Range(0.1f, 1.5f);
	}

	private void Start()
	{
		base.transform.rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
		indecisiveness = Random.Range(0.003f, 0.1f);
		timeSinceLogicTick = 0f;
		RollTheDice();
		constraints = GetComponent<FishConstraints>();
		if (constraints == null && base.transform.parent != null)
		{
			constraints = base.transform.parent.GetComponent<FishConstraints>();
		}
	}

	private void FixedUpdate()
	{
		if (Random.value < indecisiveness)
		{
			RollTheDice();
		}
		if (Random.value < directionChangeTendency)
		{
			direction *= -1f;
		}
		if (scared && (float)timeSinceSpook > 3f)
		{
			scared = false;
		}
		if (!((float)timeSinceLogicTick > 3f + jitter))
		{
			return;
		}
		RaycastHit hitInfo;
		bool flag = Physics.Raycast(base.transform.position, -base.transform.forward, out hitInfo, 10f, (int)LayerMaskDefaults.Get(LMD.Environment) | 0x10000, QueryTriggerInteraction.Collide);
		Vector3 position = MonoSingleton<NewMovement>.Instance.transform.position;
		bool flag2 = constraints == null || constraints.area.Contains(base.transform.position);
		if ((tiredness < 0.5f && Vector3.Distance(position, base.transform.position) < 15f) || flag || !flag2)
		{
			if (flag)
			{
				base.transform.rotation = Quaternion.LookRotation(hitInfo.point - base.transform.position);
			}
			if (!flag2)
			{
				Vector3 center = constraints.area.center;
				base.transform.rotation = Quaternion.LookRotation(base.transform.position - center);
			}
			base.transform.rotation = Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y, 0f);
			scared = true;
			timeSinceSpook = 0f;
			timeSinceDifficultAction = 0f;
		}
	}

	private void Update()
	{
		base.transform.Translate(-Vector3.forward * Time.deltaTime * speed * (scared ? 4f : (1f * (1f - tiredness))));
		base.transform.Rotate(Vector3.up * Time.deltaTime * turnSpeed * direction * (1f - tiredness));
	}
}
