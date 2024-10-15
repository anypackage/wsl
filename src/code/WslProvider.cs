// Copyright (c) Thomas Nieto - All Rights Reserved
// You may use, distribute and modify this code under the
// terms of the MIT license.

using System.Diagnostics;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;

namespace AnyPackage.Provider.Wsl;

[PackageProvider("Wsl")]
public class WslProvider : PackageProvider, IFindPackage, IGetPackage, IInstallPackage, IUninstallPackage
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
        if (request.IsVersionFiltered)
        {
            return;
        }

        PackageInfo package;

        if (request.Package is not null)
        {
            package = request.Package;
        }
        else
        {
            using var powershell = PowerShell.Create(RunspaceMode.CurrentRunspace);

            powershell.AddCommand("Find-Package")
                      .AddParameter("Name", request.Name)
                      .AddParameter("Provider", ProviderInfo.FullName);

            if (request.Version is not null)
            {
                powershell.AddParameter("Version", request.Version);
            }

            package = powershell.Invoke<PackageInfo>().FirstOrDefault();

            if (package is null)
            {
                return;
            }
        }

        InstallPackage(package, request);
    }

    private static void InstallPackage(PackageInfo package, PackageRequest request)
    {
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

    public void UninstallPackage(PackageRequest request)
    {
        if (request.Package is not null)
        {
            UninstallPackage(request.Package, request);
        }
        else
        {
            using var powershell = PowerShell.Create(RunspaceMode.CurrentRunspace);

            powershell.AddCommand("Get-Package")
                      .AddParameter("Name", request.Name)
                      .AddParameter("Provider", ProviderInfo.FullName);

            if (request.Version is not null)
            {
                powershell.AddParameter("Version", request.Version);
            }

            foreach (var package in powershell.Invoke<PackageInfo>())
            {
                UninstallPackage(package, request);
            }
        }
    }

    private static void UninstallPackage(PackageInfo package, PackageRequest request)
    {
        using var process = new Process();
        process.StartInfo = GetStartInfo($"--unregister {package.Name}", request);
        process.Start();
        using var reader = process.StandardOutput;

        string line;

        while ((line = reader.ReadLine()) is not null)
        {
            request.WriteVerbose(line);
        }

        if (process.ExitCode == 0)
        {
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
