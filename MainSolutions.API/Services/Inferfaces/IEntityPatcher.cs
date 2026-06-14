namespace MainSolutions.API.Services.Interfaces;

/// <summary>
/// Applies a partial set of field updates onto an existing entity.
/// Extracted so the patching strategy can change (e.g. JSON Patch)
/// without modifying every controller.
/// </summary>
public interface IEntityPatcher
{
    void Apply<T>(T entity, IDictionary<string, object?> fields) where T : class;
}
