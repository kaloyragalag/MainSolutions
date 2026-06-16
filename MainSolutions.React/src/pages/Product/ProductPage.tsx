import React, { useEffect, useState } from 'react';
import CrudPage, { CrudField, CrudDetailRow } from '../../components/Crud/CrudPage';
import ProductImageGallery from '../../components/Product/ProductImageGallery';
import { Product, ProductFormData } from '../../types/product';
import { productService } from '../../services/productService';
import { getCachedCategories } from '../../services/categoryCache';
import { useProductImages } from '../../hooks/useProductImages';
import { Category } from '../../types/category';
import { canUpdate, getUserRole } from '../../utils/auth';

const EMPTY_FORM: ProductFormData = { name: '', description: '', price: 0, stock: 0, categoryId: 0 };

const detailRows: CrudDetailRow<Product>[] = [
  { label: 'Name', render: (c) => c.name },
  {
    label: 'Description',
    render: (c) => c.description || <span className="ms-text-muted">No description provided</span>,
  },
  { label: 'Category', render: (c) => c.category?.name },
  { label: 'Price', render: (c) => `${c.price.toFixed(2)}` },
  { label: 'Stock', render: (c) => c.stock.toString() },
  { label: 'Status', render: (c) => (c.isActive ? 'Active' : 'Inactive') },
  { label: 'Created', render: (c) => (c.createdAt ? new Date(c.createdAt).toLocaleString() : null) },
  { label: 'Updated', render: (c) => (c.updatedAt ? new Date(c.updatedAt).toLocaleString() : null) },
];

const ProductPage: React.FC = () => {
  const [categories, setCategories] = useState<Category[]>([]);
  const [selectedProductId, setSelectedProductId] = useState<number | null>(null);

  useEffect(() => {
    let mounted = true;
    getCachedCategories()
      .then((items) => { if (mounted) setCategories(items); })
      .catch(() => { if (mounted) setCategories([]); });
    return () => { mounted = false; };
  }, []);

  const formFields: CrudField<ProductFormData>[] = [
    { key: 'name',        label: 'Name',        placeholder: 'e.g. Beverages',      required: true },
    { key: 'description', label: 'Description', placeholder: 'Optional description', type: 'textarea' },
    { key: 'price',       label: 'Price',       placeholder: 'e.g. 19.99',           type: 'decimal', required: true },
    { key: 'stock',       label: 'Stock',       placeholder: 'e.g. 100',             type: 'number',  required: true },
    {
      key: 'categoryId',
      label: 'Category',
      placeholder: 'Select a category',
      type: 'select',
      required: true,
      options: categories.map((cat) => ({ value: cat.id, label: cat.name })),
    },
  ];

  const role = getUserRole();
  const allowUpdate = canUpdate(role);
  const imageHook = useProductImages();

  // Fetch images whenever the selected product changes. This lives at the
  // ProductPage level (a real component) rather than inside the render-prop
  // passed to CrudPage, since render-prop functions are called conditionally
  // during CrudPage's render and must never call hooks themselves — doing so
  // causes "Rendered more hooks than during the previous render" once the
  // condition (customDetailSlot && selectedItem) toggles between renders.
  useEffect(() => {
    if (selectedProductId !== null) {
      imageHook.fetch(selectedProductId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedProductId]);

  /**
   * Purely presentational — no hooks here. CrudPage calls this as a plain
   * function during render (not as a component), so it must stay hook-free.
   */
  const renderImageSlot = (product: Product) => (
    <ProductImageGallery
      productId={product.id}
      images={imageHook.images}
      uploading={imageHook.uploading || imageHook.loading}
      uploadProgress={imageHook.uploadProgress}
      deletingId={imageHook.deletingId}
      error={imageHook.error}
      readOnly={!allowUpdate}
      onUpload={imageHook.upload}
      onDelete={imageHook.remove}
      onReorder={imageHook.reorder}
      onClearError={imageHook.clearError}
    />
  );

  return (
    <CrudPage<Product, ProductFormData>
      title="Products"
      subtitle="Manage products"
      entityName="product"
      entityNamePlural="products"
      service={productService}
      emptyFormData={EMPTY_FORM}
      getItemTitle={(product) => product.name}
      getItemSubtitle={(product) => product.category?.name || product.description || 'No description'}
      formFields={formFields}
      detailRows={detailRows}
      customDetailSlot={renderImageSlot}
      onSelectedItemChange={(product) => setSelectedProductId(product?.id ?? null)}
    />
  );
};

export default ProductPage;
