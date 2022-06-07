using System;
using System.Threading;

namespace TestShell
{
    public class APISimulator
    {
        private readonly RandomGenerator r = new RandomGenerator();

        public DateTime MethodA()
        {
            int random = r.Next(0, 12);

            //if (random > 8)
            //{
            //    throw new InvalidOperationException("Test");
            //}

            Thread.Sleep(r.Next(0, 12) * 100);
            return DateTime.Now;
        }
    }
}