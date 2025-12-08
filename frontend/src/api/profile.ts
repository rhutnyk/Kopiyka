import { ProfileResponse } from './dto';
import { authenticatedFetch } from '../auth/AuthProvider';

export const fetchProfile = async (token?: string): Promise<ProfileResponse> => {
  const response = await authenticatedFetch('/auth/me', token, {
    method: 'GET',
  });

  if (!response.ok) {
    throw new Error(`Profile request failed: ${response.status}`);
  }

  return response.json();
};
