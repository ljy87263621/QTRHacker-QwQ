using QTRHacker.Commands;
using QTRHacker.Core.GameObjects.Terraria;
using QTRHacker.Localization;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace QTRHacker.ViewModels.PlayerEditor;

public class PlayerPropertiesEditorViewModel : ViewModelBase
{
	public enum ValuePersistence
	{
		Direct,
		RuntimeOverride,
		BuffSource
	}

	public enum PropertyRole
	{
		Value,
		Computed,
		Influence
	}

	public sealed record PlayerPropertyDescriptor(
		string GroupKey,
		string PropertyName,
		Type ValueType,
		ValuePersistence Persistence = ValuePersistence.Direct,
		PropertyRole Role = PropertyRole.Value);

	public static IReadOnlyList<PlayerPropertyDescriptor> PropertyCatalog { get; } = new PlayerPropertyDescriptor[]
	{
		new("Basics", "Team", typeof(int)),
		new("Basics", "SpawnX", typeof(int)),
		new("Basics", "SpawnY", typeof(int)),
		new("Basics", "Difficulty", typeof(byte)),
		new("Basics", "CurrentLoadoutIndex", typeof(int)),
		new("Basics", "AnglerQuestsFinished", typeof(int)),
		new("Basics", "SkinVariant", typeof(int)),
		new("Basics", "Hair", typeof(int)),

		new("Vitals", "StatLife", typeof(int)),
		new("Vitals", "StatLifeMax", typeof(int)),
		new("Vitals", "StatLifeMax2", typeof(int), ValuePersistence.Direct, PropertyRole.Computed),
		new("Vitals", "StatMana", typeof(int)),
		new("Vitals", "StatManaMax", typeof(int)),
		new("Vitals", "StatManaMax2", typeof(int), ValuePersistence.Direct, PropertyRole.Computed),
		new("Vitals", "Breath", typeof(int)),
		new("Vitals", "BreathMax", typeof(int)),
		new("Vitals", "LavaTime", typeof(int)),
		new("Vitals", "LavaMax", typeof(int)),

		new("Combat", "StatDefense", typeof(int), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Combat", "ArmorPenetration", typeof(int), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Combat", "MeleeCrit", typeof(int), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Combat", "RangedCrit", typeof(int), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Combat", "MagicCrit", typeof(int), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Combat", "MeleeDamage", typeof(float), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Combat", "RangedDamage", typeof(float), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Combat", "MagicDamage", typeof(float), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Combat", "MinionDamage", typeof(float), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Combat", "RocketDamage", typeof(float), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Combat", "Endurance", typeof(float), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Combat", "Thorns", typeof(float), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Combat", "IronskinPotion", typeof(bool), ValuePersistence.BuffSource, PropertyRole.Influence),
		new("Combat", "RagePotion", typeof(bool), ValuePersistence.BuffSource, PropertyRole.Influence),
		new("Combat", "WrathPotion", typeof(bool), ValuePersistence.BuffSource, PropertyRole.Influence),
		new("Combat", "EndurancePotion", typeof(bool), ValuePersistence.BuffSource, PropertyRole.Influence),
		new("Combat", "ThornsPotion", typeof(bool), ValuePersistence.BuffSource, PropertyRole.Influence),
		new("Combat", "MagicPowerPotion", typeof(bool), ValuePersistence.BuffSource, PropertyRole.Influence),

		new("Movement", "MoveSpeed", typeof(float), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Movement", "MaxRunSpeed", typeof(float), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Movement", "AccRunSpeed", typeof(float), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Movement", "RunAcceleration", typeof(float), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Movement", "JumpSpeed", typeof(float), ValuePersistence.Direct, PropertyRole.Computed),
		new("Movement", "JumpSpeedBoost", typeof(float), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Movement", "WingTime", typeof(float), ValuePersistence.RuntimeOverride),
		new("Movement", "WingTimeMax", typeof(int), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Movement", "RocketTime", typeof(int), ValuePersistence.RuntimeOverride),
		new("Movement", "RocketTimeMax", typeof(int), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("Movement", "GravDir", typeof(float), ValuePersistence.RuntimeOverride),
		new("Movement", "SwiftnessPotion", typeof(bool), ValuePersistence.BuffSource, PropertyRole.Influence),
		new("Movement", "SugarRush", typeof(bool), ValuePersistence.BuffSource, PropertyRole.Influence),

		new("Luck", "Luck", typeof(float), ValuePersistence.Direct, PropertyRole.Computed),
		new("Luck", "TorchLuck", typeof(float), ValuePersistence.Direct, PropertyRole.Computed),
		new("Luck", "EquipmentBasedLuckBonus", typeof(float), ValuePersistence.Direct, PropertyRole.Computed),
		new("Luck", "LadyBugLuckTimeLeft", typeof(int), ValuePersistence.RuntimeOverride, PropertyRole.Influence),
		new("Luck", "CoinLuck", typeof(float), ValuePersistence.RuntimeOverride, PropertyRole.Influence),
		new("Luck", "LuckPotion", typeof(byte), ValuePersistence.BuffSource, PropertyRole.Influence),
		new("Luck", "KiteLuckLevel", typeof(byte), ValuePersistence.RuntimeOverride, PropertyRole.Influence),
		new("Luck", "BrokenMirrorBadLuckTime", typeof(int), ValuePersistence.RuntimeOverride, PropertyRole.Influence),
		new("Luck", "UsedGalaxyPearl", typeof(bool), ValuePersistence.Direct, PropertyRole.Influence),

		new("SummonBuild", "MaxMinions", typeof(int), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("SummonBuild", "MaxTurrets", typeof(int), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("SummonBuild", "TileRangeX", typeof(int), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("SummonBuild", "TileRangeY", typeof(int), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("SummonBuild", "TileSpeed", typeof(float), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("SummonBuild", "WallSpeed", typeof(float), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("SummonBuild", "PickSpeed", typeof(float), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("SummonBuild", "BlockRange", typeof(int), ValuePersistence.RuntimeOverride, PropertyRole.Computed),
		new("SummonBuild", "SummoningPotion", typeof(bool), ValuePersistence.BuffSource, PropertyRole.Influence),
		new("SummonBuild", "Bewitched", typeof(bool), ValuePersistence.BuffSource, PropertyRole.Influence),
		new("SummonBuild", "WarTable", typeof(bool), ValuePersistence.BuffSource, PropertyRole.Influence),
		new("SummonBuild", "BuilderPotion", typeof(bool), ValuePersistence.BuffSource, PropertyRole.Influence),
		new("SummonBuild", "MiningPotion", typeof(bool), ValuePersistence.BuffSource, PropertyRole.Influence),

		new("Toggles", "LavaImmune", typeof(bool)),
		new("Toggles", "Immune", typeof(bool)),
		new("Toggles", "NoKnockback", typeof(bool)),
		new("Toggles", "AutoJump", typeof(bool)),
		new("Toggles", "EnemySpawns", typeof(bool)),
		new("Toggles", "CreativeGodMode", typeof(bool)),
		new("Toggles", "ExtraAccessory", typeof(bool))
	};

	private bool updating;
	private int maxLife;
	private int maxMana;
	private readonly Player player;
	private readonly HackCommand refreshCommand;
	private readonly HackCommand applyCommand;

	public HackCommand RefreshCommand => refreshCommand;
	public HackCommand ApplyCommand => applyCommand;

	public bool Updating
	{
		get => updating;
		set
		{
			updating = value;
			OnPropertyChanged(nameof(Updating));
		}
	}
	public Player Player
	{
		get => player;
	}
	public ObservableCollection<ColorViewModel> Colors { get; } = new();
	public ObservableCollection<PlayerPropertyGroupViewModel> PropertyGroups { get; } = new();
	public int MaxLife
	{
		get => maxLife;
		set
		{
			if (value < 0)
				return;
			maxLife = value;
			OnPropertyChanged(nameof(MaxLife));
		}
	}
	public int MaxMana
	{
		get => maxMana;
		set
		{
			if (value < 0)
				return;
			maxMana = value;
			OnPropertyChanged(nameof(MaxMana));
		}
	}

	private void InitColors()
	{
		Colors.Add(new ColorViewModel("Hair", "HairColor"));
		Colors.Add(new ColorViewModel("Skin", "SkinColor"));
		Colors.Add(new ColorViewModel("Eyes", "EyeColor"));
		Colors.Add(new ColorViewModel("Shirt", "ShirtColor"));
		Colors.Add(new ColorViewModel("Undershirt", "UnderShirtColor"));
		Colors.Add(new ColorViewModel("Pants", "PantsColor"));
		Colors.Add(new ColorViewModel("Shoes", "ShoeColor"));
	}

	private void InitPropertyGroups()
	{
		foreach (var group in PropertyCatalog.GroupBy(item => item.GroupKey))
		{
			PropertyGroups.Add(new PlayerPropertyGroupViewModel(group.Key, group));
		}
	}

	public PlayerPropertiesEditorViewModel(Player player)
	{
		this.player = player;
		InitColors();
		InitPropertyGroups();
		refreshCommand = new HackCommand(o => Update());
		applyCommand = new HackCommand(o => ApplyToGame());

		Update();
	}
	public void ApplyToGame()
	{
		foreach (var colorVModel in Colors)
		{
			var property = typeof(Player).GetProperty(colorVModel.PropertyName);
			var color = new Core.GameObjects.ValueTypeRedefs.Xna.Color
			{
				R = colorVModel.Color.R,
				G = colorVModel.Color.G,
				B = colorVModel.Color.B,
				A = colorVModel.Color.A
			};
			property.SetValue(Player, color);
		}

		foreach (var field in PropertyGroups.SelectMany(group => group.Fields))
		{
			field.ApplyTo(Player);
		}

		MaxLife = Player.StatLifeMax;
		MaxMana = Player.StatManaMax;
	}
	public void Update()
	{
		foreach (var colorVModel in Colors)
		{
			var color = (Core.GameObjects.ValueTypeRedefs.Xna.Color)typeof(Player).GetProperty(colorVModel.PropertyName).GetValue(Player);
			colorVModel.ColorValue = color.R << 16 | color.G << 8 | color.B | unchecked((int)0xFF_000000);
		}

		foreach (var field in PropertyGroups.SelectMany(group => group.Fields))
		{
			field.RefreshFrom(Player);
		}

		MaxLife = Player.StatLifeMax;
		MaxMana = Player.StatManaMax;
	}
}

public class PlayerPropertyGroupViewModel : ViewModelBase, ILocalizationProvider
{
	public string GroupKey { get; }
	public ObservableCollection<PlayerPropertyFieldViewModel> Fields { get; } = new();
	public string Title => LocalizationManager.Instance.GetValue($"UI.PlayerProperties.Groups.{GroupKey}");

	public PlayerPropertyGroupViewModel(
		string groupKey,
		IEnumerable<PlayerPropertiesEditorViewModel.PlayerPropertyDescriptor> fields)
	{
		GroupKey = groupKey;
		foreach (var field in fields)
		{
			Fields.Add(new PlayerPropertyFieldViewModel(field));
		}
		LocalizationManager.RegisterLocalizationProvider(this);
	}

	public void OnCultureChanged(object sender, CultureChangedEventArgs args)
	{
		OnPropertyChanged(nameof(Title));
	}
}

public class PlayerPropertyFieldViewModel : ViewModelBase, ILocalizationProvider
{
	private readonly PlayerPropertiesEditorViewModel.PlayerPropertyDescriptor descriptor;
	private object value;
	private string valueInput;
	private bool boolValue;
	private bool hasError;
	private bool isAvailable = true;
	private bool isRuntimeOverrideEnabled;

	public string Label => LocalizationManager.Instance.GetValue($"UI.PlayerProperties.Fields.{PropertyName}");
	public string PropertyName => descriptor.PropertyName;
	public Type ValueType => descriptor.ValueType;
	public bool IsBoolean => ValueType == typeof(bool);
	public bool IsText => !IsBoolean;
	public bool IsRuntimeOverride => descriptor.Persistence == PlayerPropertiesEditorViewModel.ValuePersistence.RuntimeOverride
		|| descriptor.Persistence == PlayerPropertiesEditorViewModel.ValuePersistence.BuffSource;
	public bool IsBuffSource => descriptor.Persistence == PlayerPropertiesEditorViewModel.ValuePersistence.BuffSource;
	public bool IsComputed => descriptor.Role == PlayerPropertiesEditorViewModel.PropertyRole.Computed;
	public bool IsInfluence => descriptor.Role == PlayerPropertiesEditorViewModel.PropertyRole.Influence;
	public bool HasRoleLabel => IsComputed || IsInfluence;
	public bool IsValueReadOnly => IsComputed && !IsRuntimeOverride;
	public Thickness RowMargin => IsInfluence ? new Thickness(14, 0, 0, 2) : new Thickness(0, 0, 0, 2);
	public string RoleLabel => HasRoleLabel
		? LocalizationManager.Instance.GetValue($"UI.PlayerProperties.Roles.{descriptor.Role}")
		: string.Empty;
	public bool IsRuntimeOverrideEnabled
	{
		get => isRuntimeOverrideEnabled;
		set
		{
			isRuntimeOverrideEnabled = value;
			OnPropertyChanged(nameof(IsRuntimeOverrideEnabled));
		}
	}
	public bool IsAvailable
	{
		get => isAvailable;
		set
		{
			isAvailable = value;
			OnPropertyChanged(nameof(IsAvailable));
		}
	}
	public bool HasError
	{
		get => hasError;
		set
		{
			hasError = value;
			OnPropertyChanged(nameof(HasError));
		}
	}
	public string ValueInput
	{
		get => valueInput;
		set
		{
			valueInput = value;
			if (!IsBoolean)
			{
				bool parsed = TrySetValueFromInput(value);
				if (parsed && IsRuntimeOverride)
					IsRuntimeOverrideEnabled = true;
			}
			OnPropertyChanged(nameof(ValueInput));
		}
	}
	public bool BoolValue
	{
		get => boolValue;
		set
		{
			boolValue = value;
			this.value = value;
			HasError = false;
			if (IsRuntimeOverride)
				IsRuntimeOverrideEnabled = true;
			OnPropertyChanged(nameof(BoolValue));
		}
	}

	public PlayerPropertyFieldViewModel(PlayerPropertiesEditorViewModel.PlayerPropertyDescriptor descriptor)
	{
		this.descriptor = descriptor;
		LocalizationManager.RegisterLocalizationProvider(this);
	}

	public void RefreshFrom(Player player)
	{
		if (IsRuntimeOverride && TryRefreshOverride(player))
			return;

		var property = GetPlayerProperty();
		if (property is null)
		{
			MarkUnavailable();
			return;
		}

		try
		{
			SetValue(property.GetValue(player));
			IsAvailable = true;
		}
		catch (TargetInvocationException)
		{
			MarkUnavailable();
		}
		catch (ArgumentException)
		{
			MarkUnavailable();
		}
	}

	public void ApplyTo(Player player)
	{
		if (!IsAvailable || HasError)
			return;
		if (IsBuffSource)
		{
			ApplyBuffSource(player);
			return;
		}

		if (IsRuntimeOverride)
		{
			ApplyOverride(player);
			return;
		}

		if (IsComputed)
			return;

		var property = GetPlayerProperty();
		if (property is null || !property.CanWrite)
			return;

		try
		{
			property.SetValue(player, value);
		}
		catch (TargetInvocationException)
		{
			MarkUnavailable();
		}
		catch (ArgumentException)
		{
			MarkUnavailable();
		}
	}

	public void OnCultureChanged(object sender, CultureChangedEventArgs args)
	{
		OnPropertyChanged(nameof(Label));
		OnPropertyChanged(nameof(RoleLabel));
	}

	private PropertyInfo GetPlayerProperty()
	{
		return typeof(Player).GetProperty(PropertyName);
	}

	private bool TryRefreshOverride(Player player)
	{
		try
		{
			if (IsBuffSource)
			{
				if (!player.Context.Patches.TryGetPlayerPropertyBuffSource(PropertyName, out bool sourceEnabled, out object sourceValue))
					return false;

				IsRuntimeOverrideEnabled = sourceEnabled;
				SetValue(sourceValue ?? GetDefaultValue(ValueType));
				IsAvailable = true;
				return true;
			}

			if (!player.Context.Patches.TryGetPlayerPropertyOverride(PropertyName, out bool enabled, out object overrideValue))
				return false;

			IsRuntimeOverrideEnabled = enabled;
			if (enabled)
			{
				SetValue(overrideValue);
				IsAvailable = true;
				return true;
			}
		}
		catch
		{
			return false;
		}

		return false;
	}

	private void ApplyOverride(Player player)
	{
		try
		{
			player.Context.Patches.SetPlayerPropertyOverride(PropertyName, IsRuntimeOverrideEnabled, value);
			HasError = false;
		}
		catch
		{
			HasError = true;
		}
	}

	private void ApplyBuffSource(Player player)
	{
		try
		{
			player.Context.Patches.SetPlayerPropertyBuffSource(PropertyName, IsRuntimeOverrideEnabled, value);
			HasError = false;
		}
		catch
		{
			HasError = true;
		}
	}

	private void MarkUnavailable()
	{
		IsAvailable = false;
		HasError = false;
	}

	private void SetValue(object newValue)
	{
		value = newValue;
		HasError = false;

		if (IsBoolean)
		{
			boolValue = newValue is bool b && b;
			OnPropertyChanged(nameof(BoolValue));
		}
		else
		{
			valueInput = FormatValue(newValue);
			OnPropertyChanged(nameof(ValueInput));
		}
	}

	private bool TrySetValueFromInput(string input)
	{
		if (TryParse(input, ValueType, out object parsed))
		{
			value = parsed;
			HasError = false;
			return true;
		}

		HasError = true;
		return false;
	}

	private static string FormatValue(object value)
	{
		return value switch
		{
			float f => f.ToString("0.###", CultureInfo.InvariantCulture),
			double d => d.ToString("0.###", CultureInfo.InvariantCulture),
			IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
			_ => value?.ToString() ?? string.Empty
		};
	}

	private static bool TryParse(string input, Type valueType, out object parsed)
	{
		parsed = null;
		if (string.IsNullOrWhiteSpace(input))
			return false;

		input = input.Trim();

		try
		{
			if (valueType == typeof(int))
			{
				if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
				{
					parsed = value;
					return true;
				}
				return false;
			}
			if (valueType == typeof(byte))
			{
				if (byte.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte value))
				{
					parsed = value;
					return true;
				}
				return false;
			}
			if (valueType == typeof(float))
			{
				if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
				{
					parsed = value;
					return true;
				}
				return false;
			}
		}
		catch
		{
			return false;
		}

		return false;
	}

	private static object GetDefaultValue(Type valueType)
	{
		if (valueType == typeof(bool))
			return false;
		if (valueType == typeof(byte))
			return (byte)0;
		if (valueType == typeof(float))
			return 0f;
		return 0;
	}
}

public class ColorViewModel : ViewModelBase, ILocalizationProvider
{
	private readonly string tipKey;
	private string propertyName;
	private int colorValue;
	public string Tip => LocalizationManager.Instance.GetValue($"UI.PlayerColors.{tipKey}");
	public string PropertyName
	{
		get => propertyName;
		set
		{
			propertyName = value;
			OnPropertyChanged(nameof(PropertyName));
		}
	}
	public string ColorInput
	{
		get => (ColorValue & 0xFFFFFF).ToString("X6");
		set
		{
			if (int.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int s)
				&& s >= 0
				&& s <= 0xFFFFFF)
				ColorValue = s | unchecked((int)0xFF_000000);
		}
	}
	public int ColorValue
	{
		get => colorValue;
		set
		{
			colorValue = value;
			OnPropertyChanged(nameof(ColorValue));
			OnPropertyChanged(nameof(ColorInput));
			OnPropertyChanged(nameof(Color));
		}
	}
	public Color Color
	{
		get
		{
			byte r = (byte)(ColorValue >> 16);
			byte g = (byte)(ColorValue >> 8);
			byte b = (byte)ColorValue;
			return Color.FromRgb(r, g, b);
		}
	}

	public ColorViewModel(string tip, string propertyName)
	{
		tipKey = tip;
		PropertyName = propertyName;
		LocalizationManager.RegisterLocalizationProvider(this);

	}

	public void OnCultureChanged(object sender, CultureChangedEventArgs args)
	{
		OnPropertyChanged(nameof(Tip));
	}
}
