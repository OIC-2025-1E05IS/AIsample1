using Microsoft.VisualStudio.TestTools.UnitTesting;
using AIsample1;

namespace AIsample1.Tests;

[TestClass]
public class ExpenseLedgerTests
{
    [TestMethod]
    public void Add_ValidExpenses_UpdatesCountAndTotal()
    {
        var ledger = new ExpenseLedger();
        ledger.Add(new Expense(new DateTime(2026, 10, 19), "食費", 100, "昼食"));
        ledger.Add(new Expense(new DateTime(2026, 10, 19), "日用品", 200, ""));
        Assert.AreEqual(2, ledger.Expenses.Count);
        Assert.AreEqual(300, ledger.TotalAmount);
    }

    [TestMethod]
    public void Expense_AmountOne_IsAccepted()
    {
        Assert.AreEqual(1, new Expense(DateTime.Today, "その他", 1, "").Amount);
    }

    [TestMethod]
    public void Expense_ZeroAmount_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Expense(DateTime.Today, "食費", 0, ""));
    }

    [TestMethod]
    public void Expense_NegativeAmount_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Expense(DateTime.Today, "食費", -1, ""));
    }

    [TestMethod]
    public void Expense_EmptyCategory_Throws()
    {
        Assert.ThrowsException<ArgumentException>(() => new Expense(DateTime.Today, " ", 100, ""));
    }

    [TestMethod]
    public void Add_NullExpense_Throws()
    {
        var ledger = new ExpenseLedger();
        Assert.ThrowsException<ArgumentNullException>(() => ledger.Add(null!));
    }

    [TestMethod]
    public void RemoveAt_InvalidIndex_ReturnsFalse()
    {
        var ledger = new ExpenseLedger();
        Assert.IsFalse(ledger.RemoveAt(-1));
        Assert.IsFalse(ledger.RemoveAt(0));
    }

    [TestMethod]
    public void RemoveAt_ValidIndex_UpdatesTotal()
    {
        var ledger = new ExpenseLedger();
        ledger.Add(new Expense(DateTime.Today, "食費", 100, ""));
        ledger.Add(new Expense(DateTime.Today, "娯楽", 250, ""));
        Assert.IsTrue(ledger.RemoveAt(0));
        Assert.AreEqual(1, ledger.Expenses.Count);
        Assert.AreEqual(250, ledger.TotalAmount);
    }
}
