using Interface;
using Src.Strategy;

namespace Lab3.Tests.TestHelpers
{
    public class TestForkFactory : IForksFactory<Fork>
    {
        private int _counter = 0;
        public IFork Create()
        {
            _counter++;
            return new Fork(_counter);
        }
    }
}
