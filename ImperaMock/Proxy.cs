using System.Reflection;
using System.Threading;

namespace ImperaMock
{
    public class Proxy<T> : DispatchProxy
        where T : class
    {
        private bool _expectNext;
        private object? _returnNext;
        private Call _actualNext;

        private readonly List<Call> _funcCalls = new();

        public static Proxy<T> Create()
        {
            var proxy = Create<T, Proxy<T>>();

            new ThreadLocal<Proxy<T>>();

            return proxy as Proxy<T>;
        }

        public T Mocked => this as T;

        public async Task<VoidCall> Expect(Func<T, Task> call)
        {
            _expectNext = true;
            await call(this as T);

            var actualNext = _actualNext;

            return new VoidCall(actualNext.Method, actualNext.Args, () => actualNext.SetResult(null));
        }

        public async Task<Call<TOut>> Expect<TOut>(Func<T, Task<TOut>> call)
        {
            _expectNext = true;
            await call(this as T);

            var actualNext = _actualNext;

            return new Call<TOut>(actualNext.Method, actualNext.Args, value => actualNext.SetResult(value));
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            var type = targetMethod?.ReturnType.GetGenericArguments().FirstOrDefault();

            if (_expectNext)
            {
                _expectNext = false;

                var call = _funcCalls
                    .Where(c => c.Method == targetMethod)
                    .Where(c => c.Args.Length == args.Length)
                    .FirstOrDefault();

                if (call is null) throw new Exception($"Expected call to {targetMethod.Name}");

                _funcCalls.Remove(call);

                _actualNext = call;

                if (type is not null)
                {
                    return typeof(Task)
                        .GetMethod(nameof(Task.FromResult))
                        !.MakeGenericMethod(type)
                        .Invoke(null, new object?[] { default });
                }

                return Task.CompletedTask;
            }

            var method = typeof(Proxy<T>)
                .GetMethod(nameof(RecordFuncCall), BindingFlags.NonPublic | BindingFlags.Instance)
                !.MakeGenericMethod(type ?? typeof(object));

            return method
                .Invoke(this, new object?[] { targetMethod, args });
        }

        private object? RecordFuncCall<T>(MethodInfo? targetMethod, object?[]? args)
        {
            var task = new TaskCompletionSource<T>();

            _funcCalls.Add(new(targetMethod, args, (object? t) => task.SetResult((T)t)));

            return task.Task;
        }

        private record Call(MethodInfo? Method, object?[]? Args, Action<object?> SetResult);
    }

    public record Call<TOut>(MethodInfo? Method, object?[]? Args, Action<TOut> ResolveTo);

    public record VoidCall(MethodInfo? Method, object?[]? Args, Action Resolve);
}