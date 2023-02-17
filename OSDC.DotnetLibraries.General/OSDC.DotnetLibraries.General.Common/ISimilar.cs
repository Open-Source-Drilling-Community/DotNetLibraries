
namespace OSDC.DotnetLibraries.General.Common
{
    public interface ISimilar
    {
        bool GE(ISimilar v);
        bool GT(ISimilar v);
        bool LE(ISimilar v);
        bool LT(ISimilar v);
        bool EQ(ISimilar v);
        bool NEQ(ISimilar v); 

    }
}
