using ScottPlot;

namespace Tests;

public class PlotColourTests
{
	[Fact]
	public void Test_YearColour_ExpectTrue()
	{
		var colours = new HashSet<Color>();
		for (int year = 0; year < 9; year++)
		{
			Color c = SteamPlaytimeTracker.GlobalData.GetYearPlotColour(year);
			Assert.DoesNotContain(c, colours);
			colours.Add(c);
		}
	}
	[Fact]
	public void Test_MonthColour_ExpectTrue()
	{
		var colours = new HashSet<Color>();
		for (int month = 0; month < 12; month++)
		{
			Color c = SteamPlaytimeTracker.GlobalData.GetMonthPlotColour(month);
			Assert.DoesNotContain(c, colours);
			colours.Add(c);
		}
	}
	[Fact]
	public void Test_DayColour_ExpectTrue()
	{
		var startDate = new DateTime(2023, 1, 1);
		var colours = new HashSet<Color>();
		for (int day = 0; day < 7; day++)
		{
			Color c = SteamPlaytimeTracker.GlobalData.GetDayPlotColour(startDate.Add(TimeSpan.FromDays(day)));
			Assert.DoesNotContain(c, colours);
			colours.Add(c);
		}
	}
}
