using Microsoft.EntityFrameworkCore;
using SampleStorefront.Context;
using SampleStorefront.Models;
using SampleStorefront.Settings;
using Tomlyn;

namespace SampleStorefront.Services;

public class ServerConfigService : IHostedService
{
  private readonly IServiceProvider _serviceProvider;

  public ServerConfigService(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    using var scope = _serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var pwContext = scope.ServiceProvider.GetRequiredService<PasswordService>();
    var serverSettings = scope.ServiceProvider.GetRequiredService<GlobalServerSettings>();

    await dbContext.Database.EnsureCreatedAsync(cancellationToken);

    string filePath = "appConfig.toml";
    AppConfig appConfig = new();

    if (File.Exists(filePath))
    {
      string content = File.ReadAllText(filePath);
      appConfig = Toml.ToModel<AppConfig>(content);
    }
    // else
    // {
    //   var tomlOut = Toml.FromModel(appConfig);
    //   File.WriteAllText(filePath, tomlOut);
    // }

    if (appConfig.BotConfig == null || appConfig.BotConfig.Uid == null)
    {
      // No configured bot account
      string botPassword = appConfig.BotConfig?.Password ?? "1234";
      string botEmail = appConfig.BotConfig?.Mail ?? "bot@skynet.com";

      var botUser = new User
      {
        Name = "SystemBot",
        Password = pwContext.HashPassword(botPassword) ?? "ERROR_HASHING",
        Email = botEmail,
      };

      dbContext.Users.Add(botUser);

      await dbContext.SaveChangesAsync(cancellationToken);

      var botId = botUser.Id;
      appConfig.BotConfig = new BotConfigTable
      {
        Uid = botId.ToString(),
        Password = botPassword,
        Mail = botEmail
      };

      var tomlOut = Toml.FromModel(appConfig);
      File.WriteAllText(filePath, tomlOut);

      Console.WriteLine("Successfully created bot account.");
    }
    else
    {
      if (!Guid.TryParse(appConfig.BotConfig.Uid, out var botGuid))
        throw new ArgumentException($"Error parsing bot GUID");
      var botAccount = await dbContext.Users
        .AnyAsync(x => x.Id == botGuid, cancellationToken);
      if (botAccount == false)
        throw new ArgumentException($"Invalid Bot ID provided in config file {filePath}");

      serverSettings.BotAccountId = botGuid;
    }
  }

  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
