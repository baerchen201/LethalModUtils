namespace LethalModUtils;

internal static class ObjectExtensions
{
    internal static string str(this object? obj)
    {
        return obj == null ? "null" : obj.ToString();
    }
}
