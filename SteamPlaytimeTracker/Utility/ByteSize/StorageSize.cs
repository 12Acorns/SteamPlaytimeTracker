namespace SteamPlaytimeTracker.Utility.ByteSize;

// https://www.unitconverters.net/data-storage-converter.html
internal readonly record struct StorageSize
{
	private readonly ulong _internalBytes;

	public StorageSize(ulong bytes)
	{
		_internalBytes = bytes;
	}

	public double TotalTeraBytes => _internalBytes / (double)StorageSizes.TERABYTE;
	public double TotalTebiBytes => _internalBytes / (double)StorageSizes.TEBIBYTE;
	public double TotalTeraBits => _internalBytes * 8 / (double)StorageSizes.TERABYTE;
	public double TotalGigaBytes => _internalBytes / (double)StorageSizes.GIGABYTE;
	public double TotalGibiBytes => _internalBytes / (double)StorageSizes.GIBIBYTE;
	public double TotalGigaBits => _internalBytes * 8 / (double)StorageSizes.GIGABYTE;
	public double TotalMegaBytes => _internalBytes / (double)StorageSizes.MEGABYTE;
	public double TotalMebiBytes => _internalBytes / (double)StorageSizes.MEBIBYTE;
	public double TotalMegaBits => _internalBytes * 8 / (double)StorageSizes.MEGABYTE;
	public double TotalKiloBytes => _internalBytes / (double)StorageSizes.KILOBYTE;
	public double TotalKibiBytes => _internalBytes / (double)StorageSizes.KIBIBYTE;
	public double TotalKiloBits => _internalBytes * 8 / (double)StorageSizes.KILOBYTE;
	public ulong TeraBytes => _internalBytes / (ulong)StorageSizes.TERABYTE;
	public ulong TebiBytes => _internalBytes / (ulong)StorageSizes.TEBIBYTE;
	public ulong TeraBits =>  _internalBytes * 8 / (ulong)StorageSizes.TERABYTE;
	public ulong GigaBytes => _internalBytes / (ulong)StorageSizes.GIGABYTE;
	public ulong GibiBytes => _internalBytes / (ulong)StorageSizes.GIBIBYTE;
	public ulong GigaBits =>  _internalBytes * 8 / (ulong)StorageSizes.GIGABYTE;
	public ulong MegaBytes => _internalBytes / (ulong)StorageSizes.MEGABYTE;
	public ulong MebiBytes => _internalBytes / (ulong)StorageSizes.MEBIBYTE;
	public ulong MegaBits =>  _internalBytes * 8 / (ulong)StorageSizes.MEGABYTE;
	public ulong KiloBytes => _internalBytes / (ulong)StorageSizes.KILOBYTE;
	public ulong KibiBytes => _internalBytes / (ulong)StorageSizes.KIBIBYTE;
	public ulong KiloBits =>  _internalBytes * 8 / (ulong)StorageSizes.KILOBYTE;
	public ulong Bytes => _internalBytes;

	public static StorageSize From(StorageSizes size, double value) => new((ulong)(value * (double)size));
	public static StorageSize FromBits(StorageSizes size, double value) => From(size, value / 8);

	/// <summary>
	/// Base10
	/// </summary>
	public static StorageSize FromTeraBytes(double terabytes) => new((ulong)StorageConverter.Convert(Differential.TeraToByte, terabytes));
	/// <summary>
	/// Base10
	/// </summary>
	public static StorageSize FromGigaBytes(double gigaBytes) => new((ulong)StorageConverter.Convert(Differential.GigaToByte, gigaBytes));
	/// <summary>
	/// Base10
	/// </summary>
	public static StorageSize FromMegaBytes(double megaBytes) => new((ulong)StorageConverter.Convert(Differential.MegaToByte, megaBytes));
	/// <summary>
	/// Base10
	/// </summary>
	public static StorageSize FromKiloBytes(double kiloBytes) => new((ulong)StorageConverter.Convert(Differential.KiloToByte, kiloBytes));

	/// <summary>
	/// Base2
	/// </summary>
	public static StorageSize FromTebiBytes(double tebiBytes) => new((ulong)StorageConverter.Convert(Differential.TeraToByte, tebiBytes, StorageBase.Base2));
	/// <summary>
	/// Base2
	/// </summary>
	public static StorageSize FromGibiBytes(double gibiBytes) => new((ulong)StorageConverter.Convert(Differential.GigaToByte, gibiBytes, StorageBase.Base2));
	/// <summary>
	/// Base2
	/// </summary>
	public static StorageSize FromMebiBytes(double mebiBytes) => new((ulong)StorageConverter.Convert(Differential.MegaToByte, mebiBytes, StorageBase.Base2));
	/// <summary>
	/// Base2
	/// </summary>
	public static StorageSize FromKibiBytes(double kibiBytes) => new((ulong)StorageConverter.Convert(Differential.KiloToByte, kibiBytes, StorageBase.Base2));

	/// <summary>
	/// Base10
	/// </summary>
	public static StorageSize FromTeraBits(double teraBits) => FromTeraBytes(teraBits / 8);
	/// <summary>
	/// Base10
	/// </summary>
	public static StorageSize FromGigaBits(double gigaBits) => FromGigaBytes(gigaBits / 8);
	/// <summary>
	/// Base10
	/// </summary>
	public static StorageSize FromMegaBits(double megaBits) => FromGigaBytes(megaBits / 8);
	/// <summary>
	/// Base10
	/// </summary
	public static StorageSize FromKiloBits(double kiloBits) => FromGigaBytes(kiloBits / 8);
}

// https://stackoverflow.com/a/20338703
public enum Differential : int
{
	/// <summary>
	/// Convert Bytes to Kilobytes
	/// </summary>
	ByteToKilo,
	/// <summary>
	/// Convert Bytes to Megabytes
	/// </summary>
	ByteToMega,
	/// <summary>
	/// Convert Bytes to Gigabytes
	/// </summary>
	ByteToGiga,
	/// <summary>
	/// Convert Bytes to Teraytes
	/// </summary>
	ByteToTera,
	/// <summary>
	/// Convert Kilobytes to Bytes
	/// </summary>
	KiloToByte,
	/// <summary>
	/// Convert Kilobytes to Megabytes
	/// </summary>
	KiloToMega,
	/// <summary>
	/// Convert Kilobytes to Gigabytes
	/// </summary>
	KiloToGiga,
	/// <summary>
	/// Convert Kilobytes to Terabytes
	/// </summary>
	KiloToTera,
	/// <summary>
	/// Convert Megabytes to Bytes
	/// </summary>
	MegaToByte,
	/// <summary>
	/// Convert Megabytes to Kilobytes
	/// </summary>
	MegaToKilo,
	/// <summary>
	/// Convert Megabytes to Gigabytes
	/// </summary>
	MegaToGiga,
	/// <summary>
	/// Convert Megabytes to Terabytes
	/// </summary>
	MegaToTera,
	/// <summary>
	/// Convert Gigabytes to Bytes
	/// </summary>
	GigaToByte,
	/// <summary>
	/// Convert Gigabytes to Kilobytes
	/// </summary>
	GigaToKilo,
	/// <summary>
	/// Convert Gigabytes to Megabytes
	/// </summary>
	GigaToMega,
	/// <summary>
	/// Convert Gigabytes to Terabytes
	/// </summary>
	GigaToTera,
	/// <summary>
	/// Convert Terabyte to Bytes
	/// </summary>
	TeraToByte,
	/// <summary>
	/// Convert Terabyte to Kilobytes
	/// </summary>
	TeraToKilo,
	/// <summary>
	/// Convert Terabytes to Megabytes
	/// </summary>
	TeraToMega,
	/// <summary>
	/// Convert Terabytes to Gigabytes
	/// </summary>
	TeraToGiga,
}
/// <summary>
/// Enumeration of recognized storage sizes [in Bytes]
/// </summary>
public enum StorageSizes : ulong
{
	/// <summary>
	/// Base 10 Conversion
	/// </summary>
	KILOBYTE = 1000,
	MEGABYTE = 1000000,
	GIGABYTE = 1000000000,
	TERABYTE = 1000000000000,
	/// <summary>
	/// Base 2 Conversion
	/// </summary>
	KIBIBYTE = 1024,
	MEBIBYTE = 1048576,
	GIBIBYTE = 1073741824,
	TEBIBYTE = 1099511627776,
}
/// <summary>
/// Storage powers 10 based or 1024 based
/// </summary>
public enum StorageBase : int
{
	/// <summary>
	/// 1024 Base power, Typically used in memory measurements
	/// </summary>
	Base2,
	/// <summary>
	/// 10 Base power, Used in storage mediums like harddrives
	/// </summary>
	Base10,
}
/// <summary>
/// Convert between base 1024 storage units [TB, GB, MB, KB, Byte]
/// </summary>
public static class StorageConverter
{
	/// <summary>
	/// Convert between base 1024 storage units [TB, GB, MB, KB, Byte]
	/// </summary>
	/// <param name="SizeDifferential">Storage conversion differential [enum]</param>
	/// <param name="UnitSize">Size as mutiple of unit type units [double]</param>
	/// <param name="BaseUnit">Size of the base power [enum]</param>
	/// <returns>Converted unit size [double]</returns>
	public static double Convert(Differential SizeDifferential, double UnitSize, StorageBase BaseUnit = StorageBase.Base10)
	{
		if(UnitSize < 0.000000000001)
			return 0;

		double power1 = (double)StorageSizes.KILOBYTE;
		double power2 = (double)StorageSizes.MEGABYTE;
		double power3 = (double)StorageSizes.GIGABYTE;
		double power4 = (double)StorageSizes.TERABYTE;

		if(BaseUnit == StorageBase.Base2)
		{
			power1 = (double)StorageSizes.KIBIBYTE;
			power2 = (double)StorageSizes.MEBIBYTE;
			power3 = (double)StorageSizes.GIBIBYTE;
			power4 = (double)StorageSizes.TEBIBYTE;
		}

		return SizeDifferential switch
		{
			Differential.ByteToKilo => UnitSize / power1,
			Differential.ByteToMega => UnitSize / power2,
			Differential.ByteToGiga => UnitSize / power3,
			Differential.ByteToTera => UnitSize / power4,
			Differential.KiloToByte => UnitSize * power1,
			Differential.KiloToMega => UnitSize / power1,
			Differential.KiloToGiga => UnitSize / power2,
			Differential.KiloToTera => UnitSize / power3,
			Differential.MegaToByte => UnitSize * power2,
			Differential.MegaToKilo => UnitSize * power1,
			Differential.MegaToGiga => UnitSize / power1,
			Differential.MegaToTera => UnitSize / power2,
			Differential.GigaToByte => UnitSize * power3,
			Differential.GigaToKilo => UnitSize * power2,
			Differential.GigaToMega => UnitSize * power1,
			Differential.GigaToTera => UnitSize / power1,
			Differential.TeraToByte => UnitSize * power4,
			Differential.TeraToKilo => UnitSize * power3,
			Differential.TeraToMega => UnitSize * power2,
			Differential.TeraToGiga => UnitSize * power1,
			_ => 0,
		};
	}
}