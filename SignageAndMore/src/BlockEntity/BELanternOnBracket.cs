using SignageAndMore.Block;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace SignageAndMore.BlockEntity;

/// <summary>
/// BELantern draws its own mesh and skips behaviors, so a per-placement 0.5 wall
/// offset has to live here. Without a supporting bracket this is identical to vanilla.
/// </summary>
public class BELanternOnBracket : BELantern
{
    public const string ClassName = "SignageAndMore.BracketLantern";

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
    {
        if (!BracketBlock.TryGetSupportingBracket(Api.World.BlockAccessor, Pos, out _, out BracketBlock bracket))
        {
            return base.OnTesselation(mesher, tesselator);
        }

        Vec3f offset = (bracket.GetOutwardFace()?.Opposite ?? BlockFacing.NORTH).Normalf * 0.5f;
        return base.OnTesselation(new ShiftedMesh(mesher, offset), tesselator);
    }

    private sealed class ShiftedMesh : ITerrainMeshPool
    {
        private readonly ITerrainMeshPool inner;
        private readonly Vec3f offset;

        public ShiftedMesh(ITerrainMeshPool inner, Vec3f offset)
        {
            this.inner = inner;
            this.offset = offset;
        }

        public void AddMeshData(MeshData data, int lodLevel = 0)
            => inner.AddMeshData(data.Clone().Translate(offset), lodLevel);

        public void AddMeshData(MeshData data, float[] tfMatrix, int lodLevel = 0)
            => inner.AddMeshData(data.Clone().Translate(offset), tfMatrix, lodLevel);

        public void AddMeshData(MeshData data, ColorMapData colorMapData, int lodLevel = 0)
            => inner.AddMeshData(data.Clone().Translate(offset), colorMapData, lodLevel);
    }
}
