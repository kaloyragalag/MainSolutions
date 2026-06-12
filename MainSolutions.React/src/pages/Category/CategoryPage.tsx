import React, { useEffect, useState } from 'react';
import BasePage from '../../components/Layout/BasePage';
import { categoryService } from '../../services/categoryService';
import { Category, CategoryFormData } from '../../types/category';
import './CategoryPage.css';

const EMPTY_FORM: CategoryFormData = { name: '', description: '' };

const CategoryIcon = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
    <rect x="3" y="3" width="7" height="7" /><rect x="14" y="3" width="7" height="7" />
    <rect x="14" y="14" width="7" height="7" /><rect x="3" y="14" width="7" height="7" />
  </svg>
);

const CategoryIconLg = () => (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
    <rect x="3" y="3" width="7" height="7" /><rect x="14" y="3" width="7" height="7" />
    <rect x="14" y="14" width="7" height="7" /><rect x="3" y="14" width="7" height="7" />
  </svg>
);

const PlusIcon = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
    <line x1="12" y1="5" x2="12" y2="19" /><line x1="5" y1="12" x2="19" y2="12" />
  </svg>
);

const EditIcon = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
    <path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7" />
    <path d="M18.5 2.5a2.121 2.121 0 013 3L12 15l-4 1 1-4 9.5-9.5z" />
  </svg>
);

const TrashIcon = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
    <polyline points="3 6 5 6 21 6" />
    <path d="M19 6l-1 14a2 2 0 01-2 2H8a2 2 0 01-2-2L5 6" />
    <path d="M10 11v6M14 11v6M9 6V4a1 1 0 011-1h4a1 1 0 011 1v2" />
  </svg>
);

const EmptyIcon = () => (
  <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" aria-hidden="true">
    <rect x="3" y="3" width="7" height="7" /><rect x="14" y="3" width="7" height="7" />
    <rect x="14" y="14" width="7" height="7" /><rect x="3" y="14" width="7" height="7" />
  </svg>
);

const ChevronLeftIcon = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
    <polyline points="15 18 9 12 15 6" />
  </svg>
);

const ChevronRightIcon = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
    <polyline points="9 18 15 12 9 6" />
  </svg>
);

type ModalMode = 'create' | 'edit' | null;

const CategoryPage: React.FC = () => {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');

  const [selectedId, setSelectedId] = useState<number | null>(null);

  const [modalMode, setModalMode] = useState<ModalMode>(null);
  const [formData, setFormData] = useState<CategoryFormData>(EMPTY_FORM);
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const [deleteTarget, setDeleteTarget] = useState<Category | null>(null);
  const [deleting, setDeleting] = useState(false);

  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const fetchCategories = async (targetPage: number = page) => {
    setLoading(true);
    setError(null);
    try {
      const result = await categoryService.getAll(targetPage, pageSize);
      const list = result.items;
      setCategories(list);
      setPage(result.page);
      setTotalPages(Math.max(result.totalPages, 1));
      setTotalCount(result.totalCount);
      if (list.length > 0) {
        setSelectedId(prev => (prev && list.some(c => c.id === prev) ? prev : list[0].id));
      } else {
        setSelectedId(null);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load categories.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCategories(page);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page]);

  const goToPage = (target: number) => {
    if (target < 1 || target > totalPages || target === page) return;
    setPage(target);
  };

  const filteredCategories = Array.isArray(categories)
    ? categories.filter(c => c.name.toLowerCase().includes(search.toLowerCase()))
    : [];

  const selectedCategory = categories.find(c => c.id === selectedId) ?? null;

  const openCreateModal = () => {
    setFormData(EMPTY_FORM);
    setFormError(null);
    setModalMode('create');
  };

  const openEditModal = (category: Category) => {
    setFormData({ name: category.name, description: category.description });
    setFormError(null);
    setModalMode('edit');
  };

  const closeModal = () => {
    if (saving) return;
    setModalMode(null);
    setFormData(EMPTY_FORM);
    setFormError(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.name.trim()) {
      setFormError('Name is required.');
      return;
    }

    setSaving(true);
    setFormError(null);
    try {
      if (modalMode === 'create') {
        const created = await categoryService.create(formData);
        await fetchCategories(page);
        setSelectedId(created.id);
      } else if (modalMode === 'edit' && selectedCategory) {
        await categoryService.update(selectedCategory.id, formData);
        await fetchCategories(page);
      }
      setModalMode(null);
      setFormData(EMPTY_FORM);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Failed to save category.');
    } finally {
      setSaving(false);
    }
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await categoryService.delete(deleteTarget.id);
      setDeleteTarget(null);
      // If this was the last item on the page, step back a page (but not below 1)
      const isLastItemOnPage = categories.length === 1 && page > 1;
      await fetchCategories(isLastItemOnPage ? page - 1 : page);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete category.');
      setDeleteTarget(null);
    } finally {
      setDeleting(false);
    }
  };

  return (
    <BasePage>
      <div className="ms-page-header">
        <div className="ms-page-header__info">
          <h1 className="ms-page-header__title">Categories</h1>
          <p className="ms-page-header__subtitle">Manage product categories</p>
        </div>
        <button className="ms-btn ms-btn--primary" onClick={openCreateModal}>
          <PlusIcon />
          Create category
        </button>
      </div>

      {error && (
        <div className="ms-alert ms-alert--danger" style={{ marginBottom: 'var(--space-4)' }}>
          {error}
        </div>
      )}

      <div className="cat-page">
        {/* List panel */}
        <div className="cat-list">
          <div className="cat-list__header">
            <span className="ms-h4">All categories ({totalCount})</span>
          </div>
          <div className="cat-list__search">
            <input
              className="ms-field__input"
              placeholder="Search categories..."
              value={search}
              onChange={e => setSearch(e.target.value)}
            />
          </div>
          <div className="cat-list__items">
            {loading ? (
              <div className="cat-list__empty">Loading categories...</div>
            ) : filteredCategories.length === 0 ? (
              <div className="cat-list__empty">No categories found.</div>
            ) : (
              filteredCategories.map(category => (
                <button
                  key={category.id}
                  className={`cat-list__item ${selectedId === category.id ? 'cat-list__item--active' : ''}`}
                  onClick={() => setSelectedId(category.id)}
                >
                  <span className="cat-list__item-icon">
                    <CategoryIcon />
                  </span>
                  <span className="cat-list__item-text">
                    <span className="cat-list__item-name">{category.name}</span>
                    <span className="cat-list__item-desc">
                      {category.description || 'No description'}
                    </span>
                  </span>
                </button>
              ))
            )}
          </div>

          {!loading && totalPages > 1 && (
            <div className="cat-list__pagination">
              <button
                className="ms-btn ms-btn--ghost ms-btn--sm"
                onClick={() => goToPage(page - 1)}
                disabled={page <= 1}
                aria-label="Previous page"
              >
                <ChevronLeftIcon />
              </button>
              <span className="cat-list__pagination-info">
                Page {page} of {totalPages}
              </span>
              <button
                className="ms-btn ms-btn--ghost ms-btn--sm"
                onClick={() => goToPage(page + 1)}
                disabled={page >= totalPages}
                aria-label="Next page"
              >
                <ChevronRightIcon />
              </button>
            </div>
          )}
        </div>

        {/* Detail panel */}
        <div className="cat-detail">
          {selectedCategory ? (
            <>
              <div className="cat-detail__breadcrumb">Categories / {selectedCategory.name}</div>
              <div className="cat-detail__title">
                <span className="cat-detail__icon">
                  <CategoryIconLg />
                </span>
                <h2 className="ms-h2">{selectedCategory.name}</h2>
              </div>

              <div className="cat-detail__actions">
                <button className="ms-btn ms-btn--secondary" onClick={() => openEditModal(selectedCategory)}>
                  <EditIcon />
                  Edit
                </button>
                <button className="ms-btn ms-btn--danger" onClick={() => setDeleteTarget(selectedCategory)}>
                  <TrashIcon />
                  Delete
                </button>
              </div>

              <div className="cat-detail__section">
                <h3 className="ms-h3" style={{ marginBottom: 'var(--space-2)' }}>Details</h3>
                <div className="cat-detail__row">
                  <span className="cat-detail__row-label">Name</span>
                  <span className="cat-detail__row-value">{selectedCategory.name}</span>
                </div>
                <div className="cat-detail__row">
                  <span className="cat-detail__row-label">Description</span>
                  <span className="cat-detail__row-value">
                    {selectedCategory.description || <span className="ms-text-muted">No description provided</span>}
                  </span>
                </div>
                {selectedCategory.createdAt && (
                  <div className="cat-detail__row">
                    <span className="cat-detail__row-label">Created</span>
                    <span className="cat-detail__row-value">
                      {new Date(selectedCategory.createdAt).toLocaleString()}
                    </span>
                  </div>
                )}
                {selectedCategory.updatedAt && (
                  <div className="cat-detail__row">
                    <span className="cat-detail__row-label">Updated</span>
                    <span className="cat-detail__row-value">
                      {new Date(selectedCategory.updatedAt).toLocaleString()}
                    </span>
                  </div>
                )}
              </div>
            </>
          ) : (
            <div className="cat-detail__empty">
              <EmptyIcon />
              <p>{loading ? 'Loading...' : 'Select a category to view details, or create a new one.'}</p>
              {!loading && (
                <button className="ms-btn ms-btn--primary" onClick={openCreateModal}>
                  <PlusIcon />
                  Create category
                </button>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Create / Edit modal */}
      {modalMode && (
        <div className="cat-modal-overlay" onClick={closeModal}>
          <div className="cat-modal" onClick={e => e.stopPropagation()}>
            <h2 className="ms-h2 cat-modal__title">
              {modalMode === 'create' ? 'Create category' : 'Edit category'}
            </h2>

            <form className="ms-form" onSubmit={handleSubmit}>
              {formError && <div className="ms-alert ms-alert--danger">{formError}</div>}

              <div className="ms-field">
                <label className="ms-field__label" htmlFor="cat-name">Name</label>
                <input
                  id="cat-name"
                  className="ms-field__input"
                  value={formData.name}
                  onChange={e => setFormData(prev => ({ ...prev, name: e.target.value }))}
                  placeholder="e.g. Beverages"
                  autoFocus
                />
              </div>

              <div className="ms-field">
                <label className="ms-field__label" htmlFor="cat-description">Description</label>
                <input
                  id="cat-description"
                  className="ms-field__input"
                  value={formData.description}
                  onChange={e => setFormData(prev => ({ ...prev, description: e.target.value }))}
                  placeholder="Optional description"
                />
              </div>

              <div className="cat-modal__actions">
                <button type="button" className="ms-btn ms-btn--secondary" onClick={closeModal} disabled={saving}>
                  Cancel
                </button>
                <button type="submit" className={`ms-btn ms-btn--primary ${saving ? 'ms-btn--loading' : ''}`} disabled={saving}>
                  {saving && <span className="ms-btn__spinner" />}
                  {modalMode === 'create' ? 'Create' : 'Save changes'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Delete confirmation modal */}
      {deleteTarget && (
        <div className="cat-modal-overlay" onClick={() => !deleting && setDeleteTarget(null)}>
          <div className="cat-modal" onClick={e => e.stopPropagation()}>
            <h2 className="ms-h2 cat-modal__title">Delete category</h2>
            <p>
              Are you sure you want to delete <strong>{deleteTarget.name}</strong>? This action
              cannot be undone.
            </p>
            <div className="cat-modal__actions">
              <button
                className="ms-btn ms-btn--secondary"
                onClick={() => setDeleteTarget(null)}
                disabled={deleting}
              >
                Cancel
              </button>
              <button
                className={`ms-btn ms-btn--danger ${deleting ? 'ms-btn--loading' : ''}`}
                onClick={confirmDelete}
                disabled={deleting}
              >
                {deleting && <span className="ms-btn__spinner" />}
                Delete
              </button>
            </div>
          </div>
        </div>
      )}
    </BasePage>
  );
};

export default CategoryPage;