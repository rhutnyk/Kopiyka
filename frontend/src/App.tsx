import './App.css';
import React from 'react';
import { BrowserRouter, Route, Routes, Navigate } from 'react-router-dom';
import { AppLayout } from './components/AppLayout';
import { AuthProvider, useAuth } from './auth/AuthProvider';
import { HouseholdsPage } from './pages/HouseholdsPage';
import { CategoriesPage } from './pages/CategoriesPage';
import { AccountsPage } from './pages/AccountsPage';
import { TransactionsPage } from './pages/TransactionsPage';
import { SignInPage } from './pages/SignInPage';
import { SignUpPage } from './pages/SignUpPage';

const ProtectedRoute: React.FC<{ children: React.ReactElement }> = ({ children }) => {
  const { status } = useAuth();

  if (status === 'loading') {
    return <div className="auth-loading">Checking your session…</div>;
  }

  if (status === 'unauthenticated') {
    return <Navigate to="/sign-in" replace />;
  }

  return children;
};

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <AppLayout />
              </ProtectedRoute>
            }
          >
            <Route index element={<Navigate to="/households" replace />} />
            <Route path="households" element={<HouseholdsPage />} />
            <Route path="categories" element={<CategoriesPage />} />
            <Route path="accounts" element={<AccountsPage />} />
            <Route path="transactions" element={<TransactionsPage />} />
          </Route>
          <Route path="/sign-in" element={<SignInPage />} />
          <Route path="/sign-up" element={<SignUpPage />} />
          <Route path="*" element={<Navigate to="/sign-in" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
