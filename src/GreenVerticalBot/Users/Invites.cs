namespace GreenVerticalBot.Users
{
    /// <summary>
    /// Инвайт в закрытый чат
    /// </summary>
    internal class Invite
    {
        /// <summary>
        /// Идентификатор инвайта
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Описание инвайта
        /// </summary>
        public string Despription { get; set; }

        /// <summary>
        /// Значение (тело) инвайта
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Связанный запрос на выдачу инвайта
        /// </summary>
        public string TaskId { get; set; }
    }
}