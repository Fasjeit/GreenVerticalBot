namespace GreenVerticalBot.RestModels
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Дополнительные параметры проверки подписи
    /// </summary>
    [DataContract]
    public enum VerifyParams
    {
        /// <summary>
        /// Идентификатор подписи
        /// </summary>
        [EnumMember]
        SignatureId,

        /// <summary>
        /// Порядковый номер подписи
        /// </summary>
        /// <remarks>
        /// Начиная с 1
        /// </remarks>
        [EnumMember]
        SignatureIndex,

        /// <summary>
        /// Проверить все подписи в документе
        /// </summary>
        VerifyAll,

        /// <summary>
        /// В качестве данных передан хэш
        /// </summary>
        [EnumMember]
        Hash,

        /// <summary>
        /// Имя алгоритма хэширования
        /// </summary>
        [EnumMember]
        HashAlgorithm,

        /// <summary>
        /// Проверить PKCS7
        /// </summary>
        [EnumMember]
        VerifyPKCS7,

        /// <summary>
        /// Вернуть исходный документ
        /// </summary>
        /// <remarks>
        /// Применяется только для прикреплённой
        /// подписи формата CMS
        /// </remarks>
        [EnumMember]
        ExtractContent,

        /// <summary>
        /// Проверка прикреплённой или откреплённой подписи
        /// </summary>
        IsDetached,

        /// <summary>
        /// Список плагинов для проверки
        /// </summary>
        [EnumMember]
        CertVerifiersPluginsIds,

        /// <summary>
        /// Составлять ли отчёт по пакетной проверке
        /// </summary>
        [EnumMember]
        VerifyReport,

        /// <summary>
        /// Сертификат проверки подписи
        /// </summary>
        /// <remarks>
        /// 1. Параметр используется для проверки PKCS#7 подписи, 
        /// в которой не содержится сертификат подписи.
        /// 2. Возможно будем использвоать для провеки BES подписи, 
        /// в которой содержится только ID сертификата.
        /// 3. В API сертификат передаётся отдельным параметром, но
        /// для того чтобы не переделывать длинные цепочки вызовов
        /// перекладываем сертификат в параметры (которые уже проброшены через
        /// все методы)
        /// *. Не помечаем атрибутом [EnumMember], чтобы не ломать
        /// SOAP.
        /// </remarks>
        Certificate,

        /// <summary>
        /// Получает или задает флаг, показывающий что
        /// отрицательный результат проверки сертификата с помощью 
        /// плагинов нужно класть так же в "основное сообщение об ошибке"
        /// </summary>
        SuppressAdditionalCertVerifyInfo,
    }
}