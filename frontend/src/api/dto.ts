export type HouseholdMembership = {
  householdId: string;
  householdName: string;
  role: 'owner' | 'member' | 'viewer';
};

export type ProfileResponse = {
  id: string;
  email: string;
  displayName: string;
  memberships: HouseholdMembership[];
};

export type Household = {
  id: string;
  name: string;
};

export type Category = {
  id: string;
  name: string;
  type: 'expense' | 'income';
};

export type Account = {
  id: string;
  name: string;
  balance: number;
};

export type Transaction = {
  id: string;
  description: string;
  amount: number;
  accountId: string;
  categoryId: string;
  date: string;
};
