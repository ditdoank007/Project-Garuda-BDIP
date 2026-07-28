using BDIP.Contracts.ImportGroups;

namespace BDIP.Application.ImportGroups;

public interface ICsvGroupParser
{
    Task<IReadOnlyList<CsvGroupRecord>> ParseAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default);
}