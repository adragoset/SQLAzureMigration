using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data.SqlClient;

namespace SQLAzureMWUtils
{
    public static class Retry
    {
        private static string[] _sqlErrorCodes;
        private static RetryPolicy _CurrentRetryPolicy;
        public static RetryPolicy CurrentRetryPolicy
        {
            get
            {
                if (_CurrentRetryPolicy == null)
                {
                    string temp = CommonFunc.GetAppSettingsStringValue("RetryCount");
                    int RetryCount = Convert.ToInt32(temp.Length == 0 ? "5" : temp);

                    temp = CommonFunc.GetAppSettingsStringValue("RetryMinimunDelay");
                    string RetryMinimunDelay = temp.Length == 0 ? "00:00:00.1" : temp;

                    temp = CommonFunc.GetAppSettingsStringValue("RetryMaximunDelay");
                    string RetryMaximunDelay = temp.Length == 0 ? "00:00:05" : temp;

                    temp = CommonFunc.GetAppSettingsStringValue("RetryInitialDelay");
                    string RetryInitialDelay = temp.Length == 0 ? "00:00:00.5" : temp;

                    _CurrentRetryPolicy = new RetryPolicy(RetryCount, TimeSpan.Parse(RetryMinimunDelay), TimeSpan.Parse(RetryMaximunDelay), TimeSpan.Parse(RetryInitialDelay));
                }
                return _CurrentRetryPolicy;
            }
        }

        public static bool IsTransient(Exception ex)
        {
            if (ex != null)
            {
                if (_sqlErrorCodes == null)
                {
                    string temp = CommonFunc.GetAppSettingsStringValue("BCPSQLAzureErrorCodesRetry");
                    int start = temp.IndexOf('(');
                    int end = temp.IndexOf(')');
                    _sqlErrorCodes = temp.Substring(start + 1, end - start - 1).Split('|');
                    if (_sqlErrorCodes == null || _sqlErrorCodes.Length < 1)
                    {
                        _sqlErrorCodes = "64|233|08001|08S01|10053|10054|10060|40001|40143|40174|40197|40501|40544|40549|40550|40551|40552|40553|40613|40615".Split('|');
                    }
                }

                SqlException sqlException;
                if ((sqlException = ex as SqlException) != null)
                {
                    // Enumerate through all errors found in the exception.
                    foreach (SqlError err in sqlException.Errors)
                    {
                        foreach (string errorCode in _sqlErrorCodes)
                        {
                            if (err.Number.ToString().Equals(errorCode))
                            {
                                return true;
                            }
                        }
                    }
                }
                else if (ex is TimeoutException)
                {
                    return true;
                }
            }

            return false;
        }

        public static void ExecuteRetryAction(Action action)
        {
            ExecuteRetryAction(() => { action(); return default(object); }, null);
        }

        public static void ExecuteRetryAction(Action action, Action onFailure)
        {
            ExecuteRetryAction(() => { action(); return default(object); }, () => { onFailure(); return default(object); });
        }

        public static TResult ExecuteRetryAction<TResult>(Func<TResult> func, Func<TResult> onFailFunc)
        {
            int retryCount = 0;
            TimeSpan delay = TimeSpan.Zero;

            while (true)
            {
                try
                {
                    return func();
                }
                catch (RetryLimitExceededException limitExceededEx)
                {
                    // The user code can throw a RetryLimitExceededException to force the exit from the retry loop.
                    // The RetryLimitExceeded exception can have an inner exception attached to it. This is the exception
                    // which we will have to throw up the stack so that callers can handle it.
                    if (limitExceededEx.InnerException != null)
                    {
                        throw limitExceededEx.InnerException;
                    }
                    else
                    {
                        return default(TResult);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    if (!(IsTransient(ex) && CurrentRetryPolicy.ShouldRetry(retryCount++, ex, out delay)))
                    {
                        throw;
                    }
                }

                // Perform an extra check in the delay interval. Should prevent from accidentally ending up with the value of -1 that will block a thread indefinitely. 
                // In addition, any other negative numbers will cause an ArgumentOutOfRangeException fault that will be thrown by Thread.Sleep.
                if (delay.TotalMilliseconds < 0)
                {
                    delay = TimeSpan.Zero;
                }

                if (retryCount > 1) //  || !this.RetryStrategy.FastFirstRetry)
                {
                    Thread.Sleep(delay);
                }

                if (onFailFunc != null) onFailFunc();
            }
        }
    }
}