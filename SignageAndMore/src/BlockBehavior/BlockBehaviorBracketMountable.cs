using SignageAndMore.Block;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace SignageAndMore.BlockBehavior;

/// <summary>
/// Allows a block to be placed only on <see cref="BracketBlock"/> mount faces (not on walls).
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

        BlockFacing supportFace = blockSel.Face.Opposite;
        BlockPos supportPos = blockSel.Position.AddCopy(supportFace);
        Vintagestory.API.Common.Block supportBlock = world.BlockAccessor.GetBlock(supportPos);

        if (!BracketBlock.IsBracket(supportBlock))
        {
            failureCode = "signageandmore:requirebracket";
            return false;
        }

        if (!supportBlock.CanAttachBlockAt(world.BlockAccessor, block, supportPos, blockSel.Face))
        {
            failureCode = "signageandmore:requirebracket";
            return false;
        }

        string outwardFacing = blockSel.Face.Opposite.Code;
        Vintagestory.API.Common.Block orientedBlock = world.BlockAccessor.GetBlock(
            block.CodeWithVariant(facingCode, outwardFacing));

        if (!orientedBlock.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
        {
            return false;
        }

        orientedBlock.DoPlaceBlock(world, byPlayer, blockSel, itemstack);
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

        BlockFacing supportFace = blockSel.Face.Opposite;
        BlockPos supportPos = blockSel.Position.AddCopy(supportFace);
        Vintagestory.API.Common.Block supportBlock = world.BlockAccessor.GetBlock(supportPos);

        if (!BracketBlock.IsBracket(supportBlock))
        {
            failureCode = "signageandmore:requirebracket";
            return false;
        }

        return supportBlock.CanAttachBlockAt(world.BlockAccessor, block, supportPos, blockSel.Face);
    }

    public override void OnNeighbourBlockChange(
        IWorldAccessor world,
        BlockPos pos,
        BlockPos neibpos,
        ref EnumHandling handled)
    {
        handled = EnumHandling.PreventDefault;

        if (!HasBracketSupport(world, pos))
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

    private bool HasBracketSupport(IWorldAccessor world, BlockPos pos)
    {
        BlockFacing[] faces = BlockFacing.ALLFACES;
        for (int i = 0; i < faces.Length; i++)
        {
            BlockFacing face = faces[i];
            BlockPos supportPos = pos.AddCopy(face);
            Vintagestory.API.Common.Block support = world.BlockAccessor.GetBlock(supportPos);
            if (BracketBlock.IsBracket(support) && support.CanAttachBlockAt(world.BlockAccessor, block, supportPos, face.Opposite))
            {
                return true;
            }
        }

        return false;
    }
}
