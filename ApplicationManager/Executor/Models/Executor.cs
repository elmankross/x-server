using System;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationManager.Executor.Models
{
    internal abstract class Executor : IDisposable
    {
        protected readonly CancellationToken CancellationToken;
        protected readonly Storage.Models.StorageEnv Environment;

        internal event EventHandler<string> OnMessage;
        internal event EventHandler<string> OnError;
        internal event EventHandler OnExited;


        protected Executor(CancellationToken cancellationToken, Storage.Models.StorageEnv env)
        {
            CancellationToken = cancellationToken;
            Environment = env;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static Executor Get(CancellationToken cancellationToken,
            Downloader.Models.ApplicationExecRoot executable,
            Storage.Models.StorageEnv env)
        {
            return executable.Binary switch
            {
                CmdExecutor.COMMAND_PREFIX => new CmdExecutor(cancellationToken, executable, env),
                _ => new ProcessExecutor(cancellationToken, executable, env),
            };
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
        /// <returns></returns>
        protected string ReplaceWithEnvironment(string source)
        {
            if ((source?.Length ?? 0) == 0)
            {
                return source;
            }

            foreach (var env in Environment)
            {
                source = source.Replace(env.Key, env.Value);
            }

            return source;
        }


        /// <summary>
        /// 
        /// </summary>
        public abstract void Dispose();
    }
}
