using SignageAndMore.Config;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace SignageAndMore.Block;

public class BracketBlock : Vintagestory.API.Common.Block
{
    public override bool CanAttachBlockAt(
        IBlockAccessor blockAccessor,
        Vintagestory.API.Common.Block block,
        BlockPos pos,
        BlockFacing blockFace,
        Cuboidi? attachmentArea = null)
    {
        if (MountableBlockConfig.IsMountable(block) && IsMountFace(blockFace))
        {
            return true;
        }

        return base.CanAttachBlockAt(blockAccessor, block, pos, blockFace, attachmentArea);
    }

    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
    {
        BreakMountedBlocks(world, pos);
        base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
    }

    public BlockFacing? GetWallFace()
    {
        string side = LastCodePart();
        return string.IsNullOrEmpty(side) ? null : BlockFacing.FromCode(side);
    }

    public BlockFacing? GetOutwardFace()
    {
        BlockFacing? wallFace = GetWallFace();
        return wallFace?.Opposite;
    }

    internal bool IsMountFace(BlockFacing blockFace)
    {
        BlockFacing? outward = GetOutwardFace();
        if (outward == null)
        {
            return false;
        }

        // Underside of the arm (vanilla lanterns) and outward face (signs).
        return blockFace == BlockFacing.UP || blockFace == outward;
    }

    internal static bool IsBracket(Vintagestory.API.Common.Block block)
    {
        return block is BracketBlock;
    }

    private void BreakMountedBlocks(IWorldAccessor world, BlockPos pos)
    {
        IBlockAccessor accessor = world.BlockAccessor;
        BracketBlock? bracket = accessor.GetBlock(pos) as BracketBlock;
        BlockFacing? outward = bracket?.GetOutwardFace();
        if (outward == null)
        {
            return;
        }

        BlockPos[] candidates =
        [
            pos.DownCopy(),
            pos.AddCopy(outward)
        ];

        foreach (BlockPos candidate in candidates)
        {
            Vintagestory.API.Common.Block mounted = accessor.GetBlock(candidate);
            if (MountableBlockConfig.IsMountable(mounted))
            {
                accessor.BreakBlock(candidate, null);
            }
        }
    }
}
