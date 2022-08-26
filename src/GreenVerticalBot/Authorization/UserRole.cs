using System.ComponentModel;

namespace GreenVerticalBot.Authorization
{
    internal enum UserRole
    {
        [Description("Администратор")]
        Admin,
        [Description("Житель ЖК Зелёная вертикаль")]
        RegisteredUser,
        [Description("Доступ к общему чату")]
        AccessToGeneralChat,
        AccessToB9_ex10Chat,
        Operator,
        OperatorB9_ex10
    }
}