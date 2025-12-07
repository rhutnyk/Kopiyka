import React, { useEffect, useState } from 'react';
import { Household } from '../api/dto';
import { fetchHouseholds } from '../api/data';

export const HouseholdsPage: React.FC = () => {
  const [households, setHouseholds] = useState<Household[]>([]);
  const [state, setState] = useState<'idle' | 'loading' | 'error'>('idle');

  useEffect(() => {
    const load = async () => {
      setState('loading');
      try {
        const payload = await fetchHouseholds();
        setHouseholds(payload);
        setState('idle');
      } catch (error) {
        console.error(error);
        setState('error');
      }
    };

    load();
  }, []);

  return (
    <section>
      <header className="section-header">
        <div>
          <p className="eyebrow">Memberships</p>
          <h1>Households</h1>
        </div>
      </header>
      {state === 'loading' && <div className="hint">Loading households…</div>}
      {state === 'error' && <div className="error">Unable to load households</div>}
      {state === 'idle' && (
        <ul className="card-list">
          {households.map((household) => (
            <li key={household.id} className="card">
              <h3>{household.name}</h3>
              <p className="muted">ID: {household.id}</p>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
};
