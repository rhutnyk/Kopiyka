import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { API_BASE_URL } from '../env';
import { AuthSession as ApiAuthSession, signIn, signInWithGoogle, signUp } from '../api/auth';

export type AuthStatus = 'loading' | 'authenticated' | 'unauthenticated';

export type AuthSession = ApiAuthSession;

type AuthContextValue = {
  status: AuthStatus;
  session: AuthSession | null;
  signIn: (email: string, password: string) => Promise<void>;
  signUp: (email: string, password: string, displayName: string) => Promise<void>;
  signInWithGoogle: (email: string, displayName: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);
const SESSION_KEY = 'kopiyka-auth-session';

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
        localStorage.removeItem(SESSION_KEY);
      }
    }

    setStatus('unauthenticated');
  }, []);

  const persistSession = useCallback((payload: ApiAuthSession) => {
    localStorage.setItem(SESSION_KEY, JSON.stringify(payload));
    setSession(payload);
    setStatus('authenticated');
  }, []);

  const handleSignIn = useCallback(
    async (email: string, password: string) => {
      const nextSession = await signIn(email, password);
      persistSession(nextSession);
    },
    [persistSession],
  );

  const handleSignUp = useCallback(
    async (email: string, password: string, displayName: string) => {
      const nextSession = await signUp(email, password, displayName);
      persistSession(nextSession);
    },
    [persistSession],
  );

  const handleGoogleSignIn = useCallback(
    async (email: string, displayName: string) => {
      const nextSession = await signInWithGoogle(email, displayName);
      persistSession(nextSession);
    },
    [persistSession],
  );

  const logout = useCallback(() => {
    localStorage.removeItem(SESSION_KEY);
    setSession(null);
    setStatus('unauthenticated');
  }, []);

  const value = useMemo(
    () => ({
      status,
      session,
      signIn: handleSignIn,
      signUp: handleSignUp,
      signInWithGoogle: handleGoogleSignIn,
      logout,
    }),
    [status, session, handleSignIn, handleSignUp, handleGoogleSignIn, logout],
  );

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
