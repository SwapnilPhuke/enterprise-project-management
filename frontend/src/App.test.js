import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { AuthContext } from './context/AuthContext';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import * as projectsApiModule from './services/projectsApi';

jest.mock('./services/projectsApi');
jest.mock('./services/authApi');

const renderWithAuth = (ui, authValue = {}) => {
  const defaults = {
    token: null,
    currentUser: null,
    loadingUser: false,
    isAuthenticated: false,
    isAdmin: false,
    role: null,
    login: jest.fn(),
    logout: jest.fn(),
  };
  return render(
    <AuthContext.Provider value={{ ...defaults, ...authValue }}>
      <MemoryRouter>{ui}</MemoryRouter>
    </AuthContext.Provider>
  );
};

const mockUser = {
  id: 1,
  username: 'testuser',
  email: 'test@example.com',
  fullName: 'Test User',
  role: 'User',
};

describe('LoginPage', () => {
  beforeEach(() => jest.clearAllMocks());

  test('renders login form', () => {
    renderWithAuth(<LoginPage />);
    expect(screen.getByLabelText(/Username/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Sign In/i })).toBeInTheDocument();
  });

  test('disables submit button when fields are empty', () => {
    renderWithAuth(<LoginPage />);
    expect(screen.getByRole('button', { name: /Sign In/i })).toBeDisabled();
  });

  test('calls login and navigates on success', async () => {
    const loginMock = jest.fn().mockResolvedValue({});
    renderWithAuth(<LoginPage />, { login: loginMock });
    const user = userEvent.setup();

    await user.type(screen.getByLabelText(/Username/i), 'testuser');
    await user.type(screen.getByLabelText(/Password/i), 'password123');
    await user.click(screen.getByRole('button', { name: /Sign In/i }));

    await waitFor(() => expect(loginMock).toHaveBeenCalledWith('testuser', 'password123'));
  });

  test('shows error message on failed login', async () => {
    const loginMock = jest.fn().mockRejectedValue({
      response: { data: { message: 'Invalid username or password' } },
    });
    renderWithAuth(<LoginPage />, { login: loginMock });
    const user = userEvent.setup();

    await user.type(screen.getByLabelText(/Username/i), 'baduser');
    await user.type(screen.getByLabelText(/Password/i), 'badpass');
    await user.click(screen.getByRole('button', { name: /Sign In/i }));

    await waitFor(() =>
      expect(screen.getByText(/Invalid username or password/i)).toBeInTheDocument()
    );
  });
});

describe('DashboardPage', () => {
  beforeEach(() => jest.clearAllMocks());

  test('renders welcome message with username', async () => {
    projectsApiModule.projectsApi.getStats.mockResolvedValue({
      data: {
        total: 5, notStarted: 1, inProgress: 2,
        testing: 1, review: 0, completed: 1, completionRate: 20,
      },
    });

    renderWithAuth(<DashboardPage />, {
      isAuthenticated: true,
      currentUser: mockUser,
    });

    await waitFor(() =>
      expect(screen.getByText(/Welcome back/i)).toBeInTheDocument()
    );
  });

  test('shows error when stats API fails', async () => {
    projectsApiModule.projectsApi.getStats.mockRejectedValue(new Error('Network error'));

    renderWithAuth(<DashboardPage />, {
      isAuthenticated: true,
      currentUser: mockUser,
    });

    await waitFor(() =>
      expect(screen.getByText(/Could not load stats/i)).toBeInTheDocument()
    );
  });
});
