using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Exceptions;
using TransmissonNET.Domain;
using TransmissonNET.Infrastructure.Settings;
using Xunit;

namespace TransmissonNET.App.Tests;

public class SettingsApiTests : IClassFixture<TestWebApplicationFactory>
{
    private static TorrentTableSettingsDto ToTableDto(TorrentTableSettings table) =>
        new(
            table.Columns.Select(c => new TorrentTableColumnSettingDto(c.Id, c.Visible)).ToList(),
            table.SortColumnId,
            table.SortDescending);

    private readonly HttpClient _client;

    public SettingsApiTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSettings_ReturnsMaskedPassword()
    {
        var response = await _client.GetAsync("/api/settings");

        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<AppSettingsDto>();
        Assert.NotNull(dto);
        Assert.Null(dto.Daemon.Password);
    }

    [Fact]
    public async Task PutSettings_InvalidPort_Returns400()
    {
        var dto = new AppSettingsDto(
            new DaemonConnectionDto("127.0.0.1", 0, "/transmission/rpc", "", null),
            new UiSettingsDto(3, 1280, 800, ToTableDto(TorrentTableSettings.CreateDefault())));

        var response = await _client.PutAsJsonAsync("/api/settings", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public class TorrentsApiTests : IClassFixture<TestWebApplicationFactoryWithMocks>
{
    [Fact]
    public async Task GetTorrents_WhenDaemonFails_Returns502()
    {
        var factory = new TestWebApplicationFactoryWithMocks();
        factory.ClientMock
            .Setup(c => c.GetTorrentsAsync(It.IsAny<DaemonConnection>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DaemonConnectionException("Unauthorized"));

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/torrents");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("Unauthorized", json);
    }
}

public class TestWebApplicationFactoryWithMocks : WebApplicationFactory<Program>
{
    public Mock<ITransmissionClient> ClientMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITransmissionClient));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddScoped(_ => ClientMock.Object);
            services.AddSingleton<ISettingsStore>(_ => new JsonSettingsStore(
                Path.Combine(Path.GetTempPath(), $"tn-settings-{Guid.NewGuid():N}.json")));
        });
    }
}
