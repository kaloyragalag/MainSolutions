import React from 'react';
import CrudPage, { CrudField, CrudDetailRow } from '../../components/Crud/CrudPage';
import { categoryService } from '../../services/categoryService';
import { Category, CategoryFormData } from '../../types/category';

const EMPTY_FORM: CategoryFormData = { name: '', description: '' };

const formFields: CrudField<CategoryFormData>[] = [
  { key: 'name', label: 'Name', placeholder: 'e.g. Beverages', required: true },
  { key: 'description', label: 'Description', placeholder: 'Optional description', type: 'textarea' },
];

const detailRows: CrudDetailRow<Category>[] = [
  { label: 'Name', render: c => c.name },
  {
    label: 'Description',
    render: c => c.description || <span className="ms-text-muted">No description provided</span>,
  },
  { label: 'Status', render: c => (c.isActive ? 'Active' : 'Inactive') },
  { label: 'Created', render: c => (c.createdAt ? new Date(c.createdAt).toLocaleString() : null) },
  { label: 'Updated', render: c => (c.updatedAt ? new Date(c.updatedAt).toLocaleString() : null) },
];

const CategoryPage: React.FC = () => (
  <CrudPage<Category, CategoryFormData>
    title="Categories"
    subtitle="Manage product categories"
    entityName="category"
    entityNamePlural="categories"
    service={categoryService}
    emptyFormData={EMPTY_FORM}
    getItemTitle={category => category.name}
    getItemSubtitle={category => category.description || 'No description'}
    formFields={formFields}
    detailRows={detailRows}
  />
);

export default CategoryPage;
