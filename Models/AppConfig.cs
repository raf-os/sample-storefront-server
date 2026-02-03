namespace SampleStorefront.Models;

class AppConfig
{
  public BotConfigTable? BotConfig { get; set; }
}

class BotConfigTable
{
  public string? Uid { get; set; }
  public string? Mail { get; set; }
  public string? Password { get; set; }
}
