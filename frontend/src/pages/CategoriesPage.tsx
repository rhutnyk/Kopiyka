import React, { useEffect, useState } from 'react';
import { useOutletContext } from 'react-router-dom';
import { Category } from '../api/dto';
import { fetchCategories } from '../api/data';
import { RouteContext } from './PageTypes';

export const CategoriesPage: React.FC = () => {
  const { activeHousehold } = useOutletContext<RouteContext>();
  const [categories, setCategories] = useState<Category[]>([]);
  const [state, setState] = useState<'idle' | 'loading' | 'error'>('idle');

  useEffect(() => {
    if (!activeHousehold) return;

    const load = async () => {
      setState('loading');
      try {
        const payload = await fetchCategories(activeHousehold);
        setCategories(payload);
        setState('idle');
      } catch (error) {
        console.error(error);
        setState('error');
      }
    };

    load();
  }, [activeHousehold]);

  if (!activeHousehold) {
    return <div className="hint">Pick a household to see categories.</div>;
  }

  return (
    <section>
      <header className="section-header">
        <div>
          <p className="eyebrow">Envelope structure</p>
          <h1>Categories</h1>
        </div>
      </header>
      {state === 'loading' && <div className="hint">Loading categories…</div>}
      {state === 'error' && <div className="error">Unable to load categories</div>}
      {state === 'idle' && (
        <ul className="card-list">
          {categories.map((category) => (
            <li key={category.id} className="card">
              <h3>{category.name}</h3>
              <p className="muted">Type: {category.type}</p>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
};
