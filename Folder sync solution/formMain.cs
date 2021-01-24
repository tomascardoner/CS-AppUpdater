using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace CS_AppUpdater
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

            this.Text = CardonerSistemas.My.Application.Info.Title;
        }

        private void formMain_Shown(object sender, EventArgs e)
        {
            ConfigRootObject config = null;

            if (readConfigFile(ref config))
            {
                if (processFiles(config))
                {
                    Application.Exit();
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

        private bool processFiles(ConfigRootObject config)
        {
            string commonSourceFolder = processFolderName(config.commonSource);
            string commonDestinationFolder = processFolderName(config.commonDestination);

            if (commonDestinationFolder.Length == 0)
            {
                commonDestinationFolder = CardonerSistemas.FileSystem.PathAddBackslash(Application.StartupPath);
            }

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
            string sourceFolder = processFolderName(configFile.source);
            string destinationFolder = processFolderName(configFile.destination);

            if (sourceFolder.Length == 0)
            {
                if (commonSourceFolder.Length == 0)
                {
                    return true;
                }
                else
                {
                    sourceFolder = commonSourceFolder;
                }
            }

            if (destinationFolder.Length == 0)
            {
                if (commonDestinationFolder.Length == 0)
                {
                    return true;
                }
                else
                {
                    destinationFolder = commonDestinationFolder;
                }
            }

            if (configFile.name == null || configFile.name.Trim().Length == 0)
            {
                return true;
            }

            if (!File.Exists(sourceFolder + configFile.name.Trim()))
            {
                showStatusText($"El archivo de origen ({configFile.name}) no existe.");
                return false;
            }
            if (!File.Exists(destinationFolder + configFile.name.Trim()))
            {
                // El archivo de destino no existe, así que hay que copiarlo
                if (!copyFile(sourceFolder, destinationFolder, configFile.name.Trim()))
                {
                    return false;
                }
            }

            return true;
        }

        private bool copyFile(string sourceFolder, string destinationFolder, string fileName)
        {
            try
            {
                showStatusText($"Copiando el archivo ({fileName})...");
                File.Copy(sourceFolder + fileName, destinationFolder + fileName, true);
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
                configFilePath = CardonerSistemas.FileSystem.PathAddBackslash(applicationDatafolder);
                configFilePath = CardonerSistemas.FileSystem.PathAddBackslash(configFilePath + folderName);
                configFilePath += configFilename;

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

    }
}
