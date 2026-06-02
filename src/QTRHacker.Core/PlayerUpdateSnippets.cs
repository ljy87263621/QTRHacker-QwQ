using System;
using System.Collections.Generic;
using QHackLib.Assemble;

namespace QTRHacker.Core;

public static class PlayerUpdateSnippets
{
	private const int FloatOneThird = 0x3EAAAAAB;
	private const int FloatTen = 0x41200000;

	public static AssemblyCode InfiniteLife(GameContext ctx)
	{
		int lifeOff = GetOffset(ctx, "Terraria.Player", "statLife");
		int lifeMaxOff = GetOffset(ctx, "Terraria.Player", "statLifeMax2");

		return WithLocalPlayer(ctx, "InfiniteLife", new AssemblyCode[] {
			(Instruction)$"mov ebx, [eax+{lifeMaxOff}]",
			(Instruction)$"cmp ebx, 0",
			(Instruction)$"jle InfiniteLife_done",
			(Instruction)$"mov [eax+{lifeOff}], ebx",
		});
	}

	public static AssemblyCode InfiniteMana(GameContext ctx)
	{
		int manaOff = GetOffset(ctx, "Terraria.Player", "statMana");
		int manaMaxOff = GetOffset(ctx, "Terraria.Player", "statManaMax2");

		return WithLocalPlayer(ctx, "InfiniteMana", new AssemblyCode[] {
			(Instruction)$"mov ebx, [eax+{manaMaxOff}]",
			(Instruction)$"cmp ebx, 0",
			(Instruction)$"jle InfiniteMana_done",
			(Instruction)$"mov [eax+{manaOff}], ebx",
		});
	}

	public static AssemblyCode InfiniteAmmo(GameContext ctx)
	{
		int inventoryOff = GetOffset(ctx, "Terraria.Player", "inventory");
		int ammoOff = GetOffset(ctx, "Terraria.Item", "ammo");
		int stackOff = GetOffset(ctx, "Terraria.Item", "stack");
		int maxStackOff = GetOffset(ctx, "Terraria.Item", "maxStack");

		return WithLocalPlayer(ctx, "InfiniteAmmo", new AssemblyCode[] {
			(Instruction)$"mov edx, [eax+{inventoryOff}]",
			(Instruction)$"test edx, edx",
			(Instruction)$"jz InfiniteAmmo_done",
			(Instruction)$"xor ecx, ecx",
			(Instruction)$"InfiniteAmmo_loop:",
			(Instruction)$"cmp ecx, [edx+4]",
			(Instruction)$"jae InfiniteAmmo_done",
			(Instruction)$"cmp ecx, 59",
			(Instruction)$"jae InfiniteAmmo_done",
			(Instruction)$"mov ebx, [edx+ecx*4+8]",
			(Instruction)$"test ebx, ebx",
			(Instruction)$"jz InfiniteAmmo_next",
			(Instruction)$"cmp dword ptr [ebx+{ammoOff}], 0",
			(Instruction)$"jle InfiniteAmmo_next",
			(Instruction)$"cmp dword ptr [ebx+{stackOff}], 0",
			(Instruction)$"jle InfiniteAmmo_next",
			(Instruction)$"mov edi, [ebx+{maxStackOff}]",
			(Instruction)$"cmp edi, 999",
			(Instruction)$"jge InfiniteAmmo_write",
			(Instruction)$"mov edi, 999",
			(Instruction)$"InfiniteAmmo_write:",
			(Instruction)$"mov [ebx+{stackOff}], edi",
			(Instruction)$"InfiniteAmmo_next:",
			(Instruction)$"inc ecx",
			(Instruction)$"jmp InfiniteAmmo_loop",
		});
	}

	public static AssemblyCode InfiniteOxygen(GameContext ctx)
	{
		int breathOff = GetOffset(ctx, "Terraria.Player", "breath");
		int breathMaxOff = GetOffset(ctx, "Terraria.Player", "breathMax");

		return WithLocalPlayer(ctx, "InfiniteOxygen", new AssemblyCode[] {
			(Instruction)$"mov ebx, [eax+{breathMaxOff}]",
			(Instruction)$"cmp ebx, 0",
			(Instruction)$"jle InfiniteOxygen_done",
			(Instruction)$"mov [eax+{breathOff}], ebx",
		});
	}

	public static AssemblyCode InfiniteMinion(GameContext ctx)
	{
		int maxMinionsOff = GetOffset(ctx, "Terraria.Player", "maxMinions");
		int maxTurretsOff = GetOffset(ctx, "Terraria.Player", "maxTurrets");

		return WithLocalPlayer(ctx, "InfiniteMinion", new AssemblyCode[] {
			(Instruction)$"mov dword ptr [eax+{maxMinionsOff}], 9999",
			(Instruction)$"mov dword ptr [eax+{maxTurretsOff}], 9999",
		});
	}

	internal static AssemblyCode PlayerPropertyMinionSlots(GameContext ctx, RemotePatchState state)
	{
		int maxMinionsOff = GetOffset(ctx, "Terraria.Player", "maxMinions");
		int maxTurretsOff = GetOffset(ctx, "Terraria.Player", "maxTurrets");
		nuint maxMinionsEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MaxMinions_Enabled);
		nuint maxMinionsValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MaxMinions_Value);
		nuint maxTurretsEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MaxTurrets_Enabled);
		nuint maxTurretsValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MaxTurrets_Value);

		return WithLocalPlayer(ctx, "PlayerPropertyMinionSlots", new AssemblyCode[] {
			(Instruction)$"cmp dword ptr [{maxMinionsEnabledAddr}], 0",
			(Instruction)$"je PlayerPropertyMinionSlots_turrets",
			(Instruction)$"mov ebx, [{maxMinionsValueAddr}]",
			(Instruction)$"mov [eax+{maxMinionsOff}], ebx",
			(Instruction)$"PlayerPropertyMinionSlots_turrets:",
			(Instruction)$"cmp dword ptr [{maxTurretsEnabledAddr}], 0",
			(Instruction)$"je PlayerPropertyMinionSlots_done",
			(Instruction)$"mov ebx, [{maxTurretsValueAddr}]",
			(Instruction)$"mov [eax+{maxTurretsOff}], ebx",
		});
	}

	internal static AssemblyCode PlayerPropertyBuildStats(GameContext ctx, RemotePatchState state)
	{
		nuint tileRangeXAddr = ctx.GameModuleHelper.GetStaticFieldAddress("Terraria.Player", "tileRangeX");
		nuint tileRangeYAddr = ctx.GameModuleHelper.GetStaticFieldAddress("Terraria.Player", "tileRangeY");
		int lastTileRangeXOff = GetOffset(ctx, "Terraria.Player", "lastTileRangeX");
		int lastTileRangeYOff = GetOffset(ctx, "Terraria.Player", "lastTileRangeY");
		int tileSpeedOff = GetOffset(ctx, "Terraria.Player", "tileSpeed");
		int wallSpeedOff = GetOffset(ctx, "Terraria.Player", "wallSpeed");
		int pickSpeedOff = GetOffset(ctx, "Terraria.Player", "pickSpeed");
		int blockRangeOff = GetOffset(ctx, "Terraria.Player", "blockRange");

		nuint tileRangeXEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_TileRangeX_Enabled);
		nuint tileRangeXValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_TileRangeX_Value);
		nuint tileRangeYEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_TileRangeY_Enabled);
		nuint tileRangeYValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_TileRangeY_Value);
		nuint tileSpeedEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_TileSpeed_Enabled);
		nuint tileSpeedValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_TileSpeed_Value);
		nuint wallSpeedEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_WallSpeed_Enabled);
		nuint wallSpeedValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_WallSpeed_Value);
		nuint pickSpeedEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_PickSpeed_Enabled);
		nuint pickSpeedValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_PickSpeed_Value);
		nuint blockRangeEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_BlockRange_Enabled);
		nuint blockRangeValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_BlockRange_Value);

		return WithLocalPlayer(ctx, "PlayerPropertyBuildStats", new AssemblyCode[] {
			(Instruction)$"cmp dword ptr [{tileRangeXEnabledAddr}], 0",
			(Instruction)$"je PlayerPropertyBuildStats_tileRangeY",
			(Instruction)$"mov ebx, [{tileRangeXValueAddr}]",
			(Instruction)$"mov [{tileRangeXAddr}], ebx",
			(Instruction)$"mov [eax+{lastTileRangeXOff}], ebx",
			(Instruction)$"PlayerPropertyBuildStats_tileRangeY:",
			(Instruction)$"cmp dword ptr [{tileRangeYEnabledAddr}], 0",
			(Instruction)$"je PlayerPropertyBuildStats_tileSpeed",
			(Instruction)$"mov ebx, [{tileRangeYValueAddr}]",
			(Instruction)$"mov [{tileRangeYAddr}], ebx",
			(Instruction)$"mov [eax+{lastTileRangeYOff}], ebx",
			(Instruction)$"PlayerPropertyBuildStats_tileSpeed:",
			(Instruction)$"cmp dword ptr [{tileSpeedEnabledAddr}], 0",
			(Instruction)$"je PlayerPropertyBuildStats_wallSpeed",
			(Instruction)$"mov ebx, [{tileSpeedValueAddr}]",
			(Instruction)$"mov [eax+{tileSpeedOff}], ebx",
			(Instruction)$"PlayerPropertyBuildStats_wallSpeed:",
			(Instruction)$"cmp dword ptr [{wallSpeedEnabledAddr}], 0",
			(Instruction)$"je PlayerPropertyBuildStats_pickSpeed",
			(Instruction)$"mov ebx, [{wallSpeedValueAddr}]",
			(Instruction)$"mov [eax+{wallSpeedOff}], ebx",
			(Instruction)$"PlayerPropertyBuildStats_pickSpeed:",
			(Instruction)$"cmp dword ptr [{pickSpeedEnabledAddr}], 0",
			(Instruction)$"je PlayerPropertyBuildStats_blockRange",
			(Instruction)$"mov ebx, [{pickSpeedValueAddr}]",
			(Instruction)$"mov [eax+{pickSpeedOff}], ebx",
			(Instruction)$"PlayerPropertyBuildStats_blockRange:",
			(Instruction)$"cmp dword ptr [{blockRangeEnabledAddr}], 0",
			(Instruction)$"je PlayerPropertyBuildStats_done",
			(Instruction)$"mov ebx, [{blockRangeValueAddr}]",
			(Instruction)$"mov [eax+{blockRangeOff}], ebx",
		});
	}

	internal static AssemblyCode PlayerPropertyCombatStats(GameContext ctx, RemotePatchState state)
	{
		int statDefenseOff = GetOffset(ctx, "Terraria.Player", "statDefense");
		int armorPenetrationOff = GetOffset(ctx, "Terraria.Player", "armorPenetration");
		int meleeCritOff = GetOffset(ctx, "Terraria.Player", "meleeCrit");
		int rangedCritOff = GetOffset(ctx, "Terraria.Player", "rangedCrit");
		int magicCritOff = GetOffset(ctx, "Terraria.Player", "magicCrit");
		int meleeDamageOff = GetOffset(ctx, "Terraria.Player", "meleeDamage");
		int rangedDamageOff = GetOffset(ctx, "Terraria.Player", "rangedDamage");
		int magicDamageOff = GetOffset(ctx, "Terraria.Player", "magicDamage");
		int minionDamageOff = GetOffset(ctx, "Terraria.Player", "minionDamage");
		int rocketDamageOff = GetOffset(ctx, "Terraria.Player", "rocketDamage");
		int enduranceOff = GetOffset(ctx, "Terraria.Player", "endurance");
		int thornsOff = GetOffset(ctx, "Terraria.Player", "thorns");

		nuint statDefenseEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_StatDefense_Enabled);
		nuint statDefenseValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_StatDefense_Value);
		nuint armorPenetrationEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_ArmorPenetration_Enabled);
		nuint armorPenetrationValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_ArmorPenetration_Value);
		nuint meleeCritEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MeleeCrit_Enabled);
		nuint meleeCritValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MeleeCrit_Value);
		nuint rangedCritEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_RangedCrit_Enabled);
		nuint rangedCritValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_RangedCrit_Value);
		nuint magicCritEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MagicCrit_Enabled);
		nuint magicCritValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MagicCrit_Value);
		nuint meleeDamageEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MeleeDamage_Enabled);
		nuint meleeDamageValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MeleeDamage_Value);
		nuint rangedDamageEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_RangedDamage_Enabled);
		nuint rangedDamageValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_RangedDamage_Value);
		nuint magicDamageEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MagicDamage_Enabled);
		nuint magicDamageValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MagicDamage_Value);
		nuint minionDamageEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MinionDamage_Enabled);
		nuint minionDamageValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MinionDamage_Value);
		nuint rocketDamageEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_RocketDamage_Enabled);
		nuint rocketDamageValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_RocketDamage_Value);
		nuint enduranceEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_Endurance_Enabled);
		nuint enduranceValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_Endurance_Value);
		nuint thornsEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_Thorns_Enabled);
		nuint thornsValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_Thorns_Value);

		var body = new List<AssemblyCode>();
		AddDwordOverride(body, "PlayerPropertyCombatStats_armorPenetration", statDefenseOff, statDefenseEnabledAddr, statDefenseValueAddr);
		AddDwordOverride(body, "PlayerPropertyCombatStats_meleeCrit", armorPenetrationOff, armorPenetrationEnabledAddr, armorPenetrationValueAddr);
		AddDwordOverride(body, "PlayerPropertyCombatStats_rangedCrit", meleeCritOff, meleeCritEnabledAddr, meleeCritValueAddr);
		AddDwordOverride(body, "PlayerPropertyCombatStats_magicCrit", rangedCritOff, rangedCritEnabledAddr, rangedCritValueAddr);
		AddDwordOverride(body, "PlayerPropertyCombatStats_meleeDamage", magicCritOff, magicCritEnabledAddr, magicCritValueAddr);
		AddDwordOverride(body, "PlayerPropertyCombatStats_rangedDamage", meleeDamageOff, meleeDamageEnabledAddr, meleeDamageValueAddr);
		AddDwordOverride(body, "PlayerPropertyCombatStats_magicDamage", rangedDamageOff, rangedDamageEnabledAddr, rangedDamageValueAddr);
		AddDwordOverride(body, "PlayerPropertyCombatStats_minionDamage", magicDamageOff, magicDamageEnabledAddr, magicDamageValueAddr);
		AddDwordOverride(body, "PlayerPropertyCombatStats_rocketDamage", minionDamageOff, minionDamageEnabledAddr, minionDamageValueAddr);
		AddDwordOverride(body, "PlayerPropertyCombatStats_endurance", rocketDamageOff, rocketDamageEnabledAddr, rocketDamageValueAddr);
		AddDwordOverride(body, "PlayerPropertyCombatStats_thorns", enduranceOff, enduranceEnabledAddr, enduranceValueAddr);
		AddDwordOverride(body, "PlayerPropertyCombatStats_afterThorns", thornsOff, thornsEnabledAddr, thornsValueAddr);
		return WithLocalPlayer(ctx, "PlayerPropertyCombatStats", body);
	}

	internal static AssemblyCode PlayerPropertyMovementStats(GameContext ctx, RemotePatchState state)
	{
		int moveSpeedOff = GetOffset(ctx, "Terraria.Player", "moveSpeed");
		int maxRunSpeedOff = GetOffset(ctx, "Terraria.Player", "maxRunSpeed");
		int accRunSpeedOff = GetOffset(ctx, "Terraria.Player", "accRunSpeed");
		int runAccelerationOff = GetOffset(ctx, "Terraria.Player", "runAcceleration");
		int jumpSpeedBoostOff = GetOffset(ctx, "Terraria.Player", "jumpSpeedBoost");
		int wingTimeOff = GetOffset(ctx, "Terraria.Player", "wingTime");
		int wingTimeMaxOff = GetOffset(ctx, "Terraria.Player", "wingTimeMax");
		int rocketTimeOff = GetOffset(ctx, "Terraria.Player", "rocketTime");
		int rocketTimeMaxOff = GetOffset(ctx, "Terraria.Player", "rocketTimeMax");
		int gravDirOff = GetOffset(ctx, "Terraria.Player", "gravDir");

		nuint moveSpeedEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MoveSpeed_Enabled);
		nuint moveSpeedValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MoveSpeed_Value);
		nuint maxRunSpeedEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MaxRunSpeed_Enabled);
		nuint maxRunSpeedValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_MaxRunSpeed_Value);
		nuint accRunSpeedEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_AccRunSpeed_Enabled);
		nuint accRunSpeedValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_AccRunSpeed_Value);
		nuint runAccelerationEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_RunAcceleration_Enabled);
		nuint runAccelerationValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_RunAcceleration_Value);
		nuint jumpSpeedBoostEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_JumpSpeedBoost_Enabled);
		nuint jumpSpeedBoostValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_JumpSpeedBoost_Value);
		nuint wingTimeEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_WingTime_Enabled);
		nuint wingTimeValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_WingTime_Value);
		nuint wingTimeMaxEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_WingTimeMax_Enabled);
		nuint wingTimeMaxValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_WingTimeMax_Value);
		nuint rocketTimeEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_RocketTime_Enabled);
		nuint rocketTimeValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_RocketTime_Value);
		nuint rocketTimeMaxEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_RocketTimeMax_Enabled);
		nuint rocketTimeMaxValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_RocketTimeMax_Value);
		nuint gravDirEnabledAddr = state.GetFieldAddress(RemotePatchState.Field.Override_GravDir_Enabled);
		nuint gravDirValueAddr = state.GetFieldAddress(RemotePatchState.Field.Override_GravDir_Value);

		var body = new List<AssemblyCode>();
		AddDwordOverride(body, "PlayerPropertyMovementStats_maxRunSpeed", moveSpeedOff, moveSpeedEnabledAddr, moveSpeedValueAddr);
		AddDwordOverride(body, "PlayerPropertyMovementStats_accRunSpeed", maxRunSpeedOff, maxRunSpeedEnabledAddr, maxRunSpeedValueAddr);
		AddDwordOverride(body, "PlayerPropertyMovementStats_runAcceleration", accRunSpeedOff, accRunSpeedEnabledAddr, accRunSpeedValueAddr);
		AddDwordOverride(body, "PlayerPropertyMovementStats_jumpSpeedBoost", runAccelerationOff, runAccelerationEnabledAddr, runAccelerationValueAddr);
		AddDwordOverride(body, "PlayerPropertyMovementStats_wingTime", jumpSpeedBoostOff, jumpSpeedBoostEnabledAddr, jumpSpeedBoostValueAddr);
		AddDwordOverride(body, "PlayerPropertyMovementStats_wingTimeMax", wingTimeOff, wingTimeEnabledAddr, wingTimeValueAddr);
		AddDwordOverride(body, "PlayerPropertyMovementStats_rocketTime", wingTimeMaxOff, wingTimeMaxEnabledAddr, wingTimeMaxValueAddr);
		AddDwordOverride(body, "PlayerPropertyMovementStats_rocketTimeMax", rocketTimeOff, rocketTimeEnabledAddr, rocketTimeValueAddr);
		AddDwordOverride(body, "PlayerPropertyMovementStats_gravDir", rocketTimeMaxOff, rocketTimeMaxEnabledAddr, rocketTimeMaxValueAddr);
		AddDwordOverride(body, "PlayerPropertyMovementStats_afterGravDir", gravDirOff, gravDirEnabledAddr, gravDirValueAddr);
		return WithLocalPlayer(ctx, "PlayerPropertyMovementStats", body);
	}

	public static AssemblyCode InfiniteFlyTime(GameContext ctx)
	{
		int wingTimeOff = GetOffset(ctx, "Terraria.Player", "wingTime");
		int wingTimeMaxOff = GetOffset(ctx, "Terraria.Player", "wingTimeMax");
		int rocketTimeOff = GetOffset(ctx, "Terraria.Player", "rocketTime");
		int rocketTimeMaxOff = GetOffset(ctx, "Terraria.Player", "rocketTimeMax");

		return WithLocalPlayer(ctx, "InfiniteFlyTime", new AssemblyCode[] {
			(Instruction)$"mov ebx, [eax+{wingTimeMaxOff}]",
			(Instruction)$"cmp ebx, 0",
			(Instruction)$"jle InfiniteFlyTime_rocket",
			(Instruction)$"mov [eax+{wingTimeOff}], ebx",
			(Instruction)$"InfiniteFlyTime_rocket:",
			(Instruction)$"mov ebx, [eax+{rocketTimeMaxOff}]",
			(Instruction)$"cmp ebx, 0",
			(Instruction)$"jle InfiniteFlyTime_done",
			(Instruction)$"mov [eax+{rocketTimeOff}], ebx",
		});
	}

	public static AssemblyCode SlowFall(GameContext ctx)
	{
		return SetBool(ctx, "SlowFall", "slowFall", true);
	}

	public static AssemblyCode FastSpeed(GameContext ctx)
	{
		return SetInt32(ctx, "FastSpeed", "moveSpeed", FloatTen);
	}

	public static AssemblyCode FastTileAndWallPlacingSpeed(GameContext ctx)
	{
		int wallSpeedOff = GetOffset(ctx, "Terraria.Player", "wallSpeed");
		int tileSpeedOff = GetOffset(ctx, "Terraria.Player", "tileSpeed");

		return WithLocalPlayer(ctx, "FastTileAndWallPlacingSpeed", new AssemblyCode[] {
			(Instruction)$"mov dword ptr [eax+{wallSpeedOff}], {FloatOneThird}",
			(Instruction)$"mov dword ptr [eax+{tileSpeedOff}], {FloatOneThird}",
		});
	}

	public static AssemblyCode SuperRange(GameContext ctx)
	{
		nuint tileRangeX = ctx.GameModuleHelper.GetStaticFieldAddress("Terraria.Player", "tileRangeX");
		nuint tileRangeY = ctx.GameModuleHelper.GetStaticFieldAddress("Terraria.Player", "tileRangeY");
		int lastTileRangeXOff = GetOffset(ctx, "Terraria.Player", "lastTileRangeX");
		int lastTileRangeYOff = GetOffset(ctx, "Terraria.Player", "lastTileRangeY");

		return WithLocalPlayer(ctx, "SuperRange", new AssemblyCode[] {
			(Instruction)$"mov dword ptr [{tileRangeX}], 4096",
			(Instruction)$"mov dword ptr [{tileRangeY}], 4096",
			(Instruction)$"mov dword ptr [eax+{lastTileRangeXOff}], 4096",
			(Instruction)$"mov dword ptr [eax+{lastTileRangeYOff}], 4096",
		});
	}

	public static AssemblyCode MechanicalRuler(GameContext ctx)
	{
		int gridOff = GetOffset(ctx, "Terraria.Player", "rulerGrid");
		int lineOff = GetOffset(ctx, "Terraria.Player", "rulerLine");
		int builderAccStatusOff = GetOffset(ctx, "Terraria.Player", "builderAccStatus");

		return WithLocalPlayer(ctx, "MechanicalRuler", new AssemblyCode[] {
			(Instruction)$"mov byte ptr [eax+{gridOff}], 1",
			(Instruction)$"mov byte ptr [eax+{lineOff}], 1",
			SetBuilderAccVisible("MechanicalRuler_grid", builderAccStatusOff, 0),
			SetBuilderAccVisible("MechanicalRuler_line", builderAccStatusOff, 1),
		});
	}

	public static AssemblyCode MechanicalLens(GameContext ctx)
	{
		int wiresOff = GetOffset(ctx, "Terraria.Player", "InfoAccMechShowWires");
		int builderAccStatusOff = GetOffset(ctx, "Terraria.Player", "builderAccStatus");

		return WithLocalPlayer(ctx, "MechanicalLens", new AssemblyCode[] {
			(Instruction)$"mov byte ptr [eax+{wiresOff}], 1",
			SetBuilderAccVisible("MechanicalLens_red", builderAccStatusOff, 4),
			SetBuilderAccVisible("MechanicalLens_green", builderAccStatusOff, 5),
			SetBuilderAccVisible("MechanicalLens_blue", builderAccStatusOff, 6),
			SetBuilderAccVisible("MechanicalLens_yellow", builderAccStatusOff, 7),
			SetBuilderAccVisible("MechanicalLens_hideAll", builderAccStatusOff, 8),
			SetBuilderAccVisible("MechanicalLens_actuators", builderAccStatusOff, 9),
		});
	}

	public static AssemblyCode BonusTwoSlots(GameContext ctx)
	{
		return SetBool(ctx, "BonusTwoSlots", "extraAccessory", true);
	}

	private static AssemblyCode WithLocalPlayer(GameContext ctx, string labelPrefix, IEnumerable<AssemblyCode> body)
	{
		nuint playerArrAddr = ctx.GameModuleHelper.GetStaticFieldAddress("Terraria.Main", "player");
		nuint myPlayerAddr = ctx.GameModuleHelper.GetStaticFieldAddress("Terraria.Main", "myPlayer");

		var snippet = AssemblySnippet.FromCode(new AssemblyCode[] {
			(Instruction)$"mov eax, [{myPlayerAddr}]",
			(Instruction)$"cmp eax, 0",
			(Instruction)$"jl {labelPrefix}_done",
			(Instruction)$"mov edx, [{playerArrAddr}]",
			(Instruction)$"test edx, edx",
			(Instruction)$"jz {labelPrefix}_done",
			(Instruction)$"cmp eax, [edx+4]",
			(Instruction)$"jae {labelPrefix}_done",
			(Instruction)$"mov eax, [edx+eax*4+8]",
			(Instruction)$"test eax, eax",
			(Instruction)$"jz {labelPrefix}_done",
		});
		snippet.Content.AddRange(body);
		snippet.Content.Add((Instruction)$"{labelPrefix}_done:");
		return snippet;
	}

	private static AssemblyCode SetBool(GameContext ctx, string labelPrefix, string field, bool value)
	{
		int off = GetOffset(ctx, "Terraria.Player", field);
		return WithLocalPlayer(ctx, labelPrefix, new AssemblyCode[] {
			(Instruction)$"mov byte ptr [eax+{off}], {(value ? 1 : 0)}",
		});
	}

	private static AssemblyCode SetInt32(GameContext ctx, string labelPrefix, string field, int value)
	{
		int off = GetOffset(ctx, "Terraria.Player", field);
		return WithLocalPlayer(ctx, labelPrefix, new AssemblyCode[] {
			(Instruction)$"mov dword ptr [eax+{off}], {value}",
		});
	}

	private static void AddDwordOverride(
		List<AssemblyCode> body,
		string nextLabel,
		int playerFieldOffset,
		nuint enabledAddress,
		nuint valueAddress)
	{
		body.Add((Instruction)$"cmp dword ptr [{enabledAddress}], 0");
		body.Add((Instruction)$"je {nextLabel}");
		body.Add((Instruction)$"mov ebx, [{valueAddress}]");
		body.Add((Instruction)$"mov [eax+{playerFieldOffset}], ebx");
		body.Add((Instruction)$"{nextLabel}:");
	}

	private static AssemblyCode SetBuilderAccVisible(string labelPrefix, int builderAccStatusOff, int index)
	{
		return AssemblySnippet.FromASMCode($@"
			mov edx, [eax+{builderAccStatusOff}]
			test edx, edx
			jz {labelPrefix}_done
			cmp dword ptr [edx+4], {index + 1}
			jb {labelPrefix}_done
			mov dword ptr [edx+{8 + index * 4}], 0
			{labelPrefix}_done:
		");
	}

	private static int GetOffset(GameContext ctx, string type, string field)
	{
		return checked((int)ctx.GameModuleHelper.GetInstanceFieldOffset(type, field) + IntPtr.Size);
	}
}
