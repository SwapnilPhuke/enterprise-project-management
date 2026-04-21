import { useState, useCallback, useEffect, useRef } from 'react';
import { projectsApi } from '../services/projectsApi';
import { parseApiError } from '../utils/parseApiError';

const PAGE_SIZE = 10;

function useDebounce(value, delay) {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const t = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(t);
  }, [value, delay]);
  return debounced;
}

export function useProjects() {
  const [projects,     setProjects]     = useState([]);
  const [totalPages,   setTotalPages]   = useState(1);
  const [page,         setPage]         = useState(1);
  const [searchQuery,  setSearchQuery]  = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const debouncedSearch                 = useDebounce(searchQuery, 400);

  const [loading, setLoading] = useState(false);
  const [error,   setError]   = useState('');
  const [success, setSuccess] = useState('');

  const [expandedProjectId, setExpandedProjectId] = useState(null);
  const [attachments,       setAttachments]        = useState({});
  const [uploading,         setUploading]          = useState(false);
  const fileInputRef = useRef(null);

  const loadProjects = useCallback(async (overridePage) => {
    setLoading(true);
    setError('');
    try {
      const params = {
        page:     overridePage ?? page,
        pageSize: PAGE_SIZE,
        ...(debouncedSearch.trim() && { search: debouncedSearch.trim() }),
        ...(statusFilter !== ''    && { status: statusFilter }),
      };
      const res = await projectsApi.getAll(params);
      setProjects(res.data.items);
      setTotalPages(res.data.totalPages);
    } catch (err) {
      if (err.response?.status !== 401)
        setError('Failed to load projects. Make sure the API is running.');
    } finally {
      setLoading(false);
    }
  }, [page, debouncedSearch, statusFilter]);

  useEffect(() => { loadProjects(); }, [loadProjects]);

  useEffect(() => {
    setPage(1);
    loadProjects(1);
  }, [debouncedSearch, statusFilter, loadProjects]);

  const createProject = async (data) => {
    setLoading(true); setError(''); setSuccess('');
    try {
      await projectsApi.create(data);
      setSuccess('Project created!');
      await loadProjects();
      setTimeout(() => setSuccess(''), 3000);
      return true;
    } catch (err) {
      setError(parseApiError(err, 'Failed to create project.'));
      return false;
    } finally {
      setLoading(false);
    }
  };

  const updateProject = async (id, data) => {
    setLoading(true); setError(''); setSuccess('');
    try {
      await projectsApi.update(id, data);
      setSuccess('Project updated!');
      await loadProjects();
      setTimeout(() => setSuccess(''), 3000);
      return true;
    } catch (err) {
      setError(parseApiError(err, 'Failed to update project.'));
      return false;
    } finally {
      setLoading(false);
    }
  };

  const deleteProject = async (id) => {
    if (!window.confirm('Delete this project?')) return;
    setLoading(true); setError(''); setSuccess('');
    try {
      await projectsApi.remove(id);
      setSuccess('Project deleted.');
      await loadProjects();
      setTimeout(() => setSuccess(''), 3000);
    } catch (err) {
      setError(parseApiError(err, 'Failed to delete project.'));
    } finally {
      setLoading(false);
    }
  };

  const loadAttachments = async (projectId) => {
    try {
      const res = await projectsApi.getAttachments(projectId);
      setAttachments(prev => ({ ...prev, [projectId]: res.data }));
    } catch (_) {}
  };

  const toggleAttachments = async (projectId) => {
    if (expandedProjectId === projectId) {
      setExpandedProjectId(null);
    } else {
      setExpandedProjectId(projectId);
      if (!attachments[projectId]) await loadAttachments(projectId);
    }
  };

  const uploadAttachment = async (projectId, file) => {
    if (!file) return;
    setUploading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);
      await projectsApi.uploadAttachment(projectId, formData);
      await loadAttachments(projectId);
    } catch (err) {
      setError(err.response?.data?.message || 'Upload failed.');
    } finally {
      setUploading(false);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  const deleteAttachment = async (projectId, attachmentId) => {
    try {
      await projectsApi.deleteAttachment(projectId, attachmentId);
      setAttachments(prev => ({
        ...prev,
        [projectId]: prev[projectId].filter(a => a.id !== attachmentId),
      }));
    } catch (_) {
      setError('Failed to delete attachment.');
    }
  };

  return {
    projects, totalPages, page, setPage,
    searchQuery, setSearchQuery,
    statusFilter, setStatusFilter,
    loading, error, success,
    createProject, updateProject, deleteProject,
    expandedProjectId, attachments, uploading, fileInputRef,
    uploadAttachment, deleteAttachment, toggleAttachments,
  };
}
