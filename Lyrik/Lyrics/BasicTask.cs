using System;
using System.Threading;

namespace Lyrik.Lyrics
{
    internal class BasicTask : IDisposable
    {
        // 用来取消任务
        private CancellationTokenSource _cts;
        // 用来暂停/继续任务
        private ManualResetEventSlim _mres;

        protected void TaskBegin()
        {
            _cts = new CancellationTokenSource();
            _mres = new ManualResetEventSlim(true);
        }

        protected bool TaskCanProceed()
        {
            try
            {
                _mres.Wait(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            return true;
        }

        public void Pause()
        {
            if (_mres.IsSet)
            {
                _mres.Reset();
            }
        }

        public void Resume()
        {
            if (!_mres.IsSet)
            {
                _mres.Set();
            }
        }

        public void Halt()
        {
            _cts?.Cancel();
        }

        public void Dispose()
        {
            _cts.Dispose();
            _mres.Dispose();
        }
    }
}
