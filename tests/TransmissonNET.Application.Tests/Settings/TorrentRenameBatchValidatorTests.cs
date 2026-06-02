using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Exceptions;
using TransmissonNET.Application.Settings;

namespace TransmissonNET.Application.Tests.Settings;

public class TorrentRenameBatchValidatorTests
{
    [Fact]
    public void ValidateAndNormalize_RejectsDuplicateTargetInSameFolder()
    {
        var request = new TorrentRenameBatchRequestDto(
        [
            new TorrentRenameOperationDto("dir/a.mkv", "same.mkv"),
            new TorrentRenameOperationDto("dir/b.mkv", "same.mkv"),
        ]);

        Assert.Throws<SettingsValidationException>(() =>
            TorrentRenameBatchValidator.ValidateAndNormalize(request));
    }

    [Fact]
    public void ValidateAndNormalize_RejectsSlashInName()
    {
        var request = new TorrentRenameBatchRequestDto(
        [
            new TorrentRenameOperationDto("a.mkv", "bad/name.mkv"),
        ]);

        Assert.Throws<SettingsValidationException>(() =>
            TorrentRenameBatchValidator.ValidateAndNormalize(request));
    }

    [Fact]
    public void ValidateAndNormalize_AcceptsValidOperations()
    {
        var request = new TorrentRenameBatchRequestDto(
        [
            new TorrentRenameOperationDto("dir/a.mkv", "one.mkv"),
            new TorrentRenameOperationDto("dir/b.mkv", "two.mkv"),
        ]);

        var result = TorrentRenameBatchValidator.ValidateAndNormalize(request);

        Assert.Equal(2, result.Count);
    }
}
