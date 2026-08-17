using System.Collections.Generic;
using SignageAndMore.Config;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace SignageAndMore.Block;

public class BracketBlock : Vintagestory.API.Common.Block
{
    /// <summary>Default arm span when <c>armLengthBlocks</c> is omitted from JSON.</summary>
    public const int DefaultArmLengthBlocks = 2;

    /// <summary>Default lantern hang distance when <c>hangPoints.lantern</c> is omitted.</summary>
    public const int DefaultLanternHangDistanceBlocks = 1;

    /// <summary>Max outward steps when resolving which bracket supports a hanging block.</summary>
    private const int MaxArmSearchBlocks = 8;

    public int ArmLengthBlocks { get; private set; } = DefaultArmLengthBlocks;
    public int LanternHangDistanceBlocks { get; private set; } = DefaultLanternHangDistanceBlocks;
    public int SignHangDistanceBlocks { get; private set; } = DefaultArmLengthBlocks;

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);

        ArmLengthBlocks = Attributes?["armLengthBlocks"].AsInt(DefaultArmLengthBlocks) ?? DefaultArmLengthBlocks;

        LanternHangDistanceBlocks = Attributes?["hangPoints"]["lantern"].AsInt(DefaultLanternHangDistanceBlocks)
            ?? DefaultLanternHangDistanceBlocks;
        SignHangDistanceBlocks = Attributes?["hangPoints"]["sign"].AsInt(ArmLengthBlocks) ?? ArmLengthBlocks;
    }

    public static Vec3f? GetHangOffset(IBlockAccessor accessor, BlockPos hangPos)
    {
        if (!TryGetSupportingBracket(accessor, hangPos, out _, out BracketBlock bracket))
        {
            return null;
        }

        // Keep the lantern centered in its hang cell; only lift to meet the arm underside.
        Vec3f offset = new(0, 0, 0);
        if (bracket.CollisionBoxes != null)
        {
            foreach (Cuboidf box in bracket.CollisionBoxes)
            {
                if (box.X2 - box.X1 > 1 || box.Z2 - box.Z1 > 1)
                {
                    offset.Y = box.Y1;
                    break;
                }
            }
        }

        return offset;
    }

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
        return GetHangPos(bracketPos, SignHangDistanceBlocks);
    }

    public BlockPos GetHangingLanternPos(BlockPos bracketPos)
    {
        return GetHangPos(bracketPos, LanternHangDistanceBlocks);
    }

    private BlockPos GetHangPos(BlockPos bracketPos, int outwardDistance)
    {
        BlockFacing outward = GetOutwardFace() ?? BlockFacing.NORTH;
        return bracketPos.AddCopy(outward, outwardDistance).Down();
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

        return TryGetSupportingBracket(world.BlockAccessor, blockSel.Position, out supportPos, out bracket);
    }

    public static bool HasHangingSupport(IWorldAccessor world, BlockPos hangingPos)
    {
        return TryGetSupportingBracket(world.BlockAccessor, hangingPos, out _, out _);
    }

    public static bool TryGetSupportingBracket(
        IBlockAccessor accessor,
        BlockPos hangingPos,
        out BlockPos bracketPos,
        out BracketBlock bracket)
    {
        foreach (BlockFacing hor in BlockFacing.HORIZONTALS)
        {
            for (int dist = 1; dist <= MaxArmSearchBlocks; dist++)
            {
                BlockPos candidate = hangingPos.UpCopy().AddCopy(hor, dist);
                if (TryGetBracketAt(accessor, candidate, out BlockPos foundPos, out BracketBlock found)
                    && found.GetOutwardFace() == hor.Opposite
                    && found.IsHangPos(foundPos, hangingPos))
                {
                    bracketPos = foundPos;
                    bracket = found;
                    return true;
                }
            }
        }

        bracketPos = null!;
        bracket = null!;
        return false;
    }

    private bool IsHangPos(BlockPos bracketPos, BlockPos hangingPos)
    {
        return hangingPos == GetHangingLanternPos(bracketPos)
            || hangingPos == GetHangingSignPos(bracketPos);
    }

    private IEnumerable<BlockPos> HangingSlots(BlockPos bracketPos)
    {
        HashSet<BlockPos> slots = [];
        slots.Add(bracketPos.DownCopy());
        slots.Add(GetHangingLanternPos(bracketPos));
        slots.Add(GetHangingSignPos(bracketPos));

        foreach (BlockPos slot in slots)
        {
            yield return slot;
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
