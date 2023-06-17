using Sandbox;
using UnityEngine;

public class BrushBlock : SandboxProp
{
	public Vector3 DataSize;

	public BlockType Type;

	public BoxCollider OverrideCollider;

	public SavedBlock SaveBrushBlock()
	{
		SavedBlock obj = new SavedBlock
		{
			BlockSize = new SavedVector3(DataSize),
			BlockType = Type
		};
		SavedGeneric saveObject = obj;
		BaseSave(ref saveObject);
		return obj;
	}
}
