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
			BindingFlags.Instance | BindingFlags.NonPublic);

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
			if (fisher.rolledItemDrop <= 0 || !IsFishingCrate(fisher.rolledItemDrop) || HasNonCrateBobberResult(__instance))
			{
				fisher.rolledItemDrop = SelectFallbackCrate();
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
			ForceCrateItemDrop(ref fisher);
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

		private static void ForceCrateItemDrop(ref FishingAttempt fisher)
		{
			fisher.rolledEnemySpawn = 0;
			if (fisher.rolledItemDrop <= 0 || !IsFishingCrate(fisher.rolledItemDrop))
				fisher.rolledItemDrop = SelectFallbackCrate();
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

			return FishingContextField.GetValue(projectile) as FishingContext;
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

		private static int SelectFallbackCrate()
		{
			const int WoodenCrate = 2334;
			const int PearlwoodCrate = 3979;
			return Main.hardMode ? PearlwoodCrate : WoodenCrate;
		}

		private static void LogInstallError(string message)
		{
			File.AppendAllText("./QTRHacker.Patches.boot.log", $"FishCratesOnlyHook: {message}\n");
		}
	}
}
