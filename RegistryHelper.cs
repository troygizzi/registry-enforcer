using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace RegistryEnforcer
{
    public static class RegistryHelper
    {
        public static string GetRegistryStringValue(RegistrySetting registrySetting)
        {
            return (string)GetRegistryValue(registrySetting);
        }

        public static uint GetRegistryDWordValue(RegistrySetting registrySetting)
        {
            return (uint)GetRegistryValue(registrySetting);
        }

        public static ulong GetRegistryQWordValue(RegistrySetting registrySetting)
        {
            return (ulong)GetRegistryValue(registrySetting);
        }

        public static object GetRegistryValue(RegistrySetting registrySetting)
        {
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(registrySetting.RegistryHive, RegistryView.Registry64))
            {
                using (RegistryKey key = baseKey.OpenSubKey(registrySetting.SubKeyPath, false))
                {
                    if (key == null)
                    {
                        throw new Exception(string.Format("Key does not exist: {0}", registrySetting.FullKeyPath));
                    }
                    return key.GetValue(registrySetting.Name);
                }
            }
        }

        public static bool SetRegistryValue(RegistryHive registryHive, string subKeyPath, string name, string newValue)
        {
            return SetRegistryValue(registryHive, subKeyPath, name, newValue, RegistryValueKind.String);
        }

        public static bool SetRegistryValue(RegistryHive registryHive, string subKeyPath, string name, int newValue)
        {
            return SetRegistryValue(registryHive, subKeyPath, name, newValue, RegistryValueKind.DWord);
        }

        public static bool SetRegistryValue(RegistryHive registryHive, string subKeyPath, string name, object newValue, RegistryValueKind valueKind)
        {
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry64))
            {
                using (RegistryKey key = baseKey.OpenSubKey(subKeyPath, true))
                {
                    if (key == null)
                    {
                        throw new Exception(string.Format("Key does not exist: {0}\\{1}", registryHive, subKeyPath));
                    }
                    if (!ValuesAreEqual(key.GetValue(name), key.GetValueKind(name), newValue, valueKind))
                    {
                        key.SetValue(name, newValue);
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool SetRegistryValue(RegistrySetting registrySetting)
        {
            return SetRegistryValue(registrySetting.RegistryHive, registrySetting.SubKeyPath, registrySetting.Name, registrySetting.Value, registrySetting.RegistryValueKind);
        }

        public static bool ValuesAreEqual(object valueA, RegistryValueKind valueKindA, object valueB, RegistryValueKind valueKindB)
        {
            if (valueKindA != valueKindB) return false;
            switch (valueKindA)
            {
                case RegistryValueKind.DWord: return (uint)valueA == (uint)valueB;
                case RegistryValueKind.QWord: return (ulong)valueA == (ulong)valueB;
                case RegistryValueKind.String: return (string)valueA == (string)valueB;
                default: throw new Exception(string.Format("Comparison between value kinds '{0}' is not yet supported.", valueKindA));
            }
        }

        public static RegistryKey GetBaseKey(string nameOrPath)
        {
            switch (nameOrPath.TruncateAt(@"\").ToUpper())
            {
                case "HKCR":
                case "HKEY_CLASSES_ROOT": return Registry.LocalMachine;
                case "HKCU":
                case "HKEY_CURRENT_USER": return Registry.CurrentUser;
                case "HKLM":
                case "HKEY_LOCAL_MACHINE": return Registry.LocalMachine;
                case "HKPD":
                case "HKEY_PERFORMANCE_DATA": return Registry.PerformanceData;
                case "HKU":
                case "HKEY_USERS": return Registry.Users;
                case "HKCC":
                case "HKEY_CURRENT_CONFIG": return Registry.CurrentConfig;
                default: throw new Exception("Unable to determine Windows registry base key from name or path '" + nameOrPath + "'.");
            }
        }

        public static RegistryHive GetRegistryHive(string nameOrPath)
        {
            switch (nameOrPath.TruncateAt(@"\").ToUpper())
            {
                case "HKCR":
                case "HKEY_CLASSES_ROOT": return RegistryHive.ClassesRoot;
                case "HKCC":
                case "HKEY_CURRENT_CONFIG": return RegistryHive.CurrentConfig;
                case "HKCU":
                case "HKEY_CURRENT_USER": return RegistryHive.CurrentUser;
                case "HKLM":
                case "HKEY_LOCAL_MACHINE": return RegistryHive.LocalMachine;
                case "HKPD":
                case "HKEY_PERFORMANCE_DATA": return RegistryHive.PerformanceData;
                case "HKU":
                case "HKEY_USERS": return RegistryHive.Users;
                default: throw new Exception("Unable to determine Windows registry hive from name or path '" + nameOrPath + "'.");
            }
        }

        public static string GetRegistryHiveLongName(RegistryHive registryHive)
        {
            switch (registryHive)
            {
                case RegistryHive.ClassesRoot: return "HKEY_CLASSES_ROOT";
                case RegistryHive.CurrentConfig: return "HKEY_CURRENT_CONFIG";
                case RegistryHive.CurrentUser: return "HKEY_CURRENT_USER";
                case RegistryHive.LocalMachine: return "HKEY_LOCAL_MACHINE";
                case RegistryHive.PerformanceData: return "HKEY_PERFORMANCE_DATA";
                case RegistryHive.Users: return "HKEY_USERS";
                default: throw new Exception("Registry Hive not recognized or supported: " + registryHive.ToString());
            }
        }

        public static string GetRegistryHiveShortName(RegistryHive registryHive)
        {
            switch (registryHive)
            {
                case RegistryHive.ClassesRoot: return "HKCR";
                case RegistryHive.CurrentConfig: return "HKCC";
                case RegistryHive.CurrentUser: return "HKCU";
                case RegistryHive.LocalMachine: return "HKLM";
                case RegistryHive.PerformanceData: return "HKPD";
                case RegistryHive.Users: return "HKU";
                default: throw new Exception("Registry Hive not recognized or supported: " + registryHive.ToString());
            }
        }

        public static RegistryValueKind GetValueKind(string text)
        {
            switch (text)
            {
                case "REG_BINARY": return RegistryValueKind.Binary;
                case "REG_DWORD": return RegistryValueKind.DWord;
                case "REG_EXPAND_SZ": return RegistryValueKind.ExpandString;
                case "REG_MULTI_SZ": return RegistryValueKind.MultiString;
                case "REG_QWORD": return RegistryValueKind.QWord;
                case "REG_SZ": return RegistryValueKind.String;
                default: throw new Exception("Unable to determine registry value kind from string '" + text + "'.");
            }
        }
    }
}
