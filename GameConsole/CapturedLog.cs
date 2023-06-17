using System;
using UnityEngine;

namespace GameConsole;

[Serializable]
public class CapturedLog
{
	public DateTime time;

	public string message;

	public string stackTrace;

	public ConsoleLogType type;

	public TimeSince timeSinceLogged;

	public bool expanded;

	public CapturedLog(string message, string stackTrace, ConsoleLogType type)
	{
		time = DateTime.Now;
		this.message = message;
		this.stackTrace = stackTrace;
		this.type = type;
		timeSinceLogged = 0f;
	}

	public static ConsoleLogType LogToConLog(LogType type)
	{
		switch (type)
		{
		case LogType.Log:
			return ConsoleLogType.Log;
		case LogType.Warning:
			return ConsoleLogType.Warning;
		case LogType.Error:
		case LogType.Exception:
			return ConsoleLogType.Error;
		default:
			return ConsoleLogType.Log;
		}
	}

	public CapturedLog(string message, string stackTrace, LogType type)
	{
		ConsoleLogType consoleLogType = LogToConLog(type);
		time = DateTime.Now;
		this.message = message;
		this.stackTrace = stackTrace;
		this.type = consoleLogType;
		timeSinceLogged = 0f;
	}
}
