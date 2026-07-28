using BDIP.Application.Common;
using BDIP.Application.Groups;
using BDIP.Application.ImportGroups;
using BDIP.Contracts.ImportGroups;
using BDIP.Domain.Entities;

namespace BDIP.Infrastructure.ImportGroups;

public sealed class GroupImportService : IGroupImportService
{
    private readonly ICsvGroupParser _parser;
    private readonly IGroupRepository _repository;
    private readonly ILdapNumberGenerator _numberGenerator;

    public GroupImportService(
        ICsvGroupParser parser,
        IGroupRepository repository,
        ILdapNumberGenerator numberGenerator)
    {
        _parser = parser;
        _repository = repository;
        _numberGenerator = numberGenerator;
    }

    public async Task<ImportPreviewResponse> PreviewAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default)
    {
        var records = await _parser.ParseAsync(
            csvStream,
            cancellationToken);

        var response = new ImportPreviewResponse
        {
            TotalRows = records.Count
        };

        foreach (var record in records)
        {
            var exists = await _repository.ExistsAsync(
                record.GroupName,
                cancellationToken);

            if (exists)
                response.ExistingGroups++;
            else
                response.NewGroups++;

            response.Groups.Add(
                new ImportPreviewItem
                {
                    GroupName = record.GroupName,
                    Description = record.Description,
                    MemberCount = record.Members.Count,
                    Exists = exists,
                    Status = exists ? "Exists" : "New"
                });
        }

        return response;
    }

    public async Task<ImportExecuteResponse> ImportAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default)
    {
        var records = await _parser.ParseAsync(
            csvStream,
            cancellationToken);

        var response = new ImportExecuteResponse();

        // Ambil GID hanya sekali
        var nextGid =
            await _numberGenerator.GenerateGidNumberAsync();

        foreach (var record in records)
        {
            try
            {
                var exists = await _repository.ExistsAsync(
                    record.GroupName,
                    cancellationToken);

                if (exists)
                {
                    response.Skipped++;

                    response.Details.Add(
                        new ImportResultItem
                        {
                            GroupName = record.GroupName,
                            Status = "Skipped",
                            Message = "Group already exists."
                        });

                    continue;
                }

                var group = new Group
                {
                    Name = record.GroupName,
                    Description = record.Description,
                    GidNumber = nextGid++
                };

                await _repository.CreateAsync(
                    group,
                    cancellationToken);

                response.Imported++;

                response.Details.Add(
                    new ImportResultItem
                    {
                        GroupName = group.Name,
                        Status = "Imported",
                        Message = "Successfully created."
                    });
            }
            catch (Exception ex)
            {
                response.Failed++;

                response.Details.Add(
                    new ImportResultItem
                    {
                        GroupName = record.GroupName,
                        Status = "Failed",
                        Message = ex.Message
                    });
            }
        }

        return response;
    }
}