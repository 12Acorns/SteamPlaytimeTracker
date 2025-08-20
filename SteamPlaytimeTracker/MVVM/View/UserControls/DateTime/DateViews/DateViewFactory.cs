using FastEnumUtility;
using OneOf.Monads;
using System.Linq.Expressions;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.DateTime.DateViews;

public static class DateViewFactory
{
	private static readonly Func<IDateView>[] _view = CreateByIndexed();
	private static readonly int _max = (int)FastEnum.GetMaxValue<DateViewSelected>()!.Value;
	private static readonly int _min = (int)FastEnum.GetMinValue<DateViewSelected>()!.Value;

	public static Option<IDateView> Get(DateViewSelected selected)
	{
		var selectedIndex = (int)selected;
		if(selectedIndex < _min || selectedIndex > _max)
		{
			return new None();
		}
		return new Some<IDateView>(_view[selectedIndex]());
	}
	private static Func<IDateView>[] CreateByIndexed() => FastEnum.GetValues<DateViewSelected>().OrderBy(x => (int)x).Select(x =>
	{
		var className = $"Date{x.FastToString()}";
		var type = GetOrThrow(className);
		return CreateInstanceFuncAlt(type);
	}).ToArray();
	private static Type GetOrThrow(string className)
	{
		var type = typeof(IDateView).Assembly.GetType($"SteamPlaytimeTracker.MVVM.View.UserControls.DateTime.DateViews.{className}");
		if(type == null || !typeof(IDateView).IsAssignableFrom(type))
		{
			throw new InvalidOperationException($"Type {className} does not implement IDateView or is not found.");
		}
		return type!;
	}
	private static Func<IDateView> CreateInstanceFuncAlt(Type type) =>
		Expression.Lambda<Func<IDateView>>(Expression.New(type)).Compile();
}