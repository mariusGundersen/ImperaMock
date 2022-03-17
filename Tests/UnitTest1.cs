using System.Threading.Tasks;
using ImperaMock;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test1()
        {
            var context = Proxy<IContext>.Create();

            var task = Method(context.Mocked);

            var call = await context.Expect(c => c.Action(10));
            call.Resolve();

            await context.Expect(c => c.Function("first")).ResolveTo("last");

            var funcCall = await context.Expect(c => c.Function("test"));
            funcCall.ResolveTo("hi");

            await context.Expect(c => c.Action(2)).Resolve();
            await context.Expect(c => c.Action(1)).Resolve();

            await task;
        }

        public static async Task Method(IContext context)
        {
            await context.Action(10);

            var value = await context.Function("first");

            if (value == "last")
            {
                var result = await context.Function("test");
            }

            var task1 = context.Action(1);
            var task2 = context.Action(2);

            await Task.WhenAll(task1, task2);
        }
    }
}