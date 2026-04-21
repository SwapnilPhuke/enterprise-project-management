import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useDashboard } from '../hooks/useDashboard';
import Navbar from '../components/Navbar';

const STATUS_COLORS = {
  notStarted: '#9e9e9e',
  inProgress: '#ff9800',
  testing:    '#2196f3',
  review:     '#4caf50',
  completed:  '#1b5e20',
};

export default function DashboardPage() {
  const { currentUser }       = useAuth();
  const { stats, loading, error } = useDashboard();

  const cards = stats ? [
    { label: 'Total Projects',  value: stats.total,      color: '#667eea', icon: '📁' },
    { label: 'Not Started',     value: stats.notStarted, color: STATUS_COLORS.notStarted, icon: '⏸️' },
    { label: 'In Progress',     value: stats.inProgress, color: STATUS_COLORS.inProgress, icon: '⚡' },
    { label: 'Testing',         value: stats.testing,    color: STATUS_COLORS.testing,    icon: '🧪' },
    { label: 'Review',          value: stats.review,     color: STATUS_COLORS.review,     icon: '👀' },
    { label: 'Completed',       value: stats.completed,  color: STATUS_COLORS.completed,  icon: '✅' },
  ] : [];

  return (
    <div>
      <Navbar />
      <div className="container">

        <div className="dashboard-welcome">
          <h1>Welcome back, {currentUser?.fullName || currentUser?.username || 'User'}</h1>
          <p>Here's an overview of your project portfolio.</p>
        </div>

        {error && <div className="alert alert-error">{error}</div>}

        {loading ? (
          <div className="loading">Loading dashboard…</div>
        ) : stats && (
          <>
            <div className="stats-grid">
              {cards.map(card => (
                <div
                  key={card.label}
                  className="stat-card"
                  style={{ borderTop: `4px solid ${card.color}` }}
                >
                  <span className="stat-icon">{card.icon}</span>
                  <div>
                    <div className="stat-value" style={{ color: card.color }}>
                      {card.value}
                    </div>
                    <div className="stat-label">{card.label}</div>
                  </div>
                </div>
              ))}
            </div>

            <div className="completion-card">
              <div className="completion-header">
                <h3>Overall Completion Rate</h3>
                <span className="completion-pct">{stats.completionRate}%</span>
              </div>
              <div className="completion-track">
                <div
                  className="completion-fill"
                  style={{
                    width: `${stats.completionRate}%`,
                    background: `linear-gradient(90deg, #667eea 0%, #1b5e20 100%)`,
                  }}
                />
              </div>
              <p className="completion-note">
                {stats.completed} of {stats.total} project{stats.total !== 1 ? 's' : ''} completed
              </p>
            </div>

            {stats.total > 0 && (
              <div className="breakdown-card">
                <h3>Status Breakdown</h3>
                {[
                  { label: 'Not Started',  value: stats.notStarted, color: STATUS_COLORS.notStarted },
                  { label: 'In Progress',  value: stats.inProgress, color: STATUS_COLORS.inProgress },
                  { label: 'Testing',      value: stats.testing,    color: STATUS_COLORS.testing    },
                  { label: 'Review',       value: stats.review,     color: STATUS_COLORS.review     },
                  { label: 'Completed',    value: stats.completed,  color: STATUS_COLORS.completed  },
                ].map(row => (
                  <div key={row.label} className="breakdown-row">
                    <span className="breakdown-label">{row.label}</span>
                    <div className="breakdown-track">
                      <div
                        className="breakdown-fill"
                        style={{
                          width:      `${(row.value / stats.total) * 100}%`,
                          background: row.color,
                        }}
                      />
                    </div>
                    <span className="breakdown-count">{row.value}</span>
                  </div>
                ))}
              </div>
            )}
          </>
        )}

        <div className="dashboard-cta">
          <Link to="/projects" className="btn btn-primary btn-large">
            📋 Manage Projects →
          </Link>
        </div>

      </div>
    </div>
  );
}
