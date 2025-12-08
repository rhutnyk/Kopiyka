import React, { useEffect, useState } from 'react';
import { NavLink, Outlet } from 'react-router-dom';
import { fetchProfile } from '../api/profile';
import { HouseholdMembership } from '../api/dto';
import { useAuth } from '../auth/AuthProvider';

const ACTIVE_HOUSEHOLD_KEY = 'kopiyka-active-household';

export const AppLayout: React.FC = () => {
  const { session, status, loginMock, logout } = useAuth();
  const [memberships, setMemberships] = useState<HouseholdMembership[]>([]);
  const [activeHousehold, setActiveHousehold] = useState<string | null>(
    localStorage.getItem(ACTIVE_HOUSEHOLD_KEY),
  );
  const [profileStatus, setProfileStatus] = useState<'idle' | 'loading' | 'error'>('idle');

  useEffect(() => {
    if (status !== 'authenticated') {
      return;
    }

    const loadProfile = async () => {
      setProfileStatus('loading');
      try {
        const profile = await fetchProfile(session?.accessToken);
        setMemberships(profile.memberships);
        if (!activeHousehold && profile.memberships.length) {
          const householdId = profile.memberships[0].householdId;
          setActiveHousehold(householdId);
          localStorage.setItem(ACTIVE_HOUSEHOLD_KEY, householdId);
        }
        setProfileStatus('idle');
      } catch (error) {
        console.error('Failed to load profile', error);
        setProfileStatus('error');
      }
    };

    loadProfile();
  }, [status, session?.accessToken, activeHousehold]);

  useEffect(() => {
    if (activeHousehold) {
      localStorage.setItem(ACTIVE_HOUSEHOLD_KEY, activeHousehold);
    }
  }, [activeHousehold]);

  const handleHouseholdChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const householdId = event.target.value;
    setActiveHousehold(householdId);
  };

  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="brand">Kopiyka</div>
        <nav className="nav-links">
          <NavLink to="/households">Households</NavLink>
          <NavLink to="/categories">Categories</NavLink>
          <NavLink to="/accounts">Accounts</NavLink>
          <NavLink to="/transactions">Transactions</NavLink>
        </nav>
        <div className="session">
          {status === 'authenticated' && (
            <>
              <span className="user">{session?.userName}</span>
              <button onClick={logout} className="link-button">
                Sign out
              </button>
            </>
          )}
          {status === 'unauthenticated' && (
            <button onClick={() => loginMock()} className="primary">
              Sign in (mock)
            </button>
          )}
        </div>
      </header>

      <section className="controls">
        <div>
          <label className="label">Active household</label>
          {profileStatus === 'loading' && <div className="hint">Loading households…</div>}
          {profileStatus === 'error' && <div className="error">Could not load households</div>}
          {status === 'authenticated' && memberships.length > 0 && (
            <select value={activeHousehold ?? ''} onChange={handleHouseholdChange}>
              <option value="" disabled>
                Choose household
              </option>
              {memberships.map((membership) => (
                <option key={membership.householdId} value={membership.householdId}>
                  {membership.householdName} ({membership.role})
                </option>
              ))}
            </select>
          )}
        </div>
      </section>

      <main className="content">
        <Outlet context={{ activeHousehold }} />
      </main>
    </div>
  );
};
