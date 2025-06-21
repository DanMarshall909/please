namespace TUnit
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class TestFixtureAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public sealed class TestAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public sealed class SetUpAttribute : System.Attribute { }

    public static class Assert
    {
        public static void True(bool condition, string? message = null)
        {
            if (!condition) throw new System.Exception(message ?? "Expected true but was false");
        }

        public static void False(bool condition, string? message = null)
        {
            if (condition) throw new System.Exception(message ?? "Expected false but was true");
        }

        public static void Equal<T>(T expected, T actual, string? message = null)
        {
            if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(expected, actual))
                throw new System.Exception(message ?? $"Expected {expected} but was {actual}");
        }

        public static void NotEqual<T>(T notExpected, T actual, string? message = null)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(notExpected, actual))
                throw new System.Exception(message ?? $"Did not expect {notExpected}");
        }

        public static void Contains(string substring, string actual, string? message = null)
        {
            if (actual == null || !actual.Contains(substring))
                throw new System.Exception(message ?? $"Expected '{actual}' to contain '{substring}'");
        }

        public static T Throws<T>(System.Action action) where T : System.Exception
        {
            try
            {
                action();
            }
            catch (System.Exception ex)
            {
                if (ex is T typed)
                    return typed;
                throw new System.Exception($"Expected exception of type {typeof(T)} but got {ex.GetType()}");
            }
            throw new System.Exception($"Expected exception of type {typeof(T)} but no exception was thrown");
        }

        public static async System.Threading.Tasks.Task<T> ThrowsAsync<T>(System.Func<System.Threading.Tasks.Task> action) where T : System.Exception
        {
            try
            {
                await action();
            }
            catch (System.Exception ex)
            {
                if (ex is T typed)
                    return typed;
                throw new System.Exception($"Expected exception of type {typeof(T)} but got {ex.GetType()}");
            }
            throw new System.Exception($"Expected exception of type {typeof(T)} but no exception was thrown");
        }
    }
}
