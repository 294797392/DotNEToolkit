using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkitConsole
{
    public static class TestArrayPool
    {
        public static void Test()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(1000, 1);

            byte[] value1 = pool.Rent(1000);

            pool.Return(value1);
            pool.Return(value1);

            byte[] value2 = pool.Rent(1000);

            byte[] value3 = pool.Rent(1000);

            Console.ReadLine();
        }
    }
}
