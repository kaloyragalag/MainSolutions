import React from 'react';

interface EntityBase {
  id: number;
}

export interface CrudField<TForm> {
  /** Key into the form data object */
  key: keyof TForm;
  label: string;
  placeholder?: string;
  required?: boolean;
  type?: 'text' | 'textarea' | 'select' | 'number' | 'decimal' | 'date';
  /** Options for select fields */
  options?: { value: string | number; label: string }[];
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
  service: import('../../types/crud').CrudService<T, TForm>;
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

// MainSolutions.React/src/components/Crud/CrudPage.types.ts
export interface CrudImageField<T> {
  /** Returns the current image URL for an item, or null/undefined if none. */
  getImageUrl: (item: T) => string | null | undefined;
  /** Optional alt text for the preview image. */
  altText?: (item: T) => string;
  /** Uploads a new image for the entity; returns the updated entity. */
  upload: (id: number, file: File) => Promise<T>;
  /** Removes the current image; returns the updated entity. */
  remove?: (id: number) => Promise<T>;
  /** Accepted file types for the picker (default: 'image/*'). */
  accept?: string;
  /** Client-side max size check, in bytes. */
  maxSizeBytes?: number;
}

export interface CrudPageProps<T extends EntityBase, TForm extends Record<string, any>> {
  // ...existing props...
  /** Optional image upload/preview section shown in the detail panel. */
  imageField?: CrudImageField<T>;
}

export type { EntityBase };
