import React from 'react';

interface EntityBase {
  id: number;
}

export interface CrudField<TForm> {
  key: keyof TForm;
  label: string;
  placeholder?: string;
  required?: boolean;
  type?: 'text' | 'textarea' | 'select' | 'number' | 'decimal' | 'date';
  options?: { value: string | number; label: string }[];
}

export interface CrudDetailRow<T> {
  label: string;
  render: (item: T) => React.ReactNode;
}

export interface CrudImageField<T> {
  getImageUrl: (item: T) => string | null | undefined;
  altText?: (item: T) => string;
  upload: (id: number, file: File) => Promise<T>;
  remove?: (id: number) => Promise<T>;
  accept?: string;
  maxSizeBytes?: number;
}

export interface CrudPageProps<T extends EntityBase, TForm extends Record<string, any>> {
  title: string;
  subtitle?: string;
  entityName: string;
  entityNamePlural?: string;
  service: import('../../types/crud').CrudService<T, TForm>;
  emptyFormData: TForm;
  pageSize?: number;
  getItemTitle: (item: T) => string;
  getItemSubtitle?: (item: T) => React.ReactNode;
  formFields: CrudField<TForm>[];
  detailRows: CrudDetailRow<T>[];
  toFormData?: (item: T) => TForm;
  icon?: React.ReactNode;

  /** Legacy single-image prop. Prefer customDetailSlot for multi-image support. */
  imageField?: CrudImageField<T>;

  /**
   * Render a custom block inside the detail panel, positioned between the
   * action buttons and the Details rows. Receives the selected entity.
   *
   * IMPORTANT: this function is called like a render-prop during CrudPage's
   * own render pass, not mounted as a component. It must not call React
   * hooks (useState/useEffect/useRef/etc.) — doing so violates the Rules of
   * Hooks because the call is conditional on `selectedItem` being non-null,
   * which can differ between renders. Any stateful logic the slot needs
   * (e.g. fetching data when the selection changes) belongs in the parent
   * page component, driven by `onSelectedItemChange` below.
   */
  customDetailSlot?: (item: T) => React.ReactNode;

  /**
   * Fires whenever the selected item changes (including to null when the
   * list is empty or nothing is selected yet). Use this in the parent page
   * to drive side effects — e.g. fetching related data for customDetailSlot —
   * without putting hooks inside the render-prop itself.
   */
  onSelectedItemChange?: (item: T | null) => void;
}

export type { EntityBase };