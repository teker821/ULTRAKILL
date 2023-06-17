using System;
using UnityEngine;

public class PolymorphicFieldAttribute : PropertyAttribute
{
	public Type baseType;

	public PolymorphicFieldAttribute(Type baseType)
	{
		this.baseType = baseType;
	}
}
