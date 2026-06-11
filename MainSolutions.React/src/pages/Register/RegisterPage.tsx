import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { authService } from '../../services/authService';
import { RegisterRequest } from '../../types/auth';
import BasePage from '../../components/Layout/BasePage';
import './RegisterPage.css';

const EyeIcon = ({ open }: { open: boolean }) => open
  ? <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M17.94 17.94A10.07 10.07 0 0112 20c-7 0-11-8-11-8a18.45 18.45 0 015.06-5.94"/><path d="M9.9 4.24A9.12 9.12 0 0112 4c7 0 11 8 11 8a18.5 18.5 0 01-2.16 3.19"/><line x1="1" y1="1" x2="23" y2="23"/></svg>
  : <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>;

const RegisterPage: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState<RegisterRequest>({ firstName: '', lastName: '', email: '', password: '' });
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));
    setError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (form.password !== confirmPassword) { setError('Passwords do not match.'); return; }
    setLoading(true);
    setError('');
    try {
      const response = await authService.register(form);
      login(response);
      navigate('/');
    } catch (err: any) {
      setError(err.message || 'Something went wrong.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <BasePage showSidebar={false}>
      <div className="register__card">
        <div className="register__header">
          <h1 className="ms-h2">Create your account</h1>
          <p className="ms-text-secondary" style={{ marginTop: 4 }}>Get started with MainSolutions</p>
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

          <div className="ms-row">
            <div className="ms-field">
              <label className="ms-field__label" htmlFor="firstName">First name</label>
              <input id="firstName" name="firstName" type="text" className="ms-field__input" placeholder="John" value={form.firstName} onChange={handleChange} required />
            </div>
            <div className="ms-field">
              <label className="ms-field__label" htmlFor="lastName">Last name</label>
              <input id="lastName" name="lastName" type="text" className="ms-field__input" placeholder="Doe" value={form.lastName} onChange={handleChange} required />
            </div>
          </div>

          <div className="ms-field">
            <label className="ms-field__label" htmlFor="email">Email</label>
            <input id="email" name="email" type="email" className="ms-field__input" placeholder="you@example.com" value={form.email} onChange={handleChange} required />
          </div>

          <div className="ms-field">
            <label className="ms-field__label" htmlFor="password">Password</label>
            <div className="ms-field__input-wrapper">
              <input id="password" name="password" type={showPassword ? 'text' : 'password'} className="ms-field__input ms-field__input--password" placeholder="Min. 6 characters" value={form.password} onChange={handleChange} required />
              <button type="button" className="ms-field__toggle" onClick={() => setShowPassword(p => !p)} aria-label={showPassword ? 'Hide password' : 'Show password'}>
                <EyeIcon open={showPassword} />
              </button>
            </div>
          </div>

          <div className="ms-field">
            <label className="ms-field__label" htmlFor="confirmPassword">Confirm password</label>
            <input id="confirmPassword" name="confirmPassword" type={showPassword ? 'text' : 'password'} className="ms-field__input" placeholder="Re-enter password" value={confirmPassword} onChange={e => { setConfirmPassword(e.target.value); setError(''); }} required />
          </div>

          <button type="submit" className={`ms-btn ms-btn--primary ms-btn--full${loading ? ' ms-btn--loading' : ''}`} disabled={loading}>
            {loading ? <><span className="ms-btn__spinner" aria-hidden="true" />Creating account...</> : 'Create account'}
          </button>
        </form>

        <p className="ms-footer-link">
          Already have an account? <Link to="/login">Log in</Link>
        </p>
      </div>
    </BasePage>
  );
};

export default RegisterPage;
