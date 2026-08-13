using System.Collections.Generic;
using SignageAndMore.Config;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace SignageAndMore.Block;

public class BracketBlock : Vintagestory.API.Common.Block
{
    /// <summary>
    /// The arm occupies this many blocks beyond the wall-mounted origin cell.
    /// Signs and lanterns hang under the far cell (origin + this offset).
    /// </summary>
    public const int ArmLengthBlocks = 2;

    public override bool CanAttachBlockAt(
        IBlockAccessor blockAccessor,
        Vintagestory.API.Common.Block block,
        BlockPos pos,
        BlockFacing blockFace,
        Cuboidi? attachmentArea = null)
    {
        // Signs and lanterns are placed by our behaviors at the hang slot.
        // Vanilla attach (OmniAttachable) would hang a second lantern under the base.
        if (MountableBlockConfig.IsMountable(block))
        {
            return false;
        }

        return base.CanAttachBlockAt(blockAccessor, block, pos, blockFace, attachmentArea);
    }

    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
    {
        foreach (BlockPos slot in HangingSlots(pos))
        {
            Vintagestory.API.Common.Block mounted = world.BlockAccessor.GetBlock(slot);
            if (MountableBlockConfig.IsMountable(mounted))
            {
                world.BlockAccessor.BreakBlock(slot, null);
            }
        }

        base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
    }

    public BlockFacing? GetWallFace()
    {
        return BlockFacing.FromCode(Variant["side"] ?? LastCodePart());
    }

    public BlockFacing? GetOutwardFace()
    {
        return GetWallFace()?.Opposite;
    }

    /// <summary>
    /// Sign faces sit perpendicular to the arm so they are readable from the sides.
    /// </summary>
    public BlockFacing? GetSignFacing()
    {
        BlockFacing? outward = GetOutwardFace();
        if (outward == null || !outward.IsHorizontal)
        {
            return null;
        }

        BlockFacing[] order = BlockFacing.HORIZONTALS_ANGLEORDER;
        return order[(outward.HorizontalAngleIndex + 1) % 4];
    }

    public BlockPos GetHangingSignPos(BlockPos bracketPos)
    {
        BlockFacing outward = GetOutwardFace() ?? BlockFacing.NORTH;
        return bracketPos.AddCopy(outward, ArmLengthBlocks).Down();
    }

    public bool HasHangingAttachment(IWorldAccessor world, BlockPos bracketPos)
    {
        IBlockAccessor accessor = world.BlockAccessor;
        foreach (BlockPos slot in HangingSlots(bracketPos))
        {
            if (MountableBlockConfig.IsMountable(accessor.GetBlock(slot)))
            {
                return true;
            }
        }

        return false;
    }

    public static bool TryFindFromSelection(
        IWorldAccessor world,
        BlockSelection blockSel,
        out BlockPos supportPos,
        out BracketBlock bracket)
    {
        IBlockAccessor accessor = world.BlockAccessor;

        BlockPos adjacent = blockSel.Position.AddCopy(blockSel.Face.Opposite);
        if (TryGetBracketAt(accessor, adjacent, out supportPos, out bracket))
        {
            return true;
        }

        if (TryGetBracketAt(accessor, blockSel.Position, out supportPos, out bracket))
        {
            return true;
        }

        return TryGetSupportingBracket(world, blockSel.Position, out supportPos, out bracket, out _);
    }

    public static bool HasHangingSupport(IWorldAccessor world, BlockPos hangingPos)
    {
        return TryGetSupportingBracket(world, hangingPos, out _, out _, out _);
    }

    public static bool TryGetSupportingBracket(
        IWorldAccessor world,
        BlockPos hangingPos,
        out BlockPos bracketPos,
        out BracketBlock bracket,
        out BlockFacing towardWall)
    {
        IBlockAccessor accessor = world.BlockAccessor;
        foreach (BlockFacing hor in BlockFacing.HORIZONTALS)
        {
            for (int dist = 1; dist <= ArmLengthBlocks; dist++)
            {
                BlockPos candidate = hangingPos.UpCopy().AddCopy(hor, dist);
                if (TryGetBracketAt(accessor, candidate, out BlockPos foundPos, out BracketBlock found)
                    && found.GetOutwardFace() == hor.Opposite)
                {
                    bracketPos = foundPos;
                    bracket = found;
                    towardWall = hor;
                    return true;
                }
            }
        }

        bracketPos = null!;
        bracket = null!;
        towardWall = null!;
        return false;
    }

    public static Vec3f? GetLanternMeshOffset(IWorldAccessor world, BlockPos hangingPos)
    {
        if (!TryGetSupportingBracket(world, hangingPos, out _, out _, out BlockFacing towardWall))
        {
            return null;
        }

        return towardWall.Normalf * 0.5f;
    }

    private IEnumerable<BlockPos> HangingSlots(BlockPos bracketPos)
    {
        BlockFacing outward = GetOutwardFace() ?? BlockFacing.NORTH;
        yield return bracketPos.DownCopy();
        for (int dist = 1; dist <= ArmLengthBlocks; dist++)
        {
            yield return bracketPos.AddCopy(outward, dist).Down();
        }
    }

    private static bool TryGetBracketAt(
        IBlockAccessor accessor,
        BlockPos pos,
        out BlockPos bracketPos,
        out BracketBlock bracket)
    {
        Vintagestory.API.Common.Block block = accessor.GetBlock(pos);
        if (block is BracketBlock direct)
        {
            bracketPos = pos.Copy();
            bracket = direct;
            return true;
        }

        // Multiblock dummies (arm cells) point back to the controller via OffsetInv.
        if (block is BlockMultiblock mb)
        {
            BlockPos controllerPos = pos.AddCopy(mb.OffsetInv);
            if (accessor.GetBlock(controllerPos) is BracketBlock controller)
            {
                bracketPos = controllerPos;
                bracket = controller;
                return true;
            }
        }

        bracketPos = null!;
        bracket = null!;
        return false;
    }
}
