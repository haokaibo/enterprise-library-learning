using System;

namespace PersistenceFramework
{
    public interface IValidator<T>
    {
        bool IsValid(T instance);
    }
}
