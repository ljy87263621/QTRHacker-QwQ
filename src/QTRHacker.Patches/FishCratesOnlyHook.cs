using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.FishDropRules;
using Terraria.ID;

namespace QTRHacker.Patches
{
	internal static unsafe class FishCratesOnlyHook
	{
		private static Harmony HarmonyInstance;
		private static bool Installed;
		private static readonly FieldInfo FishingContextField = typeof(Projectile).GetField(
			"_context",
			BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);

		public static void Install()
		{
			if (Installed)
				return;

			try
			{
				MethodInfo setResults = typeof(Projectile).GetMethod(
					"SetFishingCheckResults",
					BindingFlags.Instance | BindingFlags.NonPublic,
					null,
					new[] { typeof(FishingAttempt).MakeByRefType() },
					null);
				MethodInfo setResultsPrefix = typeof(FishCratesOnlyHook).GetMethod(
					nameof(SetFishingCheckResultsPrefix),
					BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo setResultsPostfix = typeof(FishCratesOnlyHook).GetMethod(
					nameof(SetFishingCheckResultsPostfix),
					BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo rollEnemySpawns = typeof(Projectile).GetMethod(
					"FishingCheck_RollEnemySpawns",
					BindingFlags.Instance | BindingFlags.NonPublic,
					null,
					new[] { typeof(FishingAttempt).MakeByRefType() },
					null);
				MethodInfo rollEnemySpawnsPrefix = typeof(FishCratesOnlyHook).GetMethod(
					nameof(FishingCheckRollEnemySpawnsPrefix),
					BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo rollItemDrop = typeof(Projectile).GetMethod(
					"FishingCheck_RollItemDrop",
					BindingFlags.Instance | BindingFlags.NonPublic,
					null,
					new[] { typeof(FishingAttempt).MakeByRefType() },
					null);
				MethodInfo rollItemDropPrefix = typeof(FishCratesOnlyHook).GetMethod(
					nameof(FishingCheckRollItemDropPrefix),
					BindingFlags.Static | BindingFlags.NonPublic);
				if (setResults == null || setResultsPrefix == null || setResultsPostfix == null
					|| rollEnemySpawns == null || rollEnemySpawnsPrefix == null
					|| rollItemDrop == null || rollItemDropPrefix == null)
				{
					LogInstallError("Could not locate fish crates only hook targets.");
					return;
				}

				HarmonyInstance = HarmonyInstance ?? new Harmony("QTRHacker.Patches.FishCratesOnly");
				HarmonyInstance.Patch(
					setResults,
					prefix: new HarmonyMethod(setResultsPrefix),
					postfix: new HarmonyMethod(setResultsPostfix));
				HarmonyInstance.Patch(
					rollEnemySpawns,
					prefix: new HarmonyMethod(rollEnemySpawnsPrefix));
				HarmonyInstance.Patch(
					rollItemDrop,
					prefix: new HarmonyMethod(rollItemDropPrefix));
				Installed = true;
			}
			catch (Exception e)
			{
				LogInstallError($"{e.GetType()}:{e.Message}\n{e.StackTrace}");
			}
		}

		private static void SetFishingCheckResultsPrefix(Projectile __instance, ref FishingAttempt fisher)
		{
			if (!IsEnabled())
				return;

			ForceCrateAttempt(ref fisher);
			SyncFishingContext(__instance, fisher);
		}

		private static void SetFishingCheckResultsPostfix(Projectile __instance, ref FishingAttempt fisher)
		{
			if (!IsEnabled())
				return;

			fisher.rolledEnemySpawn = 0;
			FishingContext context = GetFishingContext(__instance);
			if (fisher.rolledItemDrop <= 0 || !IsFishingCrate(fisher.rolledItemDrop) || HasNonCrateBobberResult(__instance))
			{
				fisher.rolledItemDrop = SelectFallbackCrate(fisher, context);
				WriteBobberItemResult(__instance, fisher);
			}

			SyncFishingContext(__instance, fisher);
		}

		private static bool FishingCheckRollEnemySpawnsPrefix(ref FishingAttempt fisher)
		{
			if (!IsEnabled())
				return true;

			fisher.rolledEnemySpawn = 0;
			return false;
		}

		private static bool FishingCheckRollItemDropPrefix(Projectile __instance, ref FishingAttempt fisher)
		{
			if (!IsEnabled())
				return true;

			ForceCrateAttempt(ref fisher);
			SyncFishingContext(__instance, fisher);
			FishingContext context = GetFishingContext(__instance);
			fisher.rolledItemDrop = context == null ? 0 : Main.FishDropsDB.TryGetItemDropType(context);
			ForceCrateItemDrop(ref fisher, context);
			SyncFishingContext(__instance, fisher);
			return false;
		}

		private static bool IsEnabled()
		{
			return PatchState.GetBool(PatchState.Shared->FishCratesOnly);
		}

		private static void ForceCrateAttempt(ref FishingAttempt fisher)
		{
			fisher.crate = true;
			fisher.junk = false;
			fisher.rolledEnemySpawn = 0;
		}

		private static void ForceCrateItemDrop(ref FishingAttempt fisher, FishingContext context)
		{
			fisher.rolledEnemySpawn = 0;
			if (fisher.rolledItemDrop <= 0 || !IsFishingCrate(fisher.rolledItemDrop))
				fisher.rolledItemDrop = SelectFallbackCrate(fisher, context);
		}

		private static void SyncFishingContext(Projectile projectile, FishingAttempt fisher)
		{
			FishingContext context = GetFishingContext(projectile);
			if (context == null)
				return;

			context.Fisher = fisher;
		}

		private static FishingContext GetFishingContext(Projectile projectile)
		{
			if (FishingContextField == null)
				return null;

			object target = FishingContextField.IsStatic ? null : (object)projectile;
			return FishingContextField.GetValue(target) as FishingContext;
		}

		private static bool HasNonCrateBobberResult(Projectile projectile)
		{
			if (projectile.localAI == null || projectile.localAI.Length <= 1)
				return false;

			int itemType = (int)projectile.localAI[1];
			return itemType != 0 && !IsFishingCrate(itemType);
		}

		private static bool IsFishingCrate(int itemType)
		{
			return itemType > 0
				&& ItemID.Sets.IsFishingCrate != null
				&& itemType < ItemID.Sets.IsFishingCrate.Length
				&& ItemID.Sets.IsFishingCrate[itemType];
		}

		private static void WriteBobberItemResult(Projectile projectile, FishingAttempt fisher)
		{
			if (projectile.ai != null && projectile.ai.Length > 1 && projectile.ai[1] >= 0f)
				projectile.ai[1] = -Math.Max(90f, fisher.fishingLevel);

			if (projectile.localAI != null && projectile.localAI.Length > 2)
			{
				projectile.localAI[1] = fisher.rolledItemDrop;
				projectile.localAI[2] = fisher.playerFishingConditions.BaitItemType;
			}

			projectile.netUpdate = true;
		}

		internal static int SelectFallbackCrate(Projectile projectile)
		{
			FishingContext context = GetFishingContext(projectile);
			if (context == null)
				return SelectTieredFallbackCrate(default(FishingAttempt));

			return SelectFallbackCrate(context.Fisher, context);
		}

		internal static int SelectFallbackCrate(FishingAttempt fisher, FishingContext context)
		{
			if (fisher.inLava && fisher.CanFishInLava)
				return SelectHardmodeCrate(ItemID.LavaCrate, ItemID.LavaCrateHard);

			if (fisher.rare)
			{
				int biomeCrate = SelectBiomeCrate(fisher, context);
				if (biomeCrate > 0)
					return biomeCrate;
			}

			return SelectTieredFallbackCrate(fisher);
		}

		private static int SelectBiomeCrate(FishingAttempt fisher, FishingContext context)
		{
			Player player = context?.Player;
			if (player != null)
			{
				if (player.ZoneDungeon && NPC.downedBoss3)
					return SelectHardmodeCrate(ItemID.DungeonFishingCrate, ItemID.DungeonFishingCrateHard);
				if (player.ZoneBeach || IsOriginalOcean(fisher))
					return SelectHardmodeCrate(ItemID.OceanCrate, ItemID.OceanCrateHard);
				if (player.ZoneHallow)
					return SelectHardmodeCrate(ItemID.HallowedFishingCrate, ItemID.HallowedFishingCrateHard);
			}

			if (context != null)
			{
				if (context.RolledCorruption)
					return SelectHardmodeCrate(ItemID.CorruptFishingCrate, ItemID.CorruptFishingCrateHard);
				if (context.RolledCrimson)
					return SelectHardmodeCrate(ItemID.CrimsonFishingCrate, ItemID.CrimsonFishingCrateHard);
				if (context.RolledJungle)
					return SelectHardmodeCrate(ItemID.JungleFishingCrate, ItemID.JungleFishingCrateHard);
				if (context.RolledSnow)
					return SelectHardmodeCrate(ItemID.FrozenCrate, ItemID.FrozenCrateHard);
				if (context.RolledDesert)
					return SelectHardmodeCrate(ItemID.OasisCrate, ItemID.OasisCrateHard);
				if (context.RolledRemixOcean)
					return SelectHardmodeCrate(ItemID.OceanCrate, ItemID.OceanCrateHard);
			}

			if (fisher.heightLevel == 0)
				return SelectHardmodeCrate(ItemID.FloatingIslandFishingCrate, ItemID.FloatingIslandFishingCrateHard);

			return 0;
		}

		private static int SelectTieredFallbackCrate(FishingAttempt fisher)
		{
			if (fisher.legendary || fisher.veryrare)
				return SelectHardmodeCrate(ItemID.GoldenCrate, ItemID.GoldenCrateHard);
			if (fisher.rare || fisher.uncommon)
				return SelectHardmodeCrate(ItemID.IronCrate, ItemID.IronCrateHard);
			return SelectHardmodeCrate(ItemID.WoodenCrate, ItemID.WoodenCrateHard);
		}

		private static int SelectHardmodeCrate(int earlyModeCrate, int hardModeCrate)
		{
			return Main.hardMode ? hardModeCrate : earlyModeCrate;
		}

		private static bool IsOriginalOcean(FishingAttempt fisher)
		{
			return fisher.heightLevel <= 1
				&& fisher.waterTilesCount > 1000
				&& (fisher.X < 380 || fisher.X > Main.maxTilesX - 380);
		}

		private static void LogInstallError(string message)
		{
			File.AppendAllText("./QTRHacker.Patches.boot.log", $"FishCratesOnlyHook: {message}\n");
		}
	}
}
