import React, { useEffect, useState } from 'react';
import { useOutletContext } from 'react-router-dom';
import { Transaction } from '../api/dto';
import { fetchTransactions } from '../api/data';
import { RouteContext } from './PageTypes';

export const TransactionsPage: React.FC = () => {
  const { activeHousehold } = useOutletContext<RouteContext>();
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [state, setState] = useState<'idle' | 'loading' | 'error'>('idle');

  useEffect(() => {
    if (!activeHousehold) return;

    const load = async () => {
      setState('loading');
      try {
        const payload = await fetchTransactions(activeHousehold);
        setTransactions(payload);
        setState('idle');
      } catch (error) {
        console.error(error);
        setState('error');
      }
    };

    load();
  }, [activeHousehold]);

  if (!activeHousehold) {
    return <div className="hint">Pick a household to see transactions.</div>;
  }

  return (
    <section>
      <header className="section-header">
        <div>
          <p className="eyebrow">Activity feed</p>
          <h1>Transactions</h1>
        </div>
      </header>
      {state === 'loading' && <div className="hint">Loading transactions…</div>}
      {state === 'error' && <div className="error">Unable to load transactions</div>}
      {state === 'idle' && (
        <div className="table">
          <div className="table-header">
            <span>Description</span>
            <span>Amount</span>
            <span>Date</span>
          </div>
          {transactions.map((txn) => (
            <div key={txn.id} className="table-row">
              <span>{txn.description}</span>
              <span className={txn.amount < 0 ? 'negative' : 'positive'}>
                {txn.amount.toLocaleString(undefined, { style: 'currency', currency: 'USD' })}
              </span>
              <span>{new Date(txn.date).toLocaleString()}</span>
            </div>
          ))}
        </div>
      )}
    </section>
  );
};
