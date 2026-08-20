# Dawncaster Fate Shard Editor - Responsive UI Version

This version replaces the fixed-position WinForms layout with responsive layout panels.

## Improvements

- Larger default window
- Resizable window
- Proper DPI scaling
- Controls no longer depend on hard-coded X/Y positions
- File path expands with the window
- Better spacing and grouping
- Save button stays visible
- Automatic loading of the Dawncaster configuration file

## Default Dawncaster file

`%USERPROFILE%\AppData\LocalLow\Wanderlost Interactive\Dawncaster\DC_Conf.dc`

## Run from VS Code

Open the folder in VS Code and run:

```powershell
dotnet run
```

## Publish as one Windows EXE

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The EXE will appear under:

`bin\Release\net8.0-windows\win-x64\publish\DawncasterFateShardEditor.exe`
"# Dawncaster-Fate-Shard-Editor" 
