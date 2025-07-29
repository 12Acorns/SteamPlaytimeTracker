using System.Windows.Input;

namespace SteamPlaytimeTracker.Core;

internal class RelayCommand : ICommand
{
	private readonly Func<object?, bool> _canExecute;
	private readonly Action<object?> _executionBehvaiour;

	public RelayCommand(Action<object?> executionBehvaiour, Func<object?, bool>? canExecute = null)
	{
		_executionBehvaiour = executionBehvaiour;
		_canExecute = (canExecute ??= _ => true);
	}

	public event EventHandler? CanExecuteChanged
	{
		add => CommandManager.RequerySuggested += value;
		remove => CommandManager.RequerySuggested -= value;
	}

	public bool CanExecute(object? parameter) => _canExecute(parameter);

	public void Execute(object? parameter) => _executionBehvaiour(parameter);
}