using System;

namespace AliceMQ.ExtensionMethods
{
    public static class ExceptionExtensions
    {
        public static void HandleException<T1,T2>(this Exception ex, Action<T1> one, Action<T2> two) 
            where T1 : Exception 
            where T2 : Exception
        {
            try
            {
                throw ex;
            }
            catch (T1 e)
            {
                one(e);
            }
            catch (T2 e)
            {
                two(e);
            }
        }
    }
}