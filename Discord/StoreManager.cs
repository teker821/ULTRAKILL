using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Discord;

public class StoreManager
{
	internal struct FFIEvents
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void EntitlementCreateHandler(IntPtr ptr, ref Entitlement entitlement);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void EntitlementDeleteHandler(IntPtr ptr, ref Entitlement entitlement);

		internal EntitlementCreateHandler OnEntitlementCreate;

		internal EntitlementDeleteHandler OnEntitlementDelete;
	}

	internal struct FFIMethods
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void FetchSkusCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void FetchSkusMethod(IntPtr methodsPtr, IntPtr callbackData, FetchSkusCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void CountSkusMethod(IntPtr methodsPtr, ref int count);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetSkuMethod(IntPtr methodsPtr, long skuId, ref Sku sku);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetSkuAtMethod(IntPtr methodsPtr, int index, ref Sku sku);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void FetchEntitlementsCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void FetchEntitlementsMethod(IntPtr methodsPtr, IntPtr callbackData, FetchEntitlementsCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void CountEntitlementsMethod(IntPtr methodsPtr, ref int count);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetEntitlementMethod(IntPtr methodsPtr, long entitlementId, ref Entitlement entitlement);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetEntitlementAtMethod(IntPtr methodsPtr, int index, ref Entitlement entitlement);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result HasSkuEntitlementMethod(IntPtr methodsPtr, long skuId, ref bool hasEntitlement);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void StartPurchaseCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void StartPurchaseMethod(IntPtr methodsPtr, long skuId, IntPtr callbackData, StartPurchaseCallback callback);

		internal FetchSkusMethod FetchSkus;

		internal CountSkusMethod CountSkus;

		internal GetSkuMethod GetSku;

		internal GetSkuAtMethod GetSkuAt;

		internal FetchEntitlementsMethod FetchEntitlements;

		internal CountEntitlementsMethod CountEntitlements;

		internal GetEntitlementMethod GetEntitlement;

		internal GetEntitlementAtMethod GetEntitlementAt;

		internal HasSkuEntitlementMethod HasSkuEntitlement;

		internal StartPurchaseMethod StartPurchase;
	}

	public delegate void FetchSkusHandler(Result result);

	public delegate void FetchEntitlementsHandler(Result result);

	public delegate void StartPurchaseHandler(Result result);

	public delegate void EntitlementCreateHandler(ref Entitlement entitlement);

	public delegate void EntitlementDeleteHandler(ref Entitlement entitlement);

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

	public event EntitlementCreateHandler OnEntitlementCreate;

	public event EntitlementDeleteHandler OnEntitlementDelete;

	internal StoreManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
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
		events.OnEntitlementCreate = OnEntitlementCreateImpl;
		events.OnEntitlementDelete = OnEntitlementDeleteImpl;
		Marshal.StructureToPtr(events, eventsPtr, fDeleteOld: false);
	}

	[MonoPInvokeCallback]
	private static void FetchSkusCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		FetchSkusHandler obj = (FetchSkusHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void FetchSkus(FetchSkusHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.FetchSkus(MethodsPtr, GCHandle.ToIntPtr(value), FetchSkusCallbackImpl);
	}

	public int CountSkus()
	{
		int count = 0;
		Methods.CountSkus(MethodsPtr, ref count);
		return count;
	}

	public Sku GetSku(long skuId)
	{
		Sku sku = default(Sku);
		Result result = Methods.GetSku(MethodsPtr, skuId, ref sku);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return sku;
	}

	public Sku GetSkuAt(int index)
	{
		Sku sku = default(Sku);
		Result result = Methods.GetSkuAt(MethodsPtr, index, ref sku);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return sku;
	}

	[MonoPInvokeCallback]
	private static void FetchEntitlementsCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		FetchEntitlementsHandler obj = (FetchEntitlementsHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void FetchEntitlements(FetchEntitlementsHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.FetchEntitlements(MethodsPtr, GCHandle.ToIntPtr(value), FetchEntitlementsCallbackImpl);
	}

	public int CountEntitlements()
	{
		int count = 0;
		Methods.CountEntitlements(MethodsPtr, ref count);
		return count;
	}

	public Entitlement GetEntitlement(long entitlementId)
	{
		Entitlement entitlement = default(Entitlement);
		Result result = Methods.GetEntitlement(MethodsPtr, entitlementId, ref entitlement);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return entitlement;
	}

	public Entitlement GetEntitlementAt(int index)
	{
		Entitlement entitlement = default(Entitlement);
		Result result = Methods.GetEntitlementAt(MethodsPtr, index, ref entitlement);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return entitlement;
	}

	public bool HasSkuEntitlement(long skuId)
	{
		bool hasEntitlement = false;
		Result result = Methods.HasSkuEntitlement(MethodsPtr, skuId, ref hasEntitlement);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return hasEntitlement;
	}

	[MonoPInvokeCallback]
	private static void StartPurchaseCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		StartPurchaseHandler obj = (StartPurchaseHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void StartPurchase(long skuId, StartPurchaseHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.StartPurchase(MethodsPtr, skuId, GCHandle.ToIntPtr(value), StartPurchaseCallbackImpl);
	}

	[MonoPInvokeCallback]
	private static void OnEntitlementCreateImpl(IntPtr ptr, ref Entitlement entitlement)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.StoreManagerInstance.OnEntitlementCreate != null)
		{
			discord.StoreManagerInstance.OnEntitlementCreate(ref entitlement);
		}
	}

	[MonoPInvokeCallback]
	private static void OnEntitlementDeleteImpl(IntPtr ptr, ref Entitlement entitlement)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.StoreManagerInstance.OnEntitlementDelete != null)
		{
			discord.StoreManagerInstance.OnEntitlementDelete(ref entitlement);
		}
	}

	public IEnumerable<Entitlement> GetEntitlements()
	{
		int num = CountEntitlements();
		List<Entitlement> list = new List<Entitlement>();
		for (int i = 0; i < num; i++)
		{
			list.Add(GetEntitlementAt(i));
		}
		return list;
	}

	public IEnumerable<Sku> GetSkus()
	{
		int num = CountSkus();
		List<Sku> list = new List<Sku>();
		for (int i = 0; i < num; i++)
		{
			list.Add(GetSkuAt(i));
		}
		return list;
	}
}
