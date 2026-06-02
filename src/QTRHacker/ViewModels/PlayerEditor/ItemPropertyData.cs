using QTRHacker.Core.GameObjects.Terraria;
using QTRHacker.Localization;
using System.ComponentModel;
using System.Reflection;

namespace QTRHacker.ViewModels.PlayerEditor;

public abstract class ItemPropertyData : ViewModelBase
{
	private readonly LocalizationItem LocalizationItem;
	private object _Value;
	private bool isDirty;

	public PropertyInfo ItemProperty { get; }
	public Type PropertyType => ItemProperty.PropertyType;

	public string Key { get; }
	public string Tip => LocalizationItem.Value;
	public bool IsDirty
	{
		get => isDirty;
		private set
		{
			isDirty = value;
			OnPropertyChanged(nameof(IsDirty));
		}
	}

	protected object InternalValue
	{
		get => _Value;
		set
		{
			SetInternalValue(value, true);
		}
	}
	protected event EventHandler InternalValueChanged;

	public virtual void UpdateFromItem(Item item)
	{
		SetInternalValue(ItemProperty.GetValue(item), false);
		MarkClean();
	}

	public virtual void UpdateToItem(Item item)
	{
		ItemProperty.SetValue(item, InternalValue);
		MarkClean();
	}

	public object GetValue() => InternalValue;
	public void MarkClean() => IsDirty = false;

	protected void SetInternalValue(object value, bool markDirty)
	{
		_Value = value;
		if (markDirty)
			IsDirty = true;
		InternalValueChanged?.Invoke(this, EventArgs.Empty);
	}

	protected ItemPropertyData(string key)
	{
		Key = key;
		ItemProperty = typeof(Item).GetProperty(Key);
		if (ItemProperty == null)
			throw new Exception($"No such property: {Key}");//TODO: replace it with a user exception
		LocalizationItem = new LocalizationItem($"UI.ItemProperties.{Key}");
		LocalizationItem.PropertyChanged += LocalizationItem_PropertyChanged;
	}
	private void LocalizationItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		OnPropertyChanged(nameof(Tip));
	}
}
public abstract class ItemPropertyData<T> : ItemPropertyData
{
	public virtual T Value
	{
		get => (T)InternalValue;
		set
		{
			InternalValue = value;
			OnPropertyChanged(nameof(Value));
		}
	}

	public ItemPropertyData(string key) : base(key)
	{
		if (ItemProperty.PropertyType != typeof(T))
			throw new Exception(
				$"Type doesn't match. " +
				$"Expected {typeof(T).Name}, got {ItemProperty.PropertyType.Name}. " +
				$"Key: {key}");//TODO: replace it with a user exception
		InternalValueChanged += ItemPropertyData_InternalValueChanged;
	}

	private void ItemPropertyData_InternalValueChanged(object sender, EventArgs e)
	{
		OnPropertyChanged(nameof(Value));
	}
}
