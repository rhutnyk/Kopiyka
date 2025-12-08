import React, { FormEvent, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthProvider';

export const SignUpPage: React.FC = () => {
  const { signUp, signInWithGoogle } = useAuth();
  const [displayName, setDisplayName] = useState('Demo User');
  const [email, setEmail] = useState('demo@example.com');
  const [password, setPassword] = useState('Password123!');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      await signUp(email, password, displayName);
      navigate('/');
    } catch (err) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError('Unable to create your account right now.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  const handleGoogle = async () => {
    if (!email) {
      setError('Enter your Google email to continue.');
      return;
    }

    setSubmitting(true);
    setError(null);
    try {
      const fallbackName = email.split('@')[0] || 'Google User';
      await signInWithGoogle(email, displayName || fallbackName);
      navigate('/');
    } catch (err) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError('Google sign-up failed.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="form-header">
          <p className="eyebrow">Join the beta</p>
          <h1 className="form-title">Create your Kopiyka account</h1>
          <p className="form-subtitle">
            Save your households, budgets, and transactions in one secure place.
          </p>
        </div>

        {error && <div className="error">{error}</div>}

        <form className="form" onSubmit={handleSubmit}>
          <label className="input-group">
            <span>Full name</span>
            <input
              type="text"
              value={displayName}
              onChange={(event) => setDisplayName(event.target.value)}
              placeholder="Pat Taylor"
              required
            />
          </label>

          <label className="input-group">
            <span>Email</span>
            <input
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              placeholder="you@example.com"
              required
            />
          </label>

          <label className="input-group">
            <span>Password</span>
            <input
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              placeholder="••••••••"
              required
            />
          </label>

          <div className="auth-actions">
            <button className="primary" type="submit" disabled={submitting}>
              {submitting ? 'Creating account…' : 'Sign up'}
            </button>
            <Link to="/sign-in" className="link-button">
              Already have an account?
            </Link>
          </div>
        </form>

        <div className="divider">
          <span>or continue with</span>
        </div>

        <button
          className="google-button"
          type="button"
          onClick={handleGoogle}
          disabled={submitting}
        >
          <span className="google-icon" aria-hidden>
            G
          </span>
          <span>Google</span>
        </button>
      </div>
    </div>
  );
};
