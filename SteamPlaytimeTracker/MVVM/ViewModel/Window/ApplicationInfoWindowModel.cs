using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Services.Messaging;
using SteamPlaytimeTracker.Utility.Messaging;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Threading;

namespace SteamPlaytimeTracker.MVVM.ViewModel.Window;

internal sealed class ApplicationInfoWindowModel : MenuModel
{
	private readonly IMessageExchangeService _messageExchangeService;
	private readonly ILifetimeService _asyncLifetimeService;
	private readonly ObservableCollection<Message> _messages = [];
	private readonly SemaphoreSlim _messageRefreshLock = new(1, 1);

	public ApplicationInfoWindowModel(IMessageExchangeService messageExchangeService, ILifetimeService asyncLifetimeService)
	{
		Title = "Application Info";
		_messageExchangeService = messageExchangeService;
		_asyncLifetimeService = asyncLifetimeService;
		_messages = new(_messageExchangeService.GetMessages());
		MessagesView = (ListCollectionView)CollectionViewSource.GetDefaultView(_messages);
		var refreshThread = new Thread(RefreshMessages)
		{
			IsBackground = true
		};
		refreshThread.Start();
	}

	public override void OnLoad(params object[] args)
	{
		base.OnLoad(args);
		SyncMessages();
	}

	public void SyncMessages()
	{
		var currentMessages = _messageExchangeService.GetMessages();
		var messagesToAdd = currentMessages.Except(_messages);
		_messages.AddRange(messagesToAdd);
	}
	public ListCollectionView MessagesView
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	} = default!;

	private async void RefreshMessages()
	{
		try
		{
			while(true)
			{
				await _messageRefreshLock.WaitAsync(TimeSpan.FromSeconds(4));
				Dispatcher.Invoke(SyncMessages, DispatcherPriority.Normal, _asyncLifetimeService.CancellationToken);
			}
		}
		catch { }
		finally
		{
			_messageRefreshLock.Release();
		}
	}
}