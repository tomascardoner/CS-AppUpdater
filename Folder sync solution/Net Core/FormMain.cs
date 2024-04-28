using System.Diagnostics;
using System.Text.Json;

namespace CSAppUpdater
{
    public partial class FormMain : Form
    {

        #region Declarations and form events

        private const string defaultConfigFilename = "Config.json";

        public FormMain()
        {
            InitializeComponent();

            this.Icon = Properties.Resources.IconApplication;
            this.Text = Program.ApplicationTitle;
        }

        private void This_Shown(object sender, EventArgs e)
        {
            StartTheProcess();
        }

        #endregion

        #region The process

        private void StartTheProcess()
        {
            ConfigRootObject config = null;

            if (ReadConfigFile(ref config))
            {
                // Common source folder
                ShowStatusText("Verificando carpeta de origen...", "");
                config.CommonSource = CardonerSistemas.Framework.Base.FileSystem.ProcessPath(config.CommonSource, true);
                if (config.CommonSource.Length == 0)
                {
                    config.CommonSource = Application.StartupPath;
                }
                else if (config.CommonSource.StartsWith('.'))
                {
                    config.CommonSource = Path.Combine(Application.StartupPath, config.CommonSource);
                }

                // Common destination folder
                ShowStatusText("Verificando carpeta de destino...", "");
                config.CommonDestination = CardonerSistemas.Framework.Base.FileSystem.ProcessPath(config.CommonDestination, true);
                if (config.CommonDestination.Length == 0)
                {
                    config.CommonDestination = Application.StartupPath;
                }

                // Calculate progress bar values
                // Starts with 1 for reading config file
                ProgressBarStatus.Maximum = 1;
                // adds the numer of files to process
                if (config.Files != null)
                {
                    ProgressBarStatus.Maximum += config.Files.Length;
                }
                // add 1 more if there is a shortcut
                if (config.Shortcut != null)
                {
                    ProgressBarStatus.Maximum++;
                }

                // Set the first value to progress bar because the first step (read config) is complete
                ShowStatusText("Procesando archivos...", "");
                ProgressBarStatus.Value = 1;
                // This is a workaround for the progress bar to show when occurs in small time
                ProgressBarStatus.Value--;
                ProgressBarStatus.Value++;
                Application.DoEvents();

                // Process all files
                if (!ProcessFiles(config))
                {
                    return;
                }

                // Verify and create shortcuts if necessary
                ProcessShortcut(config);

                // Finally, execute corresponding file
                if (ExecuteFile(config))
                {
                    Application.Exit();
                }

            }
        }

        private void ShowStatusText(string statusText, string logText, bool addNewLine = true, bool isError = false)
        {
            // Status text
            if (!string.IsNullOrEmpty(statusText))
            {
                LabelStatus.Text = statusText;
            }

            // Log text
            if (addNewLine)
            {
                TextBoxLog.AppendText(Environment.NewLine);
            }
            if (string.IsNullOrEmpty(logText))
            {
                TextBoxLog.AppendText(statusText);
            }
            else
            {
                TextBoxLog.AppendText(logText);
            }

            // Error
            if (isError)
            {
                ProgressBarStatus.Visible = false;
                LabelStatus.Visible = false;
                TextBoxLog.Visible = true;
                TextBoxLog.SelectionStart = TextBoxLog.Text.Length;
                TextBoxLog.ScrollToCaret();
            }

            // Common
            Application.DoEvents();
        }

        #endregion

        #region Read config file

        private bool ReadConfigFile(ref ConfigRootObject config)
        {
            string[] commandLineArguments;
            string configFilename;
            string jsonConfigFileString;

            commandLineArguments = Environment.GetCommandLineArgs();
            if (commandLineArguments.Length == 1)
            {
                configFilename = defaultConfigFilename;
            }
            else
            {
                configFilename = commandLineArguments[1];
            }

            if (!Path.Exists(configFilename))
            {
                ShowStatusText("", "ERROR", false, true);
                MessageBox.Show($"El archivo de configuración ({configFilename}) no existe.", Program.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                ShowStatusText("Leyendo archivo de configuración...", "", false);
                jsonConfigFileString = File.ReadAllText(configFilename);
                ShowStatusText("", "OK", false);
            }
            catch (Exception ex)
            {
                ShowStatusText("", "ERROR", false, true);
                MessageBox.Show($"Ha ocurrido un error al leer el archivo de configuración ({configFilename})\n\nError: {ex.Message}", Program.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try
            {
                ShowStatusText("Interpretando archivo de configuración...", "");
                config = JsonSerializer.Deserialize<ConfigRootObject>(jsonConfigFileString);
                ShowStatusText("", "OK", false);
            }
            catch (Exception ex)
            {
                ShowStatusText("", "ERROR", false, true);
                MessageBox.Show($"Ha ocurrido un error al interpretar el archivo de configuración ({configFilename})\n\nError: {ex.Message}", Program.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        #endregion

        #region Files processing

        private bool ProcessFiles(ConfigRootObject config)
        {
            if (config.Files != null)
            {
                foreach (ConfigFile configFile in config.Files)
                {
                    if (!ProcessFile(configFile, config.CommonSource, config.CommonDestination))
                    {
                        return false;
                    }
                    ProgressBarStatus.Value++;
                    // This is a workaround for the progress bar to show when occurs in small time
                    ProgressBarStatus.Value--;
                    ProgressBarStatus.Value++;
                    Application.DoEvents();
                }
            }

            return true;
        }

        private bool ProcessFile(ConfigFile configFile, string commonSourceFolder, string commonDestinationFolder)
        {
            if (configFile.Name == null || configFile.Name.Trim().Length == 0)
            {
                return true;
            }

            // Proceso las carpetas de origen y destino del archivo
            configFile.Source = ProcessFileFolder(commonSourceFolder, configFile.Source);
            configFile.Destination = ProcessFileFolder(commonDestinationFolder, configFile.Destination);

            // Preparo el nombre del archivo y los paths completos de origen y destino
            string fileName = configFile.Name.Trim();
            string sourceFilePath = Path.Combine(configFile.Source, fileName);
            string destinationFilePath = Path.Combine(configFile.Destination, fileName);

            if (!VerifyFileExists(sourceFilePath, fileName, "origen", true))
            {
                return false;
            }

            if (!VerifyFileExists(destinationFilePath, fileName, "destino", false))
            {
                // El archivo de destino no existe, así que hay que copiarlo
                // pero primero hay que chequear que exista la carpeta de destino
                if (!VerifyFolderExistsAndCreate(configFile.Destination))
                {
                    return false;
                }
            }
            else
            {
                // El archivo de destino ya existe. Verificar si hay que copiarlo
                if (!configFile.Overwrite)
                {
                    return true;
                }
                if (configFile.UpdateMethodVersion)
                {
                    // Verificar la versión de ambos archivos
                    if (!VerifyIfFileVersionsDiffers(sourceFilePath, destinationFilePath, fileName))
                    {
                        return true;
                    }
                }
                else
                {
                    // Verificar la fecha y el tamaño de ambos archivos
                    if (!VerifyIfFileTimeOrSizeDiffers(sourceFilePath, destinationFilePath, fileName))
                    {
                        return true;
                    }
                }
            }

            // Ahora copio el archivo
            return CopyFile(sourceFilePath, destinationFilePath, fileName);
        }

        private bool VerifyFileExists(string filePath, string fileName, string leyenda, bool isError)
        {
            try
            {
                ShowStatusText($"Verificando si existe el archivo de {leyenda} ({fileName})...", "");
                if (File.Exists(filePath))
                {
                    ShowStatusText("", "OK", false);
                    return true;
                }
                else
                {
                    ShowStatusText("", "NO EXISTE", false, isError);
                    return false;
                }
            }
            catch (Exception ex)
            {
                ShowStatusText("", "ERROR", false, true);
                MessageBox.Show($"Ha ocurrido un error al verificar si existe el archivo de {leyenda} ({fileName})\n\nError: {ex.Message}", Program.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool VerifyFolderExistsAndCreate(string folder)
        {
            try
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                return true;
            }
            catch (Exception ex)
            {
                ShowStatusText("", "Verificando y creando carpeta de destino...ERROR", true, true);
                MessageBox.Show($"Ha ocurrido un error al verificar y crear la carpeta de destino ({folder})\n\nError: {ex.Message}", Program.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool VerifyIfFileVersionsDiffers(string sourceFilePath, string destinationFilePath, string fileName)
        {
            FileVersionInfo sourceFileVersion;
            FileVersionInfo destinationFileVersion;

            try
            {
                ShowStatusText($"Verificando versiones de ambos archivos ({fileName})...", "");
                sourceFileVersion = FileVersionInfo.GetVersionInfo(sourceFilePath);
                destinationFileVersion = FileVersionInfo.GetVersionInfo(destinationFilePath);
            }
            catch (Exception ex)
            {
                ShowStatusText("", "ERROR", false, true);
                MessageBox.Show($"Ha ocurrido un error al obtener la versión de los archivos ({fileName})\n\nError: {ex.Message}", Program.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (sourceFileVersion.FileVersion == destinationFileVersion.FileVersion)
            {
                ShowStatusText("", "IGUALES", false);
                return false;
            }
            else
            {
                ShowStatusText("", "DISTINTAS", false);
                return true;
            }
        }

        private bool VerifyIfFileTimeOrSizeDiffers(string sourceFilePath, string destinationFilePath, string fileName)
        {
            FileInfo sourceFileInfo;
            FileInfo destinationFileInfo;

            try
            {
                ShowStatusText($"Verificando información de ambos archivos ({fileName})...", "");
                sourceFileInfo = new FileInfo(sourceFilePath);
                destinationFileInfo = new FileInfo(destinationFilePath);
            }
            catch (Exception ex)
            {
                ShowStatusText("", "ERROR", false, true);
                MessageBox.Show($"Ha ocurrido un error al obtener la información de los archivos ({fileName})\n\nError: {ex.Message}", Program.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if ((sourceFileInfo.LastWriteTime == destinationFileInfo.LastWriteTime) && (sourceFileInfo.Length == destinationFileInfo.Length))
            {
                ShowStatusText("", "IGUALES", false);
                return false;
            }
            else
            {
                ShowStatusText("", "DISTINTOS", false);
                return true;
            }
        }

        private bool CopyFile(string sourceFilePath, string destinationFilePath, string fileName)
        {
            try
            {
                ShowStatusText($"Copiando el archivo ({fileName})...", "");
                File.Copy(sourceFilePath, destinationFilePath, true);
                ShowStatusText("", "OK", false);
            }
            catch (Exception ex)
            {
                ShowStatusText("", "ERROR", false, true);
                MessageBox.Show($"Ha ocurrido un error al copiar el archivo ({fileName})\n\nError: {ex.Message}", Program.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        #endregion

        #region Folder processing

        private static string ProcessFileFolder(string commonFolder, string fileFolder)
        {
            string fileFolderTemp;

            fileFolderTemp = CardonerSistemas.Framework.Base.FileSystem.ProcessPath(fileFolder, true);

            if (fileFolderTemp.Length == 0)
            {
                if (commonFolder.Length != 0)
                {
                    fileFolderTemp = commonFolder;
                }
            }
            else if (fileFolderTemp.StartsWith('.') && commonFolder.Length != 0)
            {
                fileFolderTemp = Path.Combine(commonFolder, fileFolderTemp);
            }

            return fileFolderTemp;
        }

        #endregion

        #region Shortcut processing

        private void ProcessShortcut(ConfigRootObject config)
        {
            if (config.Shortcut != null)
            {
                string iconFilePath = null;

                if (config.Files != null && config.Shortcut.IconFileNumber <= config.Files.Length - 1)
                {
                    ConfigFile configFile;

                    configFile = config.Files[config.Shortcut.IconFileNumber];
                    iconFilePath = Path.Combine(configFile.Destination, configFile.Name);
                }

                // Create desktop shortcut
                if (config.Shortcut.CreateOnDesktop)
                {
                    CardonerSistemas.Framework.Base.FileSystem.ShortcutAddToDesktop(string.Empty, config.Shortcut.DisplayName, Application.ExecutablePath, Application.StartupPath, iconFilePath);
                }

                // Create start menu shortcut
                if (config.Shortcut.CreateOnStartMenu)
                {
                    CardonerSistemas.Framework.Base.FileSystem.ShortcutAddToProgramsStartMenu(config.Shortcut.StartMenuFolder, config.Shortcut.DisplayName, Application.ExecutablePath, Application.StartupPath, iconFilePath);
                }
            }

            ProgressBarStatus.Value++;
            // This is a workaround for the progress bar to show when occurs in small time
            ProgressBarStatus.Value--;
            ProgressBarStatus.Value++;
            Application.DoEvents();
        }

        #endregion

        #region Execution of file

        private bool ExecuteFile(ConfigRootObject config)
        {
            if (config.Files != null && config.ExecuteFileNumber <= config.Files.Length - 1)
            {
                ConfigFile configFile;
                string executeFilePath;

                configFile = config.Files[config.ExecuteFileNumber];
                executeFilePath = Path.Combine(configFile.Destination, configFile.Name);

                try
                {
                    System.Diagnostics.Process.Start(executeFilePath);
                }
                catch (System.Exception ex)
                {
                    ShowStatusText("", "ERROR", false, true);
                    MessageBox.Show($"Error al iniciar el archivo ({executeFilePath}).\n\nError: {ex.Message}", Program.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

            }

            return true;
        }

        #endregion

    }
}
