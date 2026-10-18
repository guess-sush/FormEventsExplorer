# Form Event Explorer for XrmToolBox

A read-only XrmToolBox tool for inspecting Dataverse main-form configuration.

## Features

- Select a Dataverse table and main form
- View form OnLoad and OnSave handlers
- View field OnChange handlers, function names and JavaScript libraries
- View associated business rules
- Search each result grid
- Export the selected form configuration to CSV

## Build

1. Install Visual Studio 2022 with **.NET desktop development**.
2. Open `FormEventExplorerXrmToolBox.sln`.
3. Restore NuGet packages.
4. Build `Release | Any CPU`.
5. Copy `bin\Release\net48\FormEventExplorerXrmToolBox.dll` to the XrmToolBox `Plugins` folder.
6. Remove the Windows download block from the DLL if applicable, restart XrmToolBox and connect to an environment.

## Notes

- The tool is read-only.
- The connected user needs read access to metadata, System Form and Process/Workflow rows.
- Form events are parsed from `systemform.formxml`.
- Business rules are queried from `workflow` where `category = 2` and `primaryentity` matches the selected table.
