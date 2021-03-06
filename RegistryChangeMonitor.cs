﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

namespace RegistryEnforcer
{
    #region Delegates
    public delegate void RegistryChangeHandler(object sender, RegistryChangeEventArgs e);
    #endregion

    public class RegistryChangeMonitor : IDisposable
    {
        #region Fields
        private REG_NOTIFY_CHANGE _filter;
        private Thread _monitorThread;
        private RegistryKey _monitorKey;
        #endregion

        #region Properties
        public string RegistryPath { get; private set; }
        public List<RegistrySetting> RegistrySettings { get; private set; }
        #endregion

        #region Imports
        [DllImport("Advapi32.dll")]
        private static extern int RegNotifyChangeKeyValue(
           IntPtr hKey,
           bool watchSubtree,
           REG_NOTIFY_CHANGE notifyFilter,
           IntPtr hEvent,
           bool asynchronous
           );
        #endregion
        #region Delegates
        public delegate void RegistryChangeHandler(object sender, RegistryChangeEventArgs e);
        #endregion

        #region Enumerations
        [Flags]
        public enum REG_NOTIFY_CHANGE : uint
        {
            NAME = 0x1,
            ATTRIBUTES = 0x2,
            LAST_SET = 0x4,
            SECURITY = 0x8
        }
        #endregion

        #region Constructors
        public RegistryChangeMonitor(RegistrySetting registrySetting)
        {
            this.RegistrySettings = new List<RegistrySetting>();
            this.RegistrySettings.Add(registrySetting);
            this.RegistryPath = (RegistryHelper.GetRegistryHiveLongName(registrySetting.RegistryHive) + @"\" + registrySetting.SubKeyPath).ToUpper();
            this._filter = REG_NOTIFY_CHANGE.LAST_SET;
        }
        //private RegistryChangeMonitor(string registryPath) : this(registryPath, REG_NOTIFY_CHANGE.LAST_SET)
        //{
        //}
        //private RegistryChangeMonitor(string registryPath, REG_NOTIFY_CHANGE filter)
        //{
        //    this.RegistryPath = registryPath.ToUpper();
        //    this._filter = filter;
        //}
        ~RegistryChangeMonitor()
        {
            this.Dispose(false);
        }
        #endregion

        public void AddRegistrySetting(RegistrySetting registrySetting)
        {
            this.RegistrySettings.Add(registrySetting);
        }

        #region Methods
        private void Dispose(bool disposing)
        {
            if (disposing)
                GC.SuppressFinalize(this);

            this.Stop();
        }
        public void Dispose()
        {
            this.Dispose(true);
        }
        public void Start()
        {
            lock (this)
            {
                if (this._monitorThread == null)
                {
                    ThreadStart ts = new ThreadStart(this.MonitorThread);
                    this._monitorThread = new Thread(ts);
                    this._monitorThread.IsBackground = true;
                }

                if (!this._monitorThread.IsAlive)
                {
                    this._monitorThread.Start();
                }
            }
        }
        public void Stop()
        {
            lock (this)
            {
                this.Changed = null;
                this.Error = null;

                if (this._monitorThread != null)
                {
                    this._monitorThread = null;
                }

                // The "Close()" will trigger RegNotifyChangeKeyValue if it is still listening
                if (this._monitorKey != null)
                {
                    this._monitorKey.Close();
                    this._monitorKey = null;
                }
            }
        }
        private void MonitorThread()
        {
            try
            {
                IntPtr ptr = IntPtr.Zero;

                lock (this)
                {
                    if (this.RegistryPath.StartsWith("HKEY_CLASSES_ROOT"))
                        this._monitorKey = Registry.ClassesRoot.OpenSubKey(this.RegistryPath.Substring(18));
                    else if (this.RegistryPath.StartsWith("HKCR"))
                        this._monitorKey = Registry.ClassesRoot.OpenSubKey(this.RegistryPath.Substring(5));
                    else if (this.RegistryPath.StartsWith("HKEY_CURRENT_USER"))
                        this._monitorKey = Registry.CurrentUser.OpenSubKey(this.RegistryPath.Substring(18));
                    else if (this.RegistryPath.StartsWith("HKCU"))
                        this._monitorKey = Registry.CurrentUser.OpenSubKey(this.RegistryPath.Substring(5));
                    else if (this.RegistryPath.StartsWith("HKEY_LOCAL_MACHINE"))
                        this._monitorKey = Registry.LocalMachine.OpenSubKey(this.RegistryPath.Substring(19));
                    else if (this.RegistryPath.StartsWith("HKLM"))
                        this._monitorKey = Registry.LocalMachine.OpenSubKey(this.RegistryPath.Substring(5));
                    else if (this.RegistryPath.StartsWith("HKEY_USERS"))
                        this._monitorKey = Registry.Users.OpenSubKey(this.RegistryPath.Substring(11));
                    else if (this.RegistryPath.StartsWith("HKU"))
                        this._monitorKey = Registry.Users.OpenSubKey(this.RegistryPath.Substring(4));
                    else if (this.RegistryPath.StartsWith("HKEY_CURRENT_CONFIG"))
                        this._monitorKey = Registry.CurrentConfig.OpenSubKey(this.RegistryPath.Substring(20));
                    else if (this.RegistryPath.StartsWith("HKCC"))
                        this._monitorKey = Registry.CurrentConfig.OpenSubKey(this.RegistryPath.Substring(5));

                    // Fetch the native handle
                    if (this._monitorKey != null)
                    {
                        object hkey = typeof(RegistryKey).InvokeMember(
                           "hkey",
                           BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic,
                           null,
                           this._monitorKey,
                           null
                           );

                        ptr = (IntPtr)typeof(SafeHandle).InvokeMember(
                           "handle",
                           BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic,
                           null,
                           hkey,
                           null);
                    }
                }

                if (ptr != IntPtr.Zero)
                {
                    while (true)
                    {
                        // If this._monitorThread is null that probably means Dispose is being called. Don't monitor anymore.
                        if ((this._monitorThread == null) || (this._monitorKey == null))
                            break;

                        // RegNotifyChangeKeyValue blocks until a change occurs.
                        int result = RegNotifyChangeKeyValue(ptr, true, this._filter, IntPtr.Zero, false);

                        if ((this._monitorThread == null) || (this._monitorKey == null))
                            break;

                        if (result == 0)
                        {
                            if (this.Changed != null)
                            {
                                RegistryChangeEventArgs e = new RegistryChangeEventArgs(this);
                                this.Changed(this, e);

                                if (e.Stop) break;
                            }
                        }
                        else
                        {
                            if (this.Error != null)
                            {
                                Win32Exception ex = new Win32Exception();

                                // Unless the exception is thrown, nobody is nice enough to set a good stacktrace for us. Set it ourselves.
                                typeof(Exception).InvokeMember(
                                "_stackTrace",
                                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField,
                                null,
                                ex,
                                new object[] { new StackTrace(true) }
                                );

                                RegistryChangeEventArgs e = new RegistryChangeEventArgs(this);
                                e.Exception = ex;
                                this.Error(this, e);
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (this.Error != null)
                {
                    RegistryChangeEventArgs e = new RegistryChangeEventArgs(this);
                    e.Exception = ex;
                    this.Error(this, e);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                this.Stop();
            }
        }
        #endregion

        #region Events
        public event RegistryChangeHandler Changed;
        public event RegistryChangeHandler Error;
        #endregion

        #region Properties
        public bool Monitoring
        {
            get
            {
                if (this._monitorThread != null)
                    return this._monitorThread.IsAlive;

                return false;
            }
        }
        #endregion
    }

}
