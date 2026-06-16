import React, { useRef, useState } from 'react';
import { EntityImage } from '../../types/entityImage';
import './ProductImageGallery.css';

const MAX_SIZE_BYTES = 5 * 1024 * 1024; // 5 MB
const ACCEPTED = 'image/*';

interface ProductImageGalleryProps {
  productId: number;
  images: EntityImage[];
  uploading: boolean;
  /** 0–100 while uploading; null/undefined shows an indeterminate bar. */
  uploadProgress?: number | null;
  /** id of the image currently being deleted, or null/undefined if none. */
  deletingId?: number | null;
  error: string | null;
  readOnly?: boolean;
  onUpload: (productId: number, files: File[]) => Promise<void>;
  onDelete: (productId: number, imageId: number) => Promise<void>;
  onReorder: (productId: number, ordered: EntityImage[]) => Promise<void>;
  onClearError: () => void;
}

/**
 * Drop-zone upload + horizontally-scrolling thumbnail strip with
 * drag-to-reorder. All async side-effects are delegated upward so this
 * component is purely presentational and stays testable in isolation.
 */
const ProductImageGallery: React.FC<ProductImageGalleryProps> = ({
  productId,
  images,
  uploading,
  uploadProgress,
  deletingId,
  error,
  readOnly = false,
  onUpload,
  onDelete,
  onReorder,
  onClearError,
}) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [dropActive, setDropActive] = useState(false);
  const [sizeError, setSizeError] = useState<string | null>(null);

  // ── Drag-to-reorder state ────────────────────────────────────────────────
  const dragSrcIdx = useRef<number | null>(null);

  const handleDragStart = (idx: number) => {
    dragSrcIdx.current = idx;
  };

  const handleDragOver = (e: React.DragEvent, _idx: number) => {
    e.preventDefault();
  };

  const handleDrop = (e: React.DragEvent, targetIdx: number) => {
    e.preventDefault();
    const src = dragSrcIdx.current;
    if (src === null || src === targetIdx) return;

    const reordered = [...images];
    const [moved] = reordered.splice(src, 1);
    reordered.splice(targetIdx, 0, moved);
    dragSrcIdx.current = null;
    onReorder(productId, reordered);
  };

  // ── File handling ────────────────────────────────────────────────────────
  const processFiles = (files: FileList | File[]) => {
    setSizeError(null);
    onClearError();

    const valid: File[] = [];
    for (const file of Array.from(files)) {
      if (file.size > MAX_SIZE_BYTES) {
        setSizeError(`"${file.name}" exceeds 5 MB and was skipped.`);
        continue;
      }
      valid.push(file);
    }
    if (valid.length > 0) onUpload(productId, valid);
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files?.length) processFiles(e.target.files);
    e.target.value = '';
  };

  // ── Drop zone events ─────────────────────────────────────────────────────
  const handleZoneDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setDropActive(true);
  };

  const handleZoneDragLeave = () => setDropActive(false);

  const handleZoneDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setDropActive(false);
    if (e.dataTransfer.files.length) processFiles(e.dataTransfer.files);
  };

  const displayError = sizeError ?? error;
  const hasKnownProgress = typeof uploadProgress === 'number';

  return (
    <div className="img-gallery">
      <span className="img-gallery__label">Images</span>

      {/* Thumbnail strip — horizontal scroll, no wrapping */}
      {images.length > 0 && (
        <div className="img-gallery__track">
          {images.map((img, idx) => {
            const isDeleting = deletingId === img.id;
            return (
              <div
                key={img.id}
                className={`img-gallery__thumb ${isDeleting ? 'img-gallery__thumb--deleting' : ''}`}
                draggable={!readOnly && !isDeleting}
                onDragStart={() => handleDragStart(idx)}
                onDragOver={(e) => handleDragOver(e, idx)}
                onDrop={(e) => handleDrop(e, idx)}
                title={idx === 0 ? 'Primary image (drag to reorder)' : 'Drag to reorder'}
              >
                <img src={img.imagePath} alt={`Product image ${idx + 1}`} loading="lazy" />
                {isDeleting && (
                  <div className="img-gallery__thumb-deleting">
                    <span className="ms-btn__spinner" />
                    <span>Deleting…</span>
                  </div>
                )}
                {!readOnly && !isDeleting && (
                  <button
                    className="img-gallery__thumb-delete"
                    onClick={() => onDelete(productId, img.id)}
                    disabled={deletingId !== null}
                    aria-label="Remove image"
                    title="Remove"
                  >
                    ✕
                  </button>
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* Drop zone — always visible so users can add more images */}
      {!readOnly && (
        <>
          <input
            ref={fileInputRef}
            type="file"
            accept={ACCEPTED}
            multiple
            style={{ display: 'none' }}
            onChange={handleFileChange}
          />
          <div
            className={`img-gallery__dropzone ${dropActive ? 'img-gallery__dropzone--active' : ''}`}
            onClick={() => fileInputRef.current?.click()}
            onDragOver={handleZoneDragOver}
            onDragLeave={handleZoneDragLeave}
            onDrop={handleZoneDrop}
            role="button"
            tabIndex={0}
            aria-label="Upload images"
            onKeyDown={(e) => e.key === 'Enter' && fileInputRef.current?.click()}
          >
            <div className="img-gallery__dropzone-icon">
              <UploadIcon />
            </div>
            <span className="img-gallery__dropzone-title">Upload a File</span>
            <span className="img-gallery__dropzone-hint">Drag and drop files here</span>
          </div>
        </>
      )}

      {uploading && (
        <div className="img-gallery__progress">
          <div className="img-gallery__progress-label">
            <span>Uploading</span>
            {hasKnownProgress && <span>{uploadProgress}%</span>}
          </div>
          <div className="img-gallery__progress-track">
            <div
              className={`img-gallery__progress-fill ${
                hasKnownProgress ? '' : 'img-gallery__progress-fill--indeterminate'
              }`}
              style={hasKnownProgress ? { width: `${uploadProgress}%` } : undefined}
              role="progressbar"
              aria-valuenow={hasKnownProgress ? uploadProgress : undefined}
              aria-valuemin={0}
              aria-valuemax={100}
            />
          </div>
        </div>
      )}

      {displayError && (
        <div className="ms-alert ms-alert--danger img-gallery__error">{displayError}</div>
      )}
    </div>
  );
};

const UploadIcon = () => (
  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden="true">
    <polyline points="16 16 12 12 8 16" />
    <line x1="12" y1="12" x2="12" y2="21" />
    <path d="M20.39 18.39A5 5 0 0018 9h-1.26A8 8 0 103 16.3" />
  </svg>
);

export default ProductImageGallery;