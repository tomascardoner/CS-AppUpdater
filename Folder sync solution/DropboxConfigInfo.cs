public class DropboxConfigInfo
{
    public Personal personal { get; set; }
}

public class Personal
{
    public string path { get; set; }
    public long host { get; set; }
    public bool is_team { get; set; }
    public string subscription_type { get; set; }
}