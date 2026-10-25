namespace Acderator.Server;

public sealed class ServerOptions
{
    public bool MaintenanceMode { get; set; }
    public int AuthSessionTtlSeconds { get; set; } = 3600;
    public int ScoreSessionTtlSeconds { get; set; } = 900;
    public int MaxPasswordBytes { get; set; } = 128;
    public int MaxUuidBytes { get; set; } = 255;
    public string MasterDataPath { get; set; } = "Data/master.json";
    public string ChartDataDirectory { get; set; } = "Data/charts";
}
