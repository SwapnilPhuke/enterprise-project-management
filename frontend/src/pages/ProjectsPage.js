import React, { useState, useMemo } from 'react';
import Navbar from '../components/Navbar';
import { useProjects } from '../hooks/useProjects';
import { projectsApi } from '../services/projectsApi';

const STATUS_COLOR = { 0: '#9e9e9e', 25: '#ff9800', 50: '#2196f3', 75: '#4caf50', 100: '#1b5e20' };
const STATUS_LABEL = { 0: 'Not Started', 25: 'In Progress', 50: 'Testing', 75: 'Review', 100: 'Completed' };
const STATUS_OPTIONS = [0, 25, 50, 75, 100];

function formatBytes(bytes) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export default function ProjectsPage() {
  const {
    projects, totalPages, page, setPage,
    searchQuery, setSearchQuery,
    statusFilter, setStatusFilter,
    loading, error, success,
    createProject, updateProject, deleteProject,
    expandedProjectId, attachments, uploading, fileInputRef,
    uploadAttachment, deleteAttachment, toggleAttachments,
  } = useProjects();

  const [name,        setName]        = useState('');
  const [description, setDescription] = useState('');
  const [status,      setStatus]      = useState(0);
  const [editingId,   setEditingId]   = useState(null);

  const nameError = name.trim().length > 0 && name.trim().length < 3
    ? 'Name must be at least 3 characters' : '';
  const descError = description.trim().length > 0 && description.trim().length < 10
    ? 'Description must be at least 10 characters' : '';
  const formValid = useMemo(
    () => name.trim().length >= 3 && description.trim().length >= 10,
    [name, description]
  );

  const handleSubmit = async () => {
    if (!formValid) return;
    const payload = { name: name.trim(), description: description.trim(), status };
    const ok = editingId
      ? await updateProject(editingId, payload)
      : await createProject(payload);
    if (ok) {
      setEditingId(null);
      setName(''); setDescription(''); setStatus(0);
    }
  };

  const startEdit = (project) => {
    setEditingId(project.id);
    setName(project.name);
    setDescription(project.description);
    setStatus(project.status);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const cancelEdit = () => {
    setEditingId(null);
    setName(''); setDescription(''); setStatus(0);
  };

  return (
    <div>
      <Navbar />
      <div className="container">

        {error   && <div className="alert alert-error">{error}</div>}
        {success && <div className="alert alert-success">{success}</div>}

        <div className="form-section">
          <h2>{editingId ? '✏️ Edit Project' : '➕ New Project'}</h2>

          <div className="form-group">
            <label htmlFor="proj-name">Project Name</label>
            <input
              id="proj-name" type="text"
              placeholder="Enter project name (min 3 characters)"
              value={name} onChange={e => setName(e.target.value)}
              maxLength={200} disabled={loading}
            />
            <span className="char-count">{name.length}/200</span>
            {nameError && <span className="field-error">{nameError}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="proj-desc">Description</label>
            <textarea
              id="proj-desc" rows={3}
              placeholder="Enter description (min 10 characters)"
              value={description} onChange={e => setDescription(e.target.value)}
              maxLength={1000} disabled={loading}
            />
            <span className="char-count">{description.length}/1000</span>
            {descError && <span className="field-error">{descError}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="proj-status">Status</label>
            <select
              id="proj-status"
              value={status}
              onChange={e => setStatus(Number(e.target.value))}
              disabled={loading}
            >
              {STATUS_OPTIONS.map(s => <option key={s} value={s}>{STATUS_LABEL[s]}</option>)}
            </select>
          </div>

          <div className="button-group">
            <button onClick={handleSubmit} disabled={!formValid || loading} className="btn btn-primary">
              {loading ? 'Processing…' : editingId ? 'Update Project' : 'Add Project'}
            </button>
            {editingId && (
              <button onClick={cancelEdit} disabled={loading} className="btn btn-secondary">Cancel</button>
            )}
          </div>
        </div>

        <div className="filter-bar">
          <input
            type="text"
            className="search-input"
            placeholder="🔍 Search projects…"
            value={searchQuery}
            onChange={e => setSearchQuery(e.target.value)}
          />
          <select
            className="filter-select"
            value={statusFilter}
            onChange={e => setStatusFilter(e.target.value)}
          >
            <option value="">All Statuses</option>
            {STATUS_OPTIONS.map(s => <option key={s} value={s}>{STATUS_LABEL[s]}</option>)}
          </select>
        </div>

        {loading && projects.length === 0 ? (
          <div className="loading">Loading projects…</div>
        ) : projects.length === 0 ? (
          <div className="no-projects">
            <p>{searchQuery || statusFilter
              ? 'No projects match your filters.'
              : 'No projects yet. Create your first project above!'}
            </p>
          </div>
        ) : (
          <>
            <div className="projects-grid">
              {projects.map(project => (
                <div key={project.id} className="project-card">
                  <div className="card-header">
                    <h3>{project.name}</h3>
                    <span className="status-badge" style={{ backgroundColor: STATUS_COLOR[project.status] }}>
                      {STATUS_LABEL[project.status]}
                    </span>
                  </div>

                  <p className="description">{project.description}</p>

                  <div className="card-meta">
                    <small>Created: {new Date(project.createdAt).toLocaleDateString()}</small>
                    <small>Updated: {new Date(project.updatedAt).toLocaleDateString()}</small>
                  </div>

                  <div className="progress-bar">
                    <div
                      className="progress-fill"
                      style={{ width: `${project.status}%`, backgroundColor: STATUS_COLOR[project.status] }}
                    />
                  </div>

                  <div className="card-actions">
                    <button onClick={() => startEdit(project)}      disabled={loading} className="btn btn-edit">Edit</button>
                    <button onClick={() => deleteProject(project.id)} disabled={loading} className="btn btn-delete">Delete</button>
                    <button onClick={() => toggleAttachments(project.id)} className="btn btn-attach">
                      📎 {expandedProjectId === project.id ? 'Hide' : 'Files'}
                    </button>
                  </div>

                  {expandedProjectId === project.id && (
                    <div className="attachment-panel">
                      <h4>Attachments</h4>
                      <div className="upload-row">
                        <label className="btn btn-secondary btn-sm">
                          {uploading ? 'Uploading…' : '+ Upload File'}
                          <input
                            ref={fileInputRef}
                            type="file"
                            style={{ display: 'none' }}
                            disabled={uploading}
                            onChange={e => uploadAttachment(project.id, e.target.files[0])}
                          />
                        </label>
                        <span className="upload-hint">pdf / doc / xlsx / png / jpg / zip · max 10 MB</span>
                      </div>
                      {(attachments[project.id] ?? []).length === 0 ? (
                        <p className="no-attachments">No files yet.</p>
                      ) : (
                        <ul className="attachment-list">
                          {attachments[project.id].map(a => (
                            <li key={a.id} className="attachment-item">
                              <span className="attachment-name" title={a.fileName}>{a.fileName}</span>
                              <span className="attachment-size">{formatBytes(a.fileSize)}</span>
                              <a
                                href={projectsApi.getDownloadUrl(project.id, a.id)}
                                className="btn btn-sm btn-edit"
                                download={a.fileName}
                              >
                                ⬇ Download
                              </a>
                              <button
                                onClick={() => deleteAttachment(project.id, a.id)}
                                className="btn btn-sm btn-delete"
                              >
                                🗑
                              </button>
                            </li>
                          ))}
                        </ul>
                      )}
                    </div>
                  )}
                </div>
              ))}
            </div>

            {totalPages > 1 && (
              <div className="pagination">
                <button
                  className="btn btn-page"
                  onClick={() => setPage(p => p - 1)}
                  disabled={page <= 1 || loading}
                >
                  ← Prev
                </button>
                <span className="page-info">Page {page} of {totalPages}</span>
                <button
                  className="btn btn-page"
                  onClick={() => setPage(p => p + 1)}
                  disabled={page >= totalPages || loading}
                >
                  Next →
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}