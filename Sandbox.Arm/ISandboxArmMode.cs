namespace Sandbox.Arm;

public interface ISandboxArmMode
{
	string Name { get; }

	bool CanOpenMenu { get; }

	bool Raycast { get; }

	string Icon { get; }

	void OnEnable(SandboxArm hostArm);

	void OnDisable();

	void OnDestroy();

	void Update();

	void FixedUpdate();

	void OnPrimaryDown();

	void OnPrimaryUp();

	void OnSecondaryDown();

	void OnSecondaryUp();
}
