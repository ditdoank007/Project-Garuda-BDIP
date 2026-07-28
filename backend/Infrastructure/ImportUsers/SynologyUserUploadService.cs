using BDIP.Application.ImportUsers;
using BDIP.Application.Users.Import;
using BDIP.Contracts.Users.Import;

namespace BDIP.Infrastructure.ImportUsers;

public class SynologyUserUploadService : ISynologyUserUploadService
{
    private readonly ISynologyUserCsvParser _parser;
    private readonly ISynologyUserImportService _importService;

    private const string UploadDirectory =
        "/app/uploads/synology-users";

    public SynologyUserUploadService(
        ISynologyUserCsvParser parser,
        ISynologyUserImportService importService)
    {
        _parser = parser;
        _importService = importService;
    }

    public async Task<UploadSynologyUserCsvResponse> UploadAsync(
        Stream stream,
        string fileName)
    {
        if (stream == null)
        {
            throw new ArgumentException("CSV file is required.");
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("CSV filename is required.");
        }

        if (!fileName.EndsWith(
            ".csv",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only CSV files are accepted.");
        }

        Directory.CreateDirectory(UploadDirectory);

        var uploadId = Guid.NewGuid().ToString("N");
        var safeFileName = Path.GetFileName(fileName);

        var savedPath = Path.Combine(
            UploadDirectory,
            $"{uploadId}-{safeFileName}");

        await using var output = File.Create(savedPath);

        await stream.CopyToAsync(output);

        var info = new FileInfo(savedPath);

        return new UploadSynologyUserCsvResponse
        {
            UploadId = uploadId,
            FileName = safeFileName,
            FileSize = info.Length
        };
    }

    public async Task<SynologyUserImportPreviewResponse> PreviewAsync(
        string uploadId)
    {
        var filePath = GetUploadedFilePath(uploadId);

        await using var stream = File.OpenRead(filePath);

        return await _parser.PreviewAsync(stream);
    }

    public async Task<ExecuteSynologyUserImportResponse> ExecuteAsync(
        ExecuteUploadedSynologyUserImportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UploadId))
        {
            throw new ArgumentException(
                "Upload ID is required.",
                nameof(request.UploadId));
        }

        if (string.IsNullOrWhiteSpace(request.InitialPassword))
        {
            throw new ArgumentException(
                "Initial password is required.",
                nameof(request.InitialPassword));
        }

        var path = GetUploadedFilePath(request.UploadId);

        return await _importService.ExecuteAsync(
            new ExecuteSynologyUserImportRequest
            {
                CsvPath = path,
                InitialPassword = request.InitialPassword
            });
    }

    private static string GetUploadedFilePath(string uploadId)
    {
        if (string.IsNullOrWhiteSpace(uploadId))
        {
            throw new ArgumentException(
                "Upload ID is required.");
        }

        if (!Directory.Exists(UploadDirectory))
        {
            throw new FileNotFoundException(
                "Upload directory was not found.");
        }

        var files = Directory.GetFiles(
            UploadDirectory,
            $"{uploadId}-*.csv");

        if (files.Length == 0)
        {
            throw new FileNotFoundException(
                $"Uploaded CSV with ID '{uploadId}' was not found.");
        }

        return files[0];
    }
}
