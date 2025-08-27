namespace LethalModUtils;

internal static class ObjectExtensions
{
    internal static string str(this object? obj) => obj == null ? "null" : obj.ToString();
}
