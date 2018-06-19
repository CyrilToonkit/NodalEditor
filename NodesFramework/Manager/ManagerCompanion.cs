using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing;
using TK.BaseLib.CustomData;
using TK.BaseLib;
using TK.BaseLib.Processes;

namespace TK.NodalEditor
{
    /// <summary>
    /// Helper for the NodesManager that manages UI specific actions such as storing a Processes queue, displaying ProgressBars, software-specific logs and setting and restoring software environments. This class may be inherited to create software specific actions.
    /// </summary>
    public class ManagerCompanion
    {
        public event EventHandler ProcessEndedEvent;

        public virtual void OnProcessEnded(EventArgs e)
        {
            ProcessEndedEvent(this, e);
        }

        #region CONSTRUCTORS

        /// <summary>
        /// Base constructor
        /// </summary>
        public ManagerCompanion()
        {
        }

        #endregion

        #region MEMBERS

        /// <summary>
        /// Debug mode
        /// </summary>
        public bool DEBUG = false;

        /// <summary>
        /// Log steps
        /// </summary>
        public bool TRACESTEPS = false;

        /// <summary>
        /// Log commands usage per steps
        /// </summary>
        public bool TRACECOMMANDS = false;
        public bool TRACECOMMANDSOLDVALUE = false;

        /// <summary>
        /// The NodesManager that instance is bound to
        /// </summary>
        public NodesManager Manager = null;

        /// <summary>
        /// List of processes currently running
        /// </summary>
        List<TrackedProcess> _processes = new List<TrackedProcess>();

        /// <summary>
        /// List of processes that need to be executed atfer all the others
        /// </summary>
        List<PostponedProcess> _postPonedProcesses = new List<PostponedProcess>();

        /// <summary>
        /// Is the ProgressBar curretly showing
        /// </summary>
        public bool ProgressBarIsShowing = false;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Get the process currently running
        /// </summary>
        public TrackedProcess CurrentProcess
        {
            get { return _processes.Count > 0 ? _processes[_processes.Count - 1] : new TrackedProcess("Failed", 0); }
        }

        /// <summary>
        /// Returns true if there are running processes
        /// </summary>
        public bool IsProcessing
        {
            get { return _processes.Count > 0; }
        }


        #endregion

        #region METHODS

        /// <summary>
        /// Prepare a process to be executed after all the other ones, keeping track of a method name and a Data to be used as arguments (Could use Invoke for that ?)
        /// </summary>
        /// <param name="Processor">The instance implementing IPostProcessor that will be called back </param>
        /// <param name="Name">Name of the process</param>
        /// <param name="Data">Data to be used as arguments</param>
        public void PostponeProcess(IPostProcessor Processor, string Name, object Data)
        {
            PostponeProcess(Processor, Name, Data, 0);
        }

        /// <summary>
        /// Prepare a process to be executed after all the other ones, keeping track of a method name and a Data to be used as arguments (Could use Invoke for that ?)
        /// </summary>
        /// <param name="Processor">The instance implementing IPostProcessor that will be called back </param>
        /// <param name="Name">Name of the process</param>
        /// <param name="Data">Data to be used as arguments</param>
        /// <param name="Step">The level of the process, it will be inserted just before other processes that have a higher step</param>
        public void PostponeProcess(IPostProcessor Processor, string Name, object Data, int Step)
        {
            foreach (PostponedProcess proc in _postPonedProcesses)
            {
                if (proc.Name == Name)
                {
                    return;
                }
            }

            int index = 0;
            PostponedProcess newProc = new PostponedProcess(Processor, Name, Data, Step);
            foreach (PostponedProcess proc in _postPonedProcesses)
            {
                if (proc.Step > Step)
                {
                    _postPonedProcesses.Insert(index, newProc);
                    return;
                }
                index++;
            }

            _postPonedProcesses.Add(newProc);
        }

        /// <summary>
        /// Launch a node process
        /// </summary>
        /// <param name="Name">Name of the process</param>
        /// <returns>The TrackedProcess instance</returns>
        public TrackedProcess LaunchProcess(string Name)
        {
            return LaunchProcess(Name, 0);
        }

        /// <summary>
        /// Launch a node process
        /// </summary>
        /// <param name="Name">Name of the process</param>
        /// <param name="Loops">Number of repetitions of the process (used for importance in progressBars)</param>
        /// <returns>The TrackedProcess instance</returns>
        public TrackedProcess LaunchProcess(string Name, int Loops)
        {
            TrackedProcess proc = new TrackedProcess(Name, _processes.Count);

            if (_processes.Count == 0)
            {
                SetEnvironment();
            }

            if (Loops > 0)
            {
                if (!ProgressBarIsShowing)
                {
                    ShowProgressBar(Loops, Name);
                    ProgressBarIsShowing = true;
                }
                else
                {
                    if (proc.Level < 2)
                    {
                        ProgressBarSetText(proc.Name);
                    }
                }
            }

            _processes.Add(proc);
            LogSteps(proc.Start());

            return proc;
        }

        /// <summary>
        /// End the current process, and verify if the process queue has ended, and if we have to execute postponed processes
        /// </summary>
        public void EndProcess(bool processPostPoned)
        {
            if (_processes.Count > 0)
            {
                TrackedProcess proc = CurrentProcess;

                _processes.Remove(proc);

                if (_processes.Count == 0)
                {
                    if (processPostPoned)
                    {
                        foreach (PostponedProcess process in _postPonedProcesses)
                        {
                            process.Processor.PostProcess(process.Name, process.Data);
                        }
                        _postPonedProcesses.Clear();
                    }

                    if (_postPonedProcesses.Count == 0)
                    {
                        RestoreEnvironment();
                        HideProgressBar();
                        ProgressBarIsShowing = false;
                    }
                }

                LogSteps(proc.End());
            }
        }

        public void EndProcess()
        {
            EndProcess(true);
        }

        protected int progressLoops = 0;
        protected int progressLast = 0;
        protected int progressPercent = 0;

        /// <summary>
        /// Show the progressBar
        /// </summary>
        /// <param name="Loops">Number of repetitions (steps)</param>
        /// <param name="Caption">Caption printed in the progressbar</param>
        public virtual void ShowProgressBar(int Loops, string Caption)
        {

        }

        /// <summary>
        /// Hide the progressBar
        /// </summary>
        public virtual void HideProgressBar()
        {

        }

        /// <summary>
        /// Increment the progressBar
        /// </summary>
        public virtual void ProgressBarIncrement()
        {

        }

        /// <summary>
        /// Change the caption in the progressBar
        /// </summary>
        /// <param name="Caption">New Caption</param>
        public virtual void ProgressBarSetText(string Caption)
        {

        }

        /// <summary>
        /// Determine wether the cancel button was pressed
        /// </summary>
        /// <returns>true if cancle was pressed, false otherwise</returns>
        public virtual bool ProgressBarCancel()
        {
            return false;
        }

        // Log

        /// <summary>
        /// Log a verbose message 
        /// </summary>
        /// <param name="p">The message</param>
        public virtual void Log(string p)
        {

        }

        /// <summary>
        /// Log an error message 
        /// </summary>
        /// <param name="p">The message</param>
        public virtual void Error(string p)
        {

        }

        /// <summary>
        /// Log an warning message 
        /// </summary>
        /// <param name="p">The message</param>
        public virtual void Warn(string p)
        {

        }

        // Environment

        /// <summary>
        /// Prepare the environment in the software to execute processes
        /// </summary>
        public virtual void SetEnvironment()
        {

        }

        /// <summary>
        /// Restore the environment after executing processes
        /// </summary>
        public virtual void RestoreEnvironment()
        {

        }

        /// <summary>
        /// Stops and drops all processes
        /// </summary>
        /// <param name="Message">Message explaining the reason of the breaking</param>
        public void CloseProcesses(string Message)
        {
            RestoreEnvironment();
            HideProgressBar();
            ProgressBarIsShowing = false;

            Error(" *** Processes stack stopped ...( " + _processes.Count + " processes aborted) *** \nMessage : " + Message);

            foreach (TrackedProcess proc in _processes)
            {
                Log("     " + proc.Name + " aborted !");
            }

            _processes.Clear();

            Error("Environment restored successfully");
        }

        /// <summary>
        /// Log a snaphot of the processes stack when the current one is aborted
        /// </summary>
        /// <returns></returns>
        public string LogSnapshot()
        {
            string snap = "";
            string spaces = "";
            int counter = 0;

            foreach (TrackedProcess proc in _processes)
            {
                spaces = "";
                for (int i = 0; i < counter; i++)
                {
                    spaces += " ";
                }

                snap += spaces;

                if (counter == _processes.Count - 1)
                {
                    snap += "ABORTED : ";
                }

                snap += proc.Name + "\r\n";

                counter++;
            }

            return snap;
        }

        /// <summary>
        /// Log a message only when in debug mode
        /// </summary>
        /// <param name="p">The message to be logged</param>
        public void LogDebug(string p)
        {
            if (DEBUG)
            {
                Log("DEBUG - " + p);
            }
        }

        /// <summary>
        /// Log a message only when in TRACESTEPS mode
        /// </summary>
        /// <param name="p">The message to be logged</param>
        public void LogSteps(string p, bool pad)
        {
            if (TRACESTEPS)
            {
                if (pad)
                {
                    Log(CurrentProcess.Pad(p));
                }
                else
                {
                    Log(p);
                }
            }
        }

        /// <summary>
        /// Log a message only when in TRACESTEPS mode
        /// </summary>
        /// <param name="p">The message to be logged</param>
        public void LogSteps(string p)
        {
            if (TRACESTEPS)
            {
                    Log(p);
            }
        }

        public void StartTracing()
        {
            TRACECOMMANDSOLDVALUE = TRACECOMMANDS;
            TRACECOMMANDS = false;

            Tracer.Instance.Clear();
            Tracer.Instance.Active = true;
        }

        public void StopTracing()
        {
            TRACECOMMANDS = TRACECOMMANDSOLDVALUE;

            Tracer.Instance.Active = false;
            Log(Tracer.Instance.GetText());
        }

        #endregion
    }
}