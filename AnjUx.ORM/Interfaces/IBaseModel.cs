
using AnjUx.Shared.Interfaces;

namespace AnjUx.ORM.Interfaces
{
    public interface IBaseModel : IDbModel
    {
        public DateTime? Updated { get; set; }
        public string? UpdateUser { get; set; }
        public DateTime? Inserted { get; set; }
        public string? InsertUser { get; set; }
    }
}
