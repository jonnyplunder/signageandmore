using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace SignageAndMore.Block;

/// <summary>
/// Hanging lanterns share vanilla boxes; when on a bracket the boxes follow the mesh offset.
/// </summary>
public class BlockLanternOnBracket : BlockLantern
{
    public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
    {
        return Offset(blockAccessor, pos, base.GetSelectionBoxes(blockAccessor, pos));
    }

    public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
    {
        return Offset(blockAccessor, pos, base.GetCollisionBoxes(blockAccessor, pos));
    }

    public override Cuboidf[] GetParticleCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
    {
        return Offset(blockAccessor, pos, base.GetParticleCollisionBoxes(blockAccessor, pos));
    }

    private static Cuboidf[] Offset(IBlockAccessor accessor, BlockPos pos, Cuboidf[] boxes)
    {
        Vec3f? offset = BracketBlock.GetHangOffset(accessor, pos);
        if (offset == null || boxes == null || boxes.Length == 0)
        {
            return boxes ?? [];
        }

        Cuboidf[] shifted = new Cuboidf[boxes.Length];
        for (int i = 0; i < boxes.Length; i++)
        {
            shifted[i] = boxes[i].OffsetCopy(offset.X, offset.Y, offset.Z);
        }

        return shifted;
    }
}
