namespace ApplicationManager.Downloader.Models
{
    public interface IExecutable
    {
        string BaseDirectory { get; set; }
        ApplicationExec Exec { get; }
        Check Check { get; }
    }
}
