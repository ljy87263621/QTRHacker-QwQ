using QTRHacker.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace QTRHacker.Core;

public sealed class PatchesManager
{
	private const string PlayerPropertyMinionSlotsHookName = "PlayerPropertyMinionSlots";
	private const string PlayerPropertyBuildStatsHookName = "PlayerPropertyBuildStats";
	private const string PlayerPropertyCombatStatsHookName = "PlayerPropertyCombatStats";
	private const string PlayerPropertyMovementStatsHookName = "PlayerPropertyMovementStats";

	private sealed record PlayerPropertyOverrideDescriptor(
		RemotePatchState.Field EnabledField,
		RemotePatchState.Field ValueField,
		Type ValueType);

	private sealed record PlayerPropertyBuffSourceDescriptor(
		RemotePatchState.Field EnabledField,
		RemotePatchState.Field ValueField,
		Type ValueType);

	private static readonly IReadOnlyDictionary<string, PlayerPropertyOverrideDescriptor> PlayerPropertyOverrides =
		new Dictionary<string, PlayerPropertyOverrideDescriptor>(StringComparer.Ordinal)
		{
			["StatDefense"] = Override(RemotePatchState.Field.Override_StatDefense_Enabled, RemotePatchState.Field.Override_StatDefense_Value, typeof(int)),
			["ArmorPenetration"] = Override(RemotePatchState.Field.Override_ArmorPenetration_Enabled, RemotePatchState.Field.Override_ArmorPenetration_Value, typeof(int)),
			["MeleeCrit"] = Override(RemotePatchState.Field.Override_MeleeCrit_Enabled, RemotePatchState.Field.Override_MeleeCrit_Value, typeof(int)),
			["RangedCrit"] = Override(RemotePatchState.Field.Override_RangedCrit_Enabled, RemotePatchState.Field.Override_RangedCrit_Value, typeof(int)),
			["MagicCrit"] = Override(RemotePatchState.Field.Override_MagicCrit_Enabled, RemotePatchState.Field.Override_MagicCrit_Value, typeof(int)),
			["MeleeDamage"] = Override(RemotePatchState.Field.Override_MeleeDamage_Enabled, RemotePatchState.Field.Override_MeleeDamage_Value, typeof(float)),
			["RangedDamage"] = Override(RemotePatchState.Field.Override_RangedDamage_Enabled, RemotePatchState.Field.Override_RangedDamage_Value, typeof(float)),
			["MagicDamage"] = Override(RemotePatchState.Field.Override_MagicDamage_Enabled, RemotePatchState.Field.Override_MagicDamage_Value, typeof(float)),
			["MinionDamage"] = Override(RemotePatchState.Field.Override_MinionDamage_Enabled, RemotePatchState.Field.Override_MinionDamage_Value, typeof(float)),
			["RocketDamage"] = Override(RemotePatchState.Field.Override_RocketDamage_Enabled, RemotePatchState.Field.Override_RocketDamage_Value, typeof(float)),
			["Endurance"] = Override(RemotePatchState.Field.Override_Endurance_Enabled, RemotePatchState.Field.Override_Endurance_Value, typeof(float)),
			["Thorns"] = Override(RemotePatchState.Field.Override_Thorns_Enabled, RemotePatchState.Field.Override_Thorns_Value, typeof(float)),

			["MoveSpeed"] = Override(RemotePatchState.Field.Override_MoveSpeed_Enabled, RemotePatchState.Field.Override_MoveSpeed_Value, typeof(float)),
			["MaxRunSpeed"] = Override(RemotePatchState.Field.Override_MaxRunSpeed_Enabled, RemotePatchState.Field.Override_MaxRunSpeed_Value, typeof(float)),
			["AccRunSpeed"] = Override(RemotePatchState.Field.Override_AccRunSpeed_Enabled, RemotePatchState.Field.Override_AccRunSpeed_Value, typeof(float)),
			["RunAcceleration"] = Override(RemotePatchState.Field.Override_RunAcceleration_Enabled, RemotePatchState.Field.Override_RunAcceleration_Value, typeof(float)),
			["JumpSpeedBoost"] = Override(RemotePatchState.Field.Override_JumpSpeedBoost_Enabled, RemotePatchState.Field.Override_JumpSpeedBoost_Value, typeof(float)),
			["WingTime"] = Override(RemotePatchState.Field.Override_WingTime_Enabled, RemotePatchState.Field.Override_WingTime_Value, typeof(float)),
			["WingTimeMax"] = Override(RemotePatchState.Field.Override_WingTimeMax_Enabled, RemotePatchState.Field.Override_WingTimeMax_Value, typeof(int)),
			["RocketTime"] = Override(RemotePatchState.Field.Override_RocketTime_Enabled, RemotePatchState.Field.Override_RocketTime_Value, typeof(int)),
			["RocketTimeMax"] = Override(RemotePatchState.Field.Override_RocketTimeMax_Enabled, RemotePatchState.Field.Override_RocketTimeMax_Value, typeof(int)),
			["GravDir"] = Override(RemotePatchState.Field.Override_GravDir_Enabled, RemotePatchState.Field.Override_GravDir_Value, typeof(float)),

			["CoinLuck"] = Override(RemotePatchState.Field.Override_CoinLuck_Enabled, RemotePatchState.Field.Override_CoinLuck_Value, typeof(float)),
			["KiteLuckLevel"] = Override(RemotePatchState.Field.Override_KiteLuckLevel_Enabled, RemotePatchState.Field.Override_KiteLuckLevel_Value, typeof(byte)),
			["LadyBugLuckTimeLeft"] = Override(RemotePatchState.Field.Override_LadyBugLuckTimeLeft_Enabled, RemotePatchState.Field.Override_LadyBugLuckTimeLeft_Value, typeof(int)),
			["BrokenMirrorBadLuckTime"] = Override(RemotePatchState.Field.Override_BrokenMirrorBadLuckTime_Enabled, RemotePatchState.Field.Override_BrokenMirrorBadLuckTime_Value, typeof(int)),

			["MaxMinions"] = Override(RemotePatchState.Field.Override_MaxMinions_Enabled, RemotePatchState.Field.Override_MaxMinions_Value, typeof(int)),
			["MaxTurrets"] = Override(RemotePatchState.Field.Override_MaxTurrets_Enabled, RemotePatchState.Field.Override_MaxTurrets_Value, typeof(int)),
			["TileRangeX"] = Override(RemotePatchState.Field.Override_TileRangeX_Enabled, RemotePatchState.Field.Override_TileRangeX_Value, typeof(int)),
			["TileRangeY"] = Override(RemotePatchState.Field.Override_TileRangeY_Enabled, RemotePatchState.Field.Override_TileRangeY_Value, typeof(int)),
			["TileSpeed"] = Override(RemotePatchState.Field.Override_TileSpeed_Enabled, RemotePatchState.Field.Override_TileSpeed_Value, typeof(float)),
			["WallSpeed"] = Override(RemotePatchState.Field.Override_WallSpeed_Enabled, RemotePatchState.Field.Override_WallSpeed_Value, typeof(float)),
			["PickSpeed"] = Override(RemotePatchState.Field.Override_PickSpeed_Enabled, RemotePatchState.Field.Override_PickSpeed_Value, typeof(float)),
			["BlockRange"] = Override(RemotePatchState.Field.Override_BlockRange_Enabled, RemotePatchState.Field.Override_BlockRange_Value, typeof(int)),
		};

	private static readonly IReadOnlyDictionary<string, PlayerPropertyBuffSourceDescriptor> PlayerPropertyBuffSources =
		new Dictionary<string, PlayerPropertyBuffSourceDescriptor>(StringComparer.Ordinal)
		{
			["LuckPotion"] = BuffSource(RemotePatchState.Field.BuffSource_LuckPotion_Enabled, RemotePatchState.Field.BuffSource_LuckPotion_Value, typeof(byte)),
			["IronskinPotion"] = BuffSource(RemotePatchState.Field.BuffSource_IronskinPotion_Enabled, RemotePatchState.Field.BuffSource_IronskinPotion_Value, typeof(bool)),
			["RagePotion"] = BuffSource(RemotePatchState.Field.BuffSource_RagePotion_Enabled, RemotePatchState.Field.BuffSource_RagePotion_Value, typeof(bool)),
			["WrathPotion"] = BuffSource(RemotePatchState.Field.BuffSource_WrathPotion_Enabled, RemotePatchState.Field.BuffSource_WrathPotion_Value, typeof(bool)),
			["EndurancePotion"] = BuffSource(RemotePatchState.Field.BuffSource_EndurancePotion_Enabled, RemotePatchState.Field.BuffSource_EndurancePotion_Value, typeof(bool)),
			["ThornsPotion"] = BuffSource(RemotePatchState.Field.BuffSource_ThornsPotion_Enabled, RemotePatchState.Field.BuffSource_ThornsPotion_Value, typeof(bool)),
			["MagicPowerPotion"] = BuffSource(RemotePatchState.Field.BuffSource_MagicPowerPotion_Enabled, RemotePatchState.Field.BuffSource_MagicPowerPotion_Value, typeof(bool)),
			["SwiftnessPotion"] = BuffSource(RemotePatchState.Field.BuffSource_SwiftnessPotion_Enabled, RemotePatchState.Field.BuffSource_SwiftnessPotion_Value, typeof(bool)),
			["SugarRush"] = BuffSource(RemotePatchState.Field.BuffSource_SugarRush_Enabled, RemotePatchState.Field.BuffSource_SugarRush_Value, typeof(bool)),
			["SummoningPotion"] = BuffSource(RemotePatchState.Field.BuffSource_SummoningPotion_Enabled, RemotePatchState.Field.BuffSource_SummoningPotion_Value, typeof(bool)),
			["Bewitched"] = BuffSource(RemotePatchState.Field.BuffSource_Bewitched_Enabled, RemotePatchState.Field.BuffSource_Bewitched_Value, typeof(bool)),
			["WarTable"] = BuffSource(RemotePatchState.Field.BuffSource_WarTable_Enabled, RemotePatchState.Field.BuffSource_WarTable_Value, typeof(bool)),
			["BuilderPotion"] = BuffSource(RemotePatchState.Field.BuffSource_BuilderPotion_Enabled, RemotePatchState.Field.BuffSource_BuilderPotion_Value, typeof(bool)),
			["MiningPotion"] = BuffSource(RemotePatchState.Field.BuffSource_MiningPotion_Enabled, RemotePatchState.Field.BuffSource_MiningPotion_Value, typeof(bool)),
		};

	private static PlayerPropertyOverrideDescriptor Override(
		RemotePatchState.Field enabledField,
		RemotePatchState.Field valueField,
		Type valueType)
	{
		return new PlayerPropertyOverrideDescriptor(enabledField, valueField, valueType);
	}

	private static PlayerPropertyBuffSourceDescriptor BuffSource(
		RemotePatchState.Field enabledField,
		RemotePatchState.Field valueField,
		Type valueType)
	{
		return new PlayerPropertyBuffSourceDescriptor(enabledField, valueField, valueType);
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct STile
	{
		public ushort Type;
		public ushort Wall;
		public byte Liquid;
		public ushort STileHeader;
		public byte BTileHeader;
		public byte BTileHeader2;
		public byte BTileHeader3;
		public short FrameX;
		public short FrameY;

		public void Active(bool active)
		{
			if (active)
				STileHeader |= 32;
			else
				STileHeader = (ushort)(STileHeader & 0xFFDF);
		}
		public bool Active()
		{
			return (STileHeader & 0x20) == 0x20;
		}
		public int WallFrameX()
		{
			return (BTileHeader2 & 0xF) * 36;
		}
		public int WallFrameY()
		{
			return (BTileHeader3 & 7) * 36;
		}
	}
	public QHackLib.CLRHelper PatchHelper
	{
		get
		{
			var helper = Context.HContext.GetCLRHelper("QTRHacker.Patches");
			if (helper != null)
				return helper;
			helper = Context.HContext.CLRHelpers.Values.FirstOrDefault(h => IsPatchesFileName(h.Module.FileName));
			if (helper != null)
				return helper;
			return Context.HContext.CLRHelpers.Values.FirstOrDefault(h => HasBootType(h));
		}
	}
	public GameContext Context { get; }
	private readonly RemotePatchState State;
	public PatchesManager(GameContext context)
	{
		Context = context;
		State = new RemotePatchState(context);
	}

	public bool IsInitialized => State.IsInitialized;

	public void Init()
	{
		if (IsInitialized)
			return;
		string patchesPath = ResolvePatchesAssemblyPath();
		if (!Context.LoadAssemblyAsBytes(patchesPath, "QTRHacker.Patches.Boot")
			&& !Context.LoadAssemblyFrom(patchesPath, "QTRHacker.Patches.Boot"))
			throw new InvalidOperationException("Couldn't load patches");
		if (State.IsInitialized || WaitForSharedStateInitialized())
			return;
		throw new InvalidOperationException("QTRHacker.Patches boot did not complete initialization.");
	}

	public void UnlockAllDuplications()
	{
		QueueRuntimeAction("UnlockAllDuplicationsRequested");
	}

	public void RevealTheWholeMap()
	{
		QueueRuntimeAction("RevealTheWholeMapRequested");
	}

	public void ToggleLanternNight()
	{
		QueueRuntimeAction("ToggleLanternNightRequested");
	}

	private void QueueRuntimeAction(string requestFieldName)
	{
		Init();
		if (Enum.TryParse(requestFieldName, out RemotePatchState.Field field))
		{
			State.SetBool(field, true);
			return;
		}
		PatchHelper.SetStaticFieldValue("QTRHacker.Patches.RuntimeActions", requestFieldName, true);
	}

	private bool WaitForSharedStateInitialized()
	{
		try
		{
			State.EnsureInitialized();
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static bool HasBootType(QHackLib.CLRHelper helper)
	{
		try
		{
			return helper.GetClrType("QTRHacker.Patches.Boot") != null;
		}
		catch
		{
			return false;
		}
	}

	private static bool IsPatchesFileName(string fileName)
	{
		if (string.IsNullOrWhiteSpace(fileName))
			return false;
		try
		{
			return string.Equals(Path.GetFileName(fileName), "QTRHacker.Patches.dll", StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}

	private static string ResolvePatchesAssemblyPath()
	{
		string[] candidates = new[]
		{
			Path.Combine(AppContext.BaseDirectory, "QTRHacker.Patches.dll"),
			Path.GetFullPath("./QTRHacker.Patches.dll"),
			Path.GetFullPath("./bin/Debug/QTRHacker.Patches.dll"),
			Path.GetFullPath("./bin/Release/QTRHacker.Patches.dll"),
			Path.GetFullPath("./src/QTRHacker.Patches/bin/x86/Debug/QTRHacker.Patches.dll"),
			Path.GetFullPath("./src/QTRHacker.Patches/bin/x86/Release/QTRHacker.Patches.dll"),
			Path.GetFullPath("./src/QTRHacker.Patches/bin/Debug/QTRHacker.Patches.dll"),
			Path.GetFullPath("./src/QTRHacker.Patches/bin/Release/QTRHacker.Patches.dll"),
		};
		string path = candidates
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.Where(File.Exists)
			.OrderByDescending(File.GetLastWriteTimeUtc)
			.FirstOrDefault();
		if (path != null)
			return path;
		throw new FileNotFoundException("Could not locate QTRHacker.Patches.dll.", candidates[0]);
	}

	public GameObjectArray2DV<STile> WorldPainter_ClipBoard
		=> new(Context, PatchHelper.GetStaticHackObject("QTRHacker.Patches.WorldPainter", "ClipBoard"));

	public bool WorldPainter_EyeDropperActive
	{
		get => PatchHelper.GetStaticFieldValue<bool>("QTRHacker.Patches.WorldPainter", "EyeDropperActive");
		set => PatchHelper.SetStaticFieldValue("QTRHacker.Patches.WorldPainter", "EyeDropperActive", value);
	}
	public bool WorldPainter_BrushActive
	{
		get => PatchHelper.GetStaticFieldValue<bool>("QTRHacker.Patches.WorldPainter", "BrushActive");
		set => PatchHelper.SetStaticFieldValue("QTRHacker.Patches.WorldPainter", "BrushActive", value);
	}
	public bool WorldPainter_Loading
	{
		get => PatchHelper.GetStaticFieldValue<bool>("QTRHacker.Patches.WorldPainter", "Loading");
		set => PatchHelper.SetStaticFieldValue("QTRHacker.Patches.WorldPainter", "Loading", value);
	}
	public nuint WorldPainter_Buffer
	{
		get => PatchHelper.GetStaticFieldValue<nuint>("QTRHacker.Patches.WorldPainter", "Buffer");
		set => PatchHelper.SetStaticFieldValue("QTRHacker.Patches.WorldPainter", "Buffer", value);
	}

	public bool AimBot_HostileNPCsOnly
	{
		get { Init(); return State.GetBool(RemotePatchState.Field.AimBot_HostileNPCsOnly); }
		set { Init(); State.SetBool(RemotePatchState.Field.AimBot_HostileNPCsOnly, value); }
	}
	public bool AimBot_HostilePlayersOnly
	{
		get { Init(); return State.GetBool(RemotePatchState.Field.AimBot_HostilePlayersOnly); }
		set { Init(); State.SetBool(RemotePatchState.Field.AimBot_HostilePlayersOnly, value); }
	}
	public float AimBot_MaxDistance_NPC
	{
		get { Init(); return State.GetFloat(RemotePatchState.Field.AimBot_MaxDistance_NPC); }
		set { Init(); State.SetFloat(RemotePatchState.Field.AimBot_MaxDistance_NPC, value); }
	}
	public float AimBot_MaxDistance_Player
	{
		get { Init(); return State.GetFloat(RemotePatchState.Field.AimBot_MaxDistance_Player); }
		set { Init(); State.SetFloat(RemotePatchState.Field.AimBot_MaxDistance_Player, value); }
	}
	public int AimBot_TargetedPlayerIndex
	{
		get { Init(); return State.GetInt(RemotePatchState.Field.AimBot_TargetedPlayerIndex); }
		set { Init(); State.SetInt(RemotePatchState.Field.AimBot_TargetedPlayerIndex, value); }
	}
	/// <summary>
	/// This is an enum
	/// </summary>
	public int AimBot_Mode
	{
		get { Init(); return State.GetInt(RemotePatchState.Field.AimBot_Mode); }
		set { Init(); State.SetInt(RemotePatchState.Field.AimBot_Mode, value); }
	}

	public int AutoFishing_Mode
	{
		get { Init(); return State.GetInt(RemotePatchState.Field.AutoFishing_Mode); }
		set { Init(); State.SetInt(RemotePatchState.Field.AutoFishing_Mode, value); }
	}
	public bool AutoFishing_CratesOnly
	{
		get { Init(); return State.GetBool(RemotePatchState.Field.AutoFishing_CratesOnly); }
		set { Init(); State.SetBool(RemotePatchState.Field.AutoFishing_CratesOnly, value); }
	}
	public bool AutoFishing_QuestItemsOnly
	{
		get { Init(); return State.GetBool(RemotePatchState.Field.AutoFishing_QuestItemsOnly); }
		set { Init(); State.SetBool(RemotePatchState.Field.AutoFishing_QuestItemsOnly, value); }
	}

	public bool IsPlayerPropertyOverrideSupported(string propertyName)
	{
		return PlayerPropertyOverrides.ContainsKey(propertyName);
	}

	public bool IsPlayerPropertyBuffSourceSupported(string propertyName)
	{
		return PlayerPropertyBuffSources.ContainsKey(propertyName);
	}

	public bool TryGetPlayerPropertyOverride(string propertyName, out bool enabled, out object value)
	{
		enabled = false;
		value = null;
		if (!PlayerPropertyOverrides.TryGetValue(propertyName, out var descriptor))
			return false;

		if (!State.IsInitialized)
			return true;

		enabled = State.GetBool(descriptor.EnabledField);
		value = GetPlayerPropertyOverrideValue(descriptor);
		return true;
	}

	public void SetPlayerPropertyOverride(string propertyName, bool enabled, object value)
	{
		if (!PlayerPropertyOverrides.TryGetValue(propertyName, out var descriptor))
			throw new ArgumentException($"Unsupported player property override: {propertyName}", nameof(propertyName));

		if (!enabled && !State.IsInitialized)
			return;

		Init();
		if (enabled)
		{
			SetPlayerPropertyOverrideValue(descriptor, value);
			State.SetBool(descriptor.EnabledField, true);
			UpdateNativePlayerPropertyOverrideHook(propertyName);
			return;
		}

		State.SetBool(descriptor.EnabledField, false);
		if (value is not null)
			SetPlayerPropertyOverrideValue(descriptor, value);
		UpdateNativePlayerPropertyOverrideHook(propertyName);
	}

	public bool TryGetPlayerPropertyBuffSource(string propertyName, out bool enabled, out object value)
	{
		enabled = false;
		value = null;
		if (!PlayerPropertyBuffSources.TryGetValue(propertyName, out var descriptor))
			return false;

		if (!State.IsInitialized)
			return true;

		enabled = State.GetBool(descriptor.EnabledField);
		value = GetPlayerPropertyBuffSourceValue(descriptor);
		return true;
	}

	public void SetPlayerPropertyBuffSource(string propertyName, bool enabled, object value)
	{
		if (!PlayerPropertyBuffSources.TryGetValue(propertyName, out var descriptor))
			throw new ArgumentException($"Unsupported player property buff source: {propertyName}", nameof(propertyName));

		if (!enabled && !State.IsInitialized)
			return;

		Init();
		if (value is not null)
			SetPlayerPropertyBuffSourceValue(descriptor, value);
		State.SetBool(descriptor.EnabledField, enabled);
	}

	public bool InfiniteLife { get => GetPlayerToggle(nameof(InfiniteLife)); set => SetPlayerToggle(nameof(InfiniteLife), value); }
	public bool InfiniteMana { get => GetPlayerToggle(nameof(InfiniteMana)); set => SetPlayerToggle(nameof(InfiniteMana), value); }
	public bool InfiniteOxygen { get => GetPlayerToggle(nameof(InfiniteOxygen)); set => SetPlayerToggle(nameof(InfiniteOxygen), value); }
	public bool InfiniteMinion { get => GetPlayerToggle(nameof(InfiniteMinion)); set => SetPlayerToggle(nameof(InfiniteMinion), value); }
	public bool InfiniteAmmo { get => GetPlayerToggle(nameof(InfiniteAmmo)); set => SetPlayerToggle(nameof(InfiniteAmmo), value); }
	public bool InfiniteFlyTime { get => GetPlayerToggle(nameof(InfiniteFlyTime)); set => SetPlayerToggle(nameof(InfiniteFlyTime), value); }
	public bool CreativeMenu { get => GetPlayerToggle(nameof(CreativeMenu)); set => SetPlayerToggle(nameof(CreativeMenu), value); }
	public bool ImmuneToDebuffs { get => GetPlayerToggle(nameof(ImmuneToDebuffs)); set => SetPlayerToggle(nameof(ImmuneToDebuffs), value); }
	public bool SlowFall { get => GetPlayerToggle(nameof(SlowFall)); set => SetPlayerToggle(nameof(SlowFall), value); }
	public bool FastSpeed { get => GetPlayerToggle(nameof(FastSpeed)); set => SetPlayerToggle(nameof(FastSpeed), value); }
	public bool SuperGrabRange { get => GetPlayerToggle(nameof(SuperGrabRange)); set => SetPlayerToggle(nameof(SuperGrabRange), value); }
	public bool CoinPortalDropsBags { get => GetPlayerToggle(nameof(CoinPortalDropsBags)); set => SetPlayerToggle(nameof(CoinPortalDropsBags), value); }
	public bool FishCratesOnly { get => GetPlayerToggle(nameof(FishCratesOnly)); set => SetPlayerToggle(nameof(FishCratesOnly), value); }
	public bool BonusTwoSlots { get => GetPlayerToggle(nameof(BonusTwoSlots)); set => SetPlayerToggle(nameof(BonusTwoSlots), value); }
	public bool HighLight { get => GetPlayerToggle(nameof(HighLight)); set => SetPlayerToggle(nameof(HighLight), value); }
	public bool SuperRange { get => GetPlayerToggle(nameof(SuperRange)); set => SetPlayerToggle(nameof(SuperRange), value); }
	public bool FastTileAndWallPlacingSpeed { get => GetPlayerToggle(nameof(FastTileAndWallPlacingSpeed)); set => SetPlayerToggle(nameof(FastTileAndWallPlacingSpeed), value); }
	public bool MechanicalRuler { get => GetPlayerToggle(nameof(MechanicalRuler)); set => SetPlayerToggle(nameof(MechanicalRuler), value); }
	public bool MechanicalLens { get => GetPlayerToggle(nameof(MechanicalLens)); set => SetPlayerToggle(nameof(MechanicalLens), value); }
	public bool RightClickToTP { get => GetPlayerToggle(nameof(RightClickToTP)); set => SetPlayerToggle(nameof(RightClickToTP), value); }
	public bool EnableAllRecipes { get => GetPlayerToggle(nameof(EnableAllRecipes)); set => SetPlayerToggle(nameof(EnableAllRecipes), value); }
	public bool StrengthenVampireKnives { get => GetPlayerToggle(nameof(StrengthenVampireKnives)); set => SetPlayerToggle(nameof(StrengthenVampireKnives), value); }

	private bool GetPlayerToggle(string fieldName)
	{
		Init();
		return State.GetBool(Enum.Parse<RemotePatchState.Field>(fieldName));
	}

	private void SetPlayerToggle(string fieldName, bool value)
	{
		Init();
		State.SetBool(Enum.Parse<RemotePatchState.Field>(fieldName), value);
		UpdateNativeItemCheckHook(fieldName, value);
	}

	private void UpdateNativeItemCheckHook(string fieldName, bool enabled)
	{
		if (!TryCreateItemCheckSnippet(fieldName, out var code))
			return;
		if (enabled)
			ItemCheckHookManager.Register(Context, fieldName, code);
		else
			ItemCheckHookManager.Unregister(fieldName);
	}

	private bool TryCreateItemCheckSnippet(string fieldName, out QHackLib.Assemble.AssemblyCode code)
	{
		code = fieldName switch
		{
			nameof(InfiniteMinion) => PlayerUpdateSnippets.InfiniteMinion(Context),
			nameof(SuperRange) => PlayerUpdateSnippets.SuperRange(Context),
			nameof(FastTileAndWallPlacingSpeed) => PlayerUpdateSnippets.FastTileAndWallPlacingSpeed(Context),
			nameof(MechanicalRuler) => PlayerUpdateSnippets.MechanicalRuler(Context),
			nameof(MechanicalLens) => PlayerUpdateSnippets.MechanicalLens(Context),
			_ => null
		};
		return code != null;
	}

	private void UpdateNativePlayerPropertyOverrideHook(string propertyName)
	{
		if (!IsNativePlayerPropertyOverrideHookProperty(propertyName)
			&& !IsNativePlayerPropertyBuildStatsHookProperty(propertyName)
			&& !IsNativePlayerPropertyCombatStatsHookProperty(propertyName)
			&& !IsNativePlayerPropertyMovementStatsHookProperty(propertyName))
			return;

		bool minionSlotsEnabled =
			State.GetBool(RemotePatchState.Field.Override_MaxMinions_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_MaxTurrets_Enabled);
		if (minionSlotsEnabled)
			ItemCheckHookManager.Register(Context, PlayerPropertyMinionSlotsHookName, PlayerUpdateSnippets.PlayerPropertyMinionSlots(Context, State));
		else
			ItemCheckHookManager.Unregister(PlayerPropertyMinionSlotsHookName);

		bool buildStatsEnabled =
			State.GetBool(RemotePatchState.Field.Override_TileRangeX_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_TileRangeY_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_TileSpeed_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_WallSpeed_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_PickSpeed_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_BlockRange_Enabled);
		if (buildStatsEnabled)
			ItemCheckHookManager.Register(Context, PlayerPropertyBuildStatsHookName, PlayerUpdateSnippets.PlayerPropertyBuildStats(Context, State));
		else
			ItemCheckHookManager.Unregister(PlayerPropertyBuildStatsHookName);

		bool combatStatsEnabled =
			State.GetBool(RemotePatchState.Field.Override_StatDefense_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_ArmorPenetration_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_MeleeCrit_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_RangedCrit_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_MagicCrit_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_MeleeDamage_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_RangedDamage_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_MagicDamage_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_MinionDamage_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_RocketDamage_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_Endurance_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_Thorns_Enabled);
		if (combatStatsEnabled)
			UpdateHookManager.Register(Context, PlayerPropertyCombatStatsHookName, PlayerUpdateSnippets.PlayerPropertyCombatStats(Context, State));
		else
			UpdateHookManager.Unregister(PlayerPropertyCombatStatsHookName);

		bool movementStatsEnabled =
			State.GetBool(RemotePatchState.Field.Override_MoveSpeed_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_MaxRunSpeed_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_AccRunSpeed_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_RunAcceleration_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_JumpSpeedBoost_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_WingTime_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_WingTimeMax_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_RocketTime_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_RocketTimeMax_Enabled)
			|| State.GetBool(RemotePatchState.Field.Override_GravDir_Enabled);
		if (movementStatsEnabled)
			UpdateHookManager.Register(Context, PlayerPropertyMovementStatsHookName, PlayerUpdateSnippets.PlayerPropertyMovementStats(Context, State));
		else
			UpdateHookManager.Unregister(PlayerPropertyMovementStatsHookName);
	}

	private static bool IsNativePlayerPropertyOverrideHookProperty(string propertyName)
	{
		return string.Equals(propertyName, "MaxMinions", StringComparison.Ordinal)
			|| string.Equals(propertyName, "MaxTurrets", StringComparison.Ordinal);
	}

	private static bool IsNativePlayerPropertyBuildStatsHookProperty(string propertyName)
	{
		return string.Equals(propertyName, "TileRangeX", StringComparison.Ordinal)
			|| string.Equals(propertyName, "TileRangeY", StringComparison.Ordinal)
			|| string.Equals(propertyName, "TileSpeed", StringComparison.Ordinal)
			|| string.Equals(propertyName, "WallSpeed", StringComparison.Ordinal)
			|| string.Equals(propertyName, "PickSpeed", StringComparison.Ordinal)
			|| string.Equals(propertyName, "BlockRange", StringComparison.Ordinal);
	}

	private static bool IsNativePlayerPropertyCombatStatsHookProperty(string propertyName)
	{
		return string.Equals(propertyName, "StatDefense", StringComparison.Ordinal)
			|| string.Equals(propertyName, "ArmorPenetration", StringComparison.Ordinal)
			|| string.Equals(propertyName, "MeleeCrit", StringComparison.Ordinal)
			|| string.Equals(propertyName, "RangedCrit", StringComparison.Ordinal)
			|| string.Equals(propertyName, "MagicCrit", StringComparison.Ordinal)
			|| string.Equals(propertyName, "MeleeDamage", StringComparison.Ordinal)
			|| string.Equals(propertyName, "RangedDamage", StringComparison.Ordinal)
			|| string.Equals(propertyName, "MagicDamage", StringComparison.Ordinal)
			|| string.Equals(propertyName, "MinionDamage", StringComparison.Ordinal)
			|| string.Equals(propertyName, "RocketDamage", StringComparison.Ordinal)
			|| string.Equals(propertyName, "Endurance", StringComparison.Ordinal)
			|| string.Equals(propertyName, "Thorns", StringComparison.Ordinal);
	}

	private static bool IsNativePlayerPropertyMovementStatsHookProperty(string propertyName)
	{
		return string.Equals(propertyName, "MoveSpeed", StringComparison.Ordinal)
			|| string.Equals(propertyName, "MaxRunSpeed", StringComparison.Ordinal)
			|| string.Equals(propertyName, "AccRunSpeed", StringComparison.Ordinal)
			|| string.Equals(propertyName, "RunAcceleration", StringComparison.Ordinal)
			|| string.Equals(propertyName, "JumpSpeedBoost", StringComparison.Ordinal)
			|| string.Equals(propertyName, "WingTime", StringComparison.Ordinal)
			|| string.Equals(propertyName, "WingTimeMax", StringComparison.Ordinal)
			|| string.Equals(propertyName, "RocketTime", StringComparison.Ordinal)
			|| string.Equals(propertyName, "RocketTimeMax", StringComparison.Ordinal)
			|| string.Equals(propertyName, "GravDir", StringComparison.Ordinal);
	}

	private object GetPlayerPropertyOverrideValue(PlayerPropertyOverrideDescriptor descriptor)
	{
		if (descriptor.ValueType == typeof(float))
			return State.GetFloat(descriptor.ValueField);
		if (descriptor.ValueType == typeof(bool))
			return State.GetBool(descriptor.ValueField);
		if (descriptor.ValueType == typeof(byte))
			return (byte)Math.Clamp(State.GetInt(descriptor.ValueField), byte.MinValue, byte.MaxValue);
		return State.GetInt(descriptor.ValueField);
	}

	private object GetPlayerPropertyBuffSourceValue(PlayerPropertyBuffSourceDescriptor descriptor)
	{
		if (descriptor.ValueType == typeof(bool))
			return State.GetBool(descriptor.ValueField);
		if (descriptor.ValueType == typeof(byte))
			return (byte)Math.Clamp(State.GetInt(descriptor.ValueField), byte.MinValue, byte.MaxValue);
		return State.GetInt(descriptor.ValueField);
	}

	private void SetPlayerPropertyOverrideValue(PlayerPropertyOverrideDescriptor descriptor, object value)
	{
		if (descriptor.ValueType == typeof(float))
		{
			State.SetFloat(descriptor.ValueField, Convert.ToSingle(value));
			return;
		}
		if (descriptor.ValueType == typeof(bool))
		{
			State.SetBool(descriptor.ValueField, Convert.ToBoolean(value));
			return;
		}

		int intValue = descriptor.ValueType == typeof(byte)
			? Math.Clamp(Convert.ToInt32(value), byte.MinValue, byte.MaxValue)
			: Convert.ToInt32(value);
		State.SetInt(descriptor.ValueField, intValue);
	}

	private void SetPlayerPropertyBuffSourceValue(PlayerPropertyBuffSourceDescriptor descriptor, object value)
	{
		if (descriptor.ValueType == typeof(bool))
		{
			State.SetBool(descriptor.ValueField, Convert.ToBoolean(value));
			return;
		}

		int intValue = descriptor.ValueType == typeof(byte)
			? Math.Clamp(Convert.ToInt32(value), byte.MinValue, byte.MaxValue)
			: Convert.ToInt32(value);
		State.SetInt(descriptor.ValueField, intValue);
	}
}
