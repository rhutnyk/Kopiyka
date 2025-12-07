import { Account, Category, Household, Transaction } from './dto';

const simulateLatency = async <T>(payload: T, fail = false): Promise<T> => {
  await new Promise((resolve) => setTimeout(resolve, 300));
  if (fail) {
    throw new Error('Simulated network error');
  }
  return payload;
};

export const fetchHouseholds = async (): Promise<Household[]> => {
  return simulateLatency([
    { id: 'primary', name: 'Primary household' },
    { id: 'parents', name: 'Parents' },
  ]);
};

export const fetchCategories = async (householdId: string): Promise<Category[]> => {
  const categories: Category[] = [
    { id: 'groceries', name: 'Groceries', type: 'expense' },
    { id: 'salary', name: 'Salary', type: 'income' },
    { id: 'utilities', name: 'Utilities', type: 'expense' },
  ];
  return simulateLatency(categories.map((c) => ({ ...c, id: `${householdId}-${c.id}` })));
};

export const fetchAccounts = async (householdId: string): Promise<Account[]> => {
  const accounts: Account[] = [
    { id: 'checking', name: 'Checking', balance: 1520.21 },
    { id: 'savings', name: 'Savings', balance: 8950.7 },
  ];
  return simulateLatency(accounts.map((a) => ({ ...a, id: `${householdId}-${a.id}` })));
};

export const fetchTransactions = async (householdId: string): Promise<Transaction[]> => {
  const transactions: Transaction[] = [
    {
      id: 'txn1',
      description: 'Weekly groceries',
      amount: -82.35,
      accountId: `${householdId}-checking`,
      categoryId: `${householdId}-groceries`,
      date: new Date().toISOString(),
    },
    {
      id: 'txn2',
      description: 'Paycheck',
      amount: 2500,
      accountId: `${householdId}-checking`,
      categoryId: `${householdId}-salary`,
      date: new Date().toISOString(),
    },
  ];

  return simulateLatency(transactions);
};
