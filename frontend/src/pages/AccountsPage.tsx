import React, { useEffect, useState } from 'react';
import { useOutletContext } from 'react-router-dom';
import { Account } from '../api/dto';
import { fetchAccounts } from '../api/data';
import { RouteContext } from './PageTypes';

export const AccountsPage: React.FC = () => {
  const { activeHousehold } = useOutletContext<RouteContext>();
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [state, setState] = useState<'idle' | 'loading' | 'error'>('idle');

  useEffect(() => {
    if (!activeHousehold) return;

    const load = async () => {
      setState('loading');
      try {
        const payload = await fetchAccounts(activeHousehold);
        setAccounts(payload);
        setState('idle');
      } catch (error) {
        console.error(error);
        setState('error');
      }
    };

    load();
  }, [activeHousehold]);

  if (!activeHousehold) {
    return <div className="hint">Pick a household to see accounts.</div>;
  }

  return (
    <section>
      <header className="section-header">
        <div>
          <p className="eyebrow">Asset pool</p>
          <h1>Accounts</h1>
        </div>
      </header>
      {state === 'loading' && <div className="hint">Loading accounts…</div>}
      {state === 'error' && <div className="error">Unable to load accounts</div>}
      {state === 'idle' && (
        <ul className="card-list">
          {accounts.map((account) => (
            <li key={account.id} className="card">
              <h3>{account.name}</h3>
              <p className="muted">Balance: {account.balance.toLocaleString(undefined, { style: 'currency', currency: 'USD' })}</p>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
};
