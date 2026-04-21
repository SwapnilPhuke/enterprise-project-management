import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authApi } from '../services/authApi';
import { parseApiError } from '../utils/parseApiError';

export default function RegisterPage() {
  const navigate = useNavigate();
  const [form,    setForm]    = useState({ username: '', email: '', password: '', confirmPassword: '', fullName: '' });
  const [error,   setError]   = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const set = field => e => setForm(prev => ({ ...prev, [field]: e.target.value }));

  const handleSubmit = async e => {
    e.preventDefault();
    setError('');
    if (form.password !== form.confirmPassword) {
      setError('Passwords do not match.');
      return;
    }
    setLoading(true);
    try {
      await authApi.register({
        username: form.username,
        email:    form.email,
        password: form.password,
        fullName: form.fullName || undefined,
      });
      setSuccess('Account created! Redirecting to sign in…');
      setTimeout(() => navigate('/login'), 1500);
    } catch (err) {
      setError(parseApiError(err, 'Registration failed. Please try again.'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-wrapper">
      <div className="auth-card">
        <div className="auth-header">
          <div className="auth-logo">EPM</div>
          <h1>Enterprise Project Management</h1>
          <p>Create your account</p>
        </div>

        {error   && <div className="alert alert-error">{error}</div>}
        {success && <div className="alert alert-success">{success}</div>}

        <div className="auth-tabs">
          <Link to="/login"><button className="auth-tab">Sign In</button></Link>
          <button className="auth-tab active">Register</button>
        </div>

        <form onSubmit={handleSubmit} className="auth-form" noValidate>
          <div className="form-group">
            <label htmlFor="fullName">Full Name <span className="optional">(optional)</span></label>
            <input id="fullName" type="text" value={form.fullName} onChange={set('fullName')}
              placeholder="Your full name" disabled={loading} autoComplete="name" />
          </div>
          <div className="form-group">
            <label htmlFor="username">Username *</label>
            <input id="username" type="text" value={form.username} onChange={set('username')}
              placeholder="min 3 chars, letters/numbers/_" required minLength={3}
              disabled={loading} autoComplete="username" />
          </div>
          <div className="form-group">
            <label htmlFor="email">Email *</label>
            <input id="email" type="email" value={form.email} onChange={set('email')}
              placeholder="your@email.com" required disabled={loading} autoComplete="email" />
          </div>
          <div className="form-group">
            <label htmlFor="password">Password *</label>
            <input id="password" type="password" value={form.password} onChange={set('password')}
              placeholder="min 6 characters" required minLength={6}
              disabled={loading} autoComplete="new-password" />
          </div>
          <div className="form-group">
            <label htmlFor="confirmPassword">Confirm Password *</label>
            <input id="confirmPassword" type="password" value={form.confirmPassword}
              onChange={set('confirmPassword')} placeholder="Re-enter your password"
              required disabled={loading} autoComplete="new-password" />
          </div>
          <button
            type="submit"
            className="btn btn-primary"
            disabled={loading || !form.username || !form.email || !form.password || !form.confirmPassword}
          >
            {loading ? 'Creating account…' : 'Create Account'}
          </button>
        </form>
      </div>
    </div>
  );
}
