import { ProfileResponse } from './dto';
import { authenticatedFetch } from '../auth/AuthProvider';

export const fetchProfile = async (token?: string): Promise<ProfileResponse> => {
  try {
    const response = await authenticatedFetch('/auth/profile', token, {
      method: 'GET',
    });

    if (!response.ok) {
      throw new Error(`Profile request failed: ${response.status}`);
    }

    return response.json();
  } catch (error) {
    console.warn('Using mock profile because API is unreachable', error);
    return {
      userId: 'mock-user',
      displayName: 'Demo User',
      memberships: [
        { householdId: 'primary', householdName: 'Primary household', role: 'owner' },
        { householdId: 'parents', householdName: 'Parents', role: 'member' },
      ],
    };
  }
};
