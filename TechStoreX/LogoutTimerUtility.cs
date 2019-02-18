using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Android.App;
using Android.Content;
using Android.OS;
using Java.Lang;
using Java.Util;
using Java.Util.Concurrent;

namespace TechStoreX
{
    class LogoutTimerUtility
    {
        public interface ILogOutListener
        {
            void DoLogout();
        }

        private static Timer longTimer;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static /*synchronized*/ void StartLogoutTimer(Context context,
                                                          ILogOutListener logOutListener,
                                                          int logoutTime)
        {
            if (longTimer != null)
            {
                longTimer.Cancel();
                longTimer = null;
            }
            /* if (longTimer == null) */
            {
                longTimer = new Timer();
                longTimer.Schedule(new LogoutTimerTask(context, logOutListener), (long)logoutTime);
            }
        }

        private class LogoutTimerTask : TimerTask
        {
            protected Context context;
            protected ILogOutListener logOutListener;

            internal LogoutTimerTask(Context context, ILogOutListener logOutListener)
            {
                this.context = context;
                this.logOutListener = logOutListener;
            }

            public override void Run()
            {
                Cancel();
                longTimer = null;
                try
                {
                    bool foreGround = (bool) new ForegroundCheckTask().Execute(context).Get();
                    if (foreGround)
                    {
                        logOutListener.DoLogout();
                    }
                }
                catch (InterruptedException e)
                {
                    e.PrintStackTrace();
                }
                catch (ExecutionException e)
                {
                    e.PrintStackTrace();
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static /*synchronized*/ void StopLogoutTimer()
        {
            if (longTimer != null)
            {
                longTimer.Cancel();
                longTimer = null;
            }
        }

        private class ForegroundCheckTask : AsyncTask
        {

            protected override Object DoInBackground(params Object[] @params)
            {
                Context context = ((Context)@params[0]).ApplicationContext;
                return IsAppOnForeground(context);
            }



            private bool IsAppOnForeground(Context context)
            {
                ActivityManager activityManager = (ActivityManager)context.GetSystemService(Context.ActivityService);
                if (activityManager != null)
                {
                    IList<ActivityManager.RunningAppProcessInfo> appProcesses = activityManager.RunningAppProcesses;
                    if (appProcesses == null)
                    {
                        return false;
                    }
                    string packageName = context.PackageName;
                    foreach (ActivityManager.RunningAppProcessInfo appProcess in appProcesses)
                    {
                        if (appProcess.Importance == Importance.Foreground /*ActivityManager.RunningAppProcessInfo.ImportanceForeground*/
                                && appProcess.ProcessName == packageName)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }
}