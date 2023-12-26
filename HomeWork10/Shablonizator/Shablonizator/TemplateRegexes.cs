using System.Text.RegularExpressions;

namespace Shablonizator
{
    public static partial class TemplateRegexes
    {
        // Matches property templates like @{property}
        public static readonly Regex PropertyTemplateRegex = MyPropertyTemplateRegex();

        // Matches if templates like @{if(condition)}
        public static readonly Regex IfTemplateRegex = MyIfTemplateRegex();

        // Matches then templates like @then{action}
        public static readonly Regex ThenTemplateRegex = MyThenTemplateRegex();

        // Matches else templates like @else{action}
        public static readonly Regex ElseTemplateRegex = MyElseTemplateRegex();

        // Matches full if templates like @{if(condition)}@then{action}@else{action}
        public static readonly Regex FullIfTemplateRegex = MyFullIfTemplateRegex();

        // Matches full for templates like @for(condition){action}
        public static readonly Regex FullForTemplateRegex = MyFullForTemplateRegex();

        // Matches for templates like @for(condition)
        public static readonly Regex ForTemplateRegex = MyForTemplateRegex();

        // Matches for property templates like @{object.property}
        public static readonly Regex ForPropertyTemplate = MyForPropertyTemplate();

        // Dictionary for boolean expressions
        public static readonly  Dictionary<string, Func<IComparable?, IComparable?, bool>> BoolExpressionDictionary = new()
        {
            { ">", (x,y) => x?.CompareTo(y) > 0},
            {"<", (x,y) => x?.CompareTo(y) < 0},
            {"<=", (x,y) => x?.CompareTo(y) <= 0},
            {">=", (x,y) => x?.CompareTo(y) >= 0},
            {"==", (x,y) => x?.CompareTo(y) == 0}
        };

        [GeneratedRegex("@{\\w*}")]
        private static partial Regex MyPropertyTemplateRegex();
        [GeneratedRegex(@"@{if\(.*\)}")]
        private static partial Regex MyIfTemplateRegex();
        [GeneratedRegex("@then{.\\w*}")]
        private static partial Regex MyThenTemplateRegex();
        [GeneratedRegex("@else{\\w*}")]
        private static partial Regex MyElseTemplateRegex();
        [GeneratedRegex("@{if(.*)}")]
        private static partial Regex MyFullIfTemplateRegex();
        [GeneratedRegex("@for(.*){.*}")]
        private static partial Regex MyFullForTemplateRegex();
        [GeneratedRegex(@"@for\(.*\)")]
        private static partial Regex MyForTemplateRegex();
        [GeneratedRegex(@"@{\w*.\w*}")]
        private static partial Regex MyForPropertyTemplate();
    }
}