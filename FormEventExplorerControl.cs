using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using Label = System.Windows.Forms.Label;

namespace FormEventExplorerXrmToolBox
{
    public sealed class FormEventExplorerControl : PluginControlBase
    {
        private readonly ComboBox tableList = new ComboBox();
        private readonly ComboBox formList = new ComboBox();
        private readonly Button refreshButton = new Button();
        private readonly Button exportButton = new Button();
        private readonly Label statusLabel = new Label();
        private readonly Label loadCount = Metric("0\r\nOnLoad");
        private readonly Label saveCount = Metric("0\r\nOnSave");
        private readonly Label changeCount = Metric("0\r\nOnChange");
        private readonly Label ruleCount = Metric("0\r\nBusiness rules");
        private readonly DataGridView formGrid = Grid();
        private readonly DataGridView changeGrid = Grid();
        private readonly DataGridView ruleGrid = Grid();
        private readonly TextBox formSearch = SearchBox("Search form events");
        private readonly TextBox changeSearch = SearchBox("Search field OnChange events");
        private readonly TextBox ruleSearch = SearchBox("Search business rules");
        private readonly List<FormItem> forms = new List<FormItem>();
        private List<EventRow> events = new List<EventRow>();
        private List<RuleRow> rules = new List<RuleRow>();

        public FormEventExplorerControl()
        {
            BuildUi();
            tableList.SelectedIndexChanged += (s, e) => LoadFormsAndRules();
            formList.SelectedIndexChanged += (s, e) => SelectForm();
            refreshButton.Click += (s, e) => LoadTables();
            exportButton.Click += (s, e) => ExportCsv();
            formSearch.TextChanged += (s, e) => BindGrids();
            changeSearch.TextChanged += (s, e) => BindGrids();
            ruleSearch.TextChanged += (s, e) => BindGrids();
        }

        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);
            if (newService != null) LoadTables();
        }

        private void BuildUi()
        {
            BackColor = Color.FromArgb(245, 245, 245);
            Font = new Font("Segoe UI", 9F);

            var outer = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(0), BackColor = BackColor };
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var header = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(116, 39, 116), ColumnCount = 2, RowCount = 1, Padding = new Padding(0), Margin = new Padding(0) };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.Controls.Add(new Label { Text = "Form Event Explorer", AutoSize = false, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(16, 0, 0, 0) }, 0, 0);
            header.Controls.Add(new Label { Text = "Dataverse configuration overview", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(235, 215, 235), Font = new Font("Segoe UI", 9F), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0) }, 1, 0);
            outer.Controls.Add(header, 0, 0);

            var main = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1, Padding = new Padding(16), BackColor = BackColor };
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 85));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var heading = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 1, ColumnCount = 1, Margin = new Padding(0, 0, 0, 8) };
            //heading.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            heading.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            //heading.Controls.Add(new Label { Text = "Form scripts and business rules", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(36, 36, 36), Padding = new Padding(0, 0, 0, 2) }, 0, 0);
            heading.Controls.Add(new Label { Text = "Select a table and main form to inspect form events, field OnChange handlers and business rules.", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F), ForeColor = Color.Black, Padding = new Padding(0, 0, 0, 2) }, 0, 0);
            main.Controls.Add(heading, 0, 0);

            var filters = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, BackColor = Color.White, Padding = new Padding(10), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 0, 0, 10) };
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92)); filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
            tableList.Dock = DockStyle.Fill; tableList.DropDownStyle = ComboBoxStyle.DropDownList;
            formList.Dock = DockStyle.Fill; formList.DropDownStyle = ComboBoxStyle.DropDownList;
            refreshButton.Text = "Refresh"; refreshButton.Height = 26; refreshButton.BackColor = Color.FromArgb(240, 240, 240); refreshButton.FlatStyle = FlatStyle.Flat; refreshButton.Margin = new Padding(6, 18, 6, 4); refreshButton.Height = 24;
            exportButton.Text = "Export CSV"; exportButton.Height = 26; exportButton.BackColor = Color.FromArgb(15, 108, 189); exportButton.ForeColor = Color.White; exportButton.FlatStyle = FlatStyle.Flat; exportButton.Enabled = false; exportButton.Margin = new Padding(0, 18, 0, 4);
            filters.Controls.Add(Wrap("Table", tableList), 0, 0); filters.Controls.Add(Wrap("Main form", formList), 1, 0); filters.Controls.Add(refreshButton, 2, 0); filters.Controls.Add(exportButton, 3, 0);
            main.Controls.Add(filters, 0, 1);

            var statusContainer = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(239, 246, 252), Margin = new Padding(0, 0, 0, 8) };
            var statusAccent = new Panel { Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(116, 39, 116) };
            statusLabel.Dock = DockStyle.Fill; statusLabel.Padding = new Padding(12, 7, 0, 0); statusLabel.BackColor = Color.FromArgb(239, 246, 252); statusLabel.Text = "Connect to a Dataverse environment."; statusLabel.ForeColor = Color.FromArgb(40, 40, 40); statusLabel.Font = new Font("Segoe UI", 9F);
            statusContainer.Controls.Add(statusLabel); statusContainer.Controls.Add(statusAccent); statusAccent.BringToFront();
            main.Controls.Add(statusContainer, 0, 2);

            var metrics = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, Margin = new Padding(0, 0, 0, 8) };
            for (var i = 0; i < 4; i++) metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            metrics.Controls.Add(loadCount, 0, 0); metrics.Controls.Add(saveCount, 1, 0); metrics.Controls.Add(changeCount, 2, 0); metrics.Controls.Add(ruleCount, 3, 0);
            main.Controls.Add(metrics, 0, 3);

            var tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(0, 0) };
            tabs.TabPages.Add(Tab("Form OnLoad / OnSave", formSearch, formGrid));
            tabs.TabPages.Add(Tab("Field OnChange", changeSearch, changeGrid));
            tabs.TabPages.Add(Tab("Business rules", ruleSearch, ruleGrid));
            main.Controls.Add(tabs, 0, 4);

            outer.Controls.Add(main, 0, 1);
            Controls.Add(outer);
        }

        private void LoadTables()
        {
            if (Service == null) return;
            //SetWorkingMessage("Loading Dataverse tables...");
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading Dataverse tables...",
                Work = (worker, args) =>
                {
                    var response = (RetrieveAllEntitiesResponse)Service.Execute(new RetrieveAllEntitiesRequest { EntityFilters = EntityFilters.Entity, RetrieveAsIfPublished = true });
                    args.Result = response.EntityMetadata.Where(x => !string.IsNullOrWhiteSpace(x.LogicalName))
                        .Select(x => new TableItem { LogicalName = x.LogicalName, DisplayName = x.DisplayName?.UserLocalizedLabel?.Label ?? x.LogicalName })
                        .OrderBy(x => x.DisplayName).ToList();
                },
                PostWorkCallBack = args =>
                {
                    if (args.Error != null) { ShowErrorDialog(args.Error); return; }
                    tableList.DataSource = (List<TableItem>)args.Result;
                    statusLabel.Text = $"{tableList.Items.Count} tables loaded.";
                }
            });
        }

        private void LoadFormsAndRules()
        {
            if (!(tableList.SelectedItem is TableItem table) || Service == null) return;
            formList.DataSource = null; forms.Clear(); events.Clear(); rules.Clear(); BindGrids();
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading main forms and business rules...",
                Work = (worker, args) =>
                {
                    var formQuery = new QueryExpression("systemform") { ColumnSet = new ColumnSet("formid", "name", "formxml", "type") };
                    formQuery.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, table.LogicalName);
                    formQuery.Criteria.AddCondition("type", ConditionOperator.Equal, 2);
                    var formItems = Service.RetrieveMultiple(formQuery).Entities.Select(x => new FormItem
                    {
                        Id = x.Id, Name = x.GetAttributeValue<string>("name") ?? "Unnamed form", FormXml = x.GetAttributeValue<string>("formxml")
                    }).OrderBy(x => x.Name).ToList();

                    var ruleQuery = new QueryExpression("workflow") { ColumnSet = new ColumnSet("name", "statecode", "scope", "modifiedon", "clientdata", "category", "primaryentity") };
                    ruleQuery.Criteria.AddCondition("category", ConditionOperator.Equal, 2);
                    ruleQuery.Criteria.AddCondition("primaryentity", ConditionOperator.Equal, table.LogicalName);
                    var ruleItems = Service.RetrieveMultiple(ruleQuery).Entities.Select(x => new RuleRow
                    {
                        Name = x.GetAttributeValue<string>("name") ?? "Unnamed business rule",
                        Status = Formatted(x, "statecode"), Scope = Formatted(x, "scope"), ModifiedOn = x.GetAttributeValue<DateTime?>("modifiedon")
                    }).OrderBy(x => x.Name).ToList();
                    args.Result = Tuple.Create(formItems, ruleItems);
                },
                PostWorkCallBack = args =>
                {
                    if (args.Error != null) { ShowErrorDialog(args.Error); return; }
                    var result = (Tuple<List<FormItem>, List<RuleRow>>)args.Result;
                    forms.AddRange(result.Item1); rules = result.Item2;
                    formList.DataSource = forms.ToList();
                    statusLabel.Text = $"{forms.Count} main form(s) and {rules.Count} business rule(s) found.";
                    BindGrids();
                }
            });
        }

        private void SelectForm()
        {
            if (!(formList.SelectedItem is FormItem form)) return;
            try { events = FormXmlParser.Parse(form.FormXml); statusLabel.Text = $"Showing configuration for {form.Name}."; }
            catch (Exception ex) { events.Clear(); ShowErrorDialog(ex); }
            exportButton.Enabled = true; BindGrids();
        }

        private void BindGrids()
        {
            var formEvents = events.Where(x => x.Event != "OnChange" && Match(x, formSearch.Text)).Select(x => new { x.Event, x.Function, x.Library, x.Enabled, PassContext = x.PassContext }).ToList();
            var changes = events.Where(x => x.Event == "OnChange" && Match(x, changeSearch.Text)).Select(x => new { FieldLogicalName = x.Field, x.Control, x.Function, x.Library, x.Enabled, PassContext = x.PassContext }).ToList();
            var filteredRules = rules.Where(x => Contains(x.Name + " " + x.Status + " " + x.Scope, ruleSearch.Text)).Select(x => new { x.Name, x.Status, x.Scope, ModifiedOn = x.ModifiedOn?.ToLocalTime() }).ToList();
            formGrid.DataSource = formEvents; changeGrid.DataSource = changes; ruleGrid.DataSource = filteredRules;
            loadCount.Text = $"{events.Count(x => x.Event == "OnLoad")}\r\nOnLoad";
            saveCount.Text = $"{events.Count(x => x.Event == "OnSave")}\r\nOnSave";
            changeCount.Text = $"{events.Count(x => x.Event == "OnChange")}\r\nOnChange";
            ruleCount.Text = $"{rules.Count}\r\nBusiness rules";
        }

        private void ExportCsv()
        {
            if (!(tableList.SelectedItem is TableItem table) || !(formList.SelectedItem is FormItem form)) return;
            using (var dialog = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv", FileName = $"{table.LogicalName}_{SafeName(form.Name)}_configuration.csv" })
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;
                var rows = new List<string> { Csv("Type", "Table", "Form", "Event or rule", "Field", "Control", "Function", "Library", "Status or scope") };
                rows.AddRange(events.Select(x => Csv("Event", table.LogicalName, form.Name, x.Event, x.Field, x.Control, x.Function, x.Library, x.Enabled ? "Enabled" : "Disabled")));
                rows.AddRange(rules.Select(x => Csv("Business rule", table.LogicalName, form.Name, x.Name, "", "", "", "", x.Status + " / " + x.Scope)));
                File.WriteAllText(dialog.FileName, "\uFEFF" + string.Join(Environment.NewLine, rows), Encoding.UTF8);
                statusLabel.Text = $"Exported {dialog.FileName}";
            }
        }

        private static string Formatted(Entity e, string attribute) => e.FormattedValues.Contains(attribute) ? e.FormattedValues[attribute] : Convert.ToString(e.GetAttributeValue<OptionSetValue>(attribute)?.Value);
        private static bool Match(EventRow x, string text) => Contains(string.Join(" ", x.Event, x.Field, x.Control, x.Function, x.Library), text);
        private static bool Contains(string source, string text) => string.IsNullOrWhiteSpace(text) || (source ?? "").IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
        private static string SafeName(string value) => string.Concat((value ?? "form").Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        private static string Csv(params string[] values) => string.Join(",", values.Select(v => "\"" + (v ?? "").Replace("\"", "\"\"") + "\""));
        private static Control Wrap(string label, Control control) { var p = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, Margin = new Padding(0, 0, 8, 0) }; p.RowStyles.Add(new RowStyle(SizeType.Absolute, 18)); p.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); p.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }, 0, 0); p.Controls.Add(control, 0, 1); return p; }
        private static Label Metric(string text) => new Label { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.White, ForeColor = Color.FromArgb(116, 39, 116), Font = new Font("Segoe UI", 11F, FontStyle.Bold), Margin = new Padding(4), BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(0, 10, 0, 0) };
        private static TextBox SearchBox(string placeholder) => new TextBox { Dock = DockStyle.Fill, Height = 32, Tag = placeholder, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0) };
        private static DataGridView Grid() => new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, ColumnHeadersVisible = true, EnableHeadersVisualStyles = false, GridColor = Color.FromArgb(230, 230, 230), CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing, ColumnHeadersHeight = 32, RowTemplate = { Height = 32 }, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None, DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.White, Font = new Font("Segoe UI", 9F), SelectionBackColor = Color.FromArgb(230, 240, 250), SelectionForeColor = Color.FromArgb(36, 36, 36) }, ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 248, 248), ForeColor = Color.FromArgb(36, 36, 36), Font = new Font("Segoe UI", 9F, FontStyle.Bold), Alignment = DataGridViewContentAlignment.MiddleLeft } };
        private static TabPage Tab(string title, TextBox search, DataGridView grid)
        {
            var page = new TabPage(title) { BackColor = Color.White, Padding = new Padding(0) };
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.White, Padding = new Padding(0) };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var searchPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 6, 0, 6), BackColor = Color.White };
            search.Dock = DockStyle.Fill;
            searchPanel.Controls.Add(search);

            root.Controls.Add(searchPanel, 0, 0);
            root.Controls.Add(grid, 0, 1);
            page.Controls.Add(root);
            return page;
        }
    }
}
