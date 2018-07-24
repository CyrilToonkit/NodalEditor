using System;
using System.Threading;
using TK.NodalEditor;

namespace TK.NodalEditor
{
    public class NodalDirectorException : Exception
    {
        public NodalDirectorException()
        {
        }

        public NodalDirectorException(string message)
            : base(message)
        {
        }

        public NodalDirectorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// Handles a thread (unhandled) exception.

    public class NodalDirectorExceptionHandler
    {
        /// Handles the thread exception.

        public virtual void Application_ThreadException(
            object sender, ThreadExceptionEventArgs e)
        {
            if(e.Exception is NodalDirectorException)
            {
                string[] stacks = e.Exception.StackTrace.Split("\n".ToCharArray());

                int line = 0;

                foreach (string stack in stacks)
                {
                    if(stack.Contains("CSCodeEvaler.EvalCode()"))
                    {
                        string[] chunks = stack.Split(" ".ToCharArray());
                        line = int.Parse(chunks[chunks.Length - 1].Trim("\r".ToCharArray()));
                        break;
                    }
                }

                NodalDirector.Error(string.Format("in \"Interpreter\" line {0} > {1}\n{2}", line - 9, e.Exception.HelpLink.Split("\n".ToCharArray())[line - 10], e.Exception.Message));
                return;
            }

            throw e.Exception;
        }

    } // End ThreadExceptionHandler
}
