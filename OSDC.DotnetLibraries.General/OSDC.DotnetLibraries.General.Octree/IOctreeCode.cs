using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Octree
{
    public interface IOctreeCode
    {
        List<byte> DecodeToListOfByte();
        bool TryParse(string stringCode);

        bool TryParse(List<byte> list);
    }
}
