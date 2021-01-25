public class ConfigRootObject
{
    public string commonSource { get; set; }
    public string commonDestination { get; set; }
    public int executeFileNumber { get; set; }
    public ConfigShortcut shortcut { get; set; }
    public ConfigFile[] files { get; set; }
}

public class ConfigShortcut
{
    public bool createOnDesktop { get; set; }
    public bool createOnStartMenu { get; set; }
    public string startMenuFolder { get; set; }
    public string displayName { get; set; }
    public int iconFileNumber { get; set; }
    public int iconIndex { get; set; }
}

public class ConfigFile
{
    public string name { get; set; }
    public string source { get; set; }
    public string destination { get; set; }
    public bool updateMethodVersion { get; set; }
}
