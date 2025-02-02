namespace Shellf.Application;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Win32;

static class Program
{
    private static Dictionary<string, Process> processes = new Dictionary<string, Process>();
    private static ContextMenuStrip contextMenu = new ContextMenuStrip();

    [STAThread]
    static void Main()
    {
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            CreateSettingsFileIfNotExists();
            var executableDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var notifyIcon = new NotifyIcon
            {
                Icon = new Icon(Path.Combine(executableDirectory, "Assets", "icon.ico")),
                Visible = true,
                Text = "Shellf"
            };

            var commands = GetCommands();
            foreach (var commandGroup in commands)
            {
                var groupMenu = new ToolStripMenuItem(commandGroup.Name);
                contextMenu.Items.Add(groupMenu);

                foreach (var commandItem in commandGroup.Items)
                {
                    var item = new ToolStripMenuItem(commandItem.Name);
                    item.Click += (s, e) =>
                    {
                        StartProcess(commandItem, item);
                    };
                    groupMenu.DropDownItems.Add(item);
                }
            }

            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Open Settings File", null, (s, e) =>
            {
                var settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Shellf", "settings.json");
                try
                {
                    // Check if .json files have a default application associated
                    var jsonAssociation = Registry.ClassesRoot.OpenSubKey(".json");
                    var processStartInfo = new ProcessStartInfo(settingsFilePath) { UseShellExecute = true };

                    if (jsonAssociation != null)
                    {
                        Process.Start(processStartInfo);
                    }
                    else
                    {
                        // If no association, open with Notepad
                        processStartInfo.FileName = "notepad.exe";
                        processStartInfo.Arguments = settingsFilePath;
                        Process.Start(processStartInfo);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Quit Shellf", null, (s, e) =>
            {
                notifyIcon.Visible = false;
                KillAllProcesses();
                Application.Exit();
            });

            notifyIcon.ContextMenuStrip = contextMenu;
            Application.Run();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void StartProcess(CommandItem command, ToolStripMenuItem item)
    {
        var commandKey = $"{command.Name}:{command.WorkingDirectory}:{command.StartCommand}";
        if (!processes.ContainsKey(commandKey) || processes[commandKey].HasExited)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command.Exe,
                Arguments = $"-c {command.StartCommand}", // TODO: Or StopCommand
                WorkingDirectory = command.WorkingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true // TODO: Provide the option?
            };
            Process process = Process.Start(startInfo);
            processes[commandKey] = process;

            item.Checked = true;
        }
        else
        {
            KillProcess(commandKey);
            item.Checked = false;
        }
    }

    private static void KillProcess(string key)
    {
        if (processes.ContainsKey(key) && !processes[key].HasExited)
        {
            int pid = processes[key].Id;
            ProcessStartInfo killInfo = new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = $"/PID {pid} /T /F",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(killInfo);
            processes[key].WaitForExit(); // Wait for the process to exit
            processes.Remove(key);
        }
        else
        {
            MessageBox.Show($"Process is not running."); // TODO: Refactor / Might not need
        }
    }

    private static void KillAllProcesses()
    {
        foreach (var process in processes)
        {
            KillProcess(process.Key);
        }
    }

    private static void CreateSettingsFileIfNotExists()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Shellf");
        var settingFilePath = Path.Combine(appDataPath, "settings.json");
        var executableDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var settingsFilePath = Path.Combine(executableDirectory, "settings.template.json");
        var settingsFileTemplate = File.ReadAllText(settingsFilePath);

        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }

        if (!File.Exists(settingFilePath))
        {
            File.WriteAllText(settingFilePath, settingsFileTemplate);
        }
    }

    private static List<CommandGroup> GetCommands()
    {
        var settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Shellf", "settings.json");
        var settings = File.ReadAllText(settingsFilePath);

        var settingsObj = JsonSerializer.Deserialize<ApplicationSettings>(settings);

        return settingsObj?.Commands ?? new List<CommandGroup>();
    }
}

