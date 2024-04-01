namespace CSAppUpdater
{
    internal static class Program
    {
        internal const string ApplicationTitle = "CS-AppUpdater";
        internal const string Copyright = "Copyright © 2021-2024 Cardoner Sistemas";

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new FormMain());
        }
    }
}