using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace NeeView
{
    public static class NVDebug
    {
        public static void MeasureAction(Action action)
        {
            var callStack = new StackFrame(1, true);
            var sourceFile = System.IO.Path.GetFileName(callStack.GetFileName());
            int sourceLine = callStack.GetFileLineNumber();
            var sw = Stopwatch.StartNew();

            action.Invoke();

            Debug.WriteLine($"AppDispatcher.Invoke: {sourceFile}({sourceLine}):  {sw.ElapsedMilliseconds}ms");
        }

        public static TResult MeasureFunc<TResult>(Func<TResult> func)
        {
            var callStack = new StackFrame(1, true);
            var sourceFile = System.IO.Path.GetFileName(callStack.GetFileName());
            int sourceLine = callStack.GetFileLineNumber();
            var sw = Stopwatch.StartNew();

            var result = func.Invoke();

            Debug.WriteLine($"AppDispatcher.Invoke: {sourceFile}({sourceLine}):  {sw.ElapsedMilliseconds}ms");

            return result;
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string? message)
        {
            var thread = Thread.CurrentThread;
            Debug.WriteLine($"TID.{thread.ManagedThreadId}({thread.GetApartmentState()}): " + message);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(object? value) =>
            WriteLine(value?.ToString());

        [Conditional("DEBUG")]
        public static void WriteLine(object? value, string? category) =>
            WriteLine(value?.ToString(), category);

        [Conditional("DEBUG")]
        public static void WriteLine(string format, params object?[] args) =>
            WriteLine(string.Format(null, format, args));

        [Conditional("DEBUG")]
        public static void WriteLine(string? message, string? category)
        {
            if (category == null)
            {
                WriteLine(message);
            }
            else
            {
                WriteLine(category + ": " + message);
            }
        }

        [Conditional("DEBUG")]
        public static void AssertSTA()
        {
            var thread = Thread.CurrentThread;
            Debug.Assert(thread.GetApartmentState() == ApartmentState.STA);
        }

        [Conditional("DEBUG")]
        public static void AssertMTA()
        {
            var thread = Thread.CurrentThread;
            Debug.Assert(thread.GetApartmentState() == ApartmentState.MTA);
        }


        [Conditional("DEBUG")]
        public static void WriteInfo(string key, string? message)
        {
            DevTextMap.Current.SetText(key, message);
        }

        [Conditional("DEBUG")]
        public static void WriteInfo(string key, string format, params object?[] args) =>
            WriteInfo(key, string.Format(null, format, args));


        // クラスに Equals のオーバーライド（そのクラス自身で宣言されていること）を確認する。
        // これにより、EquatableAttribute を付与し忘れたクラスを検出することができる。
        [Conditional("DEBUG")]
        public static void CheckHasEqualsMethod(Type type)
        {
            Debug.Assert(type.IsClass, $"Type {type.FullName} is not a class.");
            
            // ターゲットの型自身が object.Equals(object) をオーバーライドしているかを確認する。
            // DeclaredOnly を使って継承元のメソッドは除外する（継承されたオーバーライドを誤検出しないようにする）。
            var equalsMethod = type.GetMethod(
                "Equals",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly,
                binder: null,
                types: new Type[] { typeof(object) },
                modifiers: null);

            bool declaresEquals = equalsMethod != null;

            Debug.Assert(declaresEquals, $"The Equals method is missing in the class: {type.FullName}");
        }
    }

}
