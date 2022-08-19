namespace GreenVerticalBot.EntityFramework.Store
{
    /// <summary>
    /// Базовый класс сущностей, содержащих ID
    /// </summary>
    public interface IBaseEntity
    {
        public string Id { get; set; }
    }
}