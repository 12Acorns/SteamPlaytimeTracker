using SteamPlaytimeTracker.Extensions;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using SteamPlaytimeTracker.Core;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.Settings;

public partial class SearchBarFilteredDropDownUC : UserControl
{
	private static readonly DependencyProperty _searchBarBackgroundPoperty = 
		DependencyProperty.Register<Brush, SearchBarFilteredDropDownUC>("SearchBarBackground");
	private static readonly DependencyProperty _searchBarTextColourProperty =
		DependencyProperty.Register<Brush, SearchBarFilteredDropDownUC>("SearchBarTextColour");
	private static readonly DependencyProperty _itemSourceProperty = 
		DependencyProperty.Register<object, SearchBarFilteredDropDownUC>("SearchItemsSource");
	private static readonly DependencyProperty _onTextChangedProperty = 
		DependencyProperty.Register<RelayCommand, SearchBarFilteredDropDownUC>("OnTextChanged");
	private static readonly DependencyProperty _selectedSearchItemProperty = 
		DependencyProperty.Register<object, SearchBarFilteredDropDownUC>("SelectedSearchItem");
	private static readonly DependencyProperty _keyDownSearchProperty =
		DependencyProperty.Register<RelayCommand, SearchBarFilteredDropDownUC>("OnKeyDownSearch");

	public SearchBarFilteredDropDownUC()
	{
		InitializeComponent();
	}

	public Brush SearchBarBackground
	{
		get => (Brush)GetValue(_searchBarBackgroundPoperty);
		set => SetValue(_searchBarBackgroundPoperty, value);
	}
	public Brush SearchBarTextColour
	{
		get => (Brush)GetValue(_searchBarTextColourProperty);
		set => SetValue(_searchBarTextColourProperty, value);
	}
	public object SearchItemsSource
	{
		get => GetValue(_itemSourceProperty);
		set => SetValue(_itemSourceProperty, value);
	}
	public RelayCommand OnTextChanged
	{
		get => (RelayCommand)GetValue(_onTextChangedProperty);
		set => SetValue(_onTextChangedProperty, value);
	}
	public object SelectedSearchItem
	{
		get => GetValue(_selectedSearchItemProperty);
		set => SetValue(_selectedSearchItemProperty, value);
	}
	public RelayCommand OnKeyDownSearch
	{
		get => (RelayCommand)GetValue(_keyDownSearchProperty);
		set => SetValue(_keyDownSearchProperty, value);
	}

	private void TextBox_TextChanged(object sender, TextChangedEventArgs e) => OnTextChanged?.Execute(
		(List<object>)[this, ((ComboBox)sender)?.Text ?? "", e]);

	private void SearchText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) => OnKeyDownSearch?.Execute(
		(List<object>)[this, e]);
}
