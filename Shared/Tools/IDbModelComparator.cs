using AnjUx.Shared.Interfaces;

namespace AnjUx.Shared.Tools
{
    public class IDbModelComparator : IEqualityComparer<IDbModel>
    {
        public bool Equals(IDbModel? x, IDbModel? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return x.ID == y.ID;
        }

        public int GetHashCode(IDbModel obj)
        {
            return obj.ID.GetHashCode();
        }
    }
}
