import React from 'react';
import BasePage from '../Layout/BasePage';
import { getUserRole, canCreate, canUpdate, canDelete } from '../../utils/auth';
import { useCrudList } from '../../hooks/useCrudList';
import { useCrudForm } from '../../hooks/useCrudForm';
import { useCrudDelete } from '../../hooks/useCrudDelete';
import CrudFormField from './CrudFormField';
import {
  DefaultIcon,
  DefaultIconLg,
  PlusIcon,
  EditIcon,
  TrashIcon,
  EmptyIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  ImageIcon
} from './CrudIcons';
import { CrudPageProps, EntityBase } from './CrudPage.types';
import './CrudPage.css';

export type { CrudField, CrudDetailRow } from './CrudPage.types';

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
  imageField,
  customDetailSlot,
  onSelectedItemChange,
}: CrudPageProps<T, TForm>) {
  const pluralName = entityNamePlural ?? `${entityName}s`;

  const role = getUserRole();
  const allowCreate = canCreate(role);
  const allowUpdate = canUpdate(role);
  const allowDelete = canDelete(role);

  const list = useCrudList<T, TForm>(service, pageSize, pluralName, getItemTitle);

  const form = useCrudForm<T, TForm>({
    service,
    emptyFormData,
    formFields,
    entityName,
    toFormData,
    onSaved: async (savedId) => {
      await list.refetch(list.page);
      list.setSelectedId(savedId);
    },
  });

  const remove = useCrudDelete<T, TForm>(
    service,
    entityName,
    list.items.length,
    list.page,
    (nextPage) => list.refetch(nextPage),
    (message) => setListError(message),
  );

  // useCrudList owns its own error state for fetch failures, but delete
  // failures are surfaced through the same banner. A small local override
  // keeps that wiring without leaking refetch internals into the hook.
  const [extraError, setExtraError] = React.useState<string | null>(null);
  const setListError = (message: string) => setExtraError(message);
  const error = extraError ?? list.error;

  const selectedItem = list.items.find((i) => i.id === list.selectedId) ?? null;

  // Notify the parent page whenever the selected item changes, so any
  // side effects needed by customDetailSlot (e.g. fetching related data)
  // can live in the page component instead of inside the render-prop —
  // customDetailSlot itself must stay hook-free since it's invoked
  // conditionally during this component's render pass.
  React.useEffect(() => {
    onSelectedItemChange?.(selectedItem);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedItem?.id]);

  const listIcon = icon ?? <DefaultIcon />;
  const detailIcon = icon ?? <DefaultIconLg />;
  const capitalizedPlural = pluralName.charAt(0).toUpperCase() + pluralName.slice(1);

  const handleOpenCreate = () => {
    if (!allowCreate) return;
    form.openCreateModal();
  };

  const handleOpenEdit = (item: T) => {
    if (!allowUpdate) return;
    form.openEditModal(item);
  };

  const [imageUploading, setImageUploading] = React.useState(false);
  const [imageError, setImageError] = React.useState<string | null>(null);
  const imageInputRef = React.useRef<HTMLInputElement>(null);

  const imageUrl = imageField && selectedItem ? imageField.getImageUrl(selectedItem) : null;

  const handleImageChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    e.target.value = '';
    if (!file || !imageField || !selectedItem) return;

    if (imageField.maxSizeBytes && file.size > imageField.maxSizeBytes) {
      setImageError(`Image must be ${Math.round(imageField.maxSizeBytes / (1024 * 1024))}MB or smaller.`);
      return;
    }

    setImageError(null);
    setImageUploading(true);
    try {
      await imageField.upload(selectedItem.id, file);
      await list.refetch(list.page);
    } catch (err) {
      setImageError(err instanceof Error ? err.message : 'Failed to upload image.');
    } finally {
      setImageUploading(false);
    }
  };

  const handleImageRemove = async () => {
    if (!imageField?.remove || !selectedItem) return;
    setImageError(null);
    setImageUploading(true);
    try {
      await imageField.remove(selectedItem.id);
      await list.refetch(list.page);
    } catch (err) {
      setImageError(err instanceof Error ? err.message : 'Failed to remove image.');
    } finally {
      setImageUploading(false);
    }
  };

  return (
    <BasePage>
      <div className="ms-page-header">
        <div className="ms-page-header__info">
          <h1 className="ms-page-header__title">{title}</h1>
          {subtitle && <p className="ms-page-header__subtitle">{subtitle}</p>}
        </div>
        {allowCreate && (
          <button className="ms-btn ms-btn--primary" onClick={handleOpenCreate}>
            <PlusIcon />
            Create {entityName}
          </button>
        )}
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
            <span className="ms-h4">
              All {pluralName} ({list.totalCount})
            </span>
          </div>
          <div className="crud-list__search">
            <input
              className="ms-field__input"
              placeholder={`Search ${pluralName}...`}
              value={list.search}
              onChange={(e) => list.setSearch(e.target.value)}
            />
          </div>
          <div className="crud-list__items">
            {list.loading ? (
              <div className="crud-list__empty">Loading {pluralName}...</div>
            ) : list.filteredItems.length === 0 ? (
              <div className="crud-list__empty">No {pluralName} found.</div>
            ) : (
              list.filteredItems.map((item) => (
                <button
                  key={item.id}
                  className={`crud-list__item ${list.selectedId === item.id ? 'crud-list__item--active' : ''}`}
                  onClick={() => list.setSelectedId(item.id)}
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

          {!list.loading && list.totalPages > 1 && (
            <div className="crud-list__pagination">
              <button
                className="ms-btn ms-btn--ghost ms-btn--sm"
                onClick={() => list.goToPage(list.page - 1)}
                disabled={list.page <= 1}
                aria-label="Previous page"
              >
                <ChevronLeftIcon />
              </button>
              <span className="crud-list__pagination-info">
                Page {list.page} of {list.totalPages}
              </span>
              <button
                className="ms-btn ms-btn--ghost ms-btn--sm"
                onClick={() => list.goToPage(list.page + 1)}
                disabled={list.page >= list.totalPages}
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
              {imageField && (
                <div className="crud-detail__image">
                  {imageUrl ? (
                    <img
                      src={imageUrl}
                      alt={imageField.altText?.(selectedItem) ?? getItemTitle(selectedItem)}
                      className="crud-detail__image-preview"
                    />
                  ) : (
                    <div className="crud-detail__image-placeholder">
                      <ImageIcon />
                    </div>
                  )}

                  {allowUpdate && (
                    <div className="crud-detail__image-actions">
                      <input
                        ref={imageInputRef}
                        type="file"
                        accept={imageField.accept ?? 'image/*'}
                        style={{ display: 'none' }}
                        onChange={handleImageChange}
                      />
                      <button
                        className="ms-btn ms-btn--secondary ms-btn--sm"
                        onClick={() => imageInputRef.current?.click()}
                        disabled={imageUploading}
                      >
                        {imageUploading && <span className="ms-btn__spinner" />}
                        {imageUrl ? 'Replace image' : 'Upload image'}
                      </button>
                      {imageUrl && imageField.remove && (
                        <button className="ms-btn ms-btn--danger ms-btn--sm" onClick={handleImageRemove} disabled={imageUploading}>
                          Remove
                        </button>
                      )}
                    </div>
                  )}

                  {imageError && <div className="ms-alert ms-alert--danger">{imageError}</div>}
                </div>
              )}
              <div className="crud-detail__actions">
                {allowUpdate && (
                  <button className="ms-btn ms-btn--secondary" onClick={() => handleOpenEdit(selectedItem)}>
                    <EditIcon />
                    Edit
                  </button>
                )}
                {allowDelete && (
                  <button className="ms-btn ms-btn--danger" onClick={() => remove.requestDelete(selectedItem)}>
                    <TrashIcon />
                    Delete
                  </button>
                )}
              </div>

              {/* Custom per-entity content (e.g. multi-image gallery). Rendered
                  as a plain function call — it must not use React hooks. */}
              {customDetailSlot && customDetailSlot(selectedItem)}

              <div className="crud-detail__section">
                <h3 className="ms-h3" style={{ marginBottom: 'var(--space-2)' }}>
                  Details
                </h3>
                {detailRows.map((row) => {
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
                {list.loading
                  ? 'Loading...'
                  : `Select a ${entityName} to view details, or create a new one.`}
              </p>
              {!list.loading && allowCreate && (
                <button className="ms-btn ms-btn--primary" onClick={handleOpenCreate}>
                  <PlusIcon />
                  Create {entityName}
                </button>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Create / Edit modal */}
      {form.modalMode && (
        <div className="crud-modal-overlay" onClick={form.closeModal}>
          <div className="crud-modal" onClick={(e) => e.stopPropagation()}>
            <h2 className="ms-h2 crud-modal__title">
              {form.modalMode === 'create' ? `Create ${entityName}` : `Edit ${entityName}`}
            </h2>

            <form className="ms-form" onSubmit={(e) => form.handleSubmit(e, selectedItem)}>
              {form.formError && <div className="ms-alert ms-alert--danger">{form.formError}</div>}

              {formFields.map((field, index) => (
                <div className="ms-field" key={field.key as string}>
                  <label className="ms-field__label" htmlFor={`crud-field-${field.key as string}`}>
                    {field.label}
                  </label>
                  <CrudFormField
                    field={field}
                    value={(form.formData[field.key] ?? '') as string}
                    autoFocus={index === 0}
                    onChange={form.handleFieldChange}
                  />
                </div>
              ))}

              <div className="crud-modal__actions">
                <button
                  type="button"
                  className="ms-btn ms-btn--secondary"
                  onClick={form.closeModal}
                  disabled={form.saving}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className={`ms-btn ms-btn--primary ${form.saving ? 'ms-btn--loading' : ''}`}
                  disabled={form.saving}
                >
                  {form.saving && <span className="ms-btn__spinner" />}
                  {form.modalMode === 'create' ? 'Create' : 'Save changes'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Delete confirmation modal */}
      {remove.deleteTarget && (
        <div className="crud-modal-overlay" onClick={remove.cancelDelete}>
          <div className="crud-modal" onClick={(e) => e.stopPropagation()}>
            <h2 className="ms-h2 crud-modal__title">Delete {entityName}</h2>
            <p>
              Are you sure you want to delete <strong>{getItemTitle(remove.deleteTarget)}</strong>? This action
              cannot be undone.
            </p>
            <div className="crud-modal__actions">
              <button className="ms-btn ms-btn--secondary" onClick={remove.cancelDelete} disabled={remove.deleting}>
                Cancel
              </button>
              <button
                className={`ms-btn ms-btn--danger ${remove.deleting ? 'ms-btn--loading' : ''}`}
                onClick={remove.confirmDelete}
                disabled={remove.deleting}
              >
                {remove.deleting && <span className="ms-btn__spinner" />}
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
