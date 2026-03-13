using System.Text;

namespace Poker.RangeApprox.WinForms.Dialogs;

public sealed class DatabaseConnectionDialog : Form
{
    private readonly TextBox _hostTextBox = new() { Text = "localhost" };
    private readonly TextBox _portTextBox = new() { Text = "5432" };
    private readonly TextBox _databaseTextBox = new() { Text = "PT4 DB" };
    private readonly TextBox _usernameTextBox = new() { Text = "postgres" };
    private readonly TextBox _passwordTextBox = new() { UseSystemPasswordChar = true };

    private readonly Button _okButton = new() { Text = "OK", Width = 100, Height = 32 };
    private readonly Button _cancelButton = new() { Text = "Cancel", Width = 100, Height = 32 };

    public string ConnectionString { get; private set; } = string.Empty;

    public DatabaseConnectionDialog()
    {
        Text = "Database Connection";
        Width = 520;
        Height = 320;
        MinimumSize = new Size(520, 320);
        MaximizeBox = false;
        MinimizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;

        BuildLayout();
        ConfigureEvents();

        AcceptButton = _okButton;
        CancelButton = _cancelButton;
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(12)
        };

        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            AutoSize = true
        };

        fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        for (var i = 0; i < 5; i++)
            fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        AddField(fields, 0, "Host", _hostTextBox);
        AddField(fields, 1, "Port", _portTextBox);
        AddField(fields, 2, "Database", _databaseTextBox);
        AddField(fields, 3, "Username", _usernameTextBox);
        AddField(fields, 4, "Password", _passwordTextBox);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };

        buttonPanel.Controls.Add(_cancelButton);
        buttonPanel.Controls.Add(_okButton);

        root.Controls.Add(fields, 0, 0);
        root.Controls.Add(buttonPanel, 0, 1);

        Controls.Add(root);
    }

    private static void AddField(TableLayoutPanel panel, int row, string labelText, Control input)
    {
        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 8, 12, 8),
            Font = new Font("Segoe UI", 9)
        };

        input.Dock = DockStyle.Fill;
        input.Margin = new Padding(0, 4, 0, 4);
        input.Font = new Font("Segoe UI", 9);

        panel.Controls.Add(label, 0, row);
        panel.Controls.Add(input, 1, row);
    }

    private void ConfigureEvents()
    {
        _okButton.Click += (_, _) => Confirm();
        _cancelButton.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };
    }

    private void Confirm()
    {
        var host = _hostTextBox.Text.Trim();
        var port = _portTextBox.Text.Trim();
        var database = _databaseTextBox.Text.Trim();
        var username = _usernameTextBox.Text.Trim();
        var password = _passwordTextBox.Text;

        if (string.IsNullOrWhiteSpace(host))
        {
            ShowValidationError("Host is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(port))
        {
            ShowValidationError("Port is required.");
            return;
        }

        if (!int.TryParse(port, out var parsedPort) || parsedPort <= 0)
        {
            ShowValidationError("Port must be a valid positive integer.");
            return;
        }

        if (string.IsNullOrWhiteSpace(database))
        {
            ShowValidationError("Database is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowValidationError("Username is required.");
            return;
        }

        var builder = new StringBuilder();
        builder.Append($"Host={host};");
        builder.Append($"Port={parsedPort};");
        builder.Append($"Database={database};");
        builder.Append($"Username={username};");
        builder.Append($"Password={password};");

        ConnectionString = builder.ToString();

        DialogResult = DialogResult.OK;
        Close();
    }

    private void ShowValidationError(string message)
    {
        MessageBox.Show(this, message, "Invalid database settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}