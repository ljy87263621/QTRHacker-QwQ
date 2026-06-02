using System;
using System.Runtime.InteropServices;
using System.Security;

namespace QTRHacker.Patches
{
	internal static unsafe class PatchState
	{
		public const string SignatureText = "QTRHackerPatchState1456";
		public const int SignatureSize = 32;
		public const int Version = 7;
		public static readonly State* Shared;

		static PatchState()
		{
			Shared = (State*)VirtualAlloc(
				IntPtr.Zero,
				(UIntPtr)sizeof(State),
				AllocationType.Commit | AllocationType.Reserve,
				MemoryProtection.ExecuteReadWrite);
			if (Shared == null)
				throw new InvalidOperationException("Could not allocate QTRHacker patch state.");
			*Shared = default;
			for (int i = 0; i < SignatureText.Length && i < SignatureSize - 1; i++)
				Shared->Signature[i] = (byte)SignatureText[i];
			Shared->Version = Version;
			Shared->Initialized = 1;
			Shared->AimBot_TargetedPlayerIndex = -1;
			Shared->AimBot_MaxDistance_NPC = 9600f;
			Shared->AimBot_HostileNPCsOnly = 1;
			Shared->AimBot_MaxDistance_Player = 9600f;
			Shared->AimBot_HostilePlayersOnly = 1;
		}

		public static bool GetBool(int value) => value != 0;
		public static int SetBool(bool value) => value ? 1 : 0;

		[DllImport("kernel32.dll", SetLastError = true)]
		[SuppressUnmanagedCodeSecurity]
		private static extern void* VirtualAlloc(
			IntPtr lpAddress,
			UIntPtr dwSize,
			AllocationType flAllocationType,
			MemoryProtection flProtect);

		[Flags]
		private enum AllocationType : uint
		{
			Commit = 0x1000,
			Reserve = 0x2000
		}

		private enum MemoryProtection : uint
		{
			ExecuteReadWrite = 0x40
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public unsafe struct State
		{
			public fixed byte Signature[SignatureSize];
			public int Version;
			public int Initialized;

			public int InfiniteLife;
			public int InfiniteMana;
			public int InfiniteOxygen;
			public int InfiniteMinion;
			public int InfiniteAmmo;
			public int InfiniteFlyTime;
			public int CreativeMenu;
			public int ImmuneToDebuffs;
			public int SlowFall;
			public int FastSpeed;
			public int SuperGrabRange;
			public int CoinPortalDropsBags;
			public int FishCratesOnly;
			public int BonusTwoSlots;
			public int HighLight;
			public int SuperRange;
			public int FastTileAndWallPlacingSpeed;
			public int MechanicalRuler;
			public int MechanicalLens;
			public int RightClickToTP;
			public int EnableAllRecipes;
			public int StrengthenVampireKnives;

			public int UnlockAllDuplicationsRequested;
			public int RevealTheWholeMapRequested;
			public int ToggleLanternNightRequested;
			public int UnlockAllDuplicationsCompleted;
			public int RevealTheWholeMapCompleted;
			public int ToggleLanternNightCompleted;
			public int LastErrorCode;

			public int AimBot_Mode;
			public int AimBot_TargetedPlayerIndex;
			public float AimBot_MaxDistance_NPC;
			public int AimBot_HostileNPCsOnly;
			public float AimBot_MaxDistance_Player;
			public int AimBot_HostilePlayersOnly;

			public int AutoFishing_Mode;
			public int AutoFishing_CratesOnly;
			public int AutoFishing_QuestItemsOnly;

			public int Override_StatDefense_Enabled;
			public int Override_StatDefense_Value;
			public int Override_ArmorPenetration_Enabled;
			public int Override_ArmorPenetration_Value;
			public int Override_MeleeCrit_Enabled;
			public int Override_MeleeCrit_Value;
			public int Override_RangedCrit_Enabled;
			public int Override_RangedCrit_Value;
			public int Override_MagicCrit_Enabled;
			public int Override_MagicCrit_Value;
			public int Override_MeleeDamage_Enabled;
			public float Override_MeleeDamage_Value;
			public int Override_RangedDamage_Enabled;
			public float Override_RangedDamage_Value;
			public int Override_MagicDamage_Enabled;
			public float Override_MagicDamage_Value;
			public int Override_MinionDamage_Enabled;
			public float Override_MinionDamage_Value;
			public int Override_RocketDamage_Enabled;
			public float Override_RocketDamage_Value;
			public int Override_Endurance_Enabled;
			public float Override_Endurance_Value;
			public int Override_Thorns_Enabled;
			public float Override_Thorns_Value;

			public int Override_MoveSpeed_Enabled;
			public float Override_MoveSpeed_Value;
			public int Override_MaxRunSpeed_Enabled;
			public float Override_MaxRunSpeed_Value;
			public int Override_AccRunSpeed_Enabled;
			public float Override_AccRunSpeed_Value;
			public int Override_RunAcceleration_Enabled;
			public float Override_RunAcceleration_Value;
			public int Override_JumpSpeedBoost_Enabled;
			public float Override_JumpSpeedBoost_Value;
			public int Override_WingTime_Enabled;
			public float Override_WingTime_Value;
			public int Override_WingTimeMax_Enabled;
			public int Override_WingTimeMax_Value;
			public int Override_RocketTime_Enabled;
			public int Override_RocketTime_Value;
			public int Override_RocketTimeMax_Enabled;
			public int Override_RocketTimeMax_Value;
			public int Override_GravDir_Enabled;
			public float Override_GravDir_Value;

			public int Override_FishingSkill_Enabled;
			public int Override_FishingSkill_Value;
			public int Override_Luck_Enabled;
			public float Override_Luck_Value;
			public int Override_TorchLuck_Enabled;
			public float Override_TorchLuck_Value;
			public int Override_CoinLuck_Enabled;
			public float Override_CoinLuck_Value;
			public int Override_EquipmentBasedLuckBonus_Enabled;
			public float Override_EquipmentBasedLuckBonus_Value;
			public int Override_LuckPotion_Enabled;
			public int Override_LuckPotion_Value;
			public int Override_KiteLuckLevel_Enabled;
			public int Override_KiteLuckLevel_Value;
			public int Override_LadyBugLuckTimeLeft_Enabled;
			public int Override_LadyBugLuckTimeLeft_Value;
			public int Override_AccFishingLine_Enabled;
			public int Override_AccFishingLine_Value;
			public int Override_AccFishingBobber_Enabled;
			public int Override_AccFishingBobber_Value;
			public int Override_AccTackleBox_Enabled;
			public int Override_AccTackleBox_Value;
			public int Override_AccLavaFishing_Enabled;
			public int Override_AccLavaFishing_Value;
			public int Override_CratePotion_Enabled;
			public int Override_CratePotion_Value;
			public int Override_SonarPotion_Enabled;
			public int Override_SonarPotion_Value;
			public int Override_BrokenMirrorBadLuckTime_Enabled;
			public int Override_BrokenMirrorBadLuckTime_Value;
			public int Override_HasGardenGnomeNearby_Enabled;
			public int Override_HasGardenGnomeNearby_Value;
			public int Override_Stinky_Enabled;
			public int Override_Stinky_Value;
			public int Override_HasLuck_LuckyClover_Enabled;
			public int Override_HasLuck_LuckyClover_Value;
			public int Override_HasLuck_LuckyCoin_Enabled;
			public int Override_HasLuck_LuckyCoin_Value;
			public int Override_HasLuck_LuckyHorseshoe_Enabled;
			public int Override_HasLuck_LuckyHorseshoe_Value;
			public int Override_HasLuck_RavenFeather_Enabled;
			public int Override_HasLuck_RavenFeather_Value;
			public int Override_HasLuck_WiltedClover_Enabled;
			public int Override_HasLuck_WiltedClover_Value;

			public int Override_MaxMinions_Enabled;
			public int Override_MaxMinions_Value;
			public int Override_MaxTurrets_Enabled;
			public int Override_MaxTurrets_Value;
			public int Override_TileRangeX_Enabled;
			public int Override_TileRangeX_Value;
			public int Override_TileRangeY_Enabled;
			public int Override_TileRangeY_Value;
			public int Override_TileSpeed_Enabled;
			public float Override_TileSpeed_Value;
			public int Override_WallSpeed_Enabled;
			public float Override_WallSpeed_Value;
			public int Override_PickSpeed_Enabled;
			public float Override_PickSpeed_Value;
			public int Override_BlockRange_Enabled;
			public int Override_BlockRange_Value;

			public int BuffSource_FishingPotion_Enabled;
			public int BuffSource_FishingPotion_Value;
			public int BuffSource_LuckPotion_Enabled;
			public int BuffSource_LuckPotion_Value;
			public int BuffSource_CratePotion_Enabled;
			public int BuffSource_CratePotion_Value;
			public int BuffSource_SonarPotion_Enabled;
			public int BuffSource_SonarPotion_Value;
			public int BuffSource_IronskinPotion_Enabled;
			public int BuffSource_IronskinPotion_Value;
			public int BuffSource_RagePotion_Enabled;
			public int BuffSource_RagePotion_Value;
			public int BuffSource_WrathPotion_Enabled;
			public int BuffSource_WrathPotion_Value;
			public int BuffSource_EndurancePotion_Enabled;
			public int BuffSource_EndurancePotion_Value;
			public int BuffSource_ThornsPotion_Enabled;
			public int BuffSource_ThornsPotion_Value;
			public int BuffSource_MagicPowerPotion_Enabled;
			public int BuffSource_MagicPowerPotion_Value;
			public int BuffSource_SwiftnessPotion_Enabled;
			public int BuffSource_SwiftnessPotion_Value;
			public int BuffSource_SugarRush_Enabled;
			public int BuffSource_SugarRush_Value;
			public int BuffSource_SummoningPotion_Enabled;
			public int BuffSource_SummoningPotion_Value;
			public int BuffSource_Bewitched_Enabled;
			public int BuffSource_Bewitched_Value;
			public int BuffSource_WarTable_Enabled;
			public int BuffSource_WarTable_Value;
			public int BuffSource_BuilderPotion_Enabled;
			public int BuffSource_BuilderPotion_Value;
			public int BuffSource_MiningPotion_Enabled;
			public int BuffSource_MiningPotion_Value;
		}
	}
}
