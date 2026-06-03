using System.Runtime.InteropServices;

namespace SteamPlaytimeTracker.Utility;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct Indexed<TItem>(int Index, TItem Item)
{
	public static implicit operator Indexed<TItem>((TItem Item, int Index) tuple) => new(tuple.Index, tuple.Item);
	public static implicit operator Indexed<TItem>((int Index, TItem Item) tuple) => new(tuple.Index, tuple.Item);
	public static implicit operator (int Index, TItem Item)(Indexed<TItem> indexed) => (indexed.Index, indexed.Item);
	public static explicit operator TItem(Indexed<TItem> indexed) => indexed.Item;

	public void Deconstruct(out int index, out TItem item)
	{
		index = Index;
		item = Item;
	}
}
