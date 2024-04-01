namespace CSAppUpdater
{
    public class ConfigRootObject
    {
        public string CommonSource { get; set; }
        public string CommonDestination { get; set; }
        public int ExecuteFileNumber { get; set; }
        public ConfigShortcut Shortcut { get; set; }
        public ConfigFile[] Files { get; set; }
    }

    public class ConfigShortcut
    {
        public bool CreateOnDesktop { get; set; }
        public bool CreateOnStartMenu { get; set; }
        public string StartMenuFolder { get; set; }
        public string DisplayName { get; set; }
        public int IconFileNumber { get; set; }
        public int IconIndex { get; set; }
    }

    public class ConfigFile
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public bool UpdateMethodVersion { get; set; }
    }
}