namespace AIsample1;

public sealed class MainForm : Form
{
    private readonly ExpenseLedger _ledger = new();
    private readonly DateTimePicker _datePicker = new();
    private readonly ComboBox _categoryComboBox = new();
    private readonly NumericUpDown _amountInput = new();
    private readonly TextBox _memoInput = new();
    private readonly DataGridView _expenseGrid = new();
    private readonly Label _totalLabel = new();

    public MainForm()
    {
        Text = "かんたん家計簿";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(760, 520);
        Size = new Size(900, 620);
        Font = new Font("Yu Gothic UI", 10F);
        BuildLayout();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(16)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(root);

        var inputPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 4,
            RowCount = 3,
            Padding = new Padding(0, 0, 0, 12)
        };
        for (var i = 0; i < 4; i++)
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        root.Controls.Add(inputPanel, 0, 0);

        AddInput(inputPanel, "日付", _datePicker, 0, 0);
        _datePicker.Format = DateTimePickerFormat.Short;

        _categoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _categoryComboBox.Items.AddRange(new object[]
        {
            "食費", "日用品", "交通費", "住居費", "水道・光熱費", "通信費", "娯楽", "医療費", "その他"
        });
        _categoryComboBox.SelectedIndex = 0;
        AddInput(inputPanel, "カテゴリ", _categoryComboBox, 1, 0);

        _amountInput.Minimum = 1;
        _amountInput.Maximum = 100000000;
        _amountInput.ThousandsSeparator = true;
        AddInput(inputPanel, "金額（円）", _amountInput, 0, 1);

        AddInput(inputPanel, "メモ（任意）", _memoInput, 1, 1);

        var addButton = new Button
        {
            Text = "支出を登録",
            AutoSize = true,
            Padding = new Padding(12, 6, 12, 6),
            Anchor = AnchorStyles.Left
        };
        addButton.Click += AddExpense;
        inputPanel.Controls.Add(addButton, 0, 2);

        var deleteButton = new Button
        {
            Text = "選択した支出を削除",
            AutoSize = true,
            Padding = new Padding(12, 6, 12, 6),
            Anchor = AnchorStyles.Left
        };
        deleteButton.Click += DeleteExpense;
        inputPanel.Controls.Add(deleteButton, 1, 2);

        _expenseGrid.Dock = DockStyle.Fill;
        _expenseGrid.AllowUserToAddRows = false;
        _expenseGrid.AllowUserToDeleteRows = false;
        _expenseGrid.AllowUserToOrderColumns = false;
        _expenseGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _expenseGrid.MultiSelect = false;
        _expenseGrid.ReadOnly = true;
        _expenseGrid.RowHeadersVisible = false;
        _expenseGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _expenseGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "日付", SortMode = DataGridViewColumnSortMode.NotSortable });
        _expenseGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "カテゴリ", SortMode = DataGridViewColumnSortMode.NotSortable });
        _expenseGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "金額（円）", SortMode = DataGridViewColumnSortMode.NotSortable });
        _expenseGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Memo", HeaderText = "メモ", SortMode = DataGridViewColumnSortMode.NotSortable });
        root.Controls.Add(_expenseGrid, 0, 1);

        _totalLabel.Text = "合計：0円";
        _totalLabel.Font = new Font(Font, FontStyle.Bold);
        _totalLabel.AutoSize = true;
        _totalLabel.Padding = new Padding(0, 12, 0, 0);
        root.Controls.Add(_totalLabel, 0, 2);
    }

    private static void AddInput(TableLayoutPanel panel, string labelText, Control input, int column, int row)
    {
        var container = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(4)
        };
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        container.Controls.Add(new Label { Text = labelText, AutoSize = true, Padding = new Padding(0, 0, 0, 4) }, 0, 0);
        input.Dock = DockStyle.Fill;
        container.Controls.Add(input, 0, 1);
        panel.Controls.Add(container, column * 2, row);
        panel.SetColumnSpan(container, 2);
    }

    private void AddExpense(object? sender, EventArgs e)
    {
        if (_categoryComboBox.SelectedItem is not string category)
        {
            MessageBox.Show("カテゴリを選択してください。", "入力確認", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var expense = new Expense(_datePicker.Value, category, decimal.ToInt32(_amountInput.Value), _memoInput.Text.Trim());
            _ledger.Add(expense);
            _expenseGrid.Rows.Add(expense.Date.ToString("yyyy/MM/dd"), expense.Category, expense.Amount.ToString("N0"), expense.Memo);
            UpdateTotal();
            _memoInput.Clear();
            _amountInput.Value = 1;
            _categoryComboBox.SelectedIndex = 0;
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message, "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void DeleteExpense(object? sender, EventArgs e)
    {
        if (_expenseGrid.SelectedRows.Count == 0)
        {
            MessageBox.Show("削除する支出を一覧から選択してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var index = _expenseGrid.SelectedRows[0].Index;
        if (!_ledger.RemoveAt(index))
        {
            MessageBox.Show("選択した支出を削除できませんでした。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _expenseGrid.Rows.RemoveAt(index);
        UpdateTotal();
    }

    private void UpdateTotal() => _totalLabel.Text = $"合計：{_ledger.TotalAmount:N0}円";
}
