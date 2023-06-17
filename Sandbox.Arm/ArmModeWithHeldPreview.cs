using System.Collections;
using UnityEngine;

namespace Sandbox.Arm;

public abstract class ArmModeWithHeldPreview : ISandboxArmMode
{
	protected static readonly int Holding = Animator.StringToHash("Holding");

	protected static readonly int Punch = Animator.StringToHash("Punch");

	protected SandboxArm hostArm;

	protected SpawnableObject currentObject;

	protected GameObject heldPreview;

	public virtual string Name => null;

	public virtual bool CanOpenMenu => true;

	public virtual bool Raycast => true;

	public virtual string Icon => null;

	public virtual void OnEnable(SandboxArm arm)
	{
		hostArm = arm;
		hostArm.ResetAnimator();
		hostArm.animator.SetBool(Holding, value: true);
		if ((bool)heldPreview)
		{
			heldPreview.SetActive(value: true);
		}
	}

	protected IEnumerator HandClosedAnimationThing()
	{
		heldPreview.SetActive(value: false);
		yield return new WaitForSeconds(0.85f);
		heldPreview.SetActive(value: true);
	}

	public virtual void SetPreview(SpawnableObject obj)
	{
		hostArm.selectSound.Play();
		currentObject = obj;
		if ((bool)heldPreview)
		{
			Object.Destroy(heldPreview);
		}
		if ((bool)obj.preview)
		{
			GameObject gameObject = Object.Instantiate(obj.preview, hostArm.holder, worldPositionStays: false);
			gameObject.transform.localPosition += obj.armOffset;
			gameObject.transform.Rotate(obj.armRotationOffset);
			heldPreview = gameObject;
		}
		else
		{
			GameObject gameObject2 = Object.Instantiate(obj.gameObject, hostArm.holder, worldPositionStays: false);
			SandboxUtils.StripForPreview(gameObject2.transform);
			SandboxUtils.SetLayerDeep(gameObject2.transform, hostArm.holder.gameObject.layer);
			gameObject2.SetActive(value: true);
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localScale *= 0.25f;
			heldPreview = gameObject2;
		}
	}

	public virtual void OnDisable()
	{
	}

	public virtual void OnDestroy()
	{
		if ((bool)heldPreview)
		{
			Object.Destroy(heldPreview);
		}
	}

	public virtual void Update()
	{
	}

	public virtual void FixedUpdate()
	{
	}

	public virtual void OnPrimaryDown()
	{
		if (!currentObject)
		{
			hostArm.menu.gameObject.SetActive(value: true);
			return;
		}
		hostArm.StartCoroutine(HandClosedAnimationThing());
		hostArm.jabSound.Play();
		hostArm.animator.SetTrigger(Punch);
	}

	public virtual void OnPrimaryUp()
	{
	}

	public virtual void OnSecondaryDown()
	{
	}

	public virtual void OnSecondaryUp()
	{
	}
}
