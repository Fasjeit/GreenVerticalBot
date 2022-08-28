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
        [Description("Права оператора в боте")]
        Operator,

        [Description("Житель 9-го корпуса")]
        AccessToB9_ex10Chat,

        [Description("Права оператора доступа к чату 9 корпуса")]
        OperatorAccessToB9_ex10Chat
    }
}