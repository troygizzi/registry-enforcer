using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace RegistryEnforcer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _resetting = false;
        private List<RegistrySetting> RegistrySettings = new List<RegistrySetting>();
        private Dictionary<string, RegistryChangeMonitor> RegistryChangeMonitors = new Dictionary<string, RegistryChangeMonitor>();
        private Dictionary<string, DateTime> LastChanged = new Dictionary<string, DateTime>();

        [DllImport("Advapi32.dll")]
        private static extern int RegNotifyChangeKeyValue(
           IntPtr hKey,
           bool watchSubtree,
           REG_NOTIFY_CHANGE notifyFilter,
           IntPtr hEvent,
           bool asynchronous
           );

        public MainWindow()
        {
            this.Hide();
            InitializeComponent();
            
            try
            {
                InitializeLogging();
                InitializePreferences();
                InitializeRegistrySetting();
                InitializeTimers();
            }
            catch (Exception ex)
            {
                EventLog eventLog = new EventLog();
                eventLog.Source = "RegistryEnforcer";
                eventLog.WriteEntry("Unable to initialize application." + Environment.NewLine + Environment.NewLine + ex.ToString(), EventLogEntryType.Error);
                MessageBox.Show("Fatal Exception -- Application failed to initialize properly\n\n" +
                    "Exception details are displayed below, and have been logged to the\n" +
                    "Windows Event Viewer under Windows Logs > Applications, as an\n" +
                    "event with a Source of \"RegistryEnforcer\".\n\n" +
                    ex.ToString(), "Registry Settings Keeper - Failure to Launch");
                Application.Current.Shutdown();
            }
        }

        private void InitializeLogging()
        {
            Log.Initialize();
            Log.Info("************************************************************************************************************************");
            Log.Info("* RegistryEnforcer started");
            Log.Info("************************************************************************************************************************");

            foreach (string key in ConfigurationManager.AppSettings.AllKeys)
            {
                Log.Info("App Setting '{0}': {1}", key, ConfigurationManager.AppSettings[key]);
            }
        }

        private void InitializePreferences()
        {
            bool autoStart = (ConfigurationManager.AppSettings["AutoStart"] != "false");
            RegisterInStartup(autoStart);
        }

        private void InitializeRegistrySetting()
        {
            foreach (string key in ConfigurationManager.AppSettings.AllKeys)
            {
                if (key.StartsWith("RegistrySetting"))
                {
                    RegistrySetting registrySetting = RegistrySetting.Parse(ConfigurationManager.AppSettings[key]);
                    RegistrySettings.Add(registrySetting);
                    RegistryHelper.SetRegistryValue(registrySetting);
                    MaintainRegistrySetting(registrySetting);
                }
            }
        }

        private void MaintainRegistrySetting(RegistrySetting registrySetting)
        {
            RegistryChangeMonitor monitor = RegistryChangeMonitors.ContainsKey(registrySetting.FullKeyPath)
                ? RegistryChangeMonitors[registrySetting.FullKeyPath]
                : null;

            if (monitor == null)
            {
                monitor = new RegistryChangeMonitor(registrySetting);
                monitor.Changed += RegistrySettingChanged;
                monitor.Start();
                RegistryChangeMonitors.Add(registrySetting.FullKeyPath, monitor);
            }
            else
            {
                monitor.AddRegistrySetting(registrySetting);
            }
        }

        private void RegistrySettingChanged(object sender, RegistryChangeEventArgs e)
        {
            if (!_resetting)
            {
                if (this.LastChanged.ContainsKey(e.Monitor.RegistryPath))
                {
                    if (DateTime.Now - this.LastChanged[e.Monitor.RegistryPath] < new TimeSpan(0, 0, 0, 2))
                    {
                        return;
                    }
                }

                _resetting = true;

                Log.Info("Registry setting changed at {0}. Resetting now.", e.Monitor.RegistryPath);

                e.Monitor.RegistrySettings.ForEach((registrySetting) =>
                {
                    RestoreRegistrySetting(registrySetting);
                });

                this.LastChanged[e.Monitor.RegistryPath] = DateTime.Now;

                _resetting = false;
            }
        }

        private void RestoreRegistrySetting(RegistrySetting registrySetting)
        {
            RegistryHelper.SetRegistryValue(registrySetting);
        }

        private void InitializeTimers()
        {
            System.Timers.Timer settingsDoubleCheck = new System.Timers.Timer();
            settingsDoubleCheck.Elapsed += settingsDoubleCheck_Elapsed;
            settingsDoubleCheck.Interval = 10000;
            settingsDoubleCheck.Start();
        }

        void settingsDoubleCheck_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Log.Trace("Timer 'settingsDoubleCheck' elapsed; checking registry values now.");
            EnforceRegistrySettings();
        }

        private void EnforceRegistrySettings()
        {
            RegistrySettings.ForEach((registrySetting) =>
            {
                if (RegistryHelper.SetRegistryValue(registrySetting))
                {
                    Log.Debug("Registry value for '{0}' has been reset to '{1}'...", registrySetting.FullValuePath, registrySetting.Value);
                }
            });
        }

        private void RegisterInStartup(bool autoStart)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (autoStart)
            {
                registryKey.SetValue("RegistryEnforcer", System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            else
            {
                registryKey.DeleteValue("RegistryEnforcer");
            }
        }
    }
}
