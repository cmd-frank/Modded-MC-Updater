using Renci.SshNet;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Update_Server
{
    public partial class Form1 : Form
    {
        private const string HostName = "";             // Host name or IP
        private const int PortNumber = 22;              // Port
        private const string UserName = "";             // Username
        private const string Password = "";             // Password

        private const string MinecraftDirectory = "";                                   // Minecraft Directory (EX: /games/Minecraft/)
        private const string ServerUpdateJarFile = "serverstarter-2.2.0.jar";           // Server Updater Jar Filename
        private const string UpdateCommand = "java -jar ";                              // Update command

        private SshClient _ssh;
        private SftpClient _sftp;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Show the user an import message to make sure the server is STOPPED before updating
            MessageBox.Show("Make sure the server is stopped before updating!", "Important", MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            // Prompts openFileDialog to select the update file (.yaml)
            OpenFileDialog = new OpenFileDialog();
            OpenFileDialog.Filter = "YAML File (*.yaml)|*.yaml";
            OpenFileDialog.Title = "Select update file";
            OpenFileDialog.FilterIndex = 1;

            // If a file valid file was selected fill the fileLocationTextbox with the location of the file
            if (OpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileLocationTextbox.Text = OpenFileDialog.FileName;
            }
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            // A simple check to make sure a file was selected and its location was added to the fileLocationTextbox
            if (string.IsNullOrWhiteSpace(fileLocationTextbox.Text))
            {
                MessageBox.Show("Please select the update file. Try again.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                browseButton.Focus();
                return;
            }

            // Why not try?
            try
            {
                // Initializes a new Sftp client and connects to the server
                _sftp = new SftpClient(HostName, PortNumber, UserName, Password);

                _sftp.Connect();

                // gets the file info to upload
                FileInfo file = new FileInfo(fileLocationTextbox.Text);
                string uploadFile = file.FullName;

                // if for some reason the Sftp client isn't connected, return - Maybe we can catch the error?
                if (!_sftp.IsConnected) return;

                // Lets the user know we connected
                richTextBox1.AppendText("Successfully connected\n");
                richTextBox1.AppendText("Uploading file...\n");

                var fileStream = new FileStream(uploadFile, FileMode.Open);

                // Uploads the file and "sleeps" to appear like it took some time to get work done
                _sftp.UploadFile(fileStream, MinecraftDirectory + file.Name, null);
                Thread.Sleep(500);

                // a simple check to make sure the file was in fact uploaded to the proper location - Sometimes you never know... Then it disconnects the Sftp client
                if (_sftp.Exists(MinecraftDirectory + "server-setup-config.yaml"))
                {
                    richTextBox1.AppendText("File successfully uploaded\n");
                    _sftp.Disconnect();
                }
                else
                {
                    richTextBox1.AppendText("Error, file was not uploaded. Try again.\n");
                    _sftp.Disconnect();
                    return;
                }
            }
            catch (Exception ex)
            {
                // Maybe we can catch the errors and we disconnect the Sftp client just in case
                MessageBox.Show("Error: " + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _sftp.Disconnect();
                return;
            }

            // Let update the server now since we uploaded the file
            UpdateServer();
        }

        private void UpdateServer()
        {
            // Lets try to update the server
            try
            {
                // Initializes a new SSH Client to connect to the server and run the update command
                _ssh = new SshClient(HostName, PortNumber, UserName, Password);

                _ssh.Connect();

                // If for some reason the SSH client isn't connected, return - Maybe we can catch the error?
                if (!_ssh.IsConnected) return;

                // Lets the user know we're going to update the server and it WILL take some time
                richTextBox1.AppendText("Updating server...\nThis will take some time. Please wait...\n");

                // "Moves" into the Minecraft directory and runs the update command. Otherwise, the Server Updater will look for the "config" (Update File) in the user home directory and not the Minecraft one
                SshCommand sc = _ssh.CreateCommand("cd " + MinecraftDirectory + " && " + UpdateCommand +
                                                      ServerUpdateJarFile);
                sc.Execute();

                // Prints the result but it will take some time since the process takes a bit. TODO: Maybe we can show the result in real time?
                richTextBox1.AppendText(sc.Result + "\n");
                richTextBox1.AppendText("Server updated successfully.\n");

                // Closes the connection
                richTextBox1.AppendText("Closing connection...");
                _ssh.Disconnect();
            }
            catch (Exception ex)
            {
                // Maybe we can catch the errors and we disconnect the SSH client just in case
                MessageBox.Show("Error: " + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _ssh.Disconnect();
                return;
            }
        }
    }
}