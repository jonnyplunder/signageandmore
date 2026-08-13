using System.Linq;
using SignageAndMore.Block;
using SignageAndMore.BlockBehavior;
using SignageAndMore.Config;
using Vintagestory.API.Common;

namespace SignageAndMore;

public class SignageAndMoreModSystem : ModSystem
{
    public override void StartPre(ICoreAPI api)
    {
        api.RegisterBlockClass("SignageAndMore.BracketBlock", typeof(BracketBlock));
        api.RegisterBlockBehaviorClass("BracketMountable", typeof(BlockBehaviorBracketMountable));
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        Vintagestory.API.Common.Block? bracket = api.World.Blocks
            .FirstOrDefault(b => b.Code.Domain == "signageandmore" && b.Code.Path.StartsWith("bracket"));

        if (bracket?.Attributes != null)
        {
            MountableBlockConfig.Load(bracket.Attributes);
        }

        api.Logger.Notification("Signage & More loaded.");
    }
}
