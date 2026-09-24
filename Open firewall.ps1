#Requires -Version 7
#Requires -RunAsAdministrator

<#
.SYNOPSIS
    Opens (or closes) the Windows firewall for Usp on the local network.
.DESCRIPTION
    Creates one inbound TCP rule per port, limited to the local subnet so that
    browsers on other computers on the same network can reach Usp.
    The default ports match "Start regionsfinaler 2026.ps1".
.EXAMPLE
    ./"Open firewall.ps1"
.EXAMPLE
    ./"Open firewall.ps1" -Port 8880
.EXAMPLE
    ./"Open firewall.ps1" -Remove
#>
param(
    [int[]] $Port = @(8880, 8887, 9090),
    [switch] $Remove
)

$ErrorActionPreference = 'Stop'
$group = 'Ungdomsseriepoäng'

foreach ($p in $Port) {
    $name = "Usp TCP $p"

    Get-NetFirewallRule -DisplayName $name -ErrorAction SilentlyContinue | Remove-NetFirewallRule

    if ($Remove) {
        Write-Host "Removed firewall rule '$name'"
        continue
    }

    New-NetFirewallRule `
        -DisplayName $name `
        -Group $group `
        -Direction Inbound `
        -Action Allow `
        -Protocol TCP `
        -LocalPort $p `
        -RemoteAddress LocalSubnet `
        -Profile Any | Out-Null

    Write-Host "Opened TCP port $p for the local network"
}
