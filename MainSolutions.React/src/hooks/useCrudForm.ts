import { useState } from 'react';
import { CrudField } from '../components/Crud/CrudPage.types';
import { CrudService } from '../types/crud';

interface EntityBase {
  id: number;
}

export type ModalMode = 'create' | 'edit' | null;

interface UseCrudFormArgs<T extends EntityBase, TForm extends Record<string, any>> {
  service: CrudService<T, TForm>;
  emptyFormData: TForm;
  formFields: CrudField<TForm>[];
  entityName: string;
  toFormData?: (item: T) => TForm;
  onSaved: (savedId: number) => void;
}

interface UseCrudFormResult<T extends EntityBase, TForm extends Record<string, any>> {
  modalMode: ModalMode;
  formData: TForm;
  formError: string | null;
  saving: boolean;
  openCreateModal: () => void;
  openEditModal: (item: T) => void;
  closeModal: () => void;
  handleFieldChange: (key: keyof TForm, value: string) => void;
  handleSubmit: (e: React.FormEvent, currentItem: T | null) => Promise<void>;
}

/**
 * Encapsulates create/edit modal visibility, form data, validation and
 * submission. Separated from CrudPage so form behaviour can change
 * (e.g. swap validation strategy) without touching list/pagination logic
 * (Single Responsibility + Open/Closed: new validation can be injected).
 */
export function useCrudForm<T extends EntityBase, TForm extends Record<string, any>>({
  service,
  emptyFormData,
  formFields,
  entityName,
  toFormData,
  onSaved,
}: UseCrudFormArgs<T, TForm>): UseCrudFormResult<T, TForm> {
  const [modalMode, setModalMode] = useState<ModalMode>(null);
  const [formData, setFormData] = useState<TForm>(emptyFormData);
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const buildFormData = (item: T): TForm => {
    if (toFormData) return toFormData(item);
    const data: Record<string, any> = {};
    formFields.forEach((field) => {
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
    setFormData((prev) => ({ ...prev, [key]: value }));
  };

  const validate = (): string | null => {
    const missingRequired = formFields.find(
      (f) => f.required && !String(formData[f.key] ?? '').trim(),
    );
    return missingRequired ? `${missingRequired.label} is required.` : null;
  };

  const handleSubmit = async (e: React.FormEvent, currentItem: T | null) => {
    e.preventDefault();

    const validationError = validate();
    if (validationError) {
      setFormError(validationError);
      return;
    }

    setSaving(true);
    setFormError(null);
    try {
      if (modalMode === 'create') {
        const created = await service.create(formData);
        onSaved(created.id);
      } else if (modalMode === 'edit' && currentItem) {
        await service.update(currentItem.id, formData);
        onSaved(currentItem.id);
      }
      setModalMode(null);
      setFormData(emptyFormData);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : `Failed to save ${entityName}.`);
    } finally {
      setSaving(false);
    }
  };

  return {
    modalMode,
    formData,
    formError,
    saving,
    openCreateModal,
    openEditModal,
    closeModal,
    handleFieldChange,
    handleSubmit,
  };
}
