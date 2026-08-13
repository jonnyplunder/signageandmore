using Newtonsoft.Json.Linq;
using SignageAndMore.Block;
using SignageAndMore.BlockBehavior;
using SignageAndMore.BlockEntity;
using SignageAndMore.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace SignageAndMore;

public class SignageAndMoreModSystem : ModSystem
{
    public static readonly string modID = "signageandmore";

    /// <summary>
    /// JSON entityClass for hanging lanterns. Vanilla BELantern tessellates its own mesh
    /// and does not call BlockEntityBehavior.OnTesselation, so a 0.5 wall offset requires
    /// a BELantern subclass rather than a behavior or shape patch.
    /// Applied only to position=down variants (the block actually placed when hanging).
    /// </summary>
    public const string LanternEntityClass = "SignageAndMore.BracketLantern";

    public static readonly string FailRequireBracket = modID + ":Sign.RequireBracket";
    public static readonly string FailAlreadyOccupied = modID + ":Bracket.AlreadyOccupied";

    public override void Start(ICoreAPI api)
    {
        api.RegisterBlockClass("SignageAndMore.BracketBlock", typeof(BracketBlock));
        api.RegisterBlockBehaviorClass("BracketMountable", typeof(BlockBehaviorBracketMountable));
        api.RegisterBlockBehaviorClass("BracketLanternHang", typeof(BlockBehaviorBracketLanternHang));
        api.RegisterBlockEntityClass(LanternEntityClass, typeof(BELanternOnBracket));
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        foreach (Vintagestory.API.Common.Block block in api.World.Blocks)
        {
            if (block is BracketBlock)
            {
                MountableBlockConfig.Load(block.Attributes);
                break;
            }
        }

        api.Logger.Notification("Signage & More loaded.");
    }

    public override void AssetsFinalize(ICoreAPI api)
    {
        JsonObject emptyProperties = new(new JObject());

        foreach (Vintagestory.API.Common.Block lantern in api.World.Blocks)
        {
            if (!MountableBlockConfig.IsLantern(lantern))
            {
                continue;
            }

            if (lantern.GetBehavior<BlockBehaviorBracketLanternHang>() == null)
            {
                InsertBehaviorFirst(api, lantern, new BlockBehaviorBracketLanternHang(lantern), emptyProperties);
            }

            // Only hanging (down) lanterns render via BELantern; wall/up keep the vanilla BE.
            if (lantern.Variant["position"] == "down")
            {
                lantern.EntityClass = LanternEntityClass;
            }
        }
    }

    /// <summary>
    /// Block has no AddBehavior API; both CollectibleBehaviors and BlockBehaviors must stay in sync.
    /// Inserted at index 0 so we run before vanilla OmniAttachable.
    /// </summary>
    private static void InsertBehaviorFirst(
        ICoreAPI api,
        Vintagestory.API.Common.Block block,
        Vintagestory.API.Common.BlockBehavior behavior,
        JsonObject properties)
    {
        behavior.Initialize(properties);

        CollectibleBehavior[] collectibleBehaviors = new CollectibleBehavior[block.CollectibleBehaviors.Length + 1];
        collectibleBehaviors[0] = behavior;
        block.CollectibleBehaviors.CopyTo(collectibleBehaviors, 1);
        block.CollectibleBehaviors = collectibleBehaviors;

        Vintagestory.API.Common.BlockBehavior[] blockBehaviors = new Vintagestory.API.Common.BlockBehavior[block.BlockBehaviors.Length + 1];
        blockBehaviors[0] = behavior;
        block.BlockBehaviors.CopyTo(blockBehaviors, 1);
        block.BlockBehaviors = blockBehaviors;

        behavior.OnLoaded(api);
    }
}
