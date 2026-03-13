using Poker.RangeApprox.Infrastructure.Parsing;

namespace Poker.RangeApprox.WinForms;

public sealed partial class MainForm
{
    private void RefreshRankingFileState()
    {
        ClearProfiles();
        _rankingFileValid = ValidateAndLoadRankingProfiles(_rankingPathTextBox.Text.Trim());
        UpdateDynamicFieldVisibility();
    }

    private void RefreshCsvFileState()
    {
        ClearNodeKeys();
        _csvFileValid = ValidateAndLoadNodeKeys(_csvPathTextBox.Text.Trim());
        UpdateDynamicFieldVisibility();
    }

    private void ClearProfiles()
    {
        _requestedProfileComboBox.Items.Clear();
        _requestedProfileComboBox.Items.Add("(auto)");
        _requestedProfileComboBox.SelectedIndex = 0;
    }

    private void ClearNodeKeys()
    {
        _nodeKeyComboBox.Items.Clear();
        _nodeKeyComboBox.Text = string.Empty;
    }

    private bool ValidateAndLoadRankingProfiles(string filePath)
    {
        _errorProvider.SetError(_rankingPathTextBox, string.Empty);

        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        if (!File.Exists(filePath))
        {
            _errorProvider.SetError(_rankingPathTextBox, "Ranking file does not exist.");
            return false;
        }

        try
        {
            var parser = new RankingFileParser();
            var profiles = parser.Parse(filePath);

            if (profiles.Count == 0)
            {
                _errorProvider.SetError(_rankingPathTextBox, "Ranking file did not contain any ranking profiles.");
                return false;
            }

            foreach (var profileName in profiles
                         .Select(p => p.Name)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                _requestedProfileComboBox.Items.Add(profileName);
            }

            _requestedProfileComboBox.SelectedIndex = 0;
            return true;
        }
        catch (Exception ex)
        {
            _errorProvider.SetError(_rankingPathTextBox, $"Invalid ranking file: {ex.Message}");
            return false;
        }
    }

    private bool ValidateAndLoadNodeKeys(string filePath)
    {
        _errorProvider.SetError(_csvPathTextBox, string.Empty);

        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        if (!File.Exists(filePath))
        {
            _errorProvider.SetError(_csvPathTextBox, "CSV file does not exist.");
            return false;
        }

        try
        {
            var reader = new PopulationCsvReader();
            var rows = reader.Read(filePath);

            if (rows.Count == 0)
            {
                _errorProvider.SetError(_csvPathTextBox, "CSV file contained no data rows.");
                return false;
            }

            var extractor = new PopulationNodeExtractor();
            var nodes = extractor.Extract(rows[0]);

            if (nodes.Count == 0)
            {
                _errorProvider.SetError(_csvPathTextBox, "CSV file did not contain any supported node columns.");
                return false;
            }

            foreach (var key in nodes
                         .Select(n => ToNodeKey(n.NodeId))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                _nodeKeyComboBox.Items.Add(key);
            }

            if (_nodeKeyComboBox.Items.Count > 0)
                _nodeKeyComboBox.SelectedIndex = 0;

            return true;
        }
        catch (Exception ex)
        {
            _errorProvider.SetError(_csvPathTextBox, $"Invalid CSV file: {ex.Message}");
            return false;
        }
    }

    private void UpdateDynamicFieldVisibility()
    {
        var isSingleMode = string.Equals(_modeComboBox.SelectedItem?.ToString(), "single", StringComparison.OrdinalIgnoreCase);

        SetRowVisible(
            NodeKeyRow,
            isSingleMode && _csvFileValid,
            _nodeKeyLabel,
            _nodeKeyComboBox,
            _nodeKeySidePanel);

        SetRowVisible(
            RequestedProfileRow,
            _rankingFileValid,
            _requestedProfileLabel,
            _requestedProfileComboBox,
            _requestedProfileSidePanel);
    }

    private void SetRowVisible(int rowIndex, bool visible, params Control[] controls)
    {
        if (_inputLayout is null)
            return;

        foreach (var control in controls)
            control.Visible = visible;

        _inputLayout.RowStyles[rowIndex].SizeType = visible ? SizeType.AutoSize : SizeType.Absolute;
        _inputLayout.RowStyles[rowIndex].Height = 0;
    }

    private bool ValidateInputsForRun(
        out double rakePercent,
        out double rakeCapBb,
        out double openSize,
        out double threeBetSize,
        out double fourBetSize,
        out bool useDatabaseMode)
    {
        rakePercent = 0;
        rakeCapBb = 0;
        openSize = 0;
        threeBetSize = 0;
        fourBetSize = 0;

        if (!_rankingFileValid)
            throw new InvalidOperationException("Select a valid ranking file.");

        useDatabaseMode = string.IsNullOrWhiteSpace(_csvPathTextBox.Text.Trim());

        if (!useDatabaseMode && !_csvFileValid)
            throw new InvalidOperationException("Select a valid CSV file or leave CSV blank to use database mode.");

        if (!double.TryParse(_rakePercentTextBox.Text, out rakePercent))
            throw new InvalidOperationException("Rake percent must be a valid number.");

        if (!double.TryParse(_rakeCapBbTextBox.Text, out rakeCapBb))
            throw new InvalidOperationException("Rake cap bb must be a valid number.");

        if (!double.TryParse(_openSizeTextBox.Text, out openSize))
            throw new InvalidOperationException("Open size must be a valid number.");

        if (!double.TryParse(_threeBetSizeTextBox.Text, out threeBetSize))
            throw new InvalidOperationException("3bet size must be a valid number.");

        if (!double.TryParse(_fourBetSizeTextBox.Text, out fourBetSize))
            throw new InvalidOperationException("4bet size must be a valid number.");

        if (rakePercent < 0)
            throw new InvalidOperationException("Rake percent cannot be negative.");

        if (rakeCapBb < 0)
            throw new InvalidOperationException("Rake cap bb cannot be negative.");

        if (openSize <= 0 || threeBetSize <= 0 || fourBetSize <= 0)
            throw new InvalidOperationException("Bet sizes must be greater than zero.");

        return true;
    }
}