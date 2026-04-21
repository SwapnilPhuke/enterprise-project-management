import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import axios from 'axios';
import { authApi } from '../services/authApi';

const TOKEN_KEY   = 'jwtToken';
const REFRESH_KEY = 'refreshToken';

export const AuthContext = createContext(null);
export const useAuth = () => useContext(AuthContext);

export function AuthProvider({ children }) {
  const [token,       setToken]       = useState(() => localStorage.getItem(TOKEN_KEY));
  const [currentUser, setCurrentUser] = useState(null);
  const [loadingUser, setLoadingUser] = useState(true);

  const applyTokens = useCallback((access, refresh) => {
    localStorage.setItem(TOKEN_KEY, access);
    if (refresh) localStorage.setItem(REFRESH_KEY, refresh);
    axios.defaults.headers.common['Authorization'] = `Bearer ${access}`;
    setToken(access);
  }, []);

  const logout = useCallback(async () => {
    try { await authApi.logout(); } catch (_) {}
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_KEY);
    delete axios.defaults.headers.common['Authorization'];
    setToken(null);
    setCurrentUser(null);
  }, []);

  useEffect(() => {
    if (token) {
      axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    } else {
      delete axios.defaults.headers.common['Authorization'];
    }
  }, [token]);

  useEffect(() => {
    const id = axios.interceptors.response.use(
      res => res,
      async err => {
        const original = err.config;
        if (err.response?.status === 401 && !original._retry) {
          original._retry = true;
          const stored = localStorage.getItem(REFRESH_KEY);
          if (stored) {
            try {
              const res = await authApi.refresh(stored);
              applyTokens(res.data.token, res.data.refreshToken);
              original.headers['Authorization'] = `Bearer ${res.data.token}`;
              return axios(original);
            } catch (_) {}
          }
          logout();
        }
        return Promise.reject(err);
      }
    );
    return () => axios.interceptors.response.eject(id);
  }, [logout, applyTokens]);

  useEffect(() => {
    if (!token) { setLoadingUser(false); return; }
    setLoadingUser(true);
    authApi.getMe()
      .then(res => setCurrentUser(res.data))
      .catch(() => logout())
      .finally(() => setLoadingUser(false));
  }, [token, logout]);

  const login = async (username, password) => {
    const res = await authApi.login(username, password);
    applyTokens(res.data.token, res.data.refreshToken);
    return res.data;
  };

  return (
    <AuthContext.Provider value={{
      token, currentUser, loadingUser,
      isAuthenticated: !!token,
      isAdmin:         currentUser?.role === 'Admin',
      role:            currentUser?.role ?? null,
      login, logout,
    }}>
      {children}
    </AuthContext.Provider>
  );
}