using System;
using UnityEngine;

[Serializable]
public class AuthorLink
{
	public LinkPlatform platform;

	public string username;

	public string displayName;

	[Header("Optional")]
	public string description;
}
