using SignageAndMore.Block;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace SignageAndMore.BlockBehavior;

/// <summary>
/// When targeting a bracket, hangs a single lantern under the arm.
/// Vanilla wall/ceiling lanterns are unchanged when no bracket is targeted.
/// </summary>
public class BlockBehaviorBracketLanternHang : Vintagestory.API.Common.BlockBehavior
{
    public BlockBehaviorBracketLanternHang(Vintagestory.API.Common.Block block) : base(block)
    {
    }

    public override bool CanPlaceBlock(
        IWorldAccessor world,
        IPlayer byPlayer,
        BlockSelection blockSel,
        ref EnumHandling handling,
        ref string failureCode)
    {
        if (!BracketBlock.TryFindFromSelection(world, blockSel, out BlockPos supportPos, out BracketBlock bracket))
        {
            return false;
        }

        handling = EnumHandling.PreventDefault;
        return TryValidateHang(world, supportPos, bracket, ref failureCode, out _);
    }

    public override bool TryPlaceBlock(
        IWorldAccessor world,
        IPlayer byPlayer,
        ItemStack itemstack,
        BlockSelection blockSel,
        ref EnumHandling handling,
        ref string failureCode)
    {
        if (!BracketBlock.TryFindFromSelection(world, blockSel, out BlockPos supportPos, out BracketBlock bracket))
        {
            return false;
        }

        handling = EnumHandling.PreventDefault;

        if (!TryValidateHang(world, supportPos, bracket, ref failureCode, out Vintagestory.API.Common.Block hangingLantern))
        {
            return false;
        }

        BlockSelection placeSel = blockSel.Clone();
        placeSel.Position = bracket.GetHangingSignPos(supportPos);
        placeSel.Face = BlockFacing.DOWN;

        hangingLantern.DoPlaceBlock(world, byPlayer, placeSel, itemstack);
        return true;
    }

    public override void OnNeighbourBlockChange(
        IWorldAccessor world,
        BlockPos pos,
        BlockPos neibpos,
        ref EnumHandling handled)
    {
        if (block.Variant["position"] != "down")
        {
            return;
        }

        if (!BracketBlock.HasHangingSupport(world, pos))
        {
            return;
        }

        handled = EnumHandling.PreventDefault;
    }

    private bool TryValidateHang(
        IWorldAccessor world,
        BlockPos supportPos,
        BracketBlock bracket,
        ref string failureCode,
        out Vintagestory.API.Common.Block hangingLantern)
    {
        hangingLantern = world.BlockAccessor.GetBlock(block.CodeWithVariant("position", "down"));
        if (hangingLantern.Id == 0)
        {
            failureCode = "unplaceable";
            return false;
        }

        if (bracket.HasHangingAttachment(world, supportPos))
        {
            failureCode = SignageAndMoreModSystem.FailAlreadyOccupied;
            return false;
        }

        BlockPos hangingPos = bracket.GetHangingSignPos(supportPos);
        Vintagestory.API.Common.Block occupying = world.BlockAccessor.GetBlock(hangingPos);
        if (!occupying.IsReplacableBy(hangingLantern))
        {
            failureCode = "notreplaceable";
            return false;
        }

        return true;
    }
}
