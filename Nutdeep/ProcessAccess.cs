using System;
using System.Threading;
using System.Diagnostics;
using System.Security.Principal;
using System.Runtime.InteropServices;

using Nutdeep.Tools;
using Nutdeep.Exceptions;
using Nutdeep.Tools.Flags;

/**
 * ProcessAccess - Written by Jeremi Martini (Aka Adversities)
 * Date: 10/30/2017
 */
namespace Nutdeep
{
    public class ProcessAccess : IDisposable
    {
        public IntPtr Handle { get; private set; }
        public Process Process { get; private set; }

        internal static bool IsHandleClosed { get; private set; }
        internal static void CheckAccess()
        {
            if (ProcessAccess.IsHandleClosed)
                throw new AccessNotFoundException();
        }

        public ThreadPriority Priority
        {
            get => Thread.CurrentThread.Priority;
            set => Thread.CurrentThread.Priority = value;
        }

        public ProcessAccess(int processId)
        {
            try
            {
                Process = Process.GetProcessById(processId);

            }
            catch (ArgumentException)
            {
                throw new ProcessNotFoundException();
            }

            SetupProcessAccess(Process.Id);
        }

        public ProcessAccess(Process process)
        {
            Process = process;
            SetupProcessAccess(Process.Id);
        }

        public ProcessAccess(string processName, int index = 0)
        {
            Process = GetProcessByName(processName, index);
            SetupProcessAccess(Process.Id);
        }

        private void SetupProcessAccess(int processId)
        {
            if (!IsAdministrator())
                throw new MissingAdminRightsException();

            Handle = Pinvoke.OpenProcess(ProcessAccessLevel.All,
                false, processId);

            IsHandleClosed = false;

            SetHighestThreadPriority();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Process = null;
                Handle = IntPtr.Zero;
                IsHandleClosed = true;

                Pinvoke.CloseHandle(Handle);
            }
        }

        private Process GetProcessByName(string processName, int index)
        {
            var processes = Process.GetProcessesByName(processName);

            if (processes.Length == 0)
                throw new ProcessNotFoundException();

            return processes[index];
        }

        private bool IsAdministrator()
        => (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                .IsInRole(WindowsBuiltInRole.Administrator);

        private void SetHighestThreadPriority()
        {
            Priority = ThreadPriority.Highest;
        }

        public static implicit operator MemoryDumper(ProcessAccess access)
        => new MemoryDumper(access);

        public static implicit operator MemoryScanner(ProcessAccess access)
        => new MemoryScanner(access);

        public static implicit operator MemoryEditor(ProcessAccess access)
        => new MemoryEditor(access);

        public override string ToString()
            => $"{Process.Id.ToString("x8").ToUpper()}" +
            $"-{Process.MainModule.ModuleName}";
    }
}
