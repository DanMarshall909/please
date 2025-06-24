using System.Linq.Expressions;
using System.Reflection;
using Please.Domain.Entities;
using Shouldly;

namespace Please.TestUtilities.Shouldly;

public static class ShouldlyExtensions
{
    public static void ShouldBeEquivalentToExcluding(this ScriptResponse expected, ScriptResponse scriptResponse,
        params Expression<Func<ScriptResponse, object>>[] excludeProperties)
    {
        var clone = scriptResponse with { };
        foreach (var exclude in excludeProperties)
            if (exclude.Body is MemberExpression member)
            {
                var prop = (PropertyInfo)member.Member;
                prop.SetValue(clone, prop.GetValue(expected));
            }
            else if (exclude.Body is UnaryExpression unary && unary.Operand is MemberExpression unaryMember)
            {
                var prop = (PropertyInfo)unaryMember.Member;
                prop.SetValue(clone, prop.GetValue(expected));
            }
            else
            {
                throw new ArgumentException($"Unsupported expression: {exclude}");
            }

        expected.ShouldBeEquivalentTo(clone);
    }
}
