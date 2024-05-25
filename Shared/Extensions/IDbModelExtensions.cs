using AnjUx.Shared.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace AnjUx.Shared.Extensions
{
    public static class IDbModelExtensions
    {
        public static bool IsPersisted([NotNullWhen(true)] this IDbModel? model)
        {
            return model != null && model.ID != null;
        }
    }
}
