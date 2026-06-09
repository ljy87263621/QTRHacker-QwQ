using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;

namespace QTRHacker.Patches
{
	internal static unsafe class FishCratesOnlyHook
	{
		private static Harmony HarmonyInstance;
		private static bool Installed;

		public static void Install()
		{
			if (Installed)
				return;

			try
			{
				MethodInfo original = typeof(Projectile).GetMethod(
					"FishingCheck_RollItemDrop",
					BindingFlags.Instance | BindingFlags.NonPublic,
					null,
					new[] { typeof(FishingAttempt).MakeByRefType() },
					null);
				MethodInfo prefix = typeof(FishCratesOnlyHook).GetMethod(
					nameof(FishingCheckRollItemDropPrefix),
					BindingFlags.Static | BindingFlags.NonPublic);
				if (original == null || prefix == null)
				{
					LogInstallError("Could not locate Projectile.FishingCheck_RollItemDrop or its prefix.");
					return;
				}

				HarmonyInstance = HarmonyInstance ?? new Harmony("QTRHacker.Patches.FishCratesOnly");
				HarmonyInstance.Patch(original, prefix: new HarmonyMethod(prefix));
				Installed = true;
			}
			catch (Exception e)
			{
				LogInstallError($"{e.GetType()}:{e.Message}\n{e.StackTrace}");
			}
		}

		private static void FishingCheckRollItemDropPrefix(ref FishingAttempt fisher)
		{
			if (!PatchState.GetBool(PatchState.Shared->FishCratesOnly))
				return;

			fisher.crate = true;
			fisher.junk = false;
			fisher.rolledEnemySpawn = 0;
		}

		private static void LogInstallError(string message)
		{
			File.AppendAllText("./QTRHacker.Patches.boot.log", $"FishCratesOnlyHook: {message}\n");
		}
	}
}
