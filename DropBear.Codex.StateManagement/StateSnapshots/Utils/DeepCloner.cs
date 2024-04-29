using System.Diagnostics;
using DropBear.Codex.Core;
using Newtonsoft.Json;

namespace DropBear.Codex.StateManagement.StateSnapshots.Utils;

public static class DeepCloner
{
    private static readonly JsonSerializerSettings DefaultSettings = new()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects
    };

    public static Result<T> Clone<T>(T source, JsonSerializerSettings? settings = null) where T : class
    {
        try
        {
            var jsonSettings = settings ?? DefaultSettings;
            var json = JsonConvert.SerializeObject(source, jsonSettings);
            var clonedObject = JsonConvert.DeserializeObject<T>(json);

            return Result<T>.Success(clonedObject!);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cloning object: {ex.Message}");
            return Result<T>.Failure("An error occurred while cloning the object.");
        }
    }

    public static async Task<Result<T>> CloneAsync<T>(T source, JsonSerializerSettings? settings = null) where T : class
    {
        try
        {
            return await Task.Run(() => Clone(source, settings)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cloning object: {ex.Message}");
            return Result<T>.Failure("An error occurred while cloning the object.");
        }
    }
}
