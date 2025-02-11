﻿using System;
using System.Diagnostics;
using WDE.Common.Services.CommandLine;

namespace WDE.Common.Tasks
{
    public static class GlobalApplication
    {
        public enum AppBackend
        {
            Uninitialized,
            Avalonia
        }

        public static IMainThread? mainThread;

        public static ICommandLineArgs Arguments { get; } = new CommandLineArgs();
        
        public static IMainThread MainThread
        {
            get
            {
                if (mainThread == null)
                    throw new Exception("Invalid use of MainThread before initialization, fatal error");
                return mainThread;
            }
        }
        public static AppBackend Backend { get; private set; }
        
        public static bool HighDpi { get; set; }

        public static bool IsRunning { get; set; } = true;
        public static bool Supports3D { get; set; } = true;

        public static void InitializeApplication(IMainThread thread, AppBackend backend)
        {
            Debug.Assert(mainThread == null);
            mainThread = thread;
            Backend = backend;
        }

        public static void Deinitialize()
        {
            mainThread = null;
            Backend = AppBackend.Uninitialized;
        }
    }
}