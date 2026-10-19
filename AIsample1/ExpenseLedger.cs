namespace AIsample1;

public sealed class ExpenseLedger
{
    private readonly List<Expense> _expenses = new();

    public IReadOnlyList<Expense> Expenses => _expenses.AsReadOnly();
    public int TotalAmount => _expenses.Sum(expense => expense.Amount);

    public void Add(Expense expense)
    {
        ArgumentNullException.ThrowIfNull(expense);
        _expenses.Add(expense);
    }

    public bool RemoveAt(int index)
    {
        if (index < 0 || index >= _expenses.Count)
            return false;
        _expenses.RemoveAt(index);
        return true;
    }
}
