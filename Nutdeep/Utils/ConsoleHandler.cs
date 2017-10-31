using System;

using Nutdeep.Tools;
using Nutdeep.Tools.Flags;

namespace Nutdeep.Utils
{
    public abstract class ConsoleHandler
    {
        static Action userAction = null;
        public void OnClose(Action action)
        {
            userAction = action;
            _consoleCheckHandler = new HandlerRoutine(ConsoleCtrlCheck);
            Pinvoke.SetConsoleCtrlHandler(_consoleCheckHandler, true);
        }
        

        static HandlerRoutine _consoleCheckHandler;
        private static bool ConsoleCtrlCheck(ControlType ctrlType)
        {
            if (userAction != null)
            {
                switch (ctrlType)
                {
                    case ControlType.C:
                    case ControlType.BREAK:
                    case ControlType.CLOSE:
                    case ControlType.LOGOFF:
                    case ControlType.SHUTDOWN:
                        userAction.Invoke();
                        break;
                }
            }

            return true;
        }

        public delegate bool HandlerRoutine(ControlType CtrlType);
    }
}
