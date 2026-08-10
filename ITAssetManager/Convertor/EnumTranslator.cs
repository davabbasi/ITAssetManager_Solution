using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ITAssetManager.Convertor
{
    public static class EnumTranslator
    {
        public static string GetDisplayName(this Enum value)
        {
            var member = value.GetType()
                .GetMember(value.ToString())
                .FirstOrDefault();

            var displayAttribute = member?
                .GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.Name ?? value.ToString();
        }
    }
}
