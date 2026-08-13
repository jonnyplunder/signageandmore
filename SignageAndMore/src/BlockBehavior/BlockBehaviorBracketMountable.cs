using SignageAndMore.Block;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace SignageAndMore.BlockBehavior;

/// <summary>
/// Allows a block to be placed only on <see cref="BracketBlock"/>, hanging under the arm tip.
/// </summary>
public class BlockBehaviorBracketMountable : Vintagestory.API.Common.BlockBehavior
{
    private string facingCode = "side";
    private string dropFacing = "north";

    public BlockBehaviorBracketMountable(Vintagestory.API.Common.Block block) : base(block)
    {
    }

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        facingCode = properties["facingCode"].AsString("side");
        dropFacing = properties["dropFacing"].AsString("north");
    }

    public override bool TryPlaceBlock(
        IWorldAccessor world,
        IPlayer byPlayer,
        ItemStack itemstack,
        BlockSelection blockSel,
        ref EnumHandling handling,
        ref string failureCode)
    {
        handling = EnumHandling.PreventDefault;

        if (!TryResolveBracket(world, blockSel, ref failureCode, out BlockPos supportPos, out BracketBlock bracket))
        {
            return false;
        }

        string hangingFacing = bracket.GetSignFacing()?.Code ?? dropFacing;
        Vintagestory.API.Common.Block orientedBlock = world.BlockAccessor.GetBlock(
            block.CodeWithVariant(facingCode, hangingFacing));

        BlockSelection placeSel = blockSel.Clone();
        placeSel.Position = bracket.GetHangingSignPos(supportPos);
        placeSel.Face = BlockFacing.DOWN;

        Vintagestory.API.Common.Block occupying = world.BlockAccessor.GetBlock(placeSel.Position);
        if (!occupying.IsReplacableBy(orientedBlock))
        {
            failureCode = "notreplaceable";
            return false;
        }

        orientedBlock.DoPlaceBlock(world, byPlayer, placeSel, itemstack);
        return true;
    }

    public override bool CanPlaceBlock(
        IWorldAccessor world,
        IPlayer byPlayer,
        BlockSelection blockSel,
        ref EnumHandling handling,
        ref string failureCode)
    {
        handling = EnumHandling.PreventDefault;
        return TryResolveBracket(world, blockSel, ref failureCode, out _, out _);
    }

    public override void OnNeighbourBlockChange(
        IWorldAccessor world,
        BlockPos pos,
        BlockPos neibpos,
        ref EnumHandling handled)
    {
        handled = EnumHandling.PreventDefault;

        if (!BracketBlock.HasHangingSupport(world, pos))
        {
            world.BlockAccessor.BreakBlock(pos, null);
        }
    }

    public override ItemStack[] GetDrops(
        IWorldAccessor world,
        BlockPos pos,
        IPlayer byPlayer,
        ref float dropQuantityMultiplier,
        ref EnumHandling handled)
    {
        handled = EnumHandling.PreventDefault;
        Vintagestory.API.Common.Block dropped = world.BlockAccessor.GetBlock(block.CodeWithVariant(facingCode, dropFacing));
        return [new ItemStack(dropped)];
    }

    public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos, ref EnumHandling handled)
    {
        handled = EnumHandling.PreventDefault;
        Vintagestory.API.Common.Block picked = world.BlockAccessor.GetBlock(block.CodeWithVariant(facingCode, dropFacing));
        return new ItemStack(picked);
    }

    public override bool CanAttachBlockAt(
        IBlockAccessor blockAccessor,
        Vintagestory.API.Common.Block block,
        BlockPos pos,
        BlockFacing blockFace,
        ref EnumHandling handled,
        Cuboidi? attachmentArea = null)
    {
        handled = EnumHandling.PreventDefault;
        return false;
    }

    private static bool TryResolveBracket(
        IWorldAccessor world,
        BlockSelection blockSel,
        ref string failureCode,
        out BlockPos supportPos,
        out BracketBlock bracket)
    {
        if (!BracketBlock.TryFindFromSelection(world, blockSel, out supportPos, out bracket))
        {
            failureCode = SignageAndMoreModSystem.FailRequireBracket;
            return false;
        }

        if (bracket.HasHangingAttachment(world, supportPos))
        {
            failureCode = SignageAndMoreModSystem.FailAlreadyOccupied;
            return false;
        }

        return true;
    }
}
