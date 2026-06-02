using QHackLib.Memory;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace QTRHacker.Core;

internal sealed class RemotePatchState
{
	private const string SignatureText = "QTRHackerPatchState1456";
	private const int SignatureSize = 32;
	private const int ExpectedVersion = 7;
	private readonly GameContext context;
	private nuint baseAddress;

	public RemotePatchState(GameContext context)
	{
		this.context = context;
	}

	public bool IsInitialized => TryFind(false);

	public void EnsureInitialized()
	{
		if (!TryFind(true))
			throw new InvalidOperationException("QTRHacker.Patches shared state was not initialized.");
	}

	public int GetInt(Field field)
	{
		EnsureInitialized();
		return context.HContext.DataAccess.Read<int>(AddressOf(field));
	}

	public void SetInt(Field field, int value)
	{
		EnsureInitialized();
		context.HContext.DataAccess.Write(AddressOf(field), value);
	}

	public bool GetBool(Field field) => GetInt(field) != 0;

	public void SetBool(Field field, bool value) => SetInt(field, value ? 1 : 0);

	public float GetFloat(Field field)
	{
		EnsureInitialized();
		return context.HContext.DataAccess.Read<float>(AddressOf(field));
	}

	public void SetFloat(Field field, float value)
	{
		EnsureInitialized();
		context.HContext.DataAccess.Write(AddressOf(field), value);
	}

	public nuint GetFieldAddress(Field field)
	{
		EnsureInitialized();
		return AddressOf(field);
	}

	private bool TryFind(bool retry)
	{
		if (baseAddress != 0 && IsValid(baseAddress))
			return true;

		byte[] signature = GetSignature();
		int attempts = retry ? 50 : 1;
		for (int i = 0; i < attempts; i++)
		{
			baseAddress = AobscanHelper.Aobscan(context.HContext.Handle, signature).FirstOrDefault(IsValid);
			if (baseAddress != 0)
				return true;
			if (retry)
				Thread.Sleep(100);
		}
		return false;
	}

	private bool IsValid(nuint address)
	{
		if (address == 0)
			return false;
		try
		{
			int version = context.HContext.DataAccess.Read<int>(address + SignatureSize);
			int initialized = context.HContext.DataAccess.Read<int>(address + SignatureSize + sizeof(int));
			return version == ExpectedVersion && initialized != 0;
		}
		catch
		{
			return false;
		}
	}

	private nuint AddressOf(Field field) => baseAddress + (uint)Marshal.OffsetOf<State>(field.ToString()).ToInt32();

	private static byte[] GetSignature()
	{
		byte[] signature = new byte[SignatureSize];
		Encoding.ASCII.GetBytes(SignatureText, signature);
		return signature;
	}

	public enum Field
	{
		Initialized,
		InfiniteLife,
		InfiniteMana,
		InfiniteOxygen,
		InfiniteMinion,
		InfiniteAmmo,
		InfiniteFlyTime,
		CreativeMenu,
		ImmuneToDebuffs,
		SlowFall,
		FastSpeed,
		SuperGrabRange,
		CoinPortalDropsBags,
		FishCratesOnly,
		BonusTwoSlots,
		HighLight,
		SuperRange,
		FastTileAndWallPlacingSpeed,
		MechanicalRuler,
		MechanicalLens,
		RightClickToTP,
		EnableAllRecipes,
		StrengthenVampireKnives,
		UnlockAllDuplicationsRequested,
		RevealTheWholeMapRequested,
		ToggleLanternNightRequested,
		UnlockAllDuplicationsCompleted,
		RevealTheWholeMapCompleted,
		ToggleLanternNightCompleted,
		LastErrorCode,
		AimBot_Mode,
		AimBot_TargetedPlayerIndex,
		AimBot_MaxDistance_NPC,
		AimBot_HostileNPCsOnly,
		AimBot_MaxDistance_Player,
		AimBot_HostilePlayersOnly,
		AutoFishing_Mode,
		AutoFishing_CratesOnly,
		AutoFishing_QuestItemsOnly,
		Override_StatDefense_Enabled,
		Override_StatDefense_Value,
		Override_ArmorPenetration_Enabled,
		Override_ArmorPenetration_Value,
		Override_MeleeCrit_Enabled,
		Override_MeleeCrit_Value,
		Override_RangedCrit_Enabled,
		Override_RangedCrit_Value,
		Override_MagicCrit_Enabled,
		Override_MagicCrit_Value,
		Override_MeleeDamage_Enabled,
		Override_MeleeDamage_Value,
		Override_RangedDamage_Enabled,
		Override_RangedDamage_Value,
		Override_MagicDamage_Enabled,
		Override_MagicDamage_Value,
		Override_MinionDamage_Enabled,
		Override_MinionDamage_Value,
		Override_RocketDamage_Enabled,
		Override_RocketDamage_Value,
		Override_Endurance_Enabled,
		Override_Endurance_Value,
		Override_Thorns_Enabled,
		Override_Thorns_Value,
		Override_MoveSpeed_Enabled,
		Override_MoveSpeed_Value,
		Override_MaxRunSpeed_Enabled,
		Override_MaxRunSpeed_Value,
		Override_AccRunSpeed_Enabled,
		Override_AccRunSpeed_Value,
		Override_RunAcceleration_Enabled,
		Override_RunAcceleration_Value,
		Override_JumpSpeedBoost_Enabled,
		Override_JumpSpeedBoost_Value,
		Override_WingTime_Enabled,
		Override_WingTime_Value,
		Override_WingTimeMax_Enabled,
		Override_WingTimeMax_Value,
		Override_RocketTime_Enabled,
		Override_RocketTime_Value,
		Override_RocketTimeMax_Enabled,
		Override_RocketTimeMax_Value,
		Override_GravDir_Enabled,
		Override_GravDir_Value,
		Override_FishingSkill_Enabled,
		Override_FishingSkill_Value,
		Override_Luck_Enabled,
		Override_Luck_Value,
		Override_TorchLuck_Enabled,
		Override_TorchLuck_Value,
		Override_CoinLuck_Enabled,
		Override_CoinLuck_Value,
		Override_EquipmentBasedLuckBonus_Enabled,
		Override_EquipmentBasedLuckBonus_Value,
		Override_LuckPotion_Enabled,
		Override_LuckPotion_Value,
		Override_KiteLuckLevel_Enabled,
		Override_KiteLuckLevel_Value,
		Override_LadyBugLuckTimeLeft_Enabled,
		Override_LadyBugLuckTimeLeft_Value,
		Override_AccFishingLine_Enabled,
		Override_AccFishingLine_Value,
		Override_AccFishingBobber_Enabled,
		Override_AccFishingBobber_Value,
		Override_AccTackleBox_Enabled,
		Override_AccTackleBox_Value,
		Override_AccLavaFishing_Enabled,
		Override_AccLavaFishing_Value,
		Override_CratePotion_Enabled,
		Override_CratePotion_Value,
		Override_SonarPotion_Enabled,
		Override_SonarPotion_Value,
		Override_BrokenMirrorBadLuckTime_Enabled,
		Override_BrokenMirrorBadLuckTime_Value,
		Override_HasGardenGnomeNearby_Enabled,
		Override_HasGardenGnomeNearby_Value,
		Override_Stinky_Enabled,
		Override_Stinky_Value,
		Override_HasLuck_LuckyClover_Enabled,
		Override_HasLuck_LuckyClover_Value,
		Override_HasLuck_LuckyCoin_Enabled,
		Override_HasLuck_LuckyCoin_Value,
		Override_HasLuck_LuckyHorseshoe_Enabled,
		Override_HasLuck_LuckyHorseshoe_Value,
		Override_HasLuck_RavenFeather_Enabled,
		Override_HasLuck_RavenFeather_Value,
		Override_HasLuck_WiltedClover_Enabled,
		Override_HasLuck_WiltedClover_Value,
		Override_MaxMinions_Enabled,
		Override_MaxMinions_Value,
		Override_MaxTurrets_Enabled,
		Override_MaxTurrets_Value,
		Override_TileRangeX_Enabled,
		Override_TileRangeX_Value,
		Override_TileRangeY_Enabled,
		Override_TileRangeY_Value,
		Override_TileSpeed_Enabled,
		Override_TileSpeed_Value,
		Override_WallSpeed_Enabled,
		Override_WallSpeed_Value,
		Override_PickSpeed_Enabled,
		Override_PickSpeed_Value,
		Override_BlockRange_Enabled,
		Override_BlockRange_Value,
		BuffSource_FishingPotion_Enabled,
		BuffSource_FishingPotion_Value,
		BuffSource_LuckPotion_Enabled,
		BuffSource_LuckPotion_Value,
		BuffSource_CratePotion_Enabled,
		BuffSource_CratePotion_Value,
		BuffSource_SonarPotion_Enabled,
		BuffSource_SonarPotion_Value,
		BuffSource_IronskinPotion_Enabled,
		BuffSource_IronskinPotion_Value,
		BuffSource_RagePotion_Enabled,
		BuffSource_RagePotion_Value,
		BuffSource_WrathPotion_Enabled,
		BuffSource_WrathPotion_Value,
		BuffSource_EndurancePotion_Enabled,
		BuffSource_EndurancePotion_Value,
		BuffSource_ThornsPotion_Enabled,
		BuffSource_ThornsPotion_Value,
		BuffSource_MagicPowerPotion_Enabled,
		BuffSource_MagicPowerPotion_Value,
		BuffSource_SwiftnessPotion_Enabled,
		BuffSource_SwiftnessPotion_Value,
		BuffSource_SugarRush_Enabled,
		BuffSource_SugarRush_Value,
		BuffSource_SummoningPotion_Enabled,
		BuffSource_SummoningPotion_Value,
		BuffSource_Bewitched_Enabled,
		BuffSource_Bewitched_Value,
		BuffSource_WarTable_Enabled,
		BuffSource_WarTable_Value,
		BuffSource_BuilderPotion_Enabled,
		BuffSource_BuilderPotion_Value,
		BuffSource_MiningPotion_Enabled,
		BuffSource_MiningPotion_Value
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private unsafe struct State
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
