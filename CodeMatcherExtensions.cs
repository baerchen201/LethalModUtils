using System;
using System.Linq;
using HarmonyLib;

namespace LethalModUtils;

// Copied from FunnyPlugin for use in public mods
public static class CodeMatcherExtensions
{
    private static string MatchesToString(params CodeMatch[] matches)
    {
        return string.Join(';', matches.Select(i => i.ToString()));
    }

    internal static CodeMatcher MatchForward(
        this CodeMatcher codeMatcher,
        params CodeMatch[] matches
    )
    {
        var pos = Math.Clamp(codeMatcher.Pos, 0, codeMatcher.Length - 1);
        if (codeMatcher.MatchForward(true, matches).IsInvalid)
        {
            var _pos = -1;
            LethalModUtils.Logger.LogError(
                $"Start position: {pos}\n{string.Join('\n', codeMatcher.Instructions().Select(i => $"{(++_pos > pos ? "=" : _pos == pos ? "v" : " ")} #{_pos, 2} {i.opcode}{(i.operand == null ? string.Empty : " " + i.operand)}{(i.labels.Count <= 0 ? string.Empty : " " + string.Join(" ", i.labels.Select(l => $"[{l}]")))}"))}"
            );
            throw new InvalidOperationException(
                $"{nameof(CodeMatcher.MatchForward)} failed: {MatchesToString(matches)}"
            );
        }

        return codeMatcher;
    }

    internal static CodeMatcher MatchBack(this CodeMatcher codeMatcher, params CodeMatch[] matches)
    {
        var pos = Math.Clamp(codeMatcher.Pos, 0, codeMatcher.Length - 1);
        if (codeMatcher.MatchBack(true, matches).IsInvalid)
        {
            var _pos = -1;
            LethalModUtils.Logger.LogError(
                $"Start position: {pos}\n{string.Join('\n', codeMatcher.Instructions().Select(i => $"{(++_pos < pos ? "=" : _pos == pos ? "^" : " ")} #{_pos, 2} {i.opcode}{(i.operand == null ? string.Empty : " " + i.operand)}{(i.labels.Count <= 0 ? string.Empty : " " + string.Join(" ", i.labels.Select(l => $"[{l}]")))}"))}"
            );
            throw new InvalidOperationException(
                $"{nameof(CodeMatcher.MatchBack)} failed: {MatchesToString(matches)}"
            );
        }

        return codeMatcher;
    }

    [Obsolete] // Not Obsolete but should be removed before release
    internal static CodeMatcher LogDebug(this CodeMatcher codeMatcher)
    {
        var pos = 0;
        LethalModUtils.Logger.LogDebug(
            $"Current position: {codeMatcher.Pos}\n{string.Join('\n', codeMatcher.Instructions().Select(i => $"{(++pos == codeMatcher.Pos ? ">" : " ")} #{pos, 2} {i.opcode}{(i.operand == null ? string.Empty : " " + i.operand)}{(i.labels.Count <= 0 ? string.Empty : " " + string.Join(" ", i.labels.Select(l => $"[{l}]")))}"))}"
        );
        return codeMatcher;
    }
}
