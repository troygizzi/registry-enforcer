# Registry Enforcer

## Table of Contents

- [Overview](#overview)
- [App Settings](#app-settings)
  - [Logging](#logging)
  - [Preferences](#preferences)
  - [Registry Settings](#registry-settings)
- [Troubleshooting](#troubleshooting)
  - [App exits immediately after starting](#app-exits-immediately-after-starting)

## Overview

Headless application (no user interface) that watches the Windows Registry for changes to specified keys,
and restores the preferred values if they are changed by some other process
(e.g., Windows group policy settings pushed to your workstation by your company's IT department).

See the [Registry Settings](#registry-settings) section below for instructions on
how to specify which registry values you want this application to enforce.

## App Settings

### Logging

- `LogDirectoryPath` - (optional; defaults to `C:\Temp\Logs\RegistryEnforcer`)
                     The directory into which to write the log files.
                     If this directory does not exist, it will be created automatically.
                     If the drive letter does not exist, however, the application will terminate.
                     There will be a new log file created each day,
                     e.g., *RegistryEnforcer_2014-12-19.log*.

- `LogArchiveDays` - (optional; defaults to `30`)
                   How many days to keep log files before automatically deleting them.

- `LogLevel` - (optional; defaults to `Info`) Can be any of the values below.
               Note that each of these levels is inclusive of the levels below it;
               e.g., `Error` also includes all messages that would be logged
               with a log level of `Fatal`.

  - `Trace` - Verbose debugging; logs messages to indicate every method entry and exit point,
              local variable values at key points, and other significant points in the flow of the code.
  - `Debug` - Logs app settings at startup, and parameter values at every method call.
  - `Info` - Logs informational messages about the state of the application or significant events,
             e.g., when a registry setting was externally modified and had to be reset.
  - `Warn` - Logs unusual condition that may indicate a problem, but are not causing errors.
  - `Error` - Logs non-fatal errors, i.e., errors that cause a certain piece of functionality to break,
              but allow the application to keep running.
  - `Fatal` - Only logs details about errors that force the application to exit.

### Preferences

- AutoStart - (optional; defaults to `true`) Whether to automatically start this application
              whenever Windows starts. Can be set to either `true` or `false`.

### Registry Settings

For each Registry value that you want enforced, add an app setting whose
`key` begins with `RegistrySetting`, and whose `value` is a pipe-delimited
string containing (1) the full path to the value,
(2) the value type (typically `REG_SZ` for strings, or `REG_DWORD` for integers), and
(3) the value itself.

#### Example:
```
<add key="RegistrySetting_ChromeHTTPS_ProgId"
     value="HKCU\Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice\ProgId|REG_SZ|ChromeHTML"/>
```

## Troubleshooting

### App exits immediately after starting

If any of the required startup steps fails -- such as initializing the `Log` class --
the application will terminate.

When this happens, the error details will be written to the Windows Event Log.
These details can be viewed by opening the **Windows Event Viewer**
(*Start > Control Panel > Administrative Tools > Event Viewer*),
navigating to *Windows Logs > Application*, and filtering for events with a Source of "RegistryEnforcer".
