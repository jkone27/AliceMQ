using System;
using System.Linq.Expressions;

namespace AliceMQ.ExtensionMethods
{
    public static class ActionExtensions
    {
        public static void TryDo<T1, T2, T3>(
            this Expression<Action<T1,T2,T3>> action, 
            T1 a, 
            T2 b, 
            T3 c, 
            Action<Exception> error) 
        {
            try
            {
                action.Compile()(a, b, c);
            }
            catch (Exception ex)
            {
                error(ex);
            }
        }

    }
}