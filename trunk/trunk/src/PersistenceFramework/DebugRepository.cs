using System;
using System.Collections.Generic;
using System.Text;

namespace PersistenceFramework
{
    public class DebugRepository<T> : IRepository<T>
    {
        public void Save(T instance)
        {
            Console.WriteLine("Persistence: Saving " + instance.ToString());
        }
    }
}