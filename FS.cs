using System;
using System.IO;
using System.Linq;
using EasyTextEffects.Editor.MyBoxCopy.Extensions;

namespace LethalModUtils;

public static class FS
{
    public enum ProcessFilter
    {
        All,
        None,
        DirectoriesOnly,
        FilesOnly,
    }

    /// <summary>
    /// Iterates over all files in a directory tree
    /// </summary>
    /// <param name="root">The root of the directory tree</param>
    /// <param name="callback">The callback which is called for each found file (returns success status)</param>
    /// <param name="filter">Optional filter for which content should be processed</param>
    /// <returns>Amount of successfully processed files</returns>
    public static int IterateDirectories(
        DirectoryInfo root,
        Func<FileInfo, bool> callback,
        Func<DirectoryInfo, ProcessFilter>? filter = null
    )
    {
        LethalModUtils.Logger.LogDebug(
            $">> IterateDirectories(root: {root}, callback: {callback}, filter: {filter})"
        );
        if (!root.Exists)
            return 0;
        var f = filter?.Invoke(root) ?? ProcessFilter.All;
        var i = 0;
        if (f is ProcessFilter.All or ProcessFilter.DirectoriesOnly)
            i += root.GetDirectories().Sum(RecurseWithParameters);
        if (f is ProcessFilter.All or ProcessFilter.FilesOnly)
            i += root.GetFiles().Count(callback);
        return i;

        int RecurseWithParameters(DirectoryInfo _root) =>
            IterateDirectories(_root, callback, filter);
    }

    /// <summary>
    /// Iterates over all files in a directory tree
    /// </summary>
    /// <param name="root">The root of the directory tree</param>
    /// <param name="callback">The callback which is called for each found file</param>
    /// <param name="filter">Optional filter for which content should be processed</param>
    public static void IterateDirectories(
        DirectoryInfo root,
        Action<FileInfo> callback,
        Func<DirectoryInfo, ProcessFilter>? filter = null
    )
    {
        LethalModUtils.Logger.LogDebug(
            $">> IterateDirectories(root: {root}, callback: {callback}, filter: {filter})"
        );
        if (!root.Exists)
            return;
        var f = filter?.Invoke(root) ?? ProcessFilter.All;
        if (f is ProcessFilter.All or ProcessFilter.DirectoriesOnly)
            root.GetDirectories().ForEach(RecurseWithParameters);
        if (f is ProcessFilter.All or ProcessFilter.FilesOnly)
            root.GetFiles().ForEach(callback);
        return;

        void RecurseWithParameters(DirectoryInfo _root) =>
            IterateDirectories(_root, callback, filter);
    }
}
