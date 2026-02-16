using System;
using System.Linq;
using HarmonyLib;

namespace LethalModUtils;

// Copied from FunnyPlugin for use in public mods
public static class CodeMatcherExtensions
{
    private static string MatchesToString(params CodeMatch[] matches) =>
        string.Join(';', matches.Select(i => i.ToString()));

    public static CodeMatcher MatchForward(this CodeMatcher codeMatcher, params CodeMatch[] matches)
    {
        if (codeMatcher.MatchForward(true, matches).IsInvalid)
            throw new InvalidOperationException(
                $"{nameof(CodeMatcher.MatchForward)} failed: {MatchesToString(matches)}"
            );
        return codeMatcher;
    }

    public static CodeMatcher MatchBack(this CodeMatcher codeMatcher, params CodeMatch[] matches)
    {
        if (codeMatcher.MatchBack(true, matches).IsInvalid)
            throw new InvalidOperationException(
                $"{nameof(CodeMatcher.MatchBack)} failed: {MatchesToString(matches)}"
            );
        return codeMatcher;
    }

    [Obsolete] // Not Obsolete but should be removed before release
    public static CodeMatcher LogDebug(this CodeMatcher codeMatcher)
    {
        var pos = 0;
        LethalModUtils.Logger.LogDebug(
            $"Current position: {codeMatcher.Pos}\n{string.Join('\n', codeMatcher.Instructions().Select(i => $"{(++pos == codeMatcher.Pos ? ">" : " ")} #{pos, 2} {i.opcode}{(i.operand == null ? string.Empty : " " + i.operand)}{(i.labels.Count <= 0 ? string.Empty : " " + string.Join(" ", i.labels.Select(l => $"[{l}]")))}"))}"
        );
        return codeMatcher;
    }
}
