using System;
using System.Runtime.InteropServices;

namespace Discord;

public class UserManager
{
	internal struct FFIEvents
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void CurrentUserUpdateHandler(IntPtr ptr);

		internal CurrentUserUpdateHandler OnCurrentUserUpdate;
	}

	internal struct FFIMethods
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetCurrentUserMethod(IntPtr methodsPtr, ref User currentUser);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void GetUserCallback(IntPtr ptr, Result result, ref User user);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void GetUserMethod(IntPtr methodsPtr, long userId, IntPtr callbackData, GetUserCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetCurrentUserPremiumTypeMethod(IntPtr methodsPtr, ref PremiumType premiumType);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result CurrentUserHasFlagMethod(IntPtr methodsPtr, UserFlag flag, ref bool hasFlag);

		internal GetCurrentUserMethod GetCurrentUser;

		internal GetUserMethod GetUser;

		internal GetCurrentUserPremiumTypeMethod GetCurrentUserPremiumType;

		internal CurrentUserHasFlagMethod CurrentUserHasFlag;
	}

	public delegate void GetUserHandler(Result result, ref User user);

	public delegate void CurrentUserUpdateHandler();

	private IntPtr MethodsPtr;

	private object MethodsStructure;

	private FFIMethods Methods
	{
		get
		{
			if (MethodsStructure == null)
			{
				MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
			}
			return (FFIMethods)MethodsStructure;
		}
	}

	public event CurrentUserUpdateHandler OnCurrentUserUpdate;

	internal UserManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
	{
		if (eventsPtr == IntPtr.Zero)
		{
			throw new ResultException(Result.InternalError);
		}
		InitEvents(eventsPtr, ref events);
		MethodsPtr = ptr;
		if (MethodsPtr == IntPtr.Zero)
		{
			throw new ResultException(Result.InternalError);
		}
	}

	private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
	{
		events.OnCurrentUserUpdate = OnCurrentUserUpdateImpl;
		Marshal.StructureToPtr(events, eventsPtr, fDeleteOld: false);
	}

	public User GetCurrentUser()
	{
		User currentUser = default(User);
		Result result = Methods.GetCurrentUser(MethodsPtr, ref currentUser);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return currentUser;
	}

	[MonoPInvokeCallback]
	private static void GetUserCallbackImpl(IntPtr ptr, Result result, ref User user)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		GetUserHandler obj = (GetUserHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result, ref user);
	}

	public void GetUser(long userId, GetUserHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.GetUser(MethodsPtr, userId, GCHandle.ToIntPtr(value), GetUserCallbackImpl);
	}

	public PremiumType GetCurrentUserPremiumType()
	{
		PremiumType premiumType = PremiumType.None;
		Result result = Methods.GetCurrentUserPremiumType(MethodsPtr, ref premiumType);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return premiumType;
	}

	public bool CurrentUserHasFlag(UserFlag flag)
	{
		bool hasFlag = false;
		Result result = Methods.CurrentUserHasFlag(MethodsPtr, flag, ref hasFlag);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return hasFlag;
	}

	[MonoPInvokeCallback]
	private static void OnCurrentUserUpdateImpl(IntPtr ptr)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.UserManagerInstance.OnCurrentUserUpdate != null)
		{
			discord.UserManagerInstance.OnCurrentUserUpdate();
		}
	}
}
