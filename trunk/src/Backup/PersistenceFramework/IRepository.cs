using System;
using System.Collections.Generic;
using System.Text;

namespace PersistenceFramework
{
    public interface IRepository<T>
    {
        void Save(T instance);
    }
}
