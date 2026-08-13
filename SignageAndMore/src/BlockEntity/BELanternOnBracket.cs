using SignageAndMore.Block;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace SignageAndMore.BlockEntity;

/// <summary>
/// Vanilla BELantern.OnTesselation draws its own mesh and returns without calling base
/// or BlockEntityBehavior.OnTesselation, so there is no API hook to shift that mesh.
/// This subclass is assigned only to lantern-*-down; without a supporting bracket it
/// behaves exactly like vanilla. With a bracket, the mesh is pulled 0.5 toward the wall
/// to line up with hanging signs (which use shapebytype offset, unavailable on lanterns).
/// </summary>
public class BELanternOnBracket : BELantern
{
    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
    {
        Vec3f? offset = BracketBlock.GetLanternMeshOffset(Api.World, Pos);
        if (offset == null)
        {
            return base.OnTesselation(mesher, tesselator);
        }

        return base.OnTesselation(new TranslatedMeshPool(mesher, offset), tesselator);
    }

    private sealed class TranslatedMeshPool : ITerrainMeshPool
    {
        private readonly ITerrainMeshPool inner;
        private readonly Vec3f offset;

        public TranslatedMeshPool(ITerrainMeshPool inner, Vec3f offset)
        {
            this.inner = inner;
            this.offset = offset;
        }

        public void AddMeshData(MeshData data, int lodLevel = 0)
        {
            inner.AddMeshData(Translated(data), lodLevel);
        }

        public void AddMeshData(MeshData data, float[] tfMatrix, int lodLevel = 0)
        {
            inner.AddMeshData(Translated(data), tfMatrix, lodLevel);
        }

        public void AddMeshData(MeshData data, ColorMapData colorMapData, int lodLevel = 0)
        {
            inner.AddMeshData(Translated(data), colorMapData, lodLevel);
        }

        private MeshData Translated(MeshData data)
        {
            return data.Clone().Translate(offset);
        }
    }
}
