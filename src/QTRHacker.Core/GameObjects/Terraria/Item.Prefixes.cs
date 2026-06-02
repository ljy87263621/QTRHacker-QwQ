using QHackLib.Assemble;
using QHackLib.Memory;
using System;
using System.Linq;

namespace QTRHacker.Core.GameObjects.Terraria;

public partial class Item
{
	private const int MaxPrefixId = 97;
	private const uint IntSize = 4;

	public int[] GetCompatiblePrefixes()
	{
		if (!HasValidTypeForGamePrefixLookup())
			return Array.Empty<int>();

		return GetCompatiblePrefixes(Context, Type);
	}

	public static int[] GetCompatiblePrefixes(GameContext context, int type)
	{
		if (!HasValidTypeForGamePrefixLookup(context, type))
			return Array.Empty<int>();

		using MemoryAllocation results = new(context.HContext, MaxPrefixId * IntSize);
		var snippet = AssemblySnippet.FromEmpty();
		nuint itemTypeHandle = context.GameModuleHelper.GetClrType("Terraria.Item").ClrHandle;
		nuint typeofHelper = context.JitHelpersManager.GetJitHelperAddress("CORINFO_HELP_TYPEHANDLE_TO_RUNTIMETYPE");
		nuint createInstance = context.HContext.BCLHelper.GetFunctionAddress(
			"System.Activator",
			m => m.Signature == "System.Activator.CreateInstance(System.Type)");
		nuint setDefaults = context.GameModuleHelper.GetFunctionAddress(
			"Terraria.Item",
			m => m.Signature == "Terraria.Item.SetDefaults(Int32, Terraria.GameContent.Items.ItemVariant)");
		nuint canRollPrefix = context.GameModuleHelper.GetFunctionAddress(
			"Terraria.Item",
			m => m.Signature == "Terraria.Item.CanRollPrefix(Int32)");
		nuint applyPrefix = context.GameModuleHelper.GetFunctionAddress(
			"Terraria.Item",
			m => m.Signature == "Terraria.Item.Prefix(Int32)");
		uint prefixFieldOffset = (uint)IntPtr.Size + context.GameModuleHelper.GetInstanceFieldOffset("Terraria.Item", "prefix");

		snippet.Content.Add(AssemblySnippet.FromASMCode($"""
			push esi
			mov ecx,{itemTypeHandle}
			call {typeofHelper}
			mov ecx,eax
			call {createInstance}
			mov esi,eax
			test esi,esi
			jz qtr_get_prefixes_done
			mov ecx,esi
			mov edx,{type}
			push 0
			call {setDefaults}
			"""));

		for (int prefix = 1; prefix <= MaxPrefixId; prefix++)
		{
			nuint resultAddress = results.AllocationBase + (uint)((prefix - 1) * IntSize);
			snippet.Content.Add(AssemblySnippet.FromASMCode($"""
				mov ecx,esi
				mov edx,{type}
				push 0
				call {setDefaults}
				mov ecx,esi
				mov edx,{prefix}
				call {canRollPrefix}
				test eax,eax
				jz qtr_prefix_{prefix}_done
				mov ecx,esi
				mov edx,{prefix}
				call {applyPrefix}
				xor eax,eax
				mov al,byte ptr [esi+{prefixFieldOffset}]
				cmp eax,{prefix}
				jne qtr_prefix_{prefix}_done
				mov dword ptr [{resultAddress}],1
				qtr_prefix_{prefix}_done:
				"""));
		}

		snippet.Content.Add(AssemblySnippet.FromASMCode("""
			qtr_get_prefixes_done:
			pop esi
			"""));

		if (!context.RunByHookUpdate(snippet, 0x8000))
			return Array.Empty<int>();

		int[] flags = new int[MaxPrefixId];
		context.HContext.DataAccess.Read(results.AllocationBase, flags, (uint)flags.Length);
		return Enumerable.Range(1, MaxPrefixId)
			.Where(prefix => flags[prefix - 1] != 0)
			.ToArray();
	}

	public bool CanApplyPrefix(int prefix)
	{
		if (prefix == 0)
			return true;
		if (prefix < 0 || prefix > MaxPrefixId)
			return false;
		if (!HasValidTypeForGamePrefixLookup())
			return false;

		using MemoryAllocation result = new(Context.HContext, IntSize);
		bool invoked = Context.RunByHookUpdate(
			TypedInternalObject.GetMethodCall("Terraria.Item.CanRollPrefix(Int32)")
				.Call(true, null, result.AllocationBase, new object[] { prefix }));

		return invoked && result.Read<int>(0) != 0;
	}

	private bool HasValidTypeForGamePrefixLookup()
	{
		return HasValidTypeForGamePrefixLookup(Context, Type);
	}

	private static bool HasValidTypeForGamePrefixLookup(GameContext context, int type)
	{
		return type >= 0 && type < GameConstants.GetMaxItemTypes(context);
	}

	private void ApplyPrefixByGame(int prefix)
	{
		Context.RunByHookUpdate(TypedInternalObject.GetMethodCall("Terraria.Item.Prefix(Int32)")
			.Call(true, null, null, new object[] { prefix }));

		if (Prefix != (byte)prefix)
			ResetPrefixByGame();
	}

	private void ResetPrefixSafely()
	{
		if (HasValidTypeForGamePrefixLookup())
		{
			ResetPrefixByGame();
			return;
		}

		Prefix = 0;
	}

	private void ResetPrefixByGame()
	{
		Context.RunByHookUpdate(TypedInternalObject.GetMethodCall("Terraria.Item.ResetPrefix()")
			.Call(true, null, null, Array.Empty<object>()));
	}
}
