import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { authService } from '../../services/authService';
import { LoginRequest } from '../../types/auth';
import BasePage from '../../components/Layout/BasePage';
import './LoginPage.css';

const LoginPage: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState<LoginRequest>({ email: '', password: '' });
  const [error, setError] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [showPassword, setShowPassword] = useState<boolean>(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));
    setError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      const response = await authService.login(form);
      login(response);
      navigate('/');
    } catch (err: any) {
      setError(err.message || 'Something went wrong. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <BasePage>
      <div className="login">
        <div className="login__card">
          <div className="login__header">
            <h1 className="login__title">Welcome back</h1>
            <p className="login__subtitle">Sign in to your account</p>
          </div>

          <form className="login__form" onSubmit={handleSubmit} noValidate>
            {error && (
              <div className="login__error" role="alert">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <circle cx="12" cy="12" r="10" /><line x1="12" y1="8" x2="12" y2="12" /><line x1="12" y1="16" x2="12.01" y2="16" />
                </svg>
                {error}
              </div>
            )}

            <div className="login__field">
              <label className="login__label" htmlFor="email">Email</label>
              <input id="email" name="email" type="email" className="login__input" placeholder="you@example.com" value={form.email} onChange={handleChange} autoComplete="email" required />
            </div>

            <div className="login__field">
              <label className="login__label" htmlFor="password">Password</label>
              <div className="login__input-wrapper">
                <input id="password" name="password" type={showPassword ? 'text' : 'password'} className="login__input login__input--password" placeholder="••••••••" value={form.password} onChange={handleChange} autoComplete="current-password" required />
                <button type="button" className="login__toggle-password" onClick={() => setShowPassword(p => !p)} aria-label={showPassword ? 'Hide password' : 'Show password'}>
                  {showPassword ? (
                    <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M17.94 17.94A10.07 10.07 0 0112 20c-7 0-11-8-11-8a18.45 18.45 0 015.06-5.94"/><path d="M9.9 4.24A9.12 9.12 0 0112 4c7 0 11 8 11 8a18.5 18.5 0 01-2.16 3.19"/><line x1="1" y1="1" x2="23" y2="23"/></svg>
                  ) : (
                    <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                  )}
                </button>
              </div>
            </div>

            <button type="submit" className={`login__btn${loading ? ' login__btn--loading' : ''}`} disabled={loading}>
              {loading ? <><span className="login__spinner" aria-hidden="true" />Signing in...</> : 'Sign in'}
            </button>
          </form>

          <p className="login__footer">
            Don't have an account? <Link to="/register" className="login__footer-link">Register</Link>
          </p>
        </div>
      </div>
    </BasePage>
  );
};

export default LoginPage;
