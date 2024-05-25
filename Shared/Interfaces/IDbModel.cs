namespace AnjUx.Shared.Interfaces
{
    public interface IDbModel
    {
        public long? ID { get; set; }
        public string GetDbHashCodeForDropdown();
    }
}
