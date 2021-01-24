using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions
{
    internal static class VersionInfo
    {
        private static string? _versionDescription;

        public static string Description
        {
            get
            {
                if (_versionDescription == null)
                {
                    string? versionDescription = Assembly.GetExecutingAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                    if (versionDescription != null && versionDescription.Length > 0)
                    {
                        if (versionDescription.Contains('+'))
                            versionDescription = versionDescription.Substring(0, versionDescription.IndexOf('+'));
                    }
                    else
                        versionDescription = "Unknown Build";
                    _versionDescription = versionDescription;
                }
                return _versionDescription;
            }
        }
    }
}
