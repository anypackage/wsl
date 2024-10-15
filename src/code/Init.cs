// Copyright (c) Thomas Nieto - All Rights Reserved
// You may use, distribute and modify this code under the
// terms of the MIT license.

using System.Management.Automation;

using static AnyPackage.Provider.PackageProviderManager;

namespace AnyPackage.Provider.Wsl;

public sealed class Init : IModuleAssemblyInitializer, IModuleAssemblyCleanup
{
    private readonly Guid _id = new("160a88c1-630f-4ebf-89b7-639d51f835f6");

    public void OnImport()
    {
        RegisterProvider(_id, typeof(WslProvider), "AnyPackage.Wsl");
    }

    public void OnRemove(PSModuleInfo psModuleInfo)
    {
        UnregisterProvider(_id);
    }
}
