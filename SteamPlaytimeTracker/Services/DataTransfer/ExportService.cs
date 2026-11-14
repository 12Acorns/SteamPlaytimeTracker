using SteamPlaytimeTracker.DataTransfer;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.IO;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.DbObject;

namespace SteamPlaytimeTracker.Services.DataTransfer;

internal sealed class ExportService
{
	private static readonly JsonSerializerOptions _options = new() 
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		WriteIndented = true 
	};


	private readonly DbAccess _db;

	public ExportService(DbAccess db)
	{
		_db = db;
	}

	public async Task ExportAllPlaytimeDataAsync(string path, string fileName, CancellationToken token = default)
	{
		Directory.CreateDirectory(path);
		using var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
		var allPlaytimes = await _db.PlaytimeSlices
			.Include(x => x.SteamAppEntry)
				.ThenInclude(x => x.StoreDetails)
			.GroupBy(ps => ps.SteamAppEntry.StoreDetails.Id).ToListAsync(token).ConfigureAwait(false);
		await JsonSerializer.SerializeAsync(stream, 
			new PlaytimeExportStructureContainer(allPlaytimes.Select(g => new AppPlaytimeStructure(g)).ToList()), _options, token)
			.ConfigureAwait(false);
	}
	public void ExportPlaytimeData(Stream stream, AppDTO appDTO)
	{
		var appToExport = _db.UserApps.FirstOrDefault(x => x.SteamApp != null && x.SteamApp.AppId == appDTO.AppId);
		if(appToExport is null)
		{
			return;
		}
		JsonSerializer.Serialize(stream, new AppPlaytimeStructure(appToExport), _options);
	}
}
