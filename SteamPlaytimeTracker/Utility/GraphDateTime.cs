namespace SteamPlaytimeTracker.Utility;

internal sealed record GraphDateTime
{
	public DateTime StartDate { get; private set; }
	public DateTime EndDate { get; private set; }
	public DateTime MinimumDateAllowed { get; private set; }
	public DateTime MaximumDateAllowed { get; private set; }
	
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
