using HarmonyLib;
using Terraria;

namespace QTRHacker.Patches.HooksDef
{
	[HarmonyPatch(typeof(Player), "UpdateLuck")]
	public class UpdateLuckHook
	{
		public static void Prefix(Player __instance)
		{
			if (__instance != null && __instance.whoAmI == Main.myPlayer)
				PlayerToggles.ApplyPlayerPropertyInfluenceOverrides(__instance);
		}

		public static void Postfix(Player __instance)
		{
			if (__instance != null && __instance.whoAmI == Main.myPlayer)
				PlayerToggles.ApplyPlayerPropertyFinalOverrides(__instance);
		}
	}
}
