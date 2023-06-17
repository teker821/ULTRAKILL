using UnityEngine;

public interface IPlaceholdableComponent
{
	void WillReplace(GameObject oldObject, GameObject newObject, bool isSelfBeingReplaced);
}
