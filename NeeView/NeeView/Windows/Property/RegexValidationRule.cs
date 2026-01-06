using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace NeeView.Windows.Property
{
    /// <summary>
    /// 正規表現文法チェック
    /// </summary>
    class RegexValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                var pattern = value as string;
                if (!string.IsNullOrEmpty(pattern))
                {
                    var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }
                return new ValidationResult(true, null);
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, ex.Message);
            }
        }
    }
}
