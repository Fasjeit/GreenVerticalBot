namespace GreenVerticalBot.EntityFramework.Entities.Tasks
{
    public static class TaskType
    {
        /// <summary>
        /// Нетипизированный запрос
        /// </summary>
        public const string NoType = "no_type";
        /// <summary>
        /// Запрос утрверждения
        /// </summary>
        public const string RequestClaim = "request_claim";

        /// <summary>
        /// Запрос доступа к чату
        /// </summary>
        public const string RequestChatAccess = "request_chat_access";
    }
}
