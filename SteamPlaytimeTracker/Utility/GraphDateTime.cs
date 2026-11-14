namespace SteamPlaytimeTracker.Utility;

internal sealed record GraphDateTime
{
	private event Action? _propertyChanged;

	public GraphDateTime(DateTime timestamp, Action? propertyChanged = null)
	{
		StartDate = timestamp;
		EndDate = timestamp;

		MinimumDateAllowed = DateTime.MinValue;
		MaximumDateAllowed = DateTime.MaxValue;

		_propertyChanged = propertyChanged;
	}
	
	private GraphDateTime() { }

	public DateTime StartDate { get; private set; }
	public DateTime EndDate { get; private set; }
	public DateTime MinimumDateAllowed { get; private set; }
	public DateTime MaximumDateAllowed { get; private set; }

	/// <summary>
	/// Only to be used by xaml bindings to act as function calls implicetly
	/// </summary>
	public DateTime XAML_SetStartDate
	{
		set
		{
			SetStartDate(value);
			_propertyChanged?.Invoke();
		}
	}
	/// <summary>
	/// Only to be used by xaml bindings to act as function calls implicetly
	/// </summary>
	public DateTime XAML_SetEndDate
	{
		set
		{
			SetEndDate(value);
			_propertyChanged?.Invoke();
		}
	}
	public void SetStartDate(DateTime startDate)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(startDate, MinimumDateAllowed);
		if(EndDate < startDate)
		{
			EndDate = startDate;
		}
		StartDate = startDate;
	}
	public void SetEndDate(DateTime endDate)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThan(endDate, MaximumDateAllowed);
		if(StartDate > endDate)
		{
			StartDate = endDate;
		}
		EndDate = endDate;
	}
	public void SetMinimumAllowedDate(DateTime min)
	{
		if(min > MaximumDateAllowed)
		{
			throw new ArgumentOutOfRangeException(nameof(min), $"New minimum date would be greater than {nameof(MaximumDateAllowed)}");
		}
		MinimumDateAllowed = min;
		if(StartDate < MinimumDateAllowed)
		{
			StartDate = MinimumDateAllowed;
			if(EndDate < StartDate)
			{
				EndDate = StartDate;
			}
		}
		if(EndDate < MinimumDateAllowed)
		{
			EndDate = MinimumDateAllowed;
		}
	}
	public void SetMaximumAllowedDate(DateTime max)
	{
		if(max < MinimumDateAllowed)
		{
			throw new ArgumentOutOfRangeException(nameof(max), $"New maximum date would be less than {nameof(MinimumDateAllowed)}");
		}
		MaximumDateAllowed = max;
		if(EndDate > MaximumDateAllowed)
		{
			EndDate = MaximumDateAllowed;
			if(StartDate > EndDate)
			{
				StartDate = EndDate;
			}
		}
		if(StartDate > MaximumDateAllowed)
		{
			StartDate = MaximumDateAllowed;
		}
	}
}
