// Copyright (c) Thomas Nieto - All Rights Reserved
// You may use, distribute and modify this code under the
// terms of the MIT license.

using System.Diagnostics;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;

namespace AnyPackage.Provider.Wsl;

[PackageProvider("Wsl")]
public class WslProvider : PackageProvider, IFindPackage, IGetPackage, IInstallPackage
{
    private const string _findRegex = @"^(?<name>\S+)\s{2,}(?<friendlyName>.+)$";
    private const string _getRegex = @"^(?<default>[\*]?)\s+(?<name>\S+)\s+(?<state>\S+)\s+(?<version>\d+)$";
    private const string _fileName = "wsl";
    private const string _running = "Running: {0} {1}";

    public void FindPackage(PackageRequest request)
    {
        if (request.IsVersionFiltered)
        {
            return;
        }

        using var process = new Process();
        process.StartInfo = GetStartInfo("--list --online", request);
        process.Start();
        using var reader = process.StandardOutput;

        string line;

        while ((line = reader.ReadLine()) is not null)
        {
            var match = Regex.Match(line, _findRegex);

            if (match.Success && match.Groups["name"].Value != "NAME")
            {
                var package = new PackageInfo(match.Groups["name"].Value,
                                              version: null,
                                              source: null,
                                              description: match.Groups["friendlyName"].Value,
                                              ProviderInfo);

                if (request.IsMatch(package.Name))
                {
                    request.WritePackage(package);
                }
            }
        }
    }

    public void GetPackage(PackageRequest request)
    {
        using var process = new Process();
        process.StartInfo = GetStartInfo("--list --verbose", request);
        process.Start();
        using var reader = process.StandardOutput;

        string line;

        while ((line = reader.ReadLine()) is not null)
        {
            var match = Regex.Match(line, _getRegex);

            if (match.Success)
            {
                var metadata = new Dictionary<string, object?>
                {
                    { "IsDefault", match.Groups["default"].Value == "*" },
                    { "State", match.Groups["state"].Value }
                };

                var package = new PackageInfo(match.Groups["name"].Value,
                                              match.Groups["version"].Value,
                                              source: null,
                                              description: "",
                                              dependencies: null,
                                              metadata,
                                              ProviderInfo);

                if (request.IsMatch(package.Name, package.Version!))
                {
                    request.WritePackage(package);
                }
            }
        }
    }

    public void InstallPackage(PackageRequest request)
    {
        PackageInfo package;

        if (request.ParameterSetName == "Name")
        {
            package = PowerShell.Create(RunspaceMode.CurrentRunspace)
                                .AddCommand("Find-Package")
                                .AddParameter("Name", request.Name)
                                .AddParameter("Provider", ProviderInfo.FullName)
                                .Invoke<PackageInfo>()
                                .FirstOrDefault();

            if (package is null)
            {
                return;
            }
        }
        else
        {
            package = request.Package!;
        }

        using var process = new Process();
        process.StartInfo = GetStartInfo($"--install {package.Name} --no-launch", request);
        process.Start();
        using var reader = process.StandardOutput;

        string line;

        while ((line = reader.ReadLine()) is not null)
        {
            request.WriteVerbose(line);
        }

        if (process.ExitCode == 0)
        {
            request.WriteWarning($"To complete installation run 'wsl --install {request.Name}'");
            request.WritePackage(package);
        }
    }

    private static ProcessStartInfo GetStartInfo(string args, PackageRequest request)
    {
        var message = string.Format(_running, _fileName, args);
        request.WriteVerbose(message);

        var startInfo = new ProcessStartInfo(_fileName, args)
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            StandardOutputEncoding = Encoding.Unicode
        };

        return startInfo;
    }
}
