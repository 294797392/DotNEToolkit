using DotNEToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkitConsole
{
    public static class TestBytePool
    {
        public static void Test()
        {
            BytePool pool = BytePool.Create(3 * 1024 * 1024, 4096);
            ByteBlock block1 = pool.Obtain(Convert.ToInt32(2.5 * 1024 * 1024));
            ByteBlock block2 = pool.Obtain(Convert.ToInt32(2.5 * 1024 * 1024));

            pool.Recycle(block1);
            pool.Recycle(block2);

            ByteBlock block3 = pool.Obtain(Convert.ToInt32(2.5 * 1024 * 1024));
            ByteBlock block4 = pool.Obtain(Convert.ToInt32(2.5 * 1024 * 1024));

            ByteBlock block5 = pool.Obtain(Convert.ToInt32(2.5 * 1024 * 1024));
        }
    }
}
