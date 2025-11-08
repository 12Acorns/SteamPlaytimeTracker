using HarfBuzzSharp;
using Microsoft.EntityFrameworkCore;
using SteamPlaytimeTracker.DataTransfer;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

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

	public async Task ExportPlaytimeDataAsync(string path, string exportName, CancellationToken token = default)
	{
		Directory.CreateDirectory(path);
		using var stream = new FileStream(Path.Combine(path, exportName), FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
		var allPlaytimes = await _db.PlaytimeSlices
			.Include(x => x.SteamAppEntry)
				.ThenInclude(x => x.StoreDetails)
			.GroupBy(ps => ps.SteamAppEntry.StoreDetails.Id).ToListAsync(token).ConfigureAwait(false);
		await JsonSerializer.SerializeAsync(stream, 
			new PlaytimeExportStructureContainer(allPlaytimes.Select(g => new AppPlaytimeStructure(g)).ToList()), _options, token)
			.ConfigureAwait(false);
	}
}
