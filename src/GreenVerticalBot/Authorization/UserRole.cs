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

        [Description("Права оператора в боте")]
        Operator,

        [Description("Житель 9-го корпуса")]
        AccessToB9_ex10,
        [Description("Житель 5-го корпуса")]
        AccessToB5_ex3,
        [Description("Житель 7-го корпуса")]
        AccessToB7_ex2,
        [Description("Житель 10-го корпуса")]
        AccessToB10_ex11,
        [Description("Житель 11-го корпуса")]
        AccessToB11_ex1,
        [Description("Житель 8-го корпуса")]
        AccessToB8_ex9,

        [Description("Права оператора доступа к чату 9 корпуса")]
        OperatorAccessToB9_ex10,
        [Description("Права оператора доступа к чату 5 корпуса")]
        OperatorAccessToB5_ex3,
        [Description("Права оператора доступа к чату 7 корпуса")]
        OperatorAccessToB7_ex2,
        [Description("Права оператора доступа к чату 10 корпуса")]
        OperatorAccessToB10_ex11,
        [Description("Права оператора доступа к чату 11 корпуса")]
        OperatorAccessToB11_ex1,
        [Description("Права оператора доступа к чату 8 корпуса")]
        OperatorAccessToB8_ex9,
    }
}