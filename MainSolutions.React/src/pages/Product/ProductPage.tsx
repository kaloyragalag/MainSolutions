import React, { useEffect, useState } from 'react';
import CrudPage, { CrudField, CrudDetailRow } from '../../components/Crud/CrudPage';
import { Product, ProductFormData } from '../../types/product';
import { productService } from '../../services/productService';
import { getCachedCategories } from '../../services/categoryCache';
import { Category } from '../../types/category';

const EMPTY_FORM: ProductFormData = { name: '', description: '', price: 0, stock: 0, categoryId: 0 };

const detailRows: CrudDetailRow<Product>[] = [
  { label: 'Name', render: c => c.name },
  {
    label: 'Description',
    render: c => c.description || <span className="ms-text-muted">No description provided</span>,
  },
  { label: 'Category', render: c => c.category?.name },
  { label: 'Price', render: c => `${c.price.toFixed(2)}` },
  { label: 'Stock', render: c => c.stock.toString() },
  { label: 'Status', render: c => (c.isActive ? 'Active' : 'Inactive') },
  { label: 'Created', render: c => (c.createdAt ? new Date(c.createdAt).toLocaleString() : null) },
  { label: 'Updated', render: c => (c.updatedAt ? new Date(c.updatedAt).toLocaleString() : null) },
];

const ProductPage: React.FC = () => {
  const [categories, setCategories] = useState<Category[]>([]);

  useEffect(() => {
    let mounted = true;
    getCachedCategories()
      .then(items => {
        if (mounted) setCategories(items);
      })
      .catch(() => {
        if (mounted) setCategories([]);
      });
    return () => {
      mounted = false;
    };
  }, []);

  const formFields: CrudField<ProductFormData>[] = [
    { key: 'name', label: 'Name', placeholder: 'e.g. Beverages', required: true },
    { key: 'description', label: 'Description', placeholder: 'Optional description', type: 'textarea' },
    { key: 'price', label: 'Price', placeholder: 'e.g. 19.99', type: 'decimal', required: true },
    { key: 'stock', label: 'Stock', placeholder: 'e.g. 100', type: 'number', required: true },
    {
      key: 'categoryId',
      label: 'Category',
      placeholder: 'Select a category',
      type: 'select',
      required: true,
      options: categories.map(cat => ({ value: cat.id, label: cat.name })),
    },
  ];

  return (
    <CrudPage<Product, ProductFormData>
      title="Products"
      subtitle="Manage products"
      entityName="product"
      entityNamePlural="products"
      service={productService}
      emptyFormData={EMPTY_FORM}
      getItemTitle={product => product.name}
      getItemSubtitle={product => product.category?.name || product.description || 'No description'}
      formFields={formFields}
      detailRows={detailRows}
      imageField={{
        getImageUrl: product => product.imagePath,
        altText: product => product.name,
        upload: (id, file) => productService.uploadImage(id, file),
        remove: id => productService.deleteImage(id),
        maxSizeBytes: 5 * 1024 * 1024,
      }}
    />
  );
};

export default ProductPage;
