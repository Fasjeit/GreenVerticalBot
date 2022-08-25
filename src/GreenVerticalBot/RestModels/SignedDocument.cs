namespace GreenVerticalBot.RestModels
{
    /// <summary>
    /// Объект Подписанный документ.
    /// </summary>
    /// <remarks>
    /// Используется в операции проверки подписи.
    /// </remarks>
    public class SignedDocument
    {
        /// <summary>
        /// Получает или задает тип подписи.
        /// </summary>
        public SignatureType SignatureType { get; set; }

        /// <summary>
        /// Получает или задает первоначальное содержимое документа.
        /// </summary>
        public byte[] Source { get; set; }

        /// <summary>
        /// Получает или задает значение подписи.
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// Получает или задает значение сертифката.
        /// </summary>
        public byte[] Certificate { get; set; }

        /// <summary>
        /// Получает или задает параметры проверки.
        /// </summary>
        public Dictionary<VerifyParams, string> VerifyParams { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request is
        /// made during transaction creation request.
        /// </summary>
        public bool IsTransactionCreationRequest { get; set; }

        // Получает или задает идентификаторы плагинов,
        // которые надо использовать для проверки.
        public List<int> CertVerifiersPluginsIds { get; set; }
    }
}