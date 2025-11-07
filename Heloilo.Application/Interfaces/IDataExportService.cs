namespace Heloilo.Application.Interfaces;

public interface IDataExportService
{
    Task<string> ExportUserDataAsJsonAsync(long userId);
    Task<byte[]> ExportUserDataAsPdfAsync(long userId);
}

