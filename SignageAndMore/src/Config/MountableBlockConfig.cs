using System;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace SignageAndMore.Config;

public static class MountableBlockConfig
{
    public static readonly string[] DefaultPatterns =
    [
        "game:lantern-*",
        "signageandmore:sign-*",
        "signageandmore:guildsign-*",
        "signageandmore:rusticsign-*",
        "signageandmore:tavernsign-*"
    ];

    public static string[] ReadPatterns(JsonObject? attributes)
    {
        if (attributes?["mountableBlocks"].Exists == true)
        {
            string?[]? fromJson = attributes["mountableBlocks"].AsArray<string>();
            if (fromJson is { Length: > 0 })
            {
                return Array.ConvertAll(fromJson, static s => s ?? string.Empty);
            }
        }

        return DefaultPatterns;
    }

    public static bool Matches(string[] patterns, Vintagestory.API.Common.Block block)
    {
        return block?.Code != null && WildcardUtil.Match(patterns, block.Code.ToString());
    }

    public static bool IsLantern(Vintagestory.API.Common.Block block)
    {
        return block?.Code != null && WildcardUtil.Match("game:lantern-*", block.Code.ToString());
    }

    public static bool IsHangingAttachment(Vintagestory.API.Common.Block block)
    {
        return Matches(DefaultPatterns, block);
    }
}
