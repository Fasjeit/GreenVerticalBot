namespace GreenVerticalBot.RestModels
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Классы подписи
    /// </summary>
    [DataContract]
    public enum SignatureType
    {
        [EnumMember]
        XMLDSig,

        [EnumMember]
        GOST3410,

        [EnumMember]
        CAdES,

        [EnumMember]
        PDF,

        [EnumMember]
        MSOffice,

        [EnumMember]
        CMS
    }
}