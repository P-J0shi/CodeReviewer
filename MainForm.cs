using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace D365CodeReviewer
{
    public partial class MainForm : Form
    {
        private string fddFilePath = string.Empty;
        private string axppFilePath = string.Empty;
        private string outputFilePath = string.Empty;
        private FDDParser? fddParser;
        private AXPPParser? axppParser;
        private CodeReviewer? reviewer;

        public MainForm()
        {
            InitializeComponent();
            InitializeOutputDirectory();
        }

        private void InitializeOutputDirectory()
        {
            // Create output directory if it doesn't exist
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "D365CodeReviewer");
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            // Set default output file path
            outputFilePath = Path.Combine(appDataPath, "CodeReviewReport_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json");
            txtOutputPath.Text = outputFilePath;
        }

        private void btnBrowseFDD_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "FDD Documents|*.pdf;*.docx;*.txt;*.md|All Files|*.*";
                openFileDialog.Title = "Select Functional Design Document";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    fddFilePath = openFileDialog.FileName;
                    txtFDDPath.Text = fddFilePath;
                    UpdateStatus("FDD document selected: " + Path.GetFileName(fddFilePath));
                }
            }
        }

        private void btnBrowseAXPP_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "AXPP Project Files|*.axpp;*.xml|All Files|*.*";
                openFileDialog.Title = "Select D365FO Project/Solution File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    axppFilePath = openFileDialog.FileName;
                    txtAXPPPath.Text = axppFilePath;
                    UpdateStatus("Solution file selected: " + Path.GetFileName(axppFilePath));
                }
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "JSON Files|*.json|All Files|*.*";
                saveFileDialog.Title = "Select Output Report Location";
                saveFileDialog.FileName = Path.GetFileName(outputFilePath);

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    outputFilePath = saveFileDialog.FileName;
                    txtOutputPath.Text = outputFilePath;
                    UpdateStatus("Output location set: " + outputFilePath);
                }
            }
        }

        private async void btnStartReview_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(fddFilePath) || string.IsNullOrEmpty(axppFilePath))
            {
                MessageBox.Show("Please select both a FDD document and a solution file.", "Missing Files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Show progress and disable buttons
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
                EnableControls(false);
                lstDiscrepancies.Items.Clear();
                UpdateStatus("Starting code review...");

                await Task.Run(() => PerformCodeReview());

                // Display results in the UI
                DisplayReviewResults();

                // Generate HTML report if option selected
                if (chkGenerateHTML.Checked)
                {
                    string htmlOutputPath = Path.ChangeExtension(outputFilePath, ".html");
                    reviewer?.GenerateHtmlReport(htmlOutputPath);
                    UpdateStatus("HTML report generated: " + htmlOutputPath);
                }

                UpdateStatus("Code review completed successfully!");
                MessageBox.Show("Code review completed! Report saved to: " + outputFilePath, "Review Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                UpdateStatus("Error: " + ex.Message);
                MessageBox.Show("An error occurred during the code review:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Hide progress and re-enable buttons
                progressBar.Visible = false;
                EnableControls(true);
            }
        }

        private void PerformCodeReview()
        {
            UpdateStatusThreadSafe("Parsing FDD document...");
            fddParser = new FDDParser(fddFilePath);

            UpdateStatusThreadSafe("Parsing AXPP solution file...");
            axppParser = new AXPPParser(axppFilePath);

            UpdateStatusThreadSafe("Analyzing code against design requirements...");
            reviewer = new CodeReviewer(fddParser, axppParser);
            reviewer.Review();

            UpdateStatusThreadSafe("Generating review report...");
            reviewer.GenerateReport(outputFilePath);
        }

        private void DisplayReviewResults()
        {
            if (reviewer == null) return;

            // Update summary
            lblDiscrepanciesCount.Text = reviewer.Discrepancies.Count.ToString();
            lblMissingCount.Text = reviewer.MissingFeatures.Count.ToString();
            lblImplementedCount.Text = reviewer.ImplementedFeatures.Count.ToString();

            // Populate discrepancies list
            lstDiscrepancies.Items.Clear();
            foreach (var discrepancy in reviewer.Discrepancies)
            {
                string description = GetDiscrepancyDescription(discrepancy);
                
                ListViewItem item = new ListViewItem(GetDiscrepancyType(discrepancy));
                item.SubItems.Add(description);
                item.SubItems.Add(discrepancy.Severity.ToString());
                
                // Tag the item with the full discrepancy for details view
                item.Tag = discrepancy;
                
                // Set color based on severity
                if (discrepancy.Severity == DiscrepancySeverity.High)
                    item.ForeColor = Color.Red;
                else if (discrepancy.Severity == DiscrepancySeverity.Medium)
                    item.ForeColor = Color.DarkOrange;
                
                lstDiscrepancies.Items.Add(item);
            }
        }

        private string GetDiscrepancyType(Discrepancy discrepancy)
        {
            switch (discrepancy.Type)
            {
                case DiscrepancyType.MissingEntity:
                    return "Missing Entity";
                case DiscrepancyType.MissingFunction:
                    return "Missing Function";
                case DiscrepancyType.ParameterMismatch:
                    return "Parameter Mismatch";
                case DiscrepancyType.UnimplementedRequirement:
                    return "Unimplemented Requirement";
                case DiscrepancyType.MissingExtension:
                    return "Missing Extension";
                default:
                    return discrepancy.Type.ToString();
            }
        }

        private string GetDiscrepancyDescription(Discrepancy discrepancy)
        {
            switch (discrepancy.Type)
            {
                case DiscrepancyType.MissingEntity:
                    return $"Entity '{discrepancy.EntityName}' is missing from implementation.";
                
                case DiscrepancyType.MissingFunction:
                    string desc = $"Function '{discrepancy.FunctionName}' is missing from implementation.";
                    if (!string.IsNullOrEmpty(discrepancy.Parameters))
                        desc += $" Parameters: {discrepancy.Parameters}";
                    return desc;
                
                case DiscrepancyType.ParameterMismatch:
                    return $"Function '{discrepancy.FunctionName}' has parameter mismatch. Expected: {discrepancy.ExpectedParams}, Actual: {discrepancy.ActualParams}";
                
                case DiscrepancyType.UnimplementedRequirement:
                    return $"Requirement not implemented: {discrepancy.Requirement}";
                
                case DiscrepancyType.MissingExtension:
                    return $"Extension for '{discrepancy.BaseName}' is missing from implementation.";
                
                default:
                    return discrepancy.Description;
            }
        }

        private void lstDiscrepancies_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstDiscrepancies.SelectedItems.Count > 0)
            {
                var selectedItem = lstDiscrepancies.SelectedItems[0];
                if (selectedItem.Tag is Discrepancy discrepancy)
                {
                    // Display detailed information in the details textbox
                    txtDetails.Text = GetDetailedDiscrepancyDescription(discrepancy);
                }
            }
        }

        private string GetDetailedDiscrepancyDescription(Discrepancy discrepancy)
        {
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine($"Type: {GetDiscrepancyType(discrepancy)}");
            sb.AppendLine($"Severity: {discrepancy.Severity}");
            sb.AppendLine();
            
            switch (discrepancy.Type)
            {
                case DiscrepancyType.MissingEntity:
                    sb.AppendLine($"Entity Name: {discrepancy.EntityName}");
                    sb.AppendLine();
                    sb.AppendLine("Description:");
                    sb.AppendLine(discrepancy.Description);
                    break;
                
                case DiscrepancyType.MissingFunction:
                    sb.AppendLine($"Function Name: {discrepancy.FunctionName}");
                    if (!string.IsNullOrEmpty(discrepancy.Parameters))
                        sb.AppendLine($"Expected Parameters: {discrepancy.Parameters}");
                    sb.AppendLine();
                    sb.AppendLine("Description:");
                    sb.AppendLine(discrepancy.Description);
                    break;
                
                case DiscrepancyType.ParameterMismatch:
                    sb.AppendLine($"Function Name: {discrepancy.FunctionName}");
                    sb.AppendLine($"Expected Parameters: {discrepancy.ExpectedParams}");
                    sb.AppendLine($"Actual Parameters: {discrepancy.ActualParams}");
                    break;
                
                case DiscrepancyType.UnimplementedRequirement:
                    sb.AppendLine("Requirement:");
                    sb.AppendLine(discrepancy.Requirement);
                    sb.AppendLine();
                    if (discrepancy.Keywords != null && discrepancy.Keywords.Any())
                    {
                        sb.AppendLine("Keywords:");
                        sb.AppendLine(string.Join(", ", discrepancy.Keywords));
                    }
                    break;
                
                case DiscrepancyType.MissingExtension:
                    sb.AppendLine($"Base Class: {discrepancy.BaseName}");
                    sb.AppendLine();
                    sb.AppendLine("Description:");
                    sb.AppendLine(discrepancy.Description);
                    break;
                
                default:
                    sb.AppendLine(discrepancy.Description);
                    break;
            }
            
            return sb.ToString();
        }

        private void EnableControls(bool enable)
        {
            btnBrowseFDD.Enabled = enable;
            btnBrowseAXPP.Enabled = enable;
            btnBrowseOutput.Enabled = enable;
            btnStartReview.Enabled = enable;
            chkGenerateHTML.Enabled = enable;
        }

        private void UpdateStatus(string message)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action<string>(UpdateStatus), message);
            }
            else
            {
                lblStatus.Text = message;
                Application.DoEvents();
            }
        }

        private void UpdateStatusThreadSafe(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateStatus), message);
            }
            else
            {
                UpdateStatus(message);
            }
        }

        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            if (reviewer == null || reviewer.Discrepancies.Count == 0)
            {
                MessageBox.Show("No review results to export.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Excel Files|*.csv|All Files|*.*";
                    saveFileDialog.Title = "Export Discrepancies to CSV";
                    saveFileDialog.FileName = "CodeReviewDiscrepancies_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        ExportDiscrepanciesToCsv(saveFileDialog.FileName);
                        UpdateStatus("Discrepancies exported to: " + saveFileDialog.FileName);
                        MessageBox.Show("Discrepancies exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting discrepancies: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportDiscrepanciesToCsv(string filePath)
        {
            if (reviewer == null) return;

            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // Write CSV header
                writer.WriteLine("Type,Description,Severity");

                // Write discrepancies
                foreach (var discrepancy in reviewer.Discrepancies)
                {
                    string type = GetDiscrepancyType(discrepancy);
                    string description = GetDiscrepancyDescription(discrepancy).Replace(",", " "); // Remove commas to avoid CSV parsing issues
                    string severity = discrepancy.Severity.ToString();

                    writer.WriteLine($"\"{type}\",\"{description}\",\"{severity}\"");
                }
            }
        }
    }
}