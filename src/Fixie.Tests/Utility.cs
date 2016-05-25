namespace Fixie.Tests
{
    using System.Runtime.CompilerServices;
    using Fixie.Internal;

    public static class Utility
    {
        public static string FullName<T>()
            => typeof(T).FullName;

        public static string At<T>(string method, [CallerFilePath] string path = null)
        {
            return $"   at {FullName<T>().Replace("+", ".")}.{method} in {path}:line #";
        }

        public static string PathToThisFile([CallerFilePath] string path = null)
            => path;

        public static void Run<TSampleTestClass>(object listener, Convention convention)
        {
            var sampleTestClass = typeof(TSampleTestClass);

            using (var bus = new Bus(listener))
                new Runner(bus).RunTypes(sampleTestClass.Assembly, convention, sampleTestClass);
        }
    }
}