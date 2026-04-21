import axios from 'axios';

const BASE = `${process.env.REACT_APP_API_URL || 'http://localhost:5000'}/api/v1/projects`;

export const projectsApi = {
  getAll:   (params)       => axios.get(BASE, { params }),
  getById:  (id)           => axios.get(`${BASE}/${id}`),
  getStats: ()             => axios.get(`${BASE}/stats`),
  create:   (data)         => axios.post(BASE, data),
  update:   (id, data)     => axios.put(`${BASE}/${id}`, data),
  remove:   (id)           => axios.delete(`${BASE}/${id}`),

  getAttachments:   (projectId)           => axios.get(`${BASE}/${projectId}/attachments`),
  uploadAttachment: (projectId, formData) => axios.post(`${BASE}/${projectId}/attachments`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  }),
  deleteAttachment: (projectId, attachId) => axios.delete(`${BASE}/${projectId}/attachments/${attachId}`),
  getDownloadUrl:   (projectId, attachId) => `${BASE}/${projectId}/attachments/${attachId}/download`,
};
