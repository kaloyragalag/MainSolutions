import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { authService } from '../../services/authService';
import { LoginRequest } from '../../types/auth';
import './LoginPage.css';

const EyeIcon = ({ open }: { open: boolean }) => open
  ? <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M17.94 17.94A10.07 10.07 0 0112 20c-7 0-11-8-11-8a18.45 18.45 0 015.06-5.94"/><path d="M9.9 4.24A9.12 9.12 0 0112 4c7 0 11 8 11 8a18.5 18.5 0 01-2.16 3.19"/><line x1="1" y1="1" x2="23" y2="23"/></svg>
  : <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>;

const LoginPage: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState<LoginRequest>({ email: '', password: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

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
      setError(err.message || 'Something went wrong.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-layout">
      <div className="login-layout__topbar">
        <div className="login-layout__logo">
          <span className="login-layout__logo-bracket">{`{`}</span>
          <span className="login-layout__logo-text">MS</span>
          <span className="login-layout__logo-bracket">{`}`}</span>
        </div>
        <span className="login-layout__app-name">MainSolutions</span>
      </div>

      <div className="login">
        <div className="login__card">
          <div className="login__header">
            <h1 className="ms-h2">Log in to your account</h1>
            <p className="ms-text-secondary" style={{ marginTop: 4 }}>Enter your credentials to continue</p>
          </div>

          <form className="ms-form" onSubmit={handleSubmit} noValidate>
            {error && (
              <div className="ms-alert ms-alert--danger" role="alert">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" style={{ flexShrink: 0, marginTop: 1 }}>
                  <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
                </svg>
                {error}
              </div>
            )}

            <div className="ms-field">
              <label className="ms-field__label" htmlFor="email">Email</label>
              <input id="email" name="email" type="email" className="ms-field__input" placeholder="you@example.com" value={form.email} onChange={handleChange} autoComplete="email" required />
            </div>

            <div className="ms-field">
              <label className="ms-field__label" htmlFor="password">Password</label>
              <div className="ms-field__input-wrapper">
                <input id="password" name="password" type={showPassword ? 'text' : 'password'} className="ms-field__input ms-field__input--password" placeholder="••••••••" value={form.password} onChange={handleChange} autoComplete="current-password" required />
                <button type="button" className="ms-field__toggle" onClick={() => setShowPassword(p => !p)} aria-label={showPassword ? 'Hide password' : 'Show password'}>
                  <EyeIcon open={showPassword} />
                </button>
              </div>
            </div>

            <button type="submit" className={`ms-btn ms-btn--primary ms-btn--full${loading ? ' ms-btn--loading' : ''}`} disabled={loading}>
              {loading ? <><span className="ms-btn__spinner" aria-hidden="true" />Logging in...</> : 'Log in'}
            </button>
          </form>

          <p className="ms-footer-link">
            Don't have an account? <Link to="/register">Register</Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
