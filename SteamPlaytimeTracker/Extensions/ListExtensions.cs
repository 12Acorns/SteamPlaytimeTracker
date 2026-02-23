namespace SteamPlaytimeTracker.Extensions;

internal static class ListExtensions
{
	extension<T>(List<T> list)
	{
		public void RemoveWhileReverse(Range range, Func<T, int, bool> predicate)
		{
			var (start, length) = range.GetOffsetAndLength(list.Count);
			var end = start + length;
			for(int i = end - 1; i >= start; i--)
			{
				if(!predicate(list[i], i))
				{
					break;
				}
				list.RemoveAt(i);
			}
		}
		public void RemoveWhile(Range range, Func<T, int, bool> predicate)
		{
			var (start, length) = range.GetOffsetAndLength(list.Count);
			var end = start + length;
			for(int i = start; i < end; i++)
			{
				if(!predicate(list[start], i))
				{
					break;
				}
				list.RemoveAt(start);
				end--;
				i--;
			}
		}
		public void RemoveWhileReverse(Range range, Func<T, bool> predicate)
		{
			var (start, length) = range.GetOffsetAndLength(list.Count);
			var end = start + length;
			for(int i = end - 1; i >= start; i--)
			{
				if(!predicate(list[i]))
				{
					break;
				}
				list.RemoveAt(i);
			}
		}
		public void RemoveWhile(Range range, Func<T, bool> predicate)
		{
			var (start, length) = range.GetOffsetAndLength(list.Count);
			var end = start + length;
			for(int i = start; i < end; i++)
			{
				if(!predicate(list[start]))
				{
					break;
				}
				list.RemoveAt(start);
				end--;
				i--;
			}
		}
		public void RemoveLastWhile(Func<T, bool> predicate)
		{
			while(list.Count > 0)
			{
				if(!predicate(list[^1]))
				{
					break;
				}
				list.RemoveAt(list.Count - 1);
			}
		}
		public void RemoveWhile(Func<T, bool> predicate)
		{
			while(list.Count > 0)
			{
				if(!predicate(list[0]))
				{
					break;
				}
				list.RemoveAt(0);
			}
		}
	}
}
