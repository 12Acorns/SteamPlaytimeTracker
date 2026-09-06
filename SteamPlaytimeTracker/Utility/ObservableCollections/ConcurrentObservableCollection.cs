using SteamPlaytimeTracker.Extensions;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections;

namespace SteamPlaytimeTracker.Utility.ObservableCollections;

internal sealed class ConcurrentObservableCollection<T> : IList, IList<T>, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
{
	private static readonly SemaphoreSlim _addSemaphore = new(1, 1);

	private readonly List<T> _items = [];

	public T this[int index]
	{
		get
		{
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _items.Count);
			ArgumentOutOfRangeException.ThrowIfNegative(index);
			return _items[index];
		}
		set
		{
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _items.Count);
			ArgumentOutOfRangeException.ThrowIfNegative(index);
			_addSemaphore.Wait();
			try
			{
				_items[index] = value;
				
			}
			finally
			{
				_addSemaphore.TryRelease();
			}
		}
	}

	public int Count => _items.Count;
	public bool IsReadOnly { get; } = false;
	public bool IsFixedSize { get; } = false;
	public bool IsSynchronized { get; } = false;
	public object SyncRoot { get; } = new();

	object? IList.this[int index] 
	{
		get => this[index];  
		set
		{
			if(value is not T tValue)
			{
				throw new ArgumentException($"Value is not of type {typeof(T).FullName}", nameof(value));
			}
			this[index] = tValue;
		}
	}

	public event NotifyCollectionChangedEventHandler? CollectionChanged;
	public event PropertyChangedEventHandler? PropertyChanged;

	public void Add(T item)
	{
		_addSemaphore.Wait();
		try
		{
			_items.Add(item);
			PropertyChanged?.OnPropertyChanged(this, nameof(Count));
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
		}
		finally
		{
			_addSemaphore.TryRelease();
		}
	}
	public void AddRange(params ReadOnlySpan<T> items)
	{
		if(items.Length is 0)
		{
			return;
		}
		_addSemaphore.Wait();
		try
		{
			var startIdx = _items.Count;
			_items.AddRange(items);
			PropertyChanged?.OnPropertyChanged(this, nameof(Count));
			for(int i = startIdx; i < _items.Count; i++)
			{
				CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _items[i], i));
			}
		}
		finally
		{
			_addSemaphore.Release();
		}
	}
	public void AddRange(IEnumerable<T> items)
	{
		if(!items.Any())
		{
			return;
		}

		_addSemaphore.Wait();
		try
		{
			var startIdx = _items.Count;
			_items.AddRange(items);
			PropertyChanged?.OnPropertyChanged(this, nameof(Count));
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _items[startIdx], startIdx));
		}
		finally
		{
			_addSemaphore.TryRelease();
		}
	}

	public void Clear()
	{
		_addSemaphore.Wait();
		try
		{
			_items.Clear();
			PropertyChanged?.OnPropertyChanged(this, nameof(Count));
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
		finally
		{
			_addSemaphore.TryRelease();
		}
	}

	public bool Contains(T item) => _items.Contains(item);

	public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

	public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

	public int IndexOf(T item) => _items.IndexOf(item);

	public void Insert(int index, T item)
	{
		_items.Insert(index, item);
		PropertyChanged?.OnPropertyChanged(this, nameof(Count));
		CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item, index));
	}

	public bool Remove(T item)
	{
		if(!_items.Remove(item))
		{
			return false;
		}
		PropertyChanged?.OnPropertyChanged(this, nameof(Count));
		CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item));
		return true;
	}

	public void RemoveAt(int index)
	{
		var item = _items[index];
		_items.RemoveAt(index);
		PropertyChanged?.OnPropertyChanged(this, nameof(Count));
		CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item, index));
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public void Dispose()
	{
		try
		{
			_addSemaphore.Dispose();
		}
		catch { }
	}

	public int Add(object? value)
	{
		if(value is not T tValue)
		{
			throw new ArgumentException($"Value is not of type {typeof(T).FullName}", nameof(value));
		}
		Add(tValue);
		return Count - 1;
	}

	public bool Contains(object? value)
	{
		if(value is not T tValue)
		{
			return false;
		}
		return Contains(tValue);
	}

	public int IndexOf(object? value)
	{
		if(value is not T tValue)
		{
			return -1;
		}
		return IndexOf(tValue);
	}

	public void Insert(int index, object? value)
	{
		if(value is not T tValue)
		{
			throw new ArgumentException($"Value is not of type {typeof(T).FullName}", nameof(value));
		}
		Insert(index, tValue);
	}

	public void Remove(object? value)
	{
		if(value is not T tValue)
		{
			return;
		}
		Remove(tValue);
	}

	public void CopyTo(Array array, int index)
	{
		if(array is not T[] tArray)
		{
			throw new ArgumentException($"Array is not of type {typeof(T).FullName}[]", nameof(array));
		}
		CopyTo(tArray, index);
	}
}