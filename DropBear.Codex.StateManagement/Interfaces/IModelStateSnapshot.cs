using ServiceStack.Text;

namespace DropBear.Codex.StateManagement.Interfaces;

public interface IModelStateSnapshot
{
    void InitializeSnapshot<T>(T model, TimeSpan expiration, bool trackChanges = false, JsConfigScope? options = null);
    bool IsModelDirty<T>(T model, out IEnumerable<string> changedProperties, JsConfigScope? options = null);
    void ClearSnapshot<T>(T model, JsConfigScope? options = null);
}
