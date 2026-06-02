using QTRHacker.Core.GameObjects;
using System;
using System.Linq;

namespace QTRHacker.Core.GameObjects.Terraria;

public partial class Item
{
	private const int MaxPrefixId = 97;
	private const string PrefixItemSetsTypeName = "Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets";
	private const string PrefixesTypeName = "Terraria.GameContent.Prefixes.PrefixLegacy.Prefixes";
	private const string ItemIdSetsTypeName = "Terraria.ID.ItemID.Sets";

	public int[] GetCompatiblePrefixes()
	{
		if (!HasValidTypeForGamePrefixLookup())
			return Array.Empty<int>();

		return GetCompatiblePrefixes(Context, Type, Accessory, Vanity);
	}

	public static int[] GetCompatiblePrefixes(GameContext context, int type)
	{
		return GetCompatiblePrefixes(context, type, null, null);
	}

	private static int[] GetCompatiblePrefixes(GameContext context, int type, bool? accessory, bool? vanity)
	{
		if (!HasValidTypeForGamePrefixLookup(context, type))
			return Array.Empty<int>();

		try
		{
			if (IsInItemSet(context, PrefixItemSetsTypeName, "SwordsHammersAxesPicks", type))
				return GetPrefixArray(context, "PrefixesForSwords");
			if (IsInItemSet(context, PrefixItemSetsTypeName, "SpearsMacesChainsawsDrillsPunchCannon", type))
				return GetPrefixArray(context, "PrefixesForSpears");
			if (IsInItemSet(context, PrefixItemSetsTypeName, "GunsBows", type))
				return GetPrefixArray(context, "PrefixesForGunsBows");
			if (IsInItemSet(context, PrefixItemSetsTypeName, "Magic", type))
				return GetPrefixArray(context, "PrefixesForMagic");
			if (IsInItemSet(context, PrefixItemSetsTypeName, "Summon", type))
				return GetPrefixArray(context, "PrefixesForSummons");
			if (IsInItemSet(context, PrefixItemSetsTypeName, "BoomerangsChakrams", type))
				return GetPrefixArray(context, "PrefixesForBoomeransAndChakrums");
			if (IsInItemSet(context, PrefixItemSetsTypeName, "ItemsThatCanHaveLegendary2", type))
				return GetPrefixArray(context, "PrefixesForBoomeransAndChakrums_TerrarianYoyo");
			if ((!accessory.HasValue || (accessory.Value && !vanity.GetValueOrDefault()))
				&& IsInItemSet(context, ItemIdSetsTypeName, "CanGetPrefixes", type))
			{
				return GetPrefixArray(context, "PrefixesForAccessories");
			}
		}
		catch
		{
			return Array.Empty<int>();
		}

		return Array.Empty<int>();
	}

	public bool CanApplyPrefix(int prefix)
	{
		if (prefix == 0)
			return true;
		if (prefix < 0 || prefix > MaxPrefixId)
			return false;
		if (!HasValidTypeForGamePrefixLookup())
			return false;

		return GetCompatiblePrefixes().Contains(prefix);
	}

	private static bool IsInItemSet(GameContext context, string typeName, string fieldName, int type)
	{
		var set = new GameObjectArrayV<bool>(context, context.GameModuleHelper.GetStaticHackObject(typeName, fieldName));
		return type >= 0 && type < set.Length && set[type];
	}

	private static int[] GetPrefixArray(GameContext context, string fieldName)
	{
		var prefixes = new GameObjectArrayV<int>(context, context.GameModuleHelper.GetStaticHackObject(PrefixesTypeName, fieldName));
		return prefixes.GetAllElements()
			.Where(prefix => prefix > 0 && prefix <= MaxPrefixId)
			.ToArray();
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
