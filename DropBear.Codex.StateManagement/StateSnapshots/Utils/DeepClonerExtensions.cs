using System.Diagnostics;
using DropBear.Codex.Core;
using Newtonsoft.Json;

namespace DropBear.Codex.StateManagement.StateSnapshots.Utils;

public static class DeepClonerExtensions
{
    public static Result<T> Clone<T>(this T obj, JsonSerializerSettings? settings = null) where T : class
    {
        try
        {
            return DeepCloner.Clone(obj, settings);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cloning object: {ex.Message}");
            return Result<T>.Failure("An error occurred while cloning the object.");
        }
    }

    public static async Task<Result<T>> CloneAsync<T>(this T obj, JsonSerializerSettings? settings = null)
        where T : class
    {
        try
        {
            return await Task.Run(() => DeepCloner.Clone(obj, settings)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cloning object: {ex.Message}");
            return Result<T>.Failure("An error occurred while cloning the object.");
        }
    }
}
