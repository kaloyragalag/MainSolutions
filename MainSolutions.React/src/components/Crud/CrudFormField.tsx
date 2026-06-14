import React from 'react';
import { CrudField } from './CrudPage.types';

interface CrudFormFieldProps<TForm extends Record<string, any>> {
  field: CrudField<TForm>;
  value: string;
  autoFocus: boolean;
  onChange: (key: keyof TForm, value: string) => void;
}

/**
 * Renders the appropriate input control for a given CrudField definition.
 * Extracted from CrudPage so new field types can be added here without
 * touching CrudPage's layout/state logic (Open/Closed Principle).
 */
function CrudFormField<TForm extends Record<string, any>>({
  field,
  value,
  autoFocus,
  onChange,
}: CrudFormFieldProps<TForm>) {
  const fieldId = `crud-field-${field.key as string}`;
  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) =>
    onChange(field.key, e.target.value);

  switch (field.type) {
    case 'select':
      return (
        <select id={fieldId} className="ms-field__input" value={value} onChange={handleChange} autoFocus={autoFocus}>
          <option value="" disabled>
            {field.placeholder || 'Select an option'}
          </option>
          {field.options?.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>
      );

    case 'number':
      return (
        <input
          id={fieldId}
          className="ms-field__input"
          type="number"
          value={value}
          onChange={handleChange}
          placeholder={field.placeholder}
          autoFocus={autoFocus}
        />
      );

    case 'decimal':
      return (
        <input
          id={fieldId}
          className="ms-field__input"
          type="number"
          step="0.01"
          value={value}
          onChange={handleChange}
          placeholder={field.placeholder}
          autoFocus={autoFocus}
        />
      );

    case 'date':
      return (
        <input
          id={fieldId}
          className="ms-field__input"
          type="date"
          value={value}
          onChange={handleChange}
          placeholder={field.placeholder}
          autoFocus={autoFocus}
        />
      );

    case 'textarea':
      return (
        <textarea
          id={fieldId}
          className="ms-field__input crud-field__textarea"
          value={value}
          onChange={handleChange}
          placeholder={field.placeholder}
          autoFocus={autoFocus}
        />
      );

    default:
      return (
        <input
          id={fieldId}
          className="ms-field__input"
          type="text"
          value={value}
          onChange={handleChange}
          placeholder={field.placeholder}
          autoFocus={autoFocus}
        />
      );
  }
}

export default CrudFormField;
