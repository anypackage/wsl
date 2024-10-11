// Copyright (c) Thomas Nieto - All Rights Reserved
// You may use, distribute and modify this code under the
// terms of the MIT license.

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace AnyPackage.Provider.Wsl;

[PackageProvider("Wsl")]
public class WslProvider : PackageProvider, IGetPackage
{
    private const string _getRegex = @"^(?<default>[\*]?)\s+(?<name>\S+)\s+(?<state>\S+)\s+(?<version>\d+)$";

    public void GetPackage(PackageRequest request)
    {
        using var process = new Process();
        process.StartInfo.Arguments = "--list --verbose";
        process.StartInfo.FileName = "wsl";
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.StandardOutputEncoding = Encoding.Unicode;
        process.Start();
        using var reader = process.StandardOutput;

        string line;

        while ((line = reader.ReadLine()) is not null)
        {
            var match = Regex.Match(line, _getRegex);

            if (match.Success)
            {
                var dict = new Dictionary<string, object?>
                {
                    { "IsDefault", match.Groups["default"].Value == "*" },
                    { "State", match.Groups["state"].Value }
                };

                var package = new PackageInfo(match.Groups["name"].Value,
                                              match.Groups["version"].Value,
                                              null,
                                              "",
                                              null,
                                              dict,
                                              ProviderInfo);

                request.WritePackage(package);
            }
        }
    }
}
