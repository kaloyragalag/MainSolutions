import React, { useEffect, useState } from 'react';
import BasePage from '../Layout/BasePage';
import { CrudService } from '../../types/crud';
import './CrudPage.css';

interface EntityBase {
  id: number;
}

export interface CrudField<TForm> {
  /** Key into the form data object */
  key: keyof TForm;
  label: string;
  placeholder?: string;
  required?: boolean;
  type?: 'text' | 'textarea';
}

export interface CrudDetailRow<T> {
  label: string;
  /** Return null/undefined to hide this row for a given item */
  render: (item: T) => React.ReactNode;
}

export interface CrudPageProps<T extends EntityBase, TForm extends Record<string, any>> {
  /** Page title, e.g. "Categories" */
  title: string;
  /** Page subtitle shown under the title */
  subtitle?: string;
  /** Singular lowercase entity name, e.g. "category" */
  entityName: string;
  /** Plural display name, e.g. "categories" (defaults to entityName + 's') */
  entityNamePlural?: string;
  /** CRUD service implementing getAll/create/update/delete */
  service: CrudService<T, TForm>;
  /** Default values for the create/edit form */
  emptyFormData: TForm;
  /** Items per page (default 10) */
  pageSize?: number;
  /** Returns the primary display title for a list item / detail header */
  getItemTitle: (item: T) => string;
  /** Returns secondary text shown under the title in the list */
  getItemSubtitle?: (item: T) => React.ReactNode;
  /** Fields rendered in the create/edit modal */
  formFields: CrudField<TForm>[];
  /** Rows rendered in the detail panel's "Details" section */
  detailRows: CrudDetailRow<T>[];
  /** Map an entity to form data when opening the edit modal (defaults to copying matching keys) */
  toFormData?: (item: T) => TForm;
  /** Custom icon used for list items / detail header (defaults to a generic grid icon) */
  icon?: React.ReactNode;
}

const DefaultIcon = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
    <rect x="3" y="3" width="7" height="7" /><rect x="14" y="3" width="7" height="7" />
    <rect x="14" y="14" width="7" height="7" /><rect x="3" y="14" width="7" height="7" />
  </svg>
);

const DefaultIconLg = () => (
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

function CrudPage<T extends EntityBase, TForm extends Record<string, any>>({
  title,
  subtitle,
  entityName,
  entityNamePlural,
  service,
  emptyFormData,
  pageSize = 10,
  getItemTitle,
  getItemSubtitle,
  formFields,
  detailRows,
  toFormData,
  icon,
}: CrudPageProps<T, TForm>) {
  const pluralName = entityNamePlural ?? `${entityName}s`;

  const [items, setItems] = useState<T[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');

  const [selectedId, setSelectedId] = useState<number | null>(null);

  const [modalMode, setModalMode] = useState<ModalMode>(null);
  const [formData, setFormData] = useState<TForm>(emptyFormData);
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const [deleteTarget, setDeleteTarget] = useState<T | null>(null);
  const [deleting, setDeleting] = useState(false);

  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const fetchItems = async (targetPage: number = page) => {
    setLoading(true);
    setError(null);
    try {
      const result = await service.getAll(targetPage, pageSize);
      const list = result.items;
      setItems(list);
      setPage(result.page);
      setTotalPages(Math.max(result.totalPages, 1));
      setTotalCount(result.totalCount);
      if (list.length > 0) {
        setSelectedId(prev => (prev && list.some(i => i.id === prev) ? prev : list[0].id));
      } else {
        setSelectedId(null);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : `Failed to load ${pluralName}.`);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchItems(page);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page]);

  const goToPage = (target: number) => {
    if (target < 1 || target > totalPages || target === page) return;
    setPage(target);
  };

  const filteredItems = Array.isArray(items)
    ? items.filter(item => getItemTitle(item).toLowerCase().includes(search.toLowerCase()))
    : [];

  const selectedItem = items.find(i => i.id === selectedId) ?? null;

  const buildFormData = (item: T): TForm => {
    if (toFormData) return toFormData(item);
    const data: Record<string, any> = {};
    formFields.forEach(field => {
      data[field.key as string] = (item as any)[field.key];
    });
    return data as TForm;
  };

  const openCreateModal = () => {
    setFormData(emptyFormData);
    setFormError(null);
    setModalMode('create');
  };

  const openEditModal = (item: T) => {
    setFormData(buildFormData(item));
    setFormError(null);
    setModalMode('edit');
  };

  const closeModal = () => {
    if (saving) return;
    setModalMode(null);
    setFormData(emptyFormData);
    setFormError(null);
  };

  const handleFieldChange = (key: keyof TForm, value: string) => {
    setFormData(prev => ({ ...prev, [key]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const missingRequired = formFields.find(
      f => f.required && !String(formData[f.key] ?? '').trim()
    );
    if (missingRequired) {
      setFormError(`${missingRequired.label} is required.`);
      return;
    }

    setSaving(true);
    setFormError(null);
    try {
      if (modalMode === 'create') {
        const created = await service.create(formData);
        await fetchItems(page);
        setSelectedId(created.id);
      } else if (modalMode === 'edit' && selectedItem) {
        await service.update(selectedItem.id, formData);
        await fetchItems(page);
      }
      setModalMode(null);
      setFormData(emptyFormData);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : `Failed to save ${entityName}.`);
    } finally {
      setSaving(false);
    }
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await service.delete(deleteTarget.id);
      setDeleteTarget(null);
      // If this was the last item on the page, step back a page (but not below 1)
      const isLastItemOnPage = items.length === 1 && page > 1;
      await fetchItems(isLastItemOnPage ? page - 1 : page);
    } catch (err) {
      setError(err instanceof Error ? err.message : `Failed to delete ${entityName}.`);
      setDeleteTarget(null);
    } finally {
      setDeleting(false);
    }
  };

  const listIcon = icon ?? <DefaultIcon />;
  const detailIcon = icon ?? <DefaultIconLg />;
  const capitalizedEntity = entityName.charAt(0).toUpperCase() + entityName.slice(1);
  const capitalizedPlural = pluralName.charAt(0).toUpperCase() + pluralName.slice(1);

  return (
    <BasePage>
      <div className="ms-page-header">
        <div className="ms-page-header__info">
          <h1 className="ms-page-header__title">{title}</h1>
          {subtitle && <p className="ms-page-header__subtitle">{subtitle}</p>}
        </div>
        <button className="ms-btn ms-btn--primary" onClick={openCreateModal}>
          <PlusIcon />
          Create {entityName}
        </button>
      </div>

      {error && (
        <div className="ms-alert ms-alert--danger" style={{ marginBottom: 'var(--space-4)' }}>
          {error}
        </div>
      )}

      <div className="crud-page">
        {/* List panel */}
        <div className="crud-list">
          <div className="crud-list__header">
            <span className="ms-h4">All {pluralName} ({totalCount})</span>
          </div>
          <div className="crud-list__search">
            <input
              className="ms-field__input"
              placeholder={`Search ${pluralName}...`}
              value={search}
              onChange={e => setSearch(e.target.value)}
            />
          </div>
          <div className="crud-list__items">
            {loading ? (
              <div className="crud-list__empty">Loading {pluralName}...</div>
            ) : filteredItems.length === 0 ? (
              <div className="crud-list__empty">No {pluralName} found.</div>
            ) : (
              filteredItems.map(item => (
                <button
                  key={item.id}
                  className={`crud-list__item ${selectedId === item.id ? 'crud-list__item--active' : ''}`}
                  onClick={() => setSelectedId(item.id)}
                >
                  <span className="crud-list__item-icon">{listIcon}</span>
                  <span className="crud-list__item-text">
                    <span className="crud-list__item-name">{getItemTitle(item)}</span>
                    {getItemSubtitle && (
                      <span className="crud-list__item-desc">{getItemSubtitle(item)}</span>
                    )}
                  </span>
                </button>
              ))
            )}
          </div>

          {!loading && totalPages > 1 && (
            <div className="crud-list__pagination">
              <button
                className="ms-btn ms-btn--ghost ms-btn--sm"
                onClick={() => goToPage(page - 1)}
                disabled={page <= 1}
                aria-label="Previous page"
              >
                <ChevronLeftIcon />
              </button>
              <span className="crud-list__pagination-info">
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
        <div className="crud-detail">
          {selectedItem ? (
            <>
              <div className="crud-detail__breadcrumb">
                {capitalizedPlural} / {getItemTitle(selectedItem)}
              </div>
              <div className="crud-detail__title">
                <span className="crud-detail__icon">{detailIcon}</span>
                <h2 className="ms-h2">{getItemTitle(selectedItem)}</h2>
              </div>

              <div className="crud-detail__actions">
                <button className="ms-btn ms-btn--secondary" onClick={() => openEditModal(selectedItem)}>
                  <EditIcon />
                  Edit
                </button>
                <button className="ms-btn ms-btn--danger" onClick={() => setDeleteTarget(selectedItem)}>
                  <TrashIcon />
                  Delete
                </button>
              </div>

              <div className="crud-detail__section">
                <h3 className="ms-h3" style={{ marginBottom: 'var(--space-2)' }}>Details</h3>
                {detailRows.map(row => {
                  const value = row.render(selectedItem);
                  if (value === null || value === undefined || value === '') return null;
                  return (
                    <div className="crud-detail__row" key={row.label}>
                      <span className="crud-detail__row-label">{row.label}</span>
                      <span className="crud-detail__row-value">{value}</span>
                    </div>
                  );
                })}
              </div>
            </>
          ) : (
            <div className="crud-detail__empty">
              <EmptyIcon />
              <p>
                {loading
                  ? 'Loading...'
                  : `Select a ${entityName} to view details, or create a new one.`}
              </p>
              {!loading && (
                <button className="ms-btn ms-btn--primary" onClick={openCreateModal}>
                  <PlusIcon />
                  Create {entityName}
                </button>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Create / Edit modal */}
      {modalMode && (
        <div className="crud-modal-overlay" onClick={closeModal}>
          <div className="crud-modal" onClick={e => e.stopPropagation()}>
            <h2 className="ms-h2 crud-modal__title">
              {modalMode === 'create' ? `Create ${entityName}` : `Edit ${entityName}`}
            </h2>

            <form className="ms-form" onSubmit={handleSubmit}>
              {formError && <div className="ms-alert ms-alert--danger">{formError}</div>}

              {formFields.map((field, index) => {
                const fieldId = `crud-field-${field.key as string}`;
                const value = (formData[field.key] ?? '') as string;
                return (
                  <div className="ms-field" key={field.key as string}>
                    <label className="ms-field__label" htmlFor={fieldId}>{field.label}</label>
                    {field.type === 'textarea' ? (
                      <textarea
                        id={fieldId}
                        className="ms-field__input crud-field__textarea"
                        value={value}
                        onChange={e => handleFieldChange(field.key, e.target.value)}
                        placeholder={field.placeholder}
                        autoFocus={index === 0}
                      />
                    ) : (
                      <input
                        id={fieldId}
                        className="ms-field__input"
                        value={value}
                        onChange={e => handleFieldChange(field.key, e.target.value)}
                        placeholder={field.placeholder}
                        autoFocus={index === 0}
                      />
                    )}
                  </div>
                );
              })}

              <div className="crud-modal__actions">
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
        <div className="crud-modal-overlay" onClick={() => !deleting && setDeleteTarget(null)}>
          <div className="crud-modal" onClick={e => e.stopPropagation()}>
            <h2 className="ms-h2 crud-modal__title">Delete {entityName}</h2>
            <p>
              Are you sure you want to delete <strong>{getItemTitle(deleteTarget)}</strong>? This
              action cannot be undone.
            </p>
            <div className="crud-modal__actions">
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
}

export default CrudPage;
