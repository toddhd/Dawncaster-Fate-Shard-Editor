using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DawncasterFateShardEditor;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

public sealed class MainForm : Form
{
    private readonly TextBox pathBox = new();
    private readonly TextBox currentBox = new();
    private readonly NumericUpDown newValueBox = new();
    private readonly Button loadButton = new();
    private readonly Button browseButton = new();
    private readonly Button saveButton = new();
    private readonly Label statusLabel = new();

    private const string FateShardProperty = "m_CurrentFateShards";

    public MainForm()
    {
        Text = "Dawncaster Fate Shard Editor";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(700, 550);
        Size = new Size(820, 550);
        AutoScaleMode = AutoScaleMode.Dpi;

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appData = Directory.GetParent(localAppData)?.FullName ?? localAppData;

        var defaultPath = Path.Combine(
            appData,
            "LocalLow",
            "Wanderlost Interactive",
            "Dawncaster",
            "DC_Conf.dc"
        );

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 6,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = new Label
        {
            Text = "Dawncaster Fate Shard Editor",
            Font = new Font(Font, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 18)
        };

        mainLayout.Controls.Add(title, 0, 0);

        var fileGroup = new GroupBox
        {
            Text = "Dawncaster Configuration File",
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, 14)
        };

        var fileLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            AutoSize = true
        };

        fileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        fileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        fileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        pathBox.Text = defaultPath;
        pathBox.Dock = DockStyle.Fill;
        pathBox.Margin = new Padding(0, 2, 8, 2);

        browseButton.Text = "Browse...";
        browseButton.AutoSize = true;
        browseButton.Margin = new Padding(0, 0, 8, 0);
        browseButton.Click += (_, _) => BrowseForFile();

        loadButton.Text = "Load";
        loadButton.AutoSize = true;
        loadButton.Click += (_, _) => LoadFile();

        fileLayout.Controls.Add(pathBox, 0, 0);
        fileLayout.Controls.Add(browseButton, 1, 0);
        fileLayout.Controls.Add(loadButton, 2, 0);

        fileGroup.Controls.Add(fileLayout);
        mainLayout.Controls.Add(fileGroup, 0, 1);

        var valuesGroup = new GroupBox
        {
            Text = "Fate Shards",
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, 14)
        };

        var valuesLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2,
            AutoSize = true
        };

        valuesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        valuesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        valuesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        valuesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        var currentLabel = new Label
        {
            Text = "Current Fate Shards:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 8, 10, 8)
        };

        currentBox.ReadOnly = true;
        currentBox.Dock = DockStyle.Fill;
        currentBox.Margin = new Padding(0, 5, 20, 5);

        var newLabel = new Label
        {
            Text = "Set Fate Shards to:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 8, 10, 8)
        };

        newValueBox.Minimum = 0;
        newValueBox.Maximum = 999999999;
        newValueBox.ThousandsSeparator = true;
        newValueBox.Dock = DockStyle.Fill;
        newValueBox.Margin = new Padding(0, 5, 0, 5);

        valuesLayout.Controls.Add(currentLabel, 0, 0);
        valuesLayout.Controls.Add(currentBox, 1, 0);
        valuesLayout.Controls.Add(newLabel, 2, 0);
        valuesLayout.Controls.Add(newValueBox, 3, 0);

        valuesGroup.Controls.Add(valuesLayout);
        mainLayout.Controls.Add(valuesGroup, 0, 2);

        saveButton.Text = "Save Changes";
        saveButton.AutoSize = true;
        saveButton.Enabled = false;
        saveButton.Padding = new Padding(10, 4, 10, 4);
        saveButton.Margin = new Padding(0, 0, 0, 14);
        saveButton.Click += (_, _) => SaveChanges();

        mainLayout.Controls.Add(saveButton, 0, 3);

        var warning = new Label
        {
            Text = "Tip: Close Dawncaster before saving changes.",
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(0, 0, 0, 10)
        };

        mainLayout.Controls.Add(warning, 0, 4);

        statusLabel.Text = "Load the configuration file to begin.";
        statusLabel.AutoSize = true;
        statusLabel.Dock = DockStyle.Top;
        statusLabel.Margin = new Padding(0);

        mainLayout.Controls.Add(statusLabel, 0, 5);

        Controls.Add(mainLayout);

        Shown += (_, _) =>
        {
            if (File.Exists(pathBox.Text))
                LoadFile();
        };
    }

    private void BrowseForFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select Dawncaster DC_Conf.dc",
            FileName = "DC_Conf.dc",
            Filter = "Dawncaster configuration (DC_Conf.dc)|DC_Conf.dc|All files (*.*)|*.*"
        };

        var currentDirectory = Path.GetDirectoryName(pathBox.Text);
        if (!string.IsNullOrWhiteSpace(currentDirectory) && Directory.Exists(currentDirectory))
            dialog.InitialDirectory = currentDirectory;

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            pathBox.Text = dialog.FileName;
            LoadFile();
        }
    }

    private void LoadFile()
    {
        try
        {
            var path = pathBox.Text.Trim();

            if (!File.Exists(path))
            {
                SetStatus("File not found. Use Browse to locate DC_Conf.dc.");
                saveButton.Enabled = false;
                currentBox.Clear();
                return;
            }

            string text = File.ReadAllText(path);

            using JsonDocument document = JsonDocument.Parse(text);

            if (!document.RootElement.TryGetProperty(FateShardProperty, out JsonElement valueElement) ||
                valueElement.ValueKind != JsonValueKind.Number ||
                !valueElement.TryGetInt64(out long currentValue))
            {
                throw new InvalidDataException(
                    $"The file is valid JSON, but does not contain a numeric \"{FateShardProperty}\" value."
                );
            }

            currentBox.Text = currentValue.ToString();

            if (currentValue >= (long)newValueBox.Minimum &&
                currentValue <= (long)newValueBox.Maximum)
            {
                newValueBox.Value = currentValue;
            }

            saveButton.Enabled = true;
            SetStatus("Configuration loaded successfully.");
        }
        catch (Exception ex)
        {
            saveButton.Enabled = false;
            currentBox.Clear();

            MessageBox.Show(
                this,
                ex.Message,
                "Unable to Load File",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            SetStatus("Unable to load the configuration file.");
        }
    }

    private void SaveChanges()
    {
        try
        {
            var path = pathBox.Text.Trim();

            if (!File.Exists(path))
                throw new FileNotFoundException("The configuration file no longer exists.", path);

            string original = File.ReadAllText(path);

            using (JsonDocument document = JsonDocument.Parse(original))
            {
                if (!document.RootElement.TryGetProperty(FateShardProperty, out JsonElement valueElement) ||
                    valueElement.ValueKind != JsonValueKind.Number)
                {
                    throw new InvalidDataException(
                        $"Could not find a numeric \"{FateShardProperty}\" property."
                    );
                }
            }

            long newValue = decimal.ToInt64(newValueBox.Value);

            string pattern = @"(""m_CurrentFateShards""\s*:\s*)-?\d+";
            var regex = new Regex(pattern, RegexOptions.CultureInvariant);

            Match match = regex.Match(original);
            if (!match.Success)
                throw new InvalidDataException($"Could not locate \"{FateShardProperty}\" in the file.");

            string updated = regex.Replace(
                original,
                m => m.Groups[1].Value + newValue,
                1
            );

            string directory = Path.GetDirectoryName(path)
                ?? throw new InvalidOperationException("Could not determine the configuration directory.");

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = Path.Combine(directory, $"DC_Conf.backup_{timestamp}.dc");

            File.Copy(path, backupPath, overwrite: false);

            string tempPath = path + ".tmp";
            File.WriteAllText(tempPath, updated);

            try
            {
                File.Copy(tempPath, path, overwrite: true);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }

            currentBox.Text = newValue.ToString();
            SetStatus($"Saved successfully. Backup created: {Path.GetFileName(backupPath)}");

            MessageBox.Show(
                this,
                $"Fate Shards changed to {newValue:N0}.\n\nBackup created:\n{backupPath}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Unable to Save",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            SetStatus("No changes were saved.");
        }
    }

    private void SetStatus(string message)
    {
        statusLabel.Text = message;
    }
}
