@{
    RootModule = 'WslProvider.dll'
    ModuleVersion = '0.1.0'
    CompatiblePSEditions = @('Desktop', 'Core')
    GUID = '1461be40-fb46-43ce-a838-818366f01542'
    Author = 'Thomas Nieto'
    Copyright = '(c) 2024 Thomas Nieto. All rights reserved.'
    Description = 'Windows Subsystem for Linux provider for AnyPackage.'
    PowerShellVersion = '5.1'
    RequiredModules = @(
        @{ ModuleName = 'AnyPackage'; ModuleVersion = '0.7.0' })
    FunctionsToExport = @()
    CmdletsToExport = @()
    AliasesToExport = @()
    PrivateData = @{
        AnyPackage = @{
            Providers = 'Wsl'
        }
        PSData = @{
            Tags = @('AnyPackage', 'Provider', 'WSL', 'Windows')
            LicenseUri = 'https://github.com/anypackage/wsl/blob/main/LICENSE'
            ProjectUri = 'https://github.com/anypackage/wsl'
        }
    }
    HelpInfoURI = 'https://go.anypackage.dev/help'
}
