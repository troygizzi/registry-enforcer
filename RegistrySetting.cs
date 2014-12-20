using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace RegistryEnforcer
{
    public class RegistrySetting
    {
        public RegistryHive RegistryHive { get; set; }
        public string SubKeyPath { get; set; }
        public string Name { get; set; }
        public RegistryValueKind RegistryValueKind { get; set; }
        public object Value { get; set; }

        public string FullValuePath
        {
            get
            {
                return string.Format(@"{0}\{1}\{2}", RegistryHelper.GetRegistryHiveLongName(this.RegistryHive), this.SubKeyPath, this.Name);
            }
        }

        public string FullKeyPath
        {
            get
            {
                return RegistryHelper.GetRegistryHiveLongName(this.RegistryHive) + @"\" + this.SubKeyPath;
            }
        }

        /// <summary>
        /// Parses a RegistrySetting represented as a single pipe-delimited string (e.g., "HKCU\Console\CursorSize|DWORD|25").
        /// </summary>
        /// <param name="text">String representing a RegistrySetting</param>
        /// <returns>New <see cref="RegistrySetting"/> instance.</returns>
        public static RegistrySetting Parse(string text)
        {
            RegistrySetting setting = new RegistrySetting();

            string[] settingSegments = text.Split(new char[1] { '|' }, 3);
            List<string> pathSegments = new List<string>(settingSegments[0].Split('\\'));

            if (settingSegments.Length != 3) throw new Exception("setting must consist of three pipe-delimited segments: full path, value kind, and value");
            if (pathSegments.Count < 3) throw new Exception("Registry key path must consist of at least three backslash-delimited segments: hive, subkey path, and name");

            // RegistryHive
            setting.RegistryHive = RegistryHelper.GetRegistryHive(pathSegments[0]);

            // SubKeyPath
            setting.SubKeyPath = string.Join(@"\", pathSegments.GetRange(1, pathSegments.Count - 2));

            // Name
            setting.Name = pathSegments[pathSegments.Count - 1];

            // RegistryValueKind
            setting.RegistryValueKind = RegistryHelper.GetValueKind(settingSegments[1]);

            // Value
            switch (setting.RegistryValueKind)
            {
                case RegistryValueKind.DWord: setting.Value = uint.Parse(settingSegments[2]); break;
                case RegistryValueKind.QWord: setting.Value = ulong.Parse(settingSegments[2]); break;
                case RegistryValueKind.String: setting.Value = settingSegments[2]; break;
                default: throw new Exception("Unsupported registry value kind: " + settingSegments[1]);
            }

            return setting;
        }
    }
}
