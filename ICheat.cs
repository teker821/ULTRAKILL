public interface ICheat
{
	string LongName { get; }

	string Identifier { get; }

	string ButtonEnabledOverride { get; }

	string ButtonDisabledOverride { get; }

	string Icon { get; }

	bool IsActive { get; }

	bool DefaultState { get; }

	StatePersistenceMode PersistenceMode { get; }

	void Enable();

	void Disable();

	void Update();
}
