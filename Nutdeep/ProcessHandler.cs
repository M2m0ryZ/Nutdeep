using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Security.Principal;

using Nutdeep.Tools;
using Nutdeep.Exceptions;
using Nutdeep.Tools.Flags;
using Nutdeep.Utils.Extensions;

namespace Nutdeep
{
    public class ProcessHandler : ProcessAccess, IDisposable
    {
        internal static bool IsHandleClosed { get; private set; }

        internal static void CheckAccess()
        {
            if (IsHandleClosed)
                throw new AccessNotFoundException();
        }

        public ThreadPriority Priority
        {
            get => Thread.CurrentThread.Priority;
            set => Thread.CurrentThread.Priority = value;
        }

        public ProcessHandler(int processId)
        {
            try
            {
                Process = Process.GetProcessById(processId);

            }
            catch { throw new ProcessNotFoundException(); }

            SetupProcessAccess(Process.Id);
        }

        public ProcessHandler(Process process)
        {
            Process = process ?? 
                throw new ProcessNotFoundException();
            SetupProcessAccess(Process.Id);
        }

        public ProcessHandler(string processName, int index = 0)
        {
            Process = processName.Contains("&flash") ?
                Process.GetProcessesByName("chrome")
                .Where(task => task.RunsShockwaveFlash())
                .FirstOrDefault() : GetProcessByName(processName, index);

            if (Process == null) throw new ProcessNotFoundException();

            SetupProcessAccess(Process.Id);
        }

        private void SetupProcessAccess(int processId)
        {
            if (!IsAdministrator())
                throw new MissingAdminRightsException();

            SetHighestThreadPriority();

            try
            {
                Handle = Pinvoke.OpenProcess(ProcessAccessLevel.All,
                false, processId);
            }
            catch (Win32Exception)
            {
                throw new Win32Exception($"It's not possible to open {Process.ProcessName}");
            }

            IsHandleClosed = false;
        }

        private bool IsAdministrator()
        => (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                .IsInRole(WindowsBuiltInRole.Administrator);

        private void SetHighestThreadPriority()
        {
            Priority = ThreadPriority.Highest;
        }

        private void SetNormalThreadPriority()
        {
            Priority = ThreadPriority.Normal;
        }

        private Process GetProcessByName(string processName, int index)
        {
            var processes = Process.GetProcessesByName(processName);

            if (processes.Length == 0)
                throw new ProcessNotFoundException();

            return processes[index];
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

                SetNormalThreadPriority();
                Pinvoke.CloseHandle(Handle);
            }
        }

        public override string ToString()
        {
            var inf = $"{Process.Id.ToString("x8").ToUpper()}" +
                $"-{Process.MainModule.ModuleName}";

            if (Process.RunsShockwaveFlash())
                return $"{inf} [ShockwaveFlash]";

            return inf;
        }
    }
}
