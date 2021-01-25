using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace CSAppUpdater
{
    public partial class formMain : Form
    {
        private const string defaultConfigFilename = "Config.json";
        private const string folderTagDropbox = "{DROPBOX}";
        private const string folderTagGoogleDrive = "{GOOGLEDRIVE}";
        private const string folderTagOneDrive = "{ONEDRIVE}";
        private const string folderTagICloudDrive = "{ICLOUDDRIVE}";

        public formMain()
        {
            InitializeComponent();

            this.Icon = Properties.Resources.IconApplication;
            this.Text = CardonerSistemas.My.Application.Info.Title;
        }

        private void formMain_Shown(object sender, EventArgs e)
        {
            ConfigRootObject config = null;

            if (readConfigFile(ref config))
            {
                string commonSourceFolder = processFolderName(config.commonSource);
                string commonDestinationFolder = processFolderName(config.commonDestination);
                string executeFilePath = "";

                if (commonDestinationFolder.Length == 0)
                {
                    commonDestinationFolder = CardonerSistemas.FileSystem.PathAddBackslash(Application.StartupPath);
                }

                if (processFiles(config, commonSourceFolder, commonDestinationFolder))
                {
                    processShortcut(config);

                    if (executeFile(config, commonDestinationFolder))
                    {
                        Application.Exit();
                    }
                }
            }
        }

        private void showStatusText(string newText, bool addNewLine = true)
        {
            if (addNewLine)
            {
                textboxStatus.AppendText(Environment.NewLine);
            }
            textboxStatus.AppendText(newText);
            Application.DoEvents();
        }

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
                showStatusText("Leyendo archivo de configuración...", false);
                jsonConfigFileString = File.ReadAllText(configFilename);
                showStatusText("OK", false);
            }
            catch (Exception ex)
            {
                showStatusText("ERROR", false);
                MessageBox.Show($"Ha ocurrido un error al leer el archivo de configuración ({configFilename})\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try
            {
                showStatusText("Interpretando archivo de configuración...");
                config = JsonSerializer.Deserialize<ConfigRootObject>(jsonConfigFileString);
                showStatusText("OK", false);
            }
            catch (Exception ex)
            {
                showStatusText("ERROR", false);
                MessageBox.Show($"Ha ocurrido un error al interpretar el archivo de configuración ({configFilename})\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        #endregion

        #region Files processing

        private bool processFiles(ConfigRootObject config, string commonSourceFolder, string commonDestinationFolder)
        {
            if (config.files != null)
            {
                foreach (ConfigFile configFile in config.files)
                {
                    if (!processFile(configFile, commonSourceFolder, commonDestinationFolder))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool processFile(ConfigFile configFile, string commonSourceFolder, string commonDestinationFolder)
        {
            string sourceFolder = "";
            string destinationFolder = "";

            processSourceFolder(commonSourceFolder, configFile.source, ref sourceFolder);
            processDestinationFolder(commonDestinationFolder, configFile.destination, ref destinationFolder);

            if (configFile.name == null || configFile.name.Trim().Length == 0)
            {
                return true;
            }

            string fileName = configFile.name.Trim();
            string sourceFilePath = sourceFolder + fileName;
            string destinationFilePath = destinationFolder + fileName;

            if (!File.Exists(sourceFilePath))
            {
                showStatusText($"El archivo de origen ({fileName}) no existe.");
                return false;
            }
            if (!File.Exists(destinationFilePath))
            {
                // El archivo de destino no existe, así que hay que copiarlo
                if (!copyFile(sourceFilePath, destinationFilePath, fileName))
                {
                    return false;
                }
            }
            else
            {
                // El archivo de destino ya existe. Verificar si hay que copiarlo
                if (configFile.updateMethodVersion)
                {
                    // Verificar la versión de ambos archivos
                    FileVersionInfo sourceFileVersion;
                    FileVersionInfo destinationFileVersion;

                    try
                    {
                        sourceFileVersion = FileVersionInfo.GetVersionInfo(sourceFilePath);
                        destinationFileVersion = FileVersionInfo.GetVersionInfo(destinationFilePath);
                    }
                    catch (Exception ex)
                    {
                        showStatusText("ERROR", false);
                        MessageBox.Show($"Ha ocurrido un error al obtener las versiones del archivo ({fileName})\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    if (sourceFileVersion.FileVersion != destinationFileVersion.FileVersion)
                    {
                        if (!copyFile(sourceFilePath, destinationFilePath, fileName))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    // Verificar la fecha y el tamaño de ambos archivos
                    FileInfo sourceFileInfo;
                    FileInfo destinationFileInfo;

                    try
                    {
                        sourceFileInfo = new FileInfo(sourceFilePath);
                        destinationFileInfo = new FileInfo(destinationFilePath);
                    }
                    catch (Exception ex)
                    {
                        showStatusText("ERROR", false);
                        MessageBox.Show($"Ha ocurrido un error al obtener la información del archivo ({fileName})\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    if ((sourceFileInfo.LastWriteTime != destinationFileInfo.LastWriteTime) || (sourceFileInfo.Length != destinationFileInfo.Length))
                    {
                        if (!copyFile(sourceFilePath, destinationFilePath, fileName))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool copyFile(string sourceFilePath, string destinationFilePath, string fileName)
        {
            try
            {
                showStatusText($"Copiando el archivo ({fileName})...");
                File.Copy(sourceFilePath, destinationFilePath, true);
                showStatusText("OK", false);
            }
            catch (Exception ex)
            {
                showStatusText("ERROR", false);
                MessageBox.Show($"Ha ocurrido un error al copiar el archivo ({fileName})\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        #endregion

        #region Folder processing

        private void processSourceFolder(string commonSourceFolder, string fileSourceFolder, ref string sourceFolder)
        {
            sourceFolder = processFolderName(fileSourceFolder);

            if (sourceFolder.Length == 0)
            {
                if (commonSourceFolder.Length != 0)
                {
                    sourceFolder = commonSourceFolder;
                }
            }
        }

        private void processDestinationFolder(string commonDestinationFolder, string fileDestinationFolder, ref string destinationFolder)
        {
            destinationFolder = processFolderName(fileDestinationFolder);

            if (destinationFolder.Length == 0)
            {
                if (commonDestinationFolder.Length != 0)
                {
                    destinationFolder = commonDestinationFolder;
                }
            }
        }

        private string processFolderName(string folderName)
        {
            string folderNameProcessed = folderName;
            string dropboxFolder = "";

            if (folderName == null)
            {
                return "";
            }

            // Replace DropBox path
            if (folderName.Contains(folderTagDropbox))
            {
                if (getDropboxFolder(ref dropboxFolder))
                {
                    folderNameProcessed = folderName.Replace(folderTagDropbox, dropboxFolder).Trim();
                }
            }

            return CardonerSistemas.FileSystem.PathAddBackslash(folderNameProcessed);
        }

        private bool getDropboxFolder(ref string folder)
        {
            const string folderName = "Dropbox";
            const string configFilename = "info.json";

            string applicationDatafolder;
            string configFilePath;
            string configFileString;
            DropboxConfigInfo configInfo;

            // Gets the path to the Dropbox config file
            applicationDatafolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (applicationDatafolder.Length != 0)
            {
                configFilePath = Path.Combine(applicationDatafolder, folderName, configFilename);

                if (File.Exists(configFilePath))
                {
                    try
                    {
                        showStatusText("Leyendo archivo de configuración de Dropbox...", false);
                        configFileString = File.ReadAllText(configFilePath);
                        showStatusText("OK", false);
                    }
                    catch (Exception ex)
                    {
                        showStatusText("ERROR", false);
                        MessageBox.Show($"Ha ocurrido un error al leer el archivo de configuración de Dropbox ({configFilePath})\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    try
                    {
                        showStatusText("Interpretando archivo de configuración de Dropbox...");
                        configInfo = JsonSerializer.Deserialize<DropboxConfigInfo>(configFileString);
                        showStatusText("OK", false);
                    }
                    catch (Exception ex)
                    {
                        showStatusText("ERROR", false);
                        MessageBox.Show($"Ha ocurrido un error al interpretar el archivo de configuración de Dropbox ({configFilename})\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    if (configInfo.personal != null && configInfo.personal.path != null)
                    {
                        folder = configInfo.personal.path;
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Shortcut processing

        private bool processShortcut(ConfigRootObject config)
        {
            if (config.shortcut != null)
            {
                if (config.shortcut.createOnDesktop != null && config.shortcut.createOnDesktop)
                {
                    // Create desktop shortcut
                    CardonerSistemas.FileSystem.ShortcutAddToDesktop(Application.ExecutablePath, Application.StartupPath, config.shortcut.displayName);
                }
                if (config.shortcut.createOnStartMenu != null && config.shortcut.createOnStartMenu)
                {
                    // Create start menu shortcut
                    CardonerSistemas.FileSystem.ShortcutAddToStartMenu(Application.ExecutablePath, Application.StartupPath, config.shortcut.startMenuFolder, config.shortcut.displayName);
                }
            }

            return true;
        }

        #endregion

        #region Execution of file

        private bool executeFile(ConfigRootObject config, string commonDestinationFolder)
        {
            if (config.executeFileNumber != null && config.files != null && config.executeFileNumber <= config.files.Length - 1)
            {
                ConfigFile configFile;
                string destinationFolder = "";
                string executeFilePath;

                configFile = config.files[config.executeFileNumber];

                processDestinationFolder(commonDestinationFolder, configFile.destination, ref destinationFolder);
                executeFilePath = destinationFolder + configFile.name;

                try
                {
                    System.Diagnostics.Process.Start(executeFilePath);
                }
                catch (System.Exception ex)
                {
                    showStatusText("ERROR", false);
                    MessageBox.Show($"Error al iniciar el archivo ({executeFilePath}).\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

            }

            return true;
        }

        #endregion

    }
}
