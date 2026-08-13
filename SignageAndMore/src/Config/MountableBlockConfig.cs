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
        if (attributes?["mountableBlocks"].Exists == true)
        {
            string?[]? fromJson = attributes["mountableBlocks"].AsArray<string>();
            if (fromJson is { Length: > 0 })
            {
                patterns = Array.ConvertAll(fromJson, static s => s ?? string.Empty);
                return;
            }
        }

        patterns = DefaultPatterns;
    }

    public static bool IsLantern(Vintagestory.API.Common.Block block)
    {
        return block?.Code != null && WildcardUtil.Match("game:lantern-*", block.Code.ToString());
    }

    public static bool IsMountable(Vintagestory.API.Common.Block block)
    {
        return block?.Code != null && WildcardUtil.Match(patterns, block.Code.ToString());
    }
}
