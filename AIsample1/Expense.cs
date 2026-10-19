namespace AIsample1;

public sealed class Expense
{
    public DateTime Date { get; }
    public string Category { get; }
    public int Amount { get; }
    public string Memo { get; }

    public Expense(DateTime date, string category, int amount, string memo)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("カテゴリを選択してください。", nameof(category));
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "金額は1円以上で入力してください。");

        Date = date.Date;
        Category = category;
        Amount = amount;
        Memo = memo ?? string.Empty;
    }
}
