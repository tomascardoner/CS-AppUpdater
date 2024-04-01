using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace CSAppUpdater
{
    public partial class formMain : Form
    {

        #region Declarations and form events

        private const string defaultConfigFilename = "Config.json";

        public formMain()
        {
            InitializeComponent();

            this.Icon = Properties.Resources.IconApplication;
            this.Text = CardonerSistemas.My.Application.Info.Title;
        }

        private void Me_Shown(object sender, EventArgs e)
        {
            StartTheProcess();
        }

        #endregion

        #region The process

        private void StartTheProcess()
        {
            ConfigRootObject config = null;

            if (readConfigFile(ref config))
            {
                // Common source folder
                showStatusText("Verificando carpeta de origen...", "");
                config.CommonSource = CardonerSistemas.FileSystem.ProcessFolderName(config.CommonSource, true);
                if (config.CommonSource.Length == 0)
                {
                    config.CommonSource = Application.StartupPath;
                }
                else if (config.CommonSource.StartsWith("."))
                {
                    config.CommonSource = Path.Combine(Application.StartupPath, config.CommonSource);
                }

                // Common destination folder
                showStatusText("Verificando carpeta de destino...", "");
                config.CommonDestination = CardonerSistemas.FileSystem.ProcessFolderName(config.CommonDestination, true);
                if (config.CommonDestination.Length == 0)
                {
                    config.CommonDestination = Application.StartupPath;
                }

                // Calculate progress bar values
                // Starts with 1 for reading config file
                progressbarStatus.Maximum = 1;
                // adds the numer of files to process
                if (config.Files != null)
                {
                    progressbarStatus.Maximum += config.Files.Length;
                }
                // add 1 more if there is a shortcut
                if (config.Shortcut != null)
                {
                    progressbarStatus.Maximum++;
                }

                // Set the first value to progress bar because the first step (read config) is complete
                showStatusText("Procesando archivos...", "");
                progressbarStatus.Value = 1;
                // This is a workaround for the progress bar to show when occurs in small time
                progressbarStatus.Value--;
                progressbarStatus.Value++;
                Application.DoEvents();

                // Process all files
                if (!ProcessFiles(config))
                {
                    return;
                }

                // Verify and create shortcuts if necessary
                ProcessShortcut(config);

                // Finally, execute corresponding file
                if (executeFile(config))
                {
                    Application.Exit();
                }

            }
        }

        private void showStatusText(string statusText, string logText, bool addNewLine = true, bool isError = false)
        {
            // Status text
            if (!string.IsNullOrEmpty(statusText))
            {
                labelStatus.Text = statusText;
            }

            // Log text
            if (addNewLine)
            {
                textboxLog.AppendText(Environment.NewLine);
            }
            if (string.IsNullOrEmpty(logText))
            {
                textboxLog.AppendText(statusText);
            }
            else
            {
                textboxLog.AppendText(logText);
            }

            // Error
            if (isError)
            {
                progressbarStatus.Visible = false;
                labelStatus.Visible = false;
                textboxLog.Visible = true;
                textboxLog.SelectionStart = textboxLog.Text.Length;
                textboxLog.ScrollToCaret();
            }

            // Common
            Application.DoEvents();
        }

        #endregion

        #region Read config file

        private bool readConfigFile(ref ConfigRootObject config)
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

            try
            {
                showStatusText("Leyendo archivo de configuración...", "", false);
                jsonConfigFileString = File.ReadAllText(configFilename);
                showStatusText("", "OK", false);
            }
            catch (Exception ex)
            {
                showStatusText("", "ERROR", false, true);
                MessageBox.Show($"Ha ocurrido un error al leer el archivo de configuración ({configFilename})\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try
            {
                showStatusText("Interpretando archivo de configuración...", "");
                config = JsonSerializer.Deserialize<ConfigRootObject>(jsonConfigFileString);
                showStatusText("", "OK", false);
            }
            catch (Exception ex)
            {
                showStatusText("", "ERROR", false, true);
                MessageBox.Show($"Ha ocurrido un error al interpretar el archivo de configuración ({configFilename})\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    if (!processFile(configFile, config.CommonSource, config.CommonDestination))
                    {
                        return false;
                    }
                    progressbarStatus.Value++;
                    // This is a workaround for the progress bar to show when occurs in small time
                    progressbarStatus.Value--;
                    progressbarStatus.Value++;
                    Application.DoEvents();
                }
            }

            return true;
        }

        private bool processFile(ConfigFile configFile, string commonSourceFolder, string commonDestinationFolder)
        {
            if (configFile.Name == null || configFile.Name.Trim().Length == 0)
            {
                return true;
            }

            // Proceso las carpetas de origen y destino del archivo
            configFile.Source = processFileFolder(commonSourceFolder, configFile.Source);
            configFile.Destination = processFileFolder(commonDestinationFolder, configFile.Destination);

            // Preparo el nombre del archivo y los paths completos de origen y destino
            string fileName = configFile.Name.Trim();
            string sourceFilePath = Path.Combine(configFile.Source, fileName);
            string destinationFilePath = Path.Combine(configFile.Destination, fileName);

            if (!verifyFileExists(sourceFilePath, fileName, "origen", true))
            {
                return false;
            }

            if (!verifyFileExists(destinationFilePath, fileName, "destino", false))
            {
                // El archivo de destino no existe, así que hay que copiarlo
                // pero primero hay que chequear que exista la carpeta de destino
                if (!verifyFolderExistsAndCreate(configFile.Destination))
                {
                    return false;
                }
            }
            else
            {
                // El archivo de destino ya existe. Verificar si hay que copiarlo
                if (configFile.UpdateMethodVersion)
                {
                    // Verificar la versión de ambos archivos
                    if (!verifyIfFileVersionsDiffers(sourceFilePath, destinationFilePath, fileName))
                    {
                        return true;
                    }
                }
                else
                {
                    // Verificar la fecha y el tamaño de ambos archivos
                    if (!verifyIfFileTimeOrSizeDiffers(sourceFilePath, destinationFilePath, fileName))
                    {
                        return true;
                    }
                }
            }

            // Ahora copio el archivo
            return copyFile(sourceFilePath, destinationFilePath, fileName);
        }

        private bool verifyFileExists(string filePath, string fileName, string leyenda, bool isError)
        {
            try
            {
                showStatusText($"Verificando si existe el archivo de {leyenda} ({fileName})...", "");
                if (File.Exists(filePath))
                {
                    showStatusText("", "OK", false);
                    return true;
                }
                else
                {
                    showStatusText("", "NO EXISTE", false, isError);
                    return false;
                }
            }
            catch (Exception ex)
            {
                showStatusText("", "ERROR", false, true);
                MessageBox.Show($"Ha ocurrido un error al verificar si existe el archivo de {leyenda} ({fileName})\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool verifyFolderExistsAndCreate(string folder)
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
                showStatusText("", "Verificando y creando carpeta de destino...ERROR", true, true);
                MessageBox.Show($"Ha ocurrido un error al verificar y crear la carpeta de destino ({folder})\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool verifyIfFileVersionsDiffers(string sourceFilePath, string destinationFilePath, string fileName)
        {
            FileVersionInfo sourceFileVersion;
            FileVersionInfo destinationFileVersion;

            try
            {
                showStatusText($"Verificando versiones de ambos archivos ({fileName})...", "");
                sourceFileVersion = FileVersionInfo.GetVersionInfo(sourceFilePath);
                destinationFileVersion = FileVersionInfo.GetVersionInfo(destinationFilePath);
            }
            catch (Exception ex)
            {
                showStatusText("", "ERROR", false, true);
                MessageBox.Show($"Ha ocurrido un error al obtener la versión de los archivos ({fileName})\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (sourceFileVersion.FileVersion == destinationFileVersion.FileVersion)
            {
                showStatusText("", "IGUALES", false);
                return false;
            }
            else
            {
                showStatusText("", "DISTINTAS", false);
                return true;
            }
        }

        private bool verifyIfFileTimeOrSizeDiffers(string sourceFilePath, string destinationFilePath, string fileName)
        {
            FileInfo sourceFileInfo;
            FileInfo destinationFileInfo;

            try
            {
                showStatusText($"Verificando información de ambos archivos ({fileName})...", "");
                sourceFileInfo = new FileInfo(sourceFilePath);
                destinationFileInfo = new FileInfo(destinationFilePath);
            }
            catch (Exception ex)
            {
                showStatusText("", "ERROR", false, true);
                MessageBox.Show($"Ha ocurrido un error al obtener la información de los archivos ({fileName})\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if ((sourceFileInfo.LastWriteTime == destinationFileInfo.LastWriteTime) && (sourceFileInfo.Length == destinationFileInfo.Length))
            {
                showStatusText("", "IGUALES", false);
                return false;
            }
            else
            {
                showStatusText("", "DISTINTOS", false);
                return true;
            }
        }

        private bool copyFile(string sourceFilePath, string destinationFilePath, string fileName)
        {
            try
            {
                showStatusText($"Copiando el archivo ({fileName})...", "");
                File.Copy(sourceFilePath, destinationFilePath, true);
                showStatusText("", "OK", false);
            }
            catch (Exception ex)
            {
                showStatusText("", "ERROR", false, true);
                MessageBox.Show($"Ha ocurrido un error al copiar el archivo ({fileName})\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        #endregion

        #region Folder processing

        private string processFileFolder(string commonFolder, string fileFolder)
        {
            string fileFolderTemp;

            fileFolderTemp = CardonerSistemas.FileSystem.ProcessFolderName(fileFolder, true);

            if (fileFolderTemp.Length == 0)
            {
                if (commonFolder.Length != 0)
                {
                    fileFolderTemp = commonFolder;
                }
            }
            else if (fileFolderTemp.StartsWith(".") && commonFolder.Length != 0)
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

                if (config.Shortcut.IconFileNumber != null && config.Files != null && config.Shortcut.IconFileNumber <= config.Files.Length - 1)
                {
                    ConfigFile configFile;

                    configFile = config.Files[config.Shortcut.IconFileNumber];
                    iconFilePath = Path.Combine(configFile.Destination, configFile.Name);
                }

                // Create desktop shortcut
                if (config.Shortcut.CreateOnDesktop != null && config.Shortcut.CreateOnDesktop)
                {
                    CardonerSistemas.Shortcut.AddToDesktop(Application.ExecutablePath, Application.StartupPath, config.Shortcut.DisplayName, iconFilePath);
                }

                // Create start menu shortcut
                if (config.Shortcut.CreateOnStartMenu != null && config.Shortcut.CreateOnStartMenu)
                {
                    CardonerSistemas.Shortcut.AddToStartMenu(Application.ExecutablePath, Application.StartupPath, config.Shortcut.StartMenuFolder, config.Shortcut.DisplayName, iconFilePath);
                }
            }

            progressbarStatus.Value++;
            // This is a workaround for the progress bar to show when occurs in small time
            progressbarStatus.Value--;
            progressbarStatus.Value++;
            Application.DoEvents();
        }

        #endregion

        #region Execution of file

        private bool executeFile(ConfigRootObject config)
        {
            if (config.ExecuteFileNumber != null && config.Files != null && config.ExecuteFileNumber <= config.Files.Length - 1)
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
                    showStatusText("", "ERROR", false, true);
                    MessageBox.Show($"Error al iniciar el archivo ({executeFilePath}).\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

            }

            return true;
        }

        #endregion

    }
}
