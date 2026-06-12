import React from 'react';
import CrudPage, { CrudField, CrudDetailRow } from '../../components/Crud/CrudPage';
import { Product, ProductFormData } from '../../types/product';
import { productService } from '../../services/productService';

const EMPTY_FORM: ProductFormData = { name: '', description: '', price: 0, stock: 0, categoryId: 0 };

const formFields: CrudField<ProductFormData>[] = [
  { key: 'name', label: 'Name', placeholder: 'e.g. Beverages', required: true },
  { key: 'description', label: 'Description', placeholder: 'Optional description', type: 'textarea' },
  { key: 'price', label: 'Price', placeholder: 'e.g. 19.99', type: 'decimal', required: true },
  { key: 'stock', label: 'Stock', placeholder: 'e.g. 100', type: 'number', required: true },
  { key: 'categoryId', label: 'Category', placeholder: 'Select a category', type: 'select', required: true },
];

const detailRows: CrudDetailRow<Product>[] = [
  { label: 'Name', render: c => c.name },
  {
    label: 'Description',
    render: c => c.description || <span className="ms-text-muted">No description provided</span>,
  },
  { label: 'Price', render: c => `${c.price.toFixed(2)}` },
  { label: 'Stock', render: c => c.stock.toString() },
  { label: 'Status', render: c => (c.isActive ? 'Active' : 'Inactive') },
  { label: 'Created', render: c => (c.createdAt ? new Date(c.createdAt).toLocaleString() : null) },
  { label: 'Updated', render: c => (c.updatedAt ? new Date(c.updatedAt).toLocaleString() : null) },
];

const ProductPage: React.FC = () => (
  <CrudPage<Product, ProductFormData>
    title="Products"
    subtitle="Manage products"
    entityName="product"
    entityNamePlural="products"
    service={productService}
    emptyFormData={EMPTY_FORM}
    getItemTitle={product => product.name}
    getItemSubtitle={product => product.description || 'No description'}
    formFields={formFields}
    detailRows={detailRows}
  />
);

export default ProductPage;
