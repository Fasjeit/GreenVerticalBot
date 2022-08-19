using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GreenVerticalBot.RestModels
{
    /// <summary>
    /// Результат проверки подписи и/или сертификата для Rest
    /// </summary>
    public class VerificationResultRest
    {
        /// <summary>
        /// Суммарная информация о результатах проверки подписи
        /// </summary>
        public string Message;

        /// <summary>
        /// Результат проверки подписи
        /// </summary>
        public bool Result;

        /// <summary>
        /// Сертификат
        /// </summary>
        public byte[] SignerCertificate;

        /// <summary>
        /// Набор сведений о сертификате подписи
        /// </summary>
        public Dictionary<CertificateInfoParams, string> SignerCertificateInfo;

        /// <summary>
        /// Дополнительные сведения о подписи
        /// </summary>
        public Dictionary<SignatureInfoParams, string> SignatureInfo;

        /// <summary>
        /// Сведения о доп. проверках сертификата
        /// </summary>
        public List<CertificateVerificationResult> AdditionalCertificateResult;
    }

    public class CertificateVerificationResult
    {
        [DataMember]
        public string AssemblyName;

        [DataMember]
        public string PluginDescription;

        [DataMember]
        public bool bResult;

        [DataMember]
        public List<string> ErrorsList;
    }

    public enum CertificateInfoParams
    {
        /// <summary>
        /// X500 имя субъекта
        /// </summary>
        [EnumMember]
        SubjectName,

        /// <summary>
        /// X500 имя издателя
        /// </summary>
        [EnumMember]
        IssuerName,

        /// <summary>
        /// Действителен до
        /// </summary>
        [EnumMember]
        NotAfter,

        /// <summary>
        /// Действителен с
        /// </summary>
        [EnumMember]
        NotBefore,

        /// <summary>
        /// Серийный номер
        /// </summary>
        [EnumMember]
        SerialNumber,

        /// <summary>
        /// Отпечаток
        /// </summary>
        [EnumMember]
        Thumbprint,

        /// <summary>
        /// Идентификатор ключа
        /// </summary>
        [EnumMember]
        KeyIdentifier,
    }
    public enum SignatureInfoParams
    {
        /// <summary>
        /// Тип подписи CAdES
        /// </summary>
        [EnumMember]
        CAdESType,

        /// <summary>
        /// Время подписи из штампа.
        /// </summary>
        [EnumMember]
        SigningTime,

        /// <summary>
        /// Время подписи по локальным часам.
        /// </summary>
        [EnumMember]
        LocalSigningTime,

        /// <summary>
        /// Отображаемые данные.
        /// </summary>
        [EnumMember]
        DataToBeSigned,
    }
}
