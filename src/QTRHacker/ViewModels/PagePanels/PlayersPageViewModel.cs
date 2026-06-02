using QTRHacker.Assets;
using QTRHacker.Commands;
using QTRHacker.Controls;
using QTRHacker.Localization;
using QTRHacker.ViewModels.Common;
using QTRHacker.ViewModels.Common.PropertyEditor;
using QTRHacker.ViewModels.PlayerEditor;
using QTRHacker.Views.Common;
using QTRHacker.Views.PlayerEditor;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace QTRHacker.ViewModels.PagePanels;

public class PlayersPageViewModel : PagePanelViewModel
{
	private readonly List<PetInfo> Pets = new();
	private readonly List<MountInfo> Mounts = new();
	private string infoBoxName;
	private int infoBoxMaxLife;
	private int infoBoxMaxMana;
	private float infoBoxCoordinateX;
	private float infoBoxCoordinateY;

	public RelayCommand EditPlayerCommand { get; }
	public RelayCommand EditPlayerPropertyCommand { get; }
	public RelayCommand TPToPlayerCommand { get; }
	public RelayCommand AddBuffCommand { get; }
	public RelayCommand SetPetCommand { get; }
	public RelayCommand SetMountCommand { get; }

	public PlayersListViewViewModel PlayersListViewViewModel { get; } = new();

	private bool GetIsPlayerSelected(object o) => PlayersListViewViewModel.SelectedPlayerInfo is not null;

	public string InfoBoxName
	{
		get => infoBoxName;
		set
		{
			infoBoxName = value;
			OnPropertyChanged(nameof(InfoBoxName));
		}
	}

	public int? InfoBoxMaxLife
	{
		get => infoBoxMaxLife == 0 ? null : infoBoxMaxLife;
		set
		{
			infoBoxMaxLife = value ?? 0;
			OnPropertyChanged(nameof(InfoBoxMaxLife));
		}
	}

	public int? InfoBoxMaxMana
	{
		get => infoBoxMaxMana == 0 ? null : infoBoxMaxMana;
		set
		{
			infoBoxMaxMana = value ?? 0;
			OnPropertyChanged(nameof(InfoBoxMaxMana));
		}
	}

	public float? InfoBoxCoordinateX
	{
		get => infoBoxCoordinateX == 0 ? null : infoBoxCoordinateX;
		set
		{
			infoBoxCoordinateX = value ?? 0;
			OnPropertyChanged(nameof(InfoBoxCoordinateX));
		}
	}

	public float? InfoBoxCoordinateY
	{
		get => infoBoxCoordinateY == 0 ? null : infoBoxCoordinateY;
		set
		{
			infoBoxCoordinateY = value ?? 0;
			OnPropertyChanged(nameof(InfoBoxCoordinateY));
		}
	}

	private bool ShowWindow_GetMount(out int type)
	{
		MWindow window = new();
		window.SizeToContent = SizeToContent.WidthAndHeight;
		window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		window.Title = "Mount";
		window.MinimizeBox = false;
		Grid grid = new();
		window.Content = grid;

		grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
		grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
		grid.ColumnDefinitions.Add(new ColumnDefinition());
		grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

		Label tip = new();
		tip.Foreground = new SolidColorBrush(Colors.White);
		tip.Content = $"{LocalizationManager.Instance.GetValue("UI.Type")}:";
		grid.Children.Add(tip);
		Grid.SetColumn(tip, 0);

		ComboBox box = new();
		box.Width = 160;
		box.VerticalContentAlignment = VerticalAlignment.Center;
		box.Background = new SolidColorBrush(Color.FromArgb(20, 200, 200, 200));
		box.Foreground = new SolidColorBrush(Color.FromRgb(20, 20, 20));
		grid.Children.Add(box);
		Grid.SetColumn(box, 1);

		box.ItemsSource = Mounts;
		box.DisplayMemberPath = "Name";

		Button btn = new();
		btn.Foreground = new SolidColorBrush(Colors.White);
		btn.Padding = new Thickness(2);
		btn.Content = LocalizationManager.Instance.GetValue("UI.Confirm");
		grid.Children.Add(btn);
		Grid.SetColumn(btn, 2);

		btn.Click += (s, e) =>
		{
			window.DialogResult = true;
			window.Close();
		};

		var result = window.ShowDialog();
		type = 0;
		if (box.SelectedItem is MountInfo mi)
			type = mi.BuffType;
		return result == true;
	}

	private bool ShowWindow_GetPet(out int type)
	{
		MWindow window = new();
		window.SizeToContent = SizeToContent.WidthAndHeight;
		window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		window.Title = "Pet";
		window.MinimizeBox = false;
		Grid grid = new();
		window.Content = grid;

		grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
		grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
		grid.ColumnDefinitions.Add(new ColumnDefinition());
		grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

		Label tip = new();
		tip.Foreground = new SolidColorBrush(Colors.White);
		tip.Content = $"{LocalizationManager.Instance.GetValue("UI.Type")}:";
		grid.Children.Add(tip);
		Grid.SetColumn(tip, 0);

		ComboBox box = new();
		box.Width = 160;
		box.VerticalContentAlignment = VerticalAlignment.Center;
		box.Background = new SolidColorBrush(Color.FromArgb(20, 200, 200, 200));
		box.Foreground = new SolidColorBrush(Color.FromRgb(20, 20, 20));
		grid.Children.Add(box);
		Grid.SetColumn(box, 1);

		box.ItemsSource = Pets;
		box.DisplayMemberPath = "Name";

		Button btn = new();
		btn.Foreground = new SolidColorBrush(Colors.White);
		btn.Padding = new Thickness(2);
		btn.Content = LocalizationManager.Instance.GetValue("UI.Confirm");
		grid.Children.Add(btn);
		Grid.SetColumn(btn, 2);

		btn.Click += (s, e) =>
		{
			window.DialogResult = true;
			window.Close();
		};

		var result = window.ShowDialog();
		type = 0;
		if (box.SelectedItem is PetInfo pi)
			type = pi.Type;
		return result == true;
	}

	private static bool ShowWindow_GetBuff(out int type, out int time)
	{
		type = 0;
		time = 0;

		BuffPickerViewModel viewModel = new();
		MWindow window = new();
		window.Width = 760;
		window.Height = 560;
		window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		window.Title = LocalizationManager.Instance.GetValue("UI.BuffPicker.Title");
		window.MinimizeBox = false;
		window.DataContext = viewModel;

		Grid grid = new()
		{
			Margin = new Thickness(10)
		};
		window.Content = grid;

		grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
		grid.RowDefinitions.Add(new RowDefinition());
		grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

		Grid topGrid = new();
		topGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
		topGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(110) });
		topGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
		topGrid.ColumnDefinitions.Add(new ColumnDefinition());
		grid.Children.Add(topGrid);
		Grid.SetRow(topGrid, 0);

		Label timeTip = NewDialogLabel($"{LocalizationManager.Instance.GetValue("UI.Time")}:");
		topGrid.Children.Add(timeTip);
		Grid.SetColumn(timeTip, 0);

		TextBox timeBox = NewDialogTextBox("3600");
		topGrid.Children.Add(timeBox);
		Grid.SetColumn(timeBox, 1);

		Label searchTip = NewDialogLabel($"{LocalizationManager.Instance.GetValue("UI.Search")}:");
		searchTip.Margin = new Thickness(12, 0, 4, 0);
		topGrid.Children.Add(searchTip);
		Grid.SetColumn(searchTip, 2);

		TextBox searchBox = NewDialogTextBox(string.Empty);
		searchBox.MinWidth = 240;
		searchBox.SetBinding(TextBox.TextProperty, new Binding(nameof(BuffPickerViewModel.FilterText))
		{
			UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
		});
		topGrid.Children.Add(searchBox);
		Grid.SetColumn(searchBox, 3);

		ListBox buffList = new()
		{
			Margin = new Thickness(0, 10, 0, 10),
			Background = new SolidColorBrush(Color.FromArgb(35, 255, 255, 255)),
			BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
			Foreground = new SolidColorBrush(Colors.White),
			ItemTemplate = CreateBuffItemTemplate(),
			ItemsPanel = CreateBuffItemsPanel(),
			SelectionMode = SelectionMode.Single
		};
		buffList.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(BuffPickerViewModel.BuffsView)));
		buffList.SetBinding(Selector.SelectedItemProperty, new Binding(nameof(BuffPickerViewModel.SelectedBuff)));
		buffList.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
		buffList.MouseDoubleClick += (s, e) => Confirm();

		Style itemContainerStyle = new(typeof(ListBoxItem));
		itemContainerStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(0)));
		itemContainerStyle.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(2)));
		itemContainerStyle.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
		buffList.ItemContainerStyle = itemContainerStyle;

		grid.Children.Add(buffList);
		Grid.SetRow(buffList, 1);

		StackPanel buttons = new()
		{
			Orientation = Orientation.Horizontal,
			HorizontalAlignment = HorizontalAlignment.Right
		};
		grid.Children.Add(buttons);
		Grid.SetRow(buttons, 2);

		Button btn = new();
		btn.Foreground = new SolidColorBrush(Colors.White);
		btn.Padding = new Thickness(10, 4, 10, 4);
		btn.MinWidth = 72;
		btn.Content = LocalizationManager.Instance.GetValue("UI.Confirm");
		buttons.Children.Add(btn);

		Button cancelBtn = new();
		cancelBtn.Foreground = new SolidColorBrush(Colors.White);
		cancelBtn.Padding = new Thickness(10, 4, 10, 4);
		cancelBtn.MinWidth = 72;
		cancelBtn.Margin = new Thickness(8, 0, 0, 0);
		cancelBtn.Content = LocalizationManager.Instance.GetValue("UI.Cancel");
		cancelBtn.Click += (s, e) => window.Close();
		buttons.Children.Add(cancelBtn);

		btn.Click += (s, e) => Confirm();

		bool confirmed = window.ShowDialog() == true;
		if (!confirmed || viewModel.SelectedBuff is null)
			return false;

		type = viewModel.SelectedBuff.Type;
		time = viewModel.BuffTime;
		return true;

		void Confirm()
		{
			if (viewModel.SelectedBuff is null)
				return;
			if (!int.TryParse(timeBox.Text, out int parsedTime) || parsedTime <= 0)
			{
				MessageBox.Show(LocalizationManager.Instance.GetValue("UI.BuffPicker.InvalidTime"));
				return;
			}
			viewModel.BuffTime = parsedTime;
			window.DialogResult = true;
			window.Close();
		}
	}

	private static Label NewDialogLabel(string text)
	{
		return new Label
		{
			Content = text,
			Foreground = new SolidColorBrush(Colors.White),
			VerticalContentAlignment = VerticalAlignment.Center,
			Padding = new Thickness(0, 0, 4, 0)
		};
	}

	private static TextBox NewDialogTextBox(string text)
	{
		return new TextBox
		{
			Text = text,
			VerticalContentAlignment = VerticalAlignment.Center,
			Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255)),
			Foreground = new SolidColorBrush(Colors.White),
			CaretBrush = new SolidColorBrush(Colors.White),
			BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
			Margin = new Thickness(0, 0, 0, 0)
		};
	}

	private static ItemsPanelTemplate CreateBuffItemsPanel()
	{
		FrameworkElementFactory panel = new(typeof(WrapPanel));
		panel.SetValue(WrapPanel.OrientationProperty, Orientation.Horizontal);
		return new ItemsPanelTemplate(panel);
	}

	private static DataTemplate CreateBuffItemTemplate()
	{
		FrameworkElementFactory border = new(typeof(Border));
		border.SetValue(FrameworkElement.WidthProperty, 104d);
		border.SetValue(FrameworkElement.HeightProperty, 92d);
		border.SetValue(Border.PaddingProperty, new Thickness(4));
		border.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromArgb(35, 255, 255, 255)));

		FrameworkElementFactory stack = new(typeof(StackPanel));
		stack.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);
		stack.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
		border.AppendChild(stack);

		FrameworkElementFactory icon = new(typeof(ItemSlot));
		icon.SetValue(FrameworkElement.WidthProperty, 44d);
		icon.SetValue(FrameworkElement.HeightProperty, 44d);
		icon.SetValue(Control.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0x50, 0x50, 0x50)));
		icon.SetValue(UIElement.IsHitTestVisibleProperty, false);
		icon.SetValue(Control.FocusableProperty, false);
		icon.SetBinding(ItemSlot.ItemImageSourceProperty, new Binding(nameof(BuffSelectionViewModel.Icon)));
		stack.AppendChild(icon);

		FrameworkElementFactory id = NewBuffTextBlock(12d);
		id.SetBinding(TextBlock.TextProperty, new Binding(nameof(BuffSelectionViewModel.DisplayId)));
		stack.AppendChild(id);

		FrameworkElementFactory name = NewBuffTextBlock(12d);
		name.SetValue(FrameworkElement.WidthProperty, 94d);
		name.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
		name.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
		name.SetBinding(TextBlock.TextProperty, new Binding(nameof(BuffSelectionViewModel.Name)));
		stack.AppendChild(name);

		return new DataTemplate
		{
			VisualTree = border
		};
	}

	private static FrameworkElementFactory NewBuffTextBlock(double fontSize)
	{
		FrameworkElementFactory text = new(typeof(TextBlock));
		text.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Colors.White));
		text.SetValue(TextBlock.FontSizeProperty, fontSize);
		text.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
		text.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
		return text;
	}

	private void LoadPets()
	{
		if (Pets.Any())
			return;
		var vanityPets = new Core.GameObjects.GameObjectArrayV<bool>(HackGlobal.GameContext,
			HackGlobal.GameContext.GameModuleHelper.GetStaticHackObject("Terraria.Main", "vanityPet")).GetAllElements();
		for (int i = 0; i < vanityPets.Length; i++)
		{
			if (vanityPets[i])
				Pets.Add(new PetInfo(i, WikiResLoader.BuffKeys[i]));
		}
	}

	private void LoadMounts()
	{
		if (Mounts.Any())
			return;
		var mountDatum = new Core.GameObjects.GameObjectArray(HackGlobal.GameContext,
			HackGlobal.GameContext.GameModuleHelper.GetStaticHackObject("Terraria.Mount", "mounts"));
		int len = mountDatum.Length;
		for (int i = 0; i < len; i++)
		{
			dynamic data = mountDatum[i];
			int buffType = data.buff;
			Mounts.Add(new MountInfo(i, buffType, WikiResLoader.BuffKeys[buffType]));
		}
	}

	public void UpdateInfo()
	{
		if (PlayersListViewViewModel.SelectedPlayerInfo is not PlayerInfo info)//null check
			return;
		var player = HackGlobal.GameContext.Players[info.ID];
		InfoBoxName = player.Name;
		InfoBoxMaxLife = player.StatLifeMax;
		InfoBoxMaxMana = player.StatManaMax;
		var pos = player.Position;// here we cache the whole struct so that only one read is needed.
		InfoBoxCoordinateX = pos.X;
		InfoBoxCoordinateY = pos.Y;
	}

	public PlayersPageViewModel()
	{
		PlayersListViewViewModel.SelectedPlayerInfoChanged += (s, e) =>
		{
			UpdateInfo();
			EditPlayerCommand.TriggerCanExecuteChanged();
			EditPlayerPropertyCommand.TriggerCanExecuteChanged();
			TPToPlayerCommand.TriggerCanExecuteChanged();
			AddBuffCommand.TriggerCanExecuteChanged();
			SetPetCommand.TriggerCanExecuteChanged();
			SetMountCommand.TriggerCanExecuteChanged();
		};
		PlayersListViewViewModel.AddWeakHandlerToTimer((s, e) => UpdateInfo());
		EditPlayerCommand = new RelayCommand(GetIsPlayerSelected, (o) =>
		{
			var player = HackGlobal.GameContext.Players[PlayersListViewViewModel.SelectedPlayerInfo.ID];
			if (!player.Active)
				return;
			PlayerEditorWindow window = new();
			window.DataContext = new PlayerEditorWindowViewModel(player);
			window.Show();
		});
		EditPlayerPropertyCommand = new RelayCommand(GetIsPlayerSelected, (o) =>
		{
			var player = HackGlobal.GameContext.Players[PlayersListViewViewModel.SelectedPlayerInfo.ID];
			PropertyEditorWindow window = new();
			window.DataContext = new PropertyEditorWindowViewModel();
			window.ViewModel.Roots.Add(new PropertyComplex(player.TypedInternalObject, "Player"));
			window.Show();
		});
		TPToPlayerCommand = new(GetIsPlayerSelected, (o) =>
		{
			var player = HackGlobal.GameContext.Players[PlayersListViewViewModel.SelectedPlayerInfo.ID];
			if (!player.Active)
				return;
			HackGlobal.GameContext.MyPlayer.Position = player.Position;
		});
		AddBuffCommand = new(GetIsPlayerSelected, (o) =>
		{
			if (!ShowWindow_GetBuff(out int type, out int time))
				return;
			HackGlobal.GameContext.MyPlayer.AddBuff(type, time);
		});
		SetPetCommand = new(GetIsPlayerSelected, (o) =>
		{
			LoadPets();
			if (ShowWindow_GetPet(out int type))
				HackGlobal.GameContext.MyPlayer.AddBuff(type, 3600);
		});
		SetMountCommand = new(GetIsPlayerSelected, (o) =>
		{
			LoadMounts();
			if (ShowWindow_GetMount(out int type))
				HackGlobal.GameContext.MyPlayer.AddBuff(type, 3600);
		});
	}
	public class PetInfo : ViewModelBase, ILocalizationProvider
	{
		public int Type { get; }
		public string Key { get; }
		public string Name => LocalizationManager.Instance.GetValue($"BuffName.{Key}", LocalizationType.Game);

		public PetInfo(int type, string key)
		{
			Type = type;
			Key = key;
		}
		public void OnCultureChanged(object sender, CultureChangedEventArgs args)
		{
			OnPropertyChanged(nameof(Name));
		}
	}
	public class MountInfo : ViewModelBase, ILocalizationProvider
	{
		public int Type { get; }
		public int BuffType { get; }
		public string Key { get; }
		public string Name => LocalizationManager.Instance.GetValue($"BuffName.{Key}", LocalizationType.Game);

		public MountInfo(int type, int buffType, string key)
		{
			Type = type;
			BuffType = buffType;
			Key = key;
		}

		public void OnCultureChanged(object sender, CultureChangedEventArgs args)
		{
			OnPropertyChanged(nameof(Name));
		}
	}

	public sealed class BuffPickerViewModel : ViewModelBase
	{
		private string filterText;
		private BuffSelectionViewModel selectedBuff;
		private int buffTime = 3600;

		public ObservableCollection<BuffSelectionViewModel> Buffs { get; } = new();
		public ICollectionView BuffsView { get; }

		public string FilterText
		{
			get => filterText;
			set
			{
				filterText = value;
				BuffsView.Refresh();
				OnPropertyChanged(nameof(FilterText));
			}
		}

		public BuffSelectionViewModel SelectedBuff
		{
			get => selectedBuff;
			set
			{
				selectedBuff = value;
				OnPropertyChanged(nameof(SelectedBuff));
			}
		}

		public int BuffTime
		{
			get => buffTime;
			set
			{
				buffTime = value;
				OnPropertyChanged(nameof(BuffTime));
			}
		}

		public BuffPickerViewModel()
		{
			foreach (var pair in WikiResLoader.BuffKeys.OrderBy(item => item.Key))
			{
				if (pair.Key <= 0)
					continue;
				Buffs.Add(new BuffSelectionViewModel(pair.Key, pair.Value));
			}

			BuffsView = CollectionViewSource.GetDefaultView(Buffs);
			BuffsView.Filter = FilterBuff;
		}

		private bool FilterBuff(object item)
		{
			if (item is not BuffSelectionViewModel buff)
				return false;
			if (string.IsNullOrWhiteSpace(FilterText))
				return true;

			string filter = FilterText.Trim();
			return buff.Type.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)
				|| buff.Key.Contains(filter, StringComparison.OrdinalIgnoreCase)
				|| buff.Name.Contains(filter, StringComparison.OrdinalIgnoreCase);
		}
	}

	public sealed class BuffSelectionViewModel
	{
		public int Type { get; }
		public string Key { get; }
		public string Name { get; }
		public ImageSource Icon { get; }
		public string DisplayId => $"ID: {Type}";

		public BuffSelectionViewModel(int type, string key)
		{
			Type = type;
			Key = key;
			Name = LocalizationManager.Instance.GetValue($"BuffName.{key}", LocalizationType.Game);
			Icon = GameImages.GetBuffImage(type);
		}
	}
}

