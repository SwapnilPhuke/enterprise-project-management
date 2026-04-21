import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Navbar() {
  const { currentUser, logout, isAdmin } = useAuth();
  const { pathname } = useLocation();

  const navLink = (to, label) => (
    <Link to={to} className={`nav-link${pathname === to ? ' active' : ''}`}>{label}</Link>
  );

  return (
    <nav className="navbar">
      <div className="navbar-brand">
        <Link to="/dashboard">
          <span className="navbar-brand-short">EPM</span>
          <span className="navbar-brand-full">Enterprise Project Management</span>
        </Link>
      </div>

      <div className="navbar-links">
        {navLink('/dashboard', 'Dashboard')}
        {navLink('/projects',  'Projects')}
        {isAdmin && navLink('/admin', 'Admin')}
      </div>

      <div className="navbar-user">
        {currentUser && (
          <>
            <span className="username-badge">{currentUser.username}</span>
            {currentUser.role === 'Admin' && (
              <span className="role-badge">ADMIN</span>
            )}
          </>
        )}
        <button onClick={logout} className="btn btn-logout">Logout</button>
      </div>
    </nav>
  );
}
