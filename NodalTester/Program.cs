using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using TK.NodalEditor;

namespace NodalTester
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

            NodalDirectorExceptionHandler handler = new NodalDirectorExceptionHandler();
            Application.ThreadException +=
                new ThreadExceptionEventHandler(
                    handler.Application_ThreadException);

            //Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            TestForm form = new TestForm();
            Application.Run(form);
		}
	}
}