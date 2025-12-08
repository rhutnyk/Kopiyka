import { API_BASE_URL } from '../env';
import { HouseholdMembership } from './dto';

export type AuthSession = {
  userId: string;
  email: string;
  displayName: string;
  accessToken: string;
  memberships: HouseholdMembership[];
};

const handleResponse = async (response: Response) => {
  if (response.ok) {
    return response.json();
  }

  let errorMessage = `Request failed with status ${response.status}`;
  try {
    const payload = await response.json();
    if (payload.error) {
      errorMessage = payload.error;
    }
  } catch {
    const fallback = await response.text();
    if (fallback) {
      errorMessage = fallback;
    }
  }

  throw new Error(errorMessage);
};

export const signIn = async (email: string, password: string): Promise<AuthSession> => {
  const response = await fetch(`${API_BASE_URL}/auth/sign-in`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  });

  return handleResponse(response);
};

export const signUp = async (
  email: string,
  password: string,
  displayName: string,
): Promise<AuthSession> => {
  const response = await fetch(`${API_BASE_URL}/auth/sign-up`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password, displayName }),
  });

  return handleResponse(response);
};

export const signInWithGoogle = async (
  email: string,
  displayName: string,
): Promise<AuthSession> => {
  const response = await fetch(`${API_BASE_URL}/auth/google`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, displayName }),
  });

  return handleResponse(response);
};
