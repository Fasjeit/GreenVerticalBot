using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace GreenVerticalBot.Authorization
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum UserRole
    {
        [Description("Администратор")]
        Admin,
        [Description("Житель ЖК Зелёная вертикаль")]
        RegisteredUser,
        //[Description("Доступ к общему чату")]
        //AccessToGeneralChat,
        AccessToB9_ex10Chat,
        Operator,
        OperatorB9_ex10
    }
}