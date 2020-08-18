using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNETClient
{
    /// <summary>
    /// await和async关键字用法
    /// 
    /// 1. 使用await的函数必须使用async关键字修饰函数 
    /// 2. await后面跟Task实例 
    /// 3. 使用async关键字修饰的函数的名字要使用Async结尾，表示这是一个异步函数  
    /// </summary>
    public class AwaitDemo
    {
        private class Data
        {
            public string Name { get; set; }
        }

        /// <summary>
        /// 异步加载数据
        /// </summary>
        public async void LoadDataAsync()
        {
            List<Data> datas = await Task<List<Data>>.Factory.StartNew(() => 
            {
                Thread.Sleep(3000);

                List<Data> result = new List<Data>();

                for (int i = 0; i < 500; i++)
                {
                    result.Add(new Data() { Name = Guid.NewGuid().ToString() });
                }

                Console.WriteLine("LoadDataAsyncTask Completed");

                return result;
            });

            Console.WriteLine("LoadDataAsync Completed");
        }
    }
}