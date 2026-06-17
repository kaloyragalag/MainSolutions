using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainSolutions.API.Controllers;

/// <param name="Id">EntityImage.Id</param>
/// <param name="SortOrder">Target zero-based display order.</param>
public record ImageSortItem(int Id, int SortOrder);

/// <summary>
/// Reusable multi-image upload/list/delete/reorder endpoints for any entity
/// backed by the polymorphic EntityImage table. Mounts under the same route
/// as the derived BaseController&lt;T&gt; (e.g. api/Products), adding
/// /{id}/images, /{id}/images/{imageId}, and /{id}/images/reorder.
///
/// Derived controllers only need to:
///   1. Pass the constructor dependencies through.
///   2. Override <see cref="EntityType"/> with a lowercase identifier
///      (e.g. "product") matching what's stored in EntityImage.EntityType.
///
/// All role requirements mirror BaseController's conventions:
/// Admin/Editor/Viewer can read, Admin/Editor can write.
/// </summary>
public abstract class BaseImageController<T> : BaseController<T> where T : class
{
    protected readonly IBlobStorageService ImageBlobStorage;
    protected readonly IEntityImageRepository ImageRepository;

    /// <summary>
    /// Lowercase entity type stored in EntityImage.EntityType for this
    /// controller's entity, e.g. "product". Must be overridden.
    /// </summary>
    protected abstract string EntityType { get; }

    /// <summary>Singular display name used in not-found messages, e.g. "Product".</summary>
    protected virtual string EntityDisplayName => typeof(T).Name;

    protected BaseImageController(
        IBaseService<T> service,
        IEntityPatcher patcher,
        IBlobStorageService blobStorage,
        IEntityImageRepository imageRepository)
        : base(service, patcher)
    {
        ImageBlobStorage = blobStorage;
        ImageRepository = imageRepository;
    }

    /// <summary>Returns all images for the entity, ordered by SortOrder.</summary>
    [Authorize(Roles = "Admin,Editor,Viewer")]
    [HttpGet("{id:int}/images")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> GetImages(int id, CancellationToken cancellationToken)
    {
        var entity = await _service.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return NotFound(new { message = $"{EntityDisplayName} with id {id} was not found." });

        var images = await ImageRepository.GetByEntityAsync(EntityType, id, cancellationToken);
        return Ok(images);
    }

    /// <summary>
    /// Uploads one or more images for the entity.
    /// Accepts multipart/form-data with field name "files".
    /// </summary>
    [Authorize(Roles = "Admin,Editor")]
    [HttpPost("{id:int}/images")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> UploadImages(
        int id, IFormFileCollection files, CancellationToken cancellationToken)
    {
        var entity = await _service.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return NotFound(new { message = $"{EntityDisplayName} with id {id} was not found." });

        if (files is null || files.Count == 0)
            return BadRequest(new { message = "No files were provided." });

        var existing = await ImageRepository.GetByEntityAsync(EntityType, id, cancellationToken);
        var nextSort = existing.Count > 0 ? existing.Max(i => i.SortOrder) + 1 : 0;

        var uploaded = new List<EntityImage>();

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            await using var stream = file.OpenReadStream();
            var url = await ImageBlobStorage.UploadAsync(
                stream, file.FileName, file.ContentType, cancellationToken);

            var image = await ImageRepository.AddAsync(new EntityImage
            {
                EntityType = EntityType,
                EntityId   = id,
                ImagePath  = url,
                SortOrder  = nextSort++,
            }, cancellationToken);

            uploaded.Add(image);
        }

        return Ok(uploaded);
    }

    /// <summary>Deletes a single image by its own id.</summary>
    [Authorize(Roles = "Admin,Editor")]
    [HttpDelete("{id:int}/images/{imageId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> DeleteImage(
        int id, int imageId, CancellationToken cancellationToken)
    {
        var image = await ImageRepository.GetByIdAsync(imageId, cancellationToken);
        if (image is null || image.EntityType != EntityType || image.EntityId != id)
            return NotFound(new { message = $"Image with id {imageId} was not found for {EntityType} {id}." });

        try { await ImageBlobStorage.DeleteAsync(image.ImagePath, cancellationToken); }
        catch { /* best-effort; orphaned blobs handled by storage lifecycle policy */ }

        await ImageRepository.DeleteAsync(imageId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Updates the display order of images.
    /// Body: [{ "id": 3, "sortOrder": 0 }, { "id": 7, "sortOrder": 1 }, …]
    /// </summary>
    [Authorize(Roles = "Admin,Editor")]
    [HttpPatch("{id:int}/images/reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> ReorderImages(
        int id, [FromBody] List<ImageSortItem> items, CancellationToken cancellationToken)
    {
        var entity = await _service.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return NotFound(new { message = $"{EntityDisplayName} with id {id} was not found." });

        await ImageRepository.ReorderAsync(
            items.Select(x => (x.Id, x.SortOrder)), cancellationToken);

        var updated = await ImageRepository.GetByEntityAsync(EntityType, id, cancellationToken);
        return Ok(updated);
    }
}
