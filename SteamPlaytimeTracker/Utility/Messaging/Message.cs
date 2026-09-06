using System.Windows.Media;

namespace SteamPlaytimeTracker.Utility.Messaging;

internal sealed record Message(MessageType Type, MessageCatagory? Catagory, string Content)
{
	public SolidColorBrush IconColour => Type switch
	{
		MessageType.Error => Brushes.Red,
		MessageType.Information => Brushes.Blue,
		_ => Brushes.White
	};
	public Geometry IconData => Type switch
	{
		MessageType.Error => Geometry.Parse(GlobalData.SVGData.ErrorIcon),
		MessageType.Information => Geometry.Parse(GlobalData.SVGData.InformationIcon),
		_ => Geometry.Empty
	};
}