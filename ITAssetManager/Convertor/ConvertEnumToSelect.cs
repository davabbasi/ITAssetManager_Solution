using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITAssetManager.Convertor
{
    public static class ConvertEnumToSelect
    {
        public static SelectList ToSelectList<TEnum>() where TEnum : struct, Enum
        {
            var items = Enum.GetValues<TEnum>()
                .Select(x =>
                {
                    var member = typeof(TEnum)
                        .GetMember(x.ToString())
                        .First();

                    var display = member
                        .GetCustomAttribute<DisplayAttribute>();

                    return new
                    {
                        Value = Convert.ToInt32(x),
                        Text = display?.Name ?? x.ToString()
                    };
                })
                .ToList();

            return new SelectList(items, "Value", "Text");
        }
    }
}
