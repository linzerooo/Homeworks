using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Shablonizator;

public static partial class TemplateStringExtension
{
    public static string Substitute(this string template, object? obj)
    {
        obj ??= new object();
    
        var result = new StringBuilder(template);
    
        while (TemplateRegexes.PropertyTemplateRegex.IsMatch(result.ToString()))
            result = new StringBuilder(result.ToString().SubstituteProperty(obj));

        while (TemplateRegexes.FullIfTemplateRegex.IsMatch(result.ToString()))
            result = new StringBuilder(result.ToString().SubstituteIfConstruction(obj));

        while (TemplateRegexes.FullForTemplateRegex.IsMatch(result.ToString()))
            result = new StringBuilder(result.ToString().SubstituteForConstruction(obj));
    
        return result.ToString();
    }

    private static string SubstituteProperty(this string template, object obj)
    {
        string result;
        try
        {
            var propertyTemplate = TemplateRegexes.PropertyTemplateRegex.Match(template).Value;
            var propertyName = propertyTemplate[2..^1];
            var propertyValue = obj.GetType().GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)?.GetValue(obj)?.ToString();
            result = template.Replace(propertyTemplate, propertyValue);
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при замене свойства: {ex.Message}");
        }

        return result;
    }
    
    private static string SubstituteIfConstruction(this string template, object obj)
    {
        string result;
        try
        {
            var ifTemplate = TemplateRegexes.IfTemplateRegex.Match(template).Value[5..];
            var thenValue = TemplateRegexes.ThenTemplateRegex.Match(template).Value[6..^1];
            var elseValue = TemplateRegexes.ElseTemplateRegex.Match(template).Value[6..^1];

            var fullIfTemplate = TemplateRegexes.FullIfTemplateRegex.Match(template).Value;

            var propertyName = MyRegex().Match(ifTemplate).Value;

            var compareItem1 = (double?)obj.GetType()
                .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                ?.GetValue(obj);

            var comparer = TemplateRegexes.BoolExpressionDictionary.Keys.Where(x => ifTemplate.Contains(x)).Max() ?? "";
            var compareItem2 = double.Parse(MyRegex1().Matches(ifTemplate).FirstOrDefault(x => x.Value != "")?.Value!);

            var ifExpressionResult = TemplateRegexes.BoolExpressionDictionary[comparer](compareItem1, compareItem2);

            var resultValue = ifExpressionResult ? thenValue : elseValue;
            result = template.Replace(fullIfTemplate, resultValue);
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при замене if конструкции: {ex.Message}");
        }

        return result;
    }

    private static string SubstituteForConstruction(this string template, object obj)
    {
        string result;
        try
        {
            var fullForTemplate = TemplateRegexes.FullForTemplateRegex.Match(template).Value;
            var forTemplate = TemplateRegexes.ForTemplateRegex.Match(fullForTemplate).Value[1..^1];
            var forBodyTemplate = fullForTemplate[(forTemplate.Length + 4)..^1];
            var forPropertiesTemplate = TemplateRegexes.ForPropertyTemplate.Matches(fullForTemplate)
                .Select(x => x.Value).ToArray();
            var forCollectionTemplate = forTemplate.Split().Last();

            var templateStart = template.Split(fullForTemplate).First();
            var templateEnd = template.Split(fullForTemplate).Last();

            var templateBuilder = new StringBuilder(templateStart);

            var collection = obj.GetType()
                .GetProperty(forCollectionTemplate,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)?
                .GetValue(obj) as IEnumerable<object> ?? new List<object>();

            var propertyNameRegex = MyRegex2();

            foreach (var item in collection)
            {
                var forBody = forBodyTemplate;
                foreach (var propertyTemplate in forPropertiesTemplate)
                {
                    var propertyName = propertyNameRegex.Match(propertyTemplate).Value[1..^1];
                    var propertyValue = item.GetType()
                        .GetProperty(propertyName,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)?
                        .GetValue(item)?.ToString() ?? string.Empty;

                    forBody = forBody.Replace(propertyTemplate, propertyValue);
                }

                templateBuilder.Append($"{forBody}\n");
            }

            templateBuilder.Append(templateEnd);
            result = templateBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при замене for конструкции: {ex.Message}");
        }

        return result;
    }

    [GeneratedRegex("\\w*")]
    private static partial Regex MyRegex();
    [GeneratedRegex("\\d*")]
    private static partial Regex MyRegex1();
    [GeneratedRegex("\\..*")]
    private static partial Regex MyRegex2();
}