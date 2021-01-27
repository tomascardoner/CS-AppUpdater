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

        private void formMain_Shown(object sender, EventArgs e)
        {
            startTheProcess();
        }

        #endregion

        #region The process

        private void startTheProcess()
        {
            ConfigRootObject config = null;

            if (readConfigFile(ref config))
            {
                // Common source folder
                config.commonSource = CardonerSistemas.FileSystem.ProcessFolderName(config.commonSource);
                if (config.commonSource.Length == 0)
                {
                    config.commonSource = Application.StartupPath;
                }
                else if (config.commonSource.StartsWith("."))
                {
                    config.commonSource = Path.Combine(Application.StartupPath, config.commonSource);
                }

                // Common destination folder
                config.commonDestination = CardonerSistemas.FileSystem.ProcessFolderName(config.commonDestination);
                if (config.commonDestination.Length == 0)
                {
                    config.commonDestination = Application.StartupPath;
                }

                // Process all files
                if (!processFiles(config))
                {
                    return;
                }

                // Verify and create shortcuts if necessary
                processShortcut(config);

                // Finally, execute corresponding file
                if (executeFile(config))
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
            if (config.files != null)
            {
                foreach (ConfigFile configFile in config.files)
                {
                    if (!processFile(configFile, config.commonSource, config.commonDestination))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool processFile(ConfigFile configFile, string commonSourceFolder, string commonDestinationFolder)
        {
            if (configFile.name == null || configFile.name.Trim().Length == 0)
            {
                return true;
            }

            // Proceso las carpetas de origen y destino del archivo
            configFile.source = processFileFolder(commonSourceFolder, configFile.source);
            configFile.destination = processFileFolder(commonDestinationFolder, configFile.destination);

            // Preparo el nombre del archivo y los paths completos de origen y destino
            string fileName = configFile.name.Trim();
            string sourceFilePath = Path.Combine(configFile.source, fileName);
            string destinationFilePath = Path.Combine(configFile.destination, fileName);

            if (!verifyFileExists(sourceFilePath, fileName, "origen"))
            {
                return false;
            }

            if (!verifyFileExists(destinationFilePath, fileName, "destino"))
            {
                // El archivo de destino no existe, así que hay que copiarlo
                // pero primero hay que chequear que exista la carpeta de destino
                if (!verifyFolderExistsAndCreate(configFile.destination))
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

        private bool verifyFileExists(string filePath, string fileName, string leyenda)
        {
            try
            {
                showStatusText($"Verificando si existe el archivo de {leyenda} ({fileName})...");
                if (File.Exists(filePath))
                {
                    showStatusText("OK", false);
                    return true;
                }
                else
                {
                    showStatusText("NO EXISTE", false);
                    return false;
                }
            }
            catch (Exception ex)
            {
                showStatusText("ERROR", false);
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
                showStatusText("Verificando y creando carpeta de destino...ERROR");
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
                showStatusText($"Verificando versiones de ambos archivos ({fileName})...");
                sourceFileVersion = FileVersionInfo.GetVersionInfo(sourceFilePath);
                destinationFileVersion = FileVersionInfo.GetVersionInfo(destinationFilePath);
            }
            catch (Exception ex)
            {
                showStatusText("ERROR", false);
                MessageBox.Show($"Ha ocurrido un error al obtener la versión de los archivos ({fileName})\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (sourceFileVersion.FileVersion == destinationFileVersion.FileVersion)
            {
                showStatusText("IGUALES", false);
                return false;
            }
            else
            {
                showStatusText("DISTINTAS", false);
                return true;
            }
        }

        private bool verifyIfFileTimeOrSizeDiffers(string sourceFilePath, string destinationFilePath, string fileName)
        {
            FileInfo sourceFileInfo;
            FileInfo destinationFileInfo;

            try
            {
                showStatusText($"Verificando información de ambos archivos ({fileName})...");
                sourceFileInfo = new FileInfo(sourceFilePath);
                destinationFileInfo = new FileInfo(destinationFilePath);
            }
            catch (Exception ex)
            {
                showStatusText("ERROR", false);
                MessageBox.Show($"Ha ocurrido un error al obtener la información de los archivos ({fileName})\n\nError: {ex.Message}", CardonerSistemas.My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if ((sourceFileInfo.LastWriteTime != destinationFileInfo.LastWriteTime) || (sourceFileInfo.Length != destinationFileInfo.Length))
            {
                showStatusText("IGUALES", false);
                return false;
            }
            else
            {
                showStatusText("DISTINTOS", false);
                return true;
            }
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

        private string processFileFolder(string commonFolder, string fileFolder)
        {
            string fileFolderTemp;

            fileFolderTemp = CardonerSistemas.FileSystem.ProcessFolderName(fileFolder);

            if (fileFolderTemp.Length == 0)
            {
                if (commonFolder.Length != 0)
                {
                    fileFolderTemp = commonFolder;
                }
            }
            else if (fileFolderTemp.StartsWith("."))
            {
                if (commonFolder.Length != 0)
                {
                    fileFolderTemp = Path.Combine(commonFolder, fileFolderTemp);
                }
            }

            return fileFolderTemp;
        }

        #endregion

        #region Shortcut processing

        private bool processShortcut(ConfigRootObject config)
        {
            if (config.shortcut != null)
            {
                string iconFilePath = null;

                if (config.shortcut.iconFileNumber != null && config.files != null && config.shortcut.iconFileNumber <= config.files.Length - 1)
                {
                    ConfigFile configFile;

                    configFile = config.files[config.shortcut.iconFileNumber];
                    iconFilePath = Path.Combine(configFile.destination, configFile.name);
                }

                // Create desktop shortcut
                if (config.shortcut.createOnDesktop != null && config.shortcut.createOnDesktop)
                {
                    CardonerSistemas.FileSystem.ShortcutAddToDesktop(Application.ExecutablePath, Application.StartupPath, config.shortcut.displayName, iconFilePath);
                }

                // Create start menu shortcut
                if (config.shortcut.createOnStartMenu != null && config.shortcut.createOnStartMenu)
                {
                    CardonerSistemas.FileSystem.ShortcutAddToStartMenu(Application.ExecutablePath, Application.StartupPath, config.shortcut.startMenuFolder, config.shortcut.displayName, iconFilePath);
                }
            }

            return true;
        }

        #endregion

        #region Execution of file

        private bool executeFile(ConfigRootObject config)
        {
            if (config.executeFileNumber != null && config.files != null && config.executeFileNumber <= config.files.Length - 1)
            {
                ConfigFile configFile;
                string executeFilePath;

                configFile = config.files[config.executeFileNumber];
                executeFilePath = Path.Combine(configFile.destination, configFile.name);

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
