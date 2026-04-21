import axios from 'axios';

const BASE = `${process.env.REACT_APP_API_URL || 'http://localhost:5000'}/api/v1/auth`;

export const authApi = {
  login:   (username, password) => axios.post(`${BASE}/login`,    { username, password }),
  register: (data)              => axios.post(`${BASE}/register`, data),
  getMe:   ()                   => axios.get(`${BASE}/me`),
  refresh: (refreshToken)       => axios.post(`${BASE}/refresh`,  { refreshToken }),
  logout:  ()                   => axios.post(`${BASE}/logout`),
};
