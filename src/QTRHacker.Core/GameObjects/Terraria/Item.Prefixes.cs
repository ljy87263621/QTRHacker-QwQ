using QTRHacker.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QTRHacker.Core.GameObjects.Terraria;

public partial class Item
{
	private const int MaxPrefixId = 97;
	private const string PrefixItemSetsTypeName = "Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets";
	private const string ItemIdSetsTypeName = "Terraria.ID.ItemID.Sets";
	private static readonly int[] PrefixesForSwords = new int[]
	{
		1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
		11, 12, 13, 14, 15, 36, 37, 38, 53, 54,
		55, 39, 40, 56, 41, 57, 42, 43, 44, 45,
		46, 47, 48, 49, 50, 51, 59, 60, 61, 81
	};
	private static readonly int[] PrefixesForSpears = new int[]
	{
		36, 37, 38, 53, 54, 55, 39, 40, 56, 41,
		57, 59, 60, 61
	};
	private static readonly int[] PrefixesForGunsBows = new int[]
	{
		16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
		58, 36, 37, 38, 53, 54, 55, 39, 40, 56,
		41, 57, 42, 44, 45, 46, 47, 48, 49, 50,
		51, 59, 60, 61, 82
	};
	private static readonly int[] PrefixesForMagic = new int[]
	{
		26, 27, 28, 29, 30, 31, 32, 33, 34, 35,
		52, 36, 37, 38, 53, 54, 55, 39, 40, 56,
		41, 57, 42, 43, 44, 45, 46, 47, 48, 49,
		50, 51, 59, 60, 61, 83
	};
	private static readonly int[] PrefixesForSummons = new int[]
	{
		85, 86, 87, 88, 89, 90, 91, 92, 93, 94,
		95, 96, 97, 55, 38, 54, 53, 57, 40, 56,
		41, 39
	};
	private static readonly int[] PrefixesForBoomerangsAndChakrams = new int[]
	{
		36, 37, 38, 53, 54, 55, 39, 40, 56, 41,
		57, 59, 60, 61
	};
	private static readonly int[] PrefixesForBoomerangsAndChakramsTerrarianYoyo = new int[]
	{
		36, 37, 38, 53, 54, 55, 39, 40, 56, 41,
		57, 59, 60, 61, 84
	};
	private static readonly int[] PrefixesForAccessories = new int[]
	{
		62, 63, 64, 65, 66, 67, 68, 69, 70, 71,
		72, 73, 74, 75, 76, 77, 78, 79, 80
	};
	private static readonly object CompatiblePrefixesCacheLock = new();
	private static readonly Dictionary<(int ProcessId, int Type), int[]> CompatiblePrefixesCache = new();

	public int[] GetCompatiblePrefixes()
	{
		if (!HasValidTypeForGamePrefixLookup())
			return Array.Empty<int>();

		int[] prefixes = GetCompatiblePrefixes(Context, Type, Accessory, Vanity);
		if (prefixes.Length > 0)
			return prefixes;

		return GetCompatiblePrefixesFromItemFields();
	}

	public static int[] GetCompatiblePrefixes(GameContext context, int type)
	{
		return GetCompatiblePrefixes(context, type, null, null);
	}

	private static int[] GetCompatiblePrefixes(GameContext context, int type, bool? accessory, bool? vanity)
	{
		if (!HasValidTypeForGamePrefixLookup(context, type))
			return Array.Empty<int>();

		var key = (context.GameProcess.Id, type);
		lock (CompatiblePrefixesCacheLock)
		{
			if (CompatiblePrefixesCache.TryGetValue(key, out int[] cached))
				return cached.ToArray();
		}

		int[] prefixes = GetCompatiblePrefixesUncached(context, type, accessory, vanity);
		if (prefixes.Length > 0)
		{
			lock (CompatiblePrefixesCacheLock)
				CompatiblePrefixesCache[key] = prefixes;
		}
		return prefixes.ToArray();
	}

	private static int[] GetCompatiblePrefixesUncached(GameContext context, int type, bool? accessory, bool? vanity)
	{
		try
		{
			int[] prefixes;
			if (IsInItemSet(context, PrefixItemSetsTypeName, "SwordsHammersAxesPicks", type))
				prefixes = PrefixesForSwords;
			else if (IsInItemSet(context, PrefixItemSetsTypeName, "SpearsMacesChainsawsDrillsPunchCannon", type))
				prefixes = PrefixesForSpears;
			else if (IsInItemSet(context, PrefixItemSetsTypeName, "GunsBows", type))
				prefixes = PrefixesForGunsBows;
			else if (IsInItemSet(context, PrefixItemSetsTypeName, "Magic", type))
				prefixes = PrefixesForMagic;
			else if (IsInItemSet(context, PrefixItemSetsTypeName, "Summon", type))
				prefixes = PrefixesForSummons;
			else if (IsInItemSet(context, PrefixItemSetsTypeName, "ItemsThatCanHaveLegendary2", type))
				prefixes = PrefixesForBoomerangsAndChakramsTerrarianYoyo;
			else if (IsInItemSet(context, PrefixItemSetsTypeName, "BoomerangsChakrams", type))
				prefixes = PrefixesForBoomerangsAndChakrams;
			else if ((!accessory.HasValue || (accessory.Value && !vanity.GetValueOrDefault()))
				&& IsInItemSet(context, ItemIdSetsTypeName, "CanGetPrefixes", type))
				prefixes = PrefixesForAccessories;
			else
				return Array.Empty<int>();

			return prefixes.ToArray();
		}
		catch
		{
			return Array.Empty<int>();
		}
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

	private int[] GetCompatiblePrefixesFromItemFields()
	{
		if (Summon)
			return PrefixesForSummons.ToArray();
		if (Magic)
			return PrefixesForMagic.ToArray();
		if (Ranged)
			return PrefixesForGunsBows.ToArray();
		if (Melee || Pick > 0 || Axe > 0 || Hammer > 0)
			return PrefixesForSwords.ToArray();
		if (Accessory && !Vanity)
			return PrefixesForAccessories.ToArray();

		return Array.Empty<int>();
	}

	private static bool IsInItemSet(GameContext context, string typeName, string fieldName, int type)
	{
		var set = new GameObjectArrayV<bool>(context, context.GameModuleHelper.GetStaticHackObject(typeName, fieldName));
		return type >= 0 && type < set.Length && set[type];
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
