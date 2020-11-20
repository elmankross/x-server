using System;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationManager.Executor.Models
{
    internal abstract class Executor : IDisposable
    {
        protected readonly CancellationToken CancellationToken;

        internal event EventHandler<string> OnMessage;
        internal event EventHandler<string> OnError;
        internal event EventHandler OnExited;


        protected Executor(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static Executor Get(CancellationToken cancellationToken,
            Downloader.Models.ApplicationExecRoot executable,
            string appBaseDirectory)
        {
            if (executable.Binary == CmdExecutor.COMMAND_PREFIX)
            {
                return new CmdExecutor(cancellationToken, executable, appBaseDirectory);
            }

            return new ProcessExecutor(cancellationToken, executable, appBaseDirectory);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal abstract Task StartAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal abstract Task StopAsync();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        protected void PushMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                OnMessage?.Invoke(this, message);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        protected void PushError(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                OnError?.Invoke(this, error);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        protected void PushExited()
        {
            OnExited?.Invoke(this, null);
        }


        /// <summary>
        /// 
        /// </summary>
        public abstract void Dispose();
    }
}
