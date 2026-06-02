using QTRHacker.Core.GameObjects.Terraria;
using QTRHacker.Localization;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace QTRHacker.ViewModels.PlayerEditor;

public class ItemPropertiesPanelViewModel : ViewModelBase
{
	private Item currentItem;
	private readonly ItemPropertyData_TextBox<int> typeProperty;
	private readonly ItemPropertyData_ComboBox<byte> prefixProperty;

	public ObservableCollection<ItemPropertyData> ItemPropertyDatum
	{
		get;
	} = new();
	private int _Columns = 2;

	public int Rows => (ItemPropertyDatum.Count + Columns - 1) / Columns;

	public int Columns
	{
		get => _Columns;
		set
		{
			_Columns = value;
			OnPropertyChanged(nameof(Columns));
			OnPropertyChanged(nameof(Rows));
		}
	}
	public void UpdatePropertiesFromItem(Item item)
	{
		currentItem = item;
		foreach (var prop in ItemPropertyDatum)
			prop.UpdateFromItem(item);
		UpdatePrefixSource(item);
	}

	public void UpdatePropertiesToItem(Item item)
	{
		bool typeChanged = IsDirty("Type");
		bool prefixChanged = IsDirty("Prefix");

		if (typeChanged || prefixChanged)
		{
			int type = Convert.ToInt32(GetValue("Type"));
			byte prefix = Convert.ToByte(GetValue("Prefix"));
			item.SetDefaultsAndPrefix(type, prefix);
		}

		foreach (var prop in ItemPropertyDatum)
		{
			if (!prop.IsDirty || prop.Key == "Type" || prop.Key == "Prefix")
				continue;
			prop.UpdateToItem(item);
		}

		foreach (var prop in ItemPropertyDatum)
			prop.MarkClean();
		UpdatePropertiesFromItem(item);
	}

	public object GetValue(string key) => ItemPropertyDatum.First(t => t.Key == key).GetValue();
	private bool IsDirty(string key) => ItemPropertyDatum.First(t => t.Key == key).IsDirty;

	public ItemPropertiesPanelViewModel()
	{
		ItemPropertyDatum.CollectionChanged += ItemPropertyDatum_CollectionChanged;

		typeProperty = new ItemPropertyData_TextBox<int>("Type");
		typeProperty.PropertyChanged += TypeProperty_PropertyChanged;
		ItemPropertyDatum.Add(typeProperty);
		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("Damage"));
		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("Stack"));
		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<float>("KnockBack"));
		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("Crit"));
		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<float>("Scale"));

		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("BuffType"));
		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("BuffTime"));

		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("HealLife"));
		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("HealMana"));

		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("UseTime"));
		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("UseAnimation"));

		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("FishingPole"));
		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("Bait"));

		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("Shoot"));
		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<float>("ShootSpeed"));

		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("Defense"));

		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("Pick"));
		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("Axe"));
		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("Hammer"));

		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("CreateTile"));
		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("PlaceStyle"));
		ItemPropertyDatum.Add(new ItemPropertyData_TextBox<int>("CreateWall"));

		prefixProperty = new ItemPropertyData_ComboBox<byte>("Prefix");
		prefixProperty.Source.Add(new Prefix("None", 0));
		ItemPropertyDatum.Add(prefixProperty);
		ItemPropertyDatum.Add(new ItemPropertyData_CheckBox("AutoReuse"));
		ItemPropertyDatum.Add(new ItemPropertyData_CheckBox("Accessory"));
	}

	private void UpdatePrefixSource(Item item)
	{
		UpdatePrefixSource(item.GetCompatiblePrefixes()
			.Where(t => t > 0 && t <= byte.MaxValue)
			.Select(t => (byte)t)
			.Distinct()
			.ToList());
	}

	private void UpdatePrefixSource(IEnumerable<byte> prefixValues)
	{
		prefixProperty.Source.Clear();
		prefixProperty.Source.Add(new Prefix("None", 0));
		foreach (byte prefix in prefixValues)
			prefixProperty.Source.Add(new Prefix(
				GetPrefixKey(prefix),
				prefix,
				GetPrefixLocalizationKey(prefix),
				GetPrefixSuffix(prefix)));
	}

	private static string GetPrefixKey(byte prefix)
	{
		string key = Enum.GetName(typeof(Models.Prefixes), prefix);
		return key ?? prefix.ToString();
	}

	private static string GetPrefixLocalizationKey(byte prefix)
	{
		if (prefix == (byte)Models.Prefixes.Legendary2)
			return nameof(Models.Prefixes.Legendary);
		return GetPrefixKey(prefix);
	}

	private static string GetPrefixSuffix(byte prefix)
	{
		if (prefix == (byte)Models.Prefixes.Legendary2)
			return " (Yoyo)";
		return string.Empty;
	}

	private void TypeProperty_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ItemPropertyData<int>.Value) && typeProperty.IsDirty && currentItem != null)
			UpdatePrefixSource(Item.GetCompatiblePrefixes(currentItem.Context, typeProperty.Value)
				.Where(t => t > 0 && t <= byte.MaxValue)
				.Select(t => (byte)t)
				.Distinct()
				.ToList());
	}

	private void ItemPropertyDatum_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(Rows));
	}

	public class Prefix : ViewModelBase, ILocalizationProvider
	{
		public string Key { get; }
		public string LocalizationKey { get; }
		public string Suffix { get; }
		public byte Value { get; }

		public string Name
		{
			get
			{
				if (Key == "None")
					return LocalizationManager.Instance.GetValue($"UI.None");
				string localizationKey = $"Prefix.{LocalizationKey}";
				string text = LocalizationManager.Instance.GetValue(localizationKey, LocalizationType.Game);
				return (text == localizationKey ? Key : text) + Suffix;
			}
		}

		public Prefix(string key, byte value, string localizationKey = null, string suffix = "")
		{
			Key = key;
			Value = value;
			LocalizationKey = localizationKey ?? key;
			Suffix = suffix;
			LocalizationManager.RegisterLocalizationProvider(this);
		}

		public void OnCultureChanged(object sender, CultureChangedEventArgs args)
		{
			OnPropertyChanged(nameof(Name));
		}
	}
}
