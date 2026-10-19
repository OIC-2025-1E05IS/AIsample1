(() => {
  "use strict";

  const STORAGE_KEY = "aisample1-web-expenses-v1";
  const form = document.getElementById("expenseForm");
  const dateInput = document.getElementById("expenseDate");
  const categoryInput = document.getElementById("expenseCategory");
  const amountInput = document.getElementById("expenseAmount");
  const memoInput = document.getElementById("expenseMemo");
  const totalAmount = document.getElementById("totalAmount");
  const expenseCount = document.getElementById("expenseCount");
  const expenseList = document.getElementById("expenseList");
  const emptyState = document.getElementById("emptyState");
  const tableWrap = document.getElementById("tableWrap");
  const categoryFilter = document.getElementById("categoryFilter");
  const formMessage = document.getElementById("formMessage");
  const deleteDialog = document.getElementById("deleteDialog");
  const confirmDeleteButton = document.getElementById("confirmDeleteButton");
  const toast = document.getElementById("toast");

  let expenses = loadExpenses();
  let pendingDeleteId = null;
  let toastTimer = null;

  dateInput.value = getLocalDateString();
  renderExpenses();

  form.addEventListener("submit", (event) => {
    event.preventDefault();
    clearErrors();
    formMessage.textContent = "";

    const date = dateInput.value;
    const category = categoryInput.value;
    const amountText = amountInput.value.trim();
    const amount = Number(amountText);
    const memo = memoInput.value.trim();
    let valid = true;

    if (!date || !isValidDate(date)) {
      showError(dateInput, "dateError", "日付を正しく入力してください。");
      valid = false;
    }
    if (!category) {
      showError(categoryInput, "categoryError", "カテゴリを選択してください。");
      valid = false;
    }
    if (amountText === "" || !Number.isSafeInteger(amount) || amount < 1) {
      showError(amountInput, "amountError", "金額は1円以上の整数で入力してください。");
      valid = false;
    }
    if (!valid) {
      formMessage.textContent = "入力内容を確認してください。";
      formMessage.style.color = "#c94d4d";
      return;
    }

    expenses.push({
      id: createId(),
      date,
      category,
      amount,
      memo
    });

    if (!saveExpenses()) {
      expenses.pop();
      formMessage.textContent = "保存できませんでした。ブラウザの保存容量を確認してください。";
      formMessage.style.color = "#c94d4d";
      return;
    }

    form.reset();
    dateInput.value = getLocalDateString();
    categoryFilter.value = "";
    formMessage.style.color = "";
    formMessage.textContent = "支出を登録しました。";
    renderExpenses();
    showToast("支出を登録しました");
  });

  categoryFilter.addEventListener("change", renderExpenses);

  confirmDeleteButton.addEventListener("click", (event) => {
    event.preventDefault();
    if (pendingDeleteId === null) return;

    const previousExpenses = expenses;
    expenses = expenses.filter((expense) => expense.id !== pendingDeleteId);
    if (!saveExpenses()) {
      expenses = previousExpenses;
      showToast("削除を保存できませんでした");
    } else {
      renderExpenses();
      showToast("支出を削除しました");
    }
    pendingDeleteId = null;
    deleteDialog.close();
  });

  document.querySelectorAll("input, select").forEach((field) => {
    field.addEventListener("input", () => {
      field.removeAttribute("aria-invalid");
      const error = document.getElementById(field.id.replace("expense", "").toLowerCase() + "Error");
      if (error) error.textContent = "";
    });
    field.addEventListener("change", () => {
      field.removeAttribute("aria-invalid");
    });
  });

  function loadExpenses() {
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      if (!saved) return [];
      const parsed = JSON.parse(saved);
      if (!Array.isArray(parsed)) return [];
      return parsed.filter((item) =>
        item &&
        typeof item.id === "string" &&
        typeof item.date === "string" &&
        typeof item.category === "string" &&
        Number.isSafeInteger(item.amount) &&
        item.amount >= 1 &&
        typeof item.memo === "string"
      );
    } catch (error) {
      console.error("保存データを読み込めませんでした。", error);
      return [];
    }
  }

  function saveExpenses() {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(expenses));
      return true;
    } catch (error) {
      console.error("支出データを保存できませんでした。", error);
      return false;
    }
  }

  function renderExpenses() {
    const filter = categoryFilter.value;
    const visibleExpenses = expenses
      .filter((expense) => !filter || expense.category === filter)
      .slice()
      .sort((a, b) => b.date.localeCompare(a.date) || b.id.localeCompare(a.id));

    totalAmount.textContent = formatYen(expenses.reduce((sum, expense) => sum + expense.amount, 0));
    expenseCount.textContent = expenses.length + "件";
    expenseList.replaceChildren();

    visibleExpenses.forEach((expense) => {
      const row = document.createElement("tr");

      const dateCell = document.createElement("td");
      dateCell.className = "date-cell";
      dateCell.textContent = formatDate(expense.date);

      const detailCell = document.createElement("td");
      const category = document.createElement("span");
      category.className = "category-name";
      category.textContent = expense.category;
      const memo = document.createElement("span");
      memo.className = "memo-text";
      memo.textContent = expense.memo || "メモなし";
      detailCell.append(category, memo);

      const amountCell = document.createElement("td");
      amountCell.className = "amount-cell";
      amountCell.textContent = formatYen(expense.amount);

      const actionCell = document.createElement("td");
      const deleteButton = document.createElement("button");
      deleteButton.type = "button";
      deleteButton.className = "delete-button";
      deleteButton.textContent = "削除";
      deleteButton.setAttribute("aria-label", formatDate(expense.date) + " " + expense.category + " " + formatYen(expense.amount) + "を削除");
      deleteButton.addEventListener("click", () => {
        pendingDeleteId = expense.id;
        if (typeof deleteDialog.showModal === "function") {
          deleteDialog.showModal();
        } else if (window.confirm("この支出を削除しますか？")) {
          confirmDeleteButton.click();
        }
      });
      actionCell.append(deleteButton);
      row.append(dateCell, detailCell, amountCell, actionCell);
      expenseList.append(row);
    });

    const hasExpenses = visibleExpenses.length > 0;
    emptyState.hidden = hasExpenses;
    tableWrap.hidden = !hasExpenses;
    if (!hasExpenses && expenses.length > 0 && filter) {
      emptyState.querySelector("h3").textContent = "該当する支出がありません";
      emptyState.querySelector("p").textContent = "別のカテゴリを選択してください。";
    } else {
      emptyState.querySelector("h3").textContent = "支出はまだありません";
      emptyState.querySelector("p").textContent = "左のフォームから最初の支出を登録しましょう。";
    }
  }

  function showError(input, errorId, message) {
    input.setAttribute("aria-invalid", "true");
    document.getElementById(errorId).textContent = message;
  }

  function clearErrors() {
    document.querySelectorAll(".field-error").forEach((error) => { error.textContent = ""; });
    document.querySelectorAll("[aria-invalid='true']").forEach((field) => field.removeAttribute("aria-invalid"));
  }

  function formatYen(value) {
    return new Intl.NumberFormat("ja-JP", {
      style: "currency",
      currency: "JPY",
      maximumFractionDigits: 0
    }).format(value);
  }

  function formatDate(value) {
    const parts = value.split("-");
    if (parts.length !== 3) return value;
    return Number(parts[1]) + "/" + Number(parts[2]);
  }

  function getLocalDateString() {
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, "0");
    const day = String(now.getDate()).padStart(2, "0");
    return year + "-" + month + "-" + day;
  }

  function isValidDate(value) {
    if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) return false;
    const date = new Date(value + "T00:00:00");
    return !Number.isNaN(date.getTime()) && getDateParts(date) === value;
  }

  function getDateParts(date) {
    return date.getFullYear() + "-" +
      String(date.getMonth() + 1).padStart(2, "0") + "-" +
      String(date.getDate()).padStart(2, "0");
  }

  function createId() {
    if (window.crypto && typeof window.crypto.randomUUID === "function") {
      return window.crypto.randomUUID();
    }
    return Date.now().toString(36) + Math.random().toString(36).slice(2);
  }

  function showToast(message) {
    toast.textContent = message;
    toast.classList.add("is-visible");
    window.clearTimeout(toastTimer);
    toastTimer = window.setTimeout(() => toast.classList.remove("is-visible"), 2400);
  }
})();