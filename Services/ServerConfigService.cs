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

  private static async Task<User> CreateBotAccount(AppConfig appConfig, PasswordService pwContext, AppDbContext dbContext, string filePath, CancellationToken cancellationToken)
  {
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
    return botUser;
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
      await CreateBotAccount(appConfig, pwContext, dbContext, filePath, cancellationToken);
    }
    else
    {
      if (!Guid.TryParse(appConfig.BotConfig.Uid, out var botGuid))
        throw new ArgumentException($"Error parsing bot GUID");
      var botAccount = await dbContext.Users
        .AnyAsync(x => x.Id == botGuid, cancellationToken);
      if (botAccount == false)
      {
        Console.WriteLine("Invalid system bot account ID. Creating new one...");
        var botUser = await CreateBotAccount(appConfig, pwContext, dbContext, filePath, cancellationToken);
        botGuid = botUser.Id;
      }
      serverSettings.BotAccountId = botGuid;
    }
  }

  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
