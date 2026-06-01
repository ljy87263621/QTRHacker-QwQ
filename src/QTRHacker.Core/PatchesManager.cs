using QTRHacker.Core.GameObjects;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace QTRHacker.Core;

public sealed class PatchesManager
{
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
	public PatchesManager(GameContext context)
	{
		Context = context;
	}

	public bool IsInitialized => PatchHelper != null;

	public void Init()
	{
		if (IsInitialized)
			return;
		string patchesPath = ResolvePatchesAssemblyPath();
		if (!Context.LoadAssemblyAsBytes(patchesPath, "QTRHacker.Patches.Boot")
			&& !Context.LoadAssemblyFrom(patchesPath, "QTRHacker.Patches.Boot"))
			throw new InvalidOperationException("Couldn't load patches");
		WaitForPatchHelper();
		if (PatchHelper == null)
			throw new InvalidOperationException("QTRHacker.Patches was loaded but could not be found in the CLR module list.");
		if (!WaitForBootInitialized())
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

	private void QueueRuntimeAction(string requestFieldName)
	{
		Init();
		PatchHelper.SetStaticFieldValue("QTRHacker.Patches.RuntimeActions", requestFieldName, true);
	}

	private void WaitForPatchHelper()
	{
		for (int i = 0; i < 100; i++)
		{
			Context.Flush();
			if (PatchHelper != null)
				return;
			Thread.Sleep(100);
		}
	}

	private bool WaitForBootInitialized()
	{
		for (int i = 0; i < 100; i++)
		{
			try
			{
				Context.Flush();
				var helper = PatchHelper;
				if (helper != null && helper.GetStaticFieldValue<bool>("QTRHacker.Patches.Boot", "Initialized"))
					return true;
			}
			catch { }
			Thread.Sleep(100);
		}
		return false;
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
			Path.GetFullPath("./src/QTRHacker.Patches/bin/Debug/QTRHacker.Patches.dll"),
			Path.GetFullPath("./src/QTRHacker.Patches/bin/Release/QTRHacker.Patches.dll"),
		};
		foreach (string path in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
		{
			if (File.Exists(path))
				return path;
		}
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
		get => PatchHelper.GetStaticFieldValue<bool>("QTRHacker.Patches.AimBot", "HostileNPCsOnly");
		set => PatchHelper.SetStaticFieldValue("QTRHacker.Patches.AimBot", "HostileNPCsOnly", value);
	}
	public bool AimBot_HostilePlayersOnly
	{
		get => PatchHelper.GetStaticFieldValue<bool>("QTRHacker.Patches.AimBot", "HostilePlayersOnly");
		set => PatchHelper.SetStaticFieldValue("QTRHacker.Patches.AimBot", "HostilePlayersOnly", value);
	}
	public float AimBot_MaxDistance_NPC
	{
		get => PatchHelper.GetStaticFieldValue<float>("QTRHacker.Patches.AimBot", "MaxDistance_NPC");
		set => PatchHelper.SetStaticFieldValue("QTRHacker.Patches.AimBot", "MaxDistance_NPC", value);
	}
	public float AimBot_MaxDistance_Player
	{
		get => PatchHelper.GetStaticFieldValue<float>("QTRHacker.Patches.AimBot", "MaxDistance_Player");
		set => PatchHelper.SetStaticFieldValue("QTRHacker.Patches.AimBot", "MaxDistance_Player", value);
	}
	public int AimBot_TargetedPlayerIndex
	{
		get => PatchHelper.GetStaticFieldValue<int>("QTRHacker.Patches.AimBot", "TargetedPlayerIndex");
		set => PatchHelper.SetStaticFieldValue("QTRHacker.Patches.AimBot", "TargetedPlayerIndex", value);
	}
	/// <summary>
	/// This is an enum
	/// </summary>
	public int AimBot_Mode
	{
		get => PatchHelper.GetStaticFieldValue<int>("QTRHacker.Patches.AimBot", "Mode");
		set => PatchHelper.SetStaticFieldValue("QTRHacker.Patches.AimBot", "Mode", value);
	}

	public int AutoFishing_Mode
	{
		get => PatchHelper.GetStaticFieldValue<int>("QTRHacker.Patches.AutoFishing", "Mode");
		set => PatchHelper.SetStaticFieldValue("QTRHacker.Patches.AutoFishing", "Mode", value);
	}
	public bool AutoFishing_CratesOnly
	{
		get => PatchHelper.GetStaticFieldValue<bool>("QTRHacker.Patches.AutoFishing", "CratesOnly");
		set => PatchHelper.SetStaticFieldValue("QTRHacker.Patches.AutoFishing", "CratesOnly", value);
	}
	public bool AutoFishing_QuestItemsOnly
	{
		get => PatchHelper.GetStaticFieldValue<bool>("QTRHacker.Patches.AutoFishing", "QuestItemsOnly");
		set => PatchHelper.SetStaticFieldValue("QTRHacker.Patches.AutoFishing", "QuestItemsOnly", value);
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
		return PatchHelper.GetStaticFieldValue<bool>("QTRHacker.Patches.PlayerToggles", fieldName);
	}

	private void SetPlayerToggle(string fieldName, bool value)
	{
		Init();
		PatchHelper.SetStaticFieldValue("QTRHacker.Patches.PlayerToggles", fieldName, value);
	}
}
