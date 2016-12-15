using System;
using System.Threading;

namespace Lyrik.Lyrics
{
    class BasicTask : IDisposable
    {
        // 用来取消任务
        private CancellationTokenSource cts;
        // 用来暂停/继续任务
        private ManualResetEventSlim mres;

        protected void taskBegin()
        {
            cts = new CancellationTokenSource();
            mres = new ManualResetEventSlim(true);
        }

        protected void taskProceed()
        {
            mres.Wait(cts.Token);
        }

        public void pause()
        {
            if (mres.IsSet)
            {
                mres.Reset();
            }
        }

        public void resume()
        {
            if (!mres.IsSet)
            {
                mres.Set();
            }
        }

        public void halt()
        {
            cts.Cancel();
        }

        public void Dispose()
        {
            cts.Dispose();
            mres.Dispose();
        }
    }
}
