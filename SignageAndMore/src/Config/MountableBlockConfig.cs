using System;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace SignageAndMore.Config;

public static class MountableBlockConfig
{
    private static readonly string[] DefaultPatterns =
    [
        "game:lantern-*",
        "signageandmore:sign-*"
    ];

    private static string[] patterns = DefaultPatterns;

    public static void Load(JsonObject? attributes)
    {
        patterns = DefaultPatterns;

        if (attributes?.IsTrue("mountableBlocksFromDefault") == false)
        {
            patterns = [];
        }

        if (attributes == null || !attributes["mountableBlocks"].Exists)
        {
            return;
        }

        string[]? fromJson = attributes["mountableBlocks"].AsArray<string>();
        if (fromJson is { Length: > 0 })
        {
            patterns = Array.ConvertAll(fromJson, static s => s ?? string.Empty);
        }
    }

    public static bool IsMountable(Vintagestory.API.Common.Block block)
    {
        return block?.Code != null && WildcardUtil.Match(patterns, block.Code.ToString());
    }
}
