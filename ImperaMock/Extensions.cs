using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImperaMock
{
    public static class Extensions
    {
        public static async Task Resolve(this Task<VoidCall> target)
        {
            var call = await target;
            call.Resolve();
        }
        public static async Task ResolveTo<T>(this Task<Call<T>> target, T value)
        {
            var call = await target;
            call.ResolveTo(value);
        }
    }
}
