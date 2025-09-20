using Microsoft.Maui.Controls;
using System;
using System.Globalization;
using MainToDoList;
namespace MainToDoList
{
    public class StrikeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // اگر تسک تکمیل شده باشد، از Strikethrough (خط زدن) استفاده کن
            return value is bool isCompleted && isCompleted
                ? TextDecorations.Strikethrough
                : TextDecorations.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // اگر decoration برابر Strikethrough باشد، خروجی true برگردان
            return value is TextDecorations decorations && decorations == TextDecorations.Strikethrough;
        }
    }
}