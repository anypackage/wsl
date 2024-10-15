# AnyPackage.Wsl

[![gallery-image]][gallery-site]
[![build-image]][build-site]
[![cf-image]][cf-site]

[gallery-image]: https://img.shields.io/powershellgallery/dt/AnyPackage.Wsl
[build-image]: https://img.shields.io/github/actions/workflow/status/anypackage/wsl/ci.yml
[cf-image]: https://img.shields.io/codefactor/grade/github/anypackage/wsl
[gallery-site]: https://www.powershellgallery.com/packages/AnyPackage.Wsl
[build-site]: https://github.com/anypackage/wsl/actions/workflows/ci.yml
[cf-site]: https://www.codefactor.io/repository/github/anypackage/wsl

Windows Subsystem for Linux provider for AnyPackage.

## Install AnyPackage.Wsl

```powershell
Install-PSResource AnyPackage.Wsl
```

## Import AnyPackage.Wsl

```powershell
Import-Module AnyPackage.Wsl
```

## Sample usages

### Find available distributions

```powershell
Find-Package
```

### Get list of installed distributions

```powershell
Get-Package -Name *Ubuntu*
```

### Install distribution

> Note: Installation uses --no-launch parameter. This does not fully complete
> installation. To finish installation run the suggested command to configure
> Linux username and password.

```powershell
Install-Package -Name Ubuntu
```

### Uninstall distribution

```powershell
Get-Package -Name *Ubuntu* | Uninstall-Package
```
