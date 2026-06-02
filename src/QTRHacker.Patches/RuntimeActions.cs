using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Map;

namespace QTRHacker.Patches
{
	public static class RuntimeActions
	{
		public static bool UnlockAllDuplicationsRequested;
		public static bool RevealTheWholeMapRequested;
		public static bool ToggleLanternNightRequested;
		public static int UnlockAllDuplicationsCompleted;
		public static int RevealTheWholeMapCompleted;
		public static int ToggleLanternNightCompleted;
		public static int LastErrorCode;
		public static bool BuffSource_LuckPotion_Enabled;
		public static int BuffSource_LuckPotion_Value;
		public static bool BuffSource_IronskinPotion_Enabled;
		public static bool BuffSource_IronskinPotion_Value;
		public static bool BuffSource_RagePotion_Enabled;
		public static bool BuffSource_RagePotion_Value;
		public static bool BuffSource_WrathPotion_Enabled;
		public static bool BuffSource_WrathPotion_Value;
		public static bool BuffSource_EndurancePotion_Enabled;
		public static bool BuffSource_EndurancePotion_Value;
		public static bool BuffSource_ThornsPotion_Enabled;
		public static bool BuffSource_ThornsPotion_Value;
		public static bool BuffSource_MagicPowerPotion_Enabled;
		public static bool BuffSource_MagicPowerPotion_Value;
		public static bool BuffSource_SwiftnessPotion_Enabled;
		public static bool BuffSource_SwiftnessPotion_Value;
		public static bool BuffSource_SugarRush_Enabled;
		public static bool BuffSource_SugarRush_Value;
		public static bool BuffSource_SummoningPotion_Enabled;
		public static bool BuffSource_SummoningPotion_Value;
		public static bool BuffSource_Bewitched_Enabled;
		public static bool BuffSource_Bewitched_Value;
		public static bool BuffSource_WarTable_Enabled;
		public static bool BuffSource_WarTable_Value;
		public static bool BuffSource_BuilderPotion_Enabled;
		public static bool BuffSource_BuilderPotion_Value;
		public static bool BuffSource_MiningPotion_Enabled;
		public static bool BuffSource_MiningPotion_Value;

		public static void ApplyQueuedActions()
		{
			ReadSharedState();
			ApplyPlayerPropertyBuffSources();
			if (UnlockAllDuplicationsRequested)
			{
				UnlockAllDuplicationsRequested = false;
				unsafe { PatchState.Shared->UnlockAllDuplicationsRequested = 0; }
				Run(UnlockAllDuplications, ref UnlockAllDuplicationsCompleted);
			}
			if (RevealTheWholeMapRequested)
			{
				RevealTheWholeMapRequested = false;
				unsafe { PatchState.Shared->RevealTheWholeMapRequested = 0; }
				Run(RevealTheWholeMap, ref RevealTheWholeMapCompleted);
			}
			if (ToggleLanternNightRequested)
			{
				ToggleLanternNightRequested = false;
				unsafe { PatchState.Shared->ToggleLanternNightRequested = 0; }
				Run(ToggleLanternNight, ref ToggleLanternNightCompleted);
			}
		}

		private static unsafe void ReadSharedState()
		{
			PatchState.State* state = PatchState.Shared;
			UnlockAllDuplicationsRequested = PatchState.GetBool(state->UnlockAllDuplicationsRequested);
			RevealTheWholeMapRequested = PatchState.GetBool(state->RevealTheWholeMapRequested);
			ToggleLanternNightRequested = PatchState.GetBool(state->ToggleLanternNightRequested);
			UnlockAllDuplicationsCompleted = state->UnlockAllDuplicationsCompleted;
			RevealTheWholeMapCompleted = state->RevealTheWholeMapCompleted;
			ToggleLanternNightCompleted = state->ToggleLanternNightCompleted;
			LastErrorCode = state->LastErrorCode;
			BuffSource_LuckPotion_Enabled = PatchState.GetBool(state->BuffSource_LuckPotion_Enabled);
			BuffSource_LuckPotion_Value = state->BuffSource_LuckPotion_Value;
			BuffSource_IronskinPotion_Enabled = PatchState.GetBool(state->BuffSource_IronskinPotion_Enabled);
			BuffSource_IronskinPotion_Value = PatchState.GetBool(state->BuffSource_IronskinPotion_Value);
			BuffSource_RagePotion_Enabled = PatchState.GetBool(state->BuffSource_RagePotion_Enabled);
			BuffSource_RagePotion_Value = PatchState.GetBool(state->BuffSource_RagePotion_Value);
			BuffSource_WrathPotion_Enabled = PatchState.GetBool(state->BuffSource_WrathPotion_Enabled);
			BuffSource_WrathPotion_Value = PatchState.GetBool(state->BuffSource_WrathPotion_Value);
			BuffSource_EndurancePotion_Enabled = PatchState.GetBool(state->BuffSource_EndurancePotion_Enabled);
			BuffSource_EndurancePotion_Value = PatchState.GetBool(state->BuffSource_EndurancePotion_Value);
			BuffSource_ThornsPotion_Enabled = PatchState.GetBool(state->BuffSource_ThornsPotion_Enabled);
			BuffSource_ThornsPotion_Value = PatchState.GetBool(state->BuffSource_ThornsPotion_Value);
			BuffSource_MagicPowerPotion_Enabled = PatchState.GetBool(state->BuffSource_MagicPowerPotion_Enabled);
			BuffSource_MagicPowerPotion_Value = PatchState.GetBool(state->BuffSource_MagicPowerPotion_Value);
			BuffSource_SwiftnessPotion_Enabled = PatchState.GetBool(state->BuffSource_SwiftnessPotion_Enabled);
			BuffSource_SwiftnessPotion_Value = PatchState.GetBool(state->BuffSource_SwiftnessPotion_Value);
			BuffSource_SugarRush_Enabled = PatchState.GetBool(state->BuffSource_SugarRush_Enabled);
			BuffSource_SugarRush_Value = PatchState.GetBool(state->BuffSource_SugarRush_Value);
			BuffSource_SummoningPotion_Enabled = PatchState.GetBool(state->BuffSource_SummoningPotion_Enabled);
			BuffSource_SummoningPotion_Value = PatchState.GetBool(state->BuffSource_SummoningPotion_Value);
			BuffSource_Bewitched_Enabled = PatchState.GetBool(state->BuffSource_Bewitched_Enabled);
			BuffSource_Bewitched_Value = PatchState.GetBool(state->BuffSource_Bewitched_Value);
			BuffSource_WarTable_Enabled = PatchState.GetBool(state->BuffSource_WarTable_Enabled);
			BuffSource_WarTable_Value = PatchState.GetBool(state->BuffSource_WarTable_Value);
			BuffSource_BuilderPotion_Enabled = PatchState.GetBool(state->BuffSource_BuilderPotion_Enabled);
			BuffSource_BuilderPotion_Value = PatchState.GetBool(state->BuffSource_BuilderPotion_Value);
			BuffSource_MiningPotion_Enabled = PatchState.GetBool(state->BuffSource_MiningPotion_Enabled);
			BuffSource_MiningPotion_Value = PatchState.GetBool(state->BuffSource_MiningPotion_Value);
		}

		public static void ApplyPlayerPropertyBuffSources()
		{
			Player player = Main.LocalPlayer;
			if (Main.gameMenu || player == null || !player.active)
				return;

			if (BuffSource_LuckPotion_Enabled)
				ApplyLuckPotionBuffSource(player, BuffSource_LuckPotion_Value);
			if (BuffSource_IronskinPotion_Enabled)
				ApplyBoolBuffSource(player, BuffID.Ironskin, BuffSource_IronskinPotion_Value, 3600);
			if (BuffSource_RagePotion_Enabled)
				ApplyBoolBuffSource(player, BuffID.Rage, BuffSource_RagePotion_Value, 3600);
			if (BuffSource_WrathPotion_Enabled)
				ApplyBoolBuffSource(player, BuffID.Wrath, BuffSource_WrathPotion_Value, 3600);
			if (BuffSource_EndurancePotion_Enabled)
				ApplyBoolBuffSource(player, BuffID.Endurance, BuffSource_EndurancePotion_Value, 3600);
			if (BuffSource_ThornsPotion_Enabled)
				ApplyBoolBuffSource(player, BuffID.Thorns, BuffSource_ThornsPotion_Value, 3600);
			if (BuffSource_MagicPowerPotion_Enabled)
				ApplyBoolBuffSource(player, BuffID.MagicPower, BuffSource_MagicPowerPotion_Value, 3600);
			if (BuffSource_SwiftnessPotion_Enabled)
				ApplyBoolBuffSource(player, BuffID.Swiftness, BuffSource_SwiftnessPotion_Value, 3600);
			if (BuffSource_SugarRush_Enabled)
				ApplyBoolBuffSource(player, BuffID.SugarRush, BuffSource_SugarRush_Value, 3600);
			if (BuffSource_SummoningPotion_Enabled)
				ApplyBoolBuffSource(player, BuffID.Summoning, BuffSource_SummoningPotion_Value, 3600);
			if (BuffSource_Bewitched_Enabled)
				ApplyBoolBuffSource(player, BuffID.Bewitched, BuffSource_Bewitched_Value, 3600);
			if (BuffSource_WarTable_Enabled)
				ApplyBoolBuffSource(player, BuffID.WarTable, BuffSource_WarTable_Value, 3600);
			if (BuffSource_BuilderPotion_Enabled)
				ApplyBoolBuffSource(player, BuffID.Builder, BuffSource_BuilderPotion_Value, 3600);
			if (BuffSource_MiningPotion_Enabled)
				ApplyBoolBuffSource(player, BuffID.Mining, BuffSource_MiningPotion_Value, 3600);
		}

		private static void ApplyBoolBuffSource(Player player, int buffType, bool enabled, int targetTime)
		{
			if (!enabled)
			{
				RemoveBuff(player, buffType);
				return;
			}

			int index = player.FindBuffIndex(buffType);
			if (index < 0 || player.buffTime[index] < targetTime / 2)
				player.AddBuff(buffType, targetTime);
		}

		private static void ApplyLuckPotionBuffSource(Player player, int level)
		{
			level = Math.Max(0, Math.Min(3, level));
			if (level == 0)
			{
				RemoveBuff(player, BuffID.Lucky);
				return;
			}

			int targetTime;
			switch (level)
			{
				case 1:
					targetTime = 18000;
					break;
				case 2:
					targetTime = 36000;
					break;
				default:
					targetTime = 54000;
					break;
			}
			int index = player.FindBuffIndex(BuffID.Lucky);
			if (index >= 0 && player.buffTime[index] > GetMaxLuckPotionTimeForLevel(level))
				player.DelBuff(index);

			index = player.FindBuffIndex(BuffID.Lucky);
			if (index < 0 || player.buffTime[index] < targetTime / 2)
				player.AddBuff(BuffID.Lucky, targetTime);
		}

		private static int GetMaxLuckPotionTimeForLevel(int level)
		{
			switch (level)
			{
				case 1:
					return 18000;
				case 2:
					return 36000;
				default:
					return int.MaxValue;
			}
		}

		private static void RemoveBuff(Player player, int buffType)
		{
			int index;
			while ((index = player.FindBuffIndex(buffType)) >= 0)
				player.DelBuff(index);
		}

		private static void UnlockAllDuplications()
		{
			if (Main.gameMenu || Main.LocalPlayer?.creativeTracker?.ItemSacrifices == null)
				return;

			var sacrifices = Main.LocalPlayer.creativeTracker.ItemSacrifices;
			for (int itemId = 0; itemId < ItemID.Count; itemId++)
				sacrifices.RegisterItemSacrifice(itemId, 9999);
		}

		private static void RevealTheWholeMap()
		{
			if (Main.gameMenu || Main.Map == null)
				return;

			int margin = WorldMap.BlackEdgeWidth;
			int maxX = Math.Max(margin, Main.maxTilesX - margin);
			int maxY = Math.Max(margin, Main.maxTilesY - margin);
			for (int x = margin; x < maxX; x++)
			{
				for (int y = margin; y < maxY; y++)
					Main.Map.UpdateLighting(x, y, byte.MaxValue);
			}
			Main.refreshMap = true;
		}

		private static void ToggleLanternNight()
		{
			if (Main.gameMenu)
				return;
			LanternNight.ToggleManualLanterns();
			LanternNight.UpdateTime();
		}

		private static void Run(Action action, ref int completed)
		{
			try
			{
				action();
				completed++;
				LastErrorCode = 0;
				WriteSharedCounters();
			}
			catch
			{
				LastErrorCode = 1;
				WriteSharedCounters();
			}
		}

		private static unsafe void WriteSharedCounters()
		{
			PatchState.Shared->UnlockAllDuplicationsCompleted = UnlockAllDuplicationsCompleted;
			PatchState.Shared->RevealTheWholeMapCompleted = RevealTheWholeMapCompleted;
			PatchState.Shared->ToggleLanternNightCompleted = ToggleLanternNightCompleted;
			PatchState.Shared->LastErrorCode = LastErrorCode;
		}
	}
}
