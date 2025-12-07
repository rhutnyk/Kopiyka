import React, { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { API_BASE_URL } from '../env';

type AuthStatus = 'loading' | 'authenticated' | 'unauthenticated';

type AuthSession = {
  userName: string;
  accessToken?: string;
};

type AuthContextValue = {
  status: AuthStatus;
  session: AuthSession | null;
  loginMock: (userName?: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);
const SESSION_KEY = 'kopiyka-auth-session';

const mockPrincipal: AuthSession = {
  userName: 'demo@contoso.test',
  accessToken: 'mock-token',
};

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [status, setStatus] = useState<AuthStatus>('loading');
  const [session, setSession] = useState<AuthSession | null>(null);

  useEffect(() => {
    const stored = localStorage.getItem(SESSION_KEY);
    if (stored) {
      try {
        const parsed: AuthSession = JSON.parse(stored);
        setSession(parsed);
        setStatus('authenticated');
        return;
      } catch (error) {
        console.warn('Failed to parse stored session', error);
      }
    }

    const loadAzureAuth = async () => {
      try {
        const response = await fetch('/.auth/me');
        if (!response.ok) throw new Error('No Static Web Apps auth');
        const payload = await response.json();
        const clientPrincipal = payload.clientPrincipal;
        if (!clientPrincipal) throw new Error('Missing client principal');
        const principal: AuthSession = {
          userName: clientPrincipal.userDetails,
          accessToken: clientPrincipal.userId,
        };
        localStorage.setItem(SESSION_KEY, JSON.stringify(principal));
        setSession(principal);
        setStatus('authenticated');
      } catch (error) {
        console.info('Falling back to mock auth flow', error);
        setStatus('unauthenticated');
      }
    };

    loadAzureAuth();
  }, []);

  const loginMock = async (userName = mockPrincipal.userName) => {
    localStorage.setItem(SESSION_KEY, JSON.stringify({ ...mockPrincipal, userName }));
    setSession({ ...mockPrincipal, userName });
    setStatus('authenticated');
  };

  const logout = () => {
    localStorage.removeItem(SESSION_KEY);
    setSession(null);
    setStatus('unauthenticated');
  };

  const value = useMemo(() => ({ status, session, loginMock, logout }), [status, session]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error('useAuth must be used inside AuthProvider');
  }
  return ctx;
};

export const authHeaders = (token?: string) =>
  token
    ? {
        Authorization: `Bearer ${token}`,
      }
    : {};

export const authenticatedFetch = (path: string, token?: string, init?: RequestInit) => {
  const url = path.startsWith('http') ? path : `${API_BASE_URL}${path}`;
  const headers = new Headers();
  headers.set('Content-Type', 'application/json');

  if (token) {
    headers.set('Authorization', `Bearer ${token}`);
  }

  if (init?.headers) {
    const provided = new Headers(init.headers as HeadersInit);
    provided.forEach((value, key) => headers.set(key, value));
  }

  return fetch(url, {
    ...init,
    headers,
  });
};
