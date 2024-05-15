# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## Version 0.2.5
 - Fixed issue where NewProject Dialog was not properly stringifying Rendering Driver.
 - Corrected getting minor version, to ensure it's numeric, instead of numeric-release
 - Ensured that dir_selected signal is disconnected only if it is connected, on OK signal.
 - Implemented using Config Version for 3.0 to 3.0.6.
 - Fix attempting to launch a project that has no godot engine associated with it.  This can happen when the engine that a project was associated with, is removed.
 - Fix for [#93](https://github.com/eumario/godot-manager/issues/93)
   - Fixed crash when attempting to Edit a project that has no engine assoicated.
   - When we have 1 version of engine installed, ensure that we mark the project as dirty, and allow the person to be able to save the project with the only engine installed.
   - Added signal to ensure that Project is updated, when something has changed about the project.  Such as the engine associated with it, is uninstalled, this only Updates the UI for the projects.
 - Ensure that cached Godot Version is cleared when opening the project, and before populating the Project Data information.  This ensures that an old version, that has been uninstalled, isn't cached when showing the dialog.
 - Implemented proper catches for any Network Traffic, where a failure to connect is caught. (Fixes [#96](https://github.com/eumario/godot-manager/issues/96))
 - Adds Development configuration, so that main CentralStore json isn't modified with new features, that would not be readable in older stable versions.
 
 - [Changes between 0.2.4 and 0.2.5](https://github.com/eumario/godot-manager/compare/0b62b7b...73bc7b5)

## Version 0.2.4
 - Updated First Run Wizard:
    - Added Hook for first run wizard being completed.  Ensures that the directory structure is fully made once we have successfully completed the first run wizard.
    - Updated Godot Panel reuse to ensure that if we are in First Run Wizard mode, that we aren't saving settings till we have finished the wizard.
    - Ensure that all Paths are properly setup for Projects, Cache and Godot Versions when we transition from Page 2, to Page 3 or 4.
    - Ensure that folders for Godot Versions and Cache Folder is at least setup by the time we go to download a Version of the Engine, so that they are properly existing to receive files.
 - Fixed Text Changed Signal Event handler was not proper signature, so never properly connected the method to the signal handler.
 - New Feature: Added ability to Scan for missing projects, where a user can select a folder, and it will scan for all missing projects, if found, will update the project with the new path, otherwise the project will remain in a Missing Project Status.
 - Fixed bug on Mac OS, where moving the Godot Window, without Native Titlebars, cause Godot Manager to crash.  (Thanks to [@shiena](https://github.com/shiena))
 - Added Version sorting to properly line up with Major, Minor and Patch level.  (Thanks to [@Cammymoop](https://github.com/Cammymoop))
 - Fixed tooltip on News Page, to actually show News, instead of Godot Engines.  (Thanks to [@DarkDemiurg](https://github.com/DarkDemiurg))
 - Updated SixLabors.ImageSharp to v2.1.7, as CVE-2024-27929 reported issue affected 2.1.6, which allowed for a specific PNG image would expose a security hole, that could leak personal information.

## Version 0.2.3
 - Fixed naming convention on Github, when downloading Godot 4 from Github.
 - Fixed bug that after First Run setup, no Godot Engine is set as default, added safety checks to ensure that we don't attmept to find a default engine, when none is selected.
 - Added wait for Ready function to allow idle frames to complete, allowing all UI controls to be created, including Dialogs that were not created by the time we were trying to show them.
 - Rewrote RecursiveScan to take into account running into an Exception when the user attempts to read from a folder they do not have permission to access.  Handle it through Godot's own Directory functionality, as it has a cleaner error reporting for when we are trying to scan through things.
 - Added check to ensure validity of custom version information, before attempting to fetch any items.
 - Checks to see if the ~ is being used in the path (Which can be done by manually entering the path), if so, expand ~ into the HOME folder. through fetching Environment Variable, or using OS.SystemDir.Documents.
 - Added check to ensure that the path ends with project.godot in Import Dialog, and if not found, warns the user, and aborts adding it.
 - Fixed BusyDialog to actually hide, when a connection failure occurs.
 - Updated to 2.1.6 of ImageSharp, as it's the last supported version for .NET 4.7.2
 - Updated News Panel to include Base URL, since HTML now only provides root path for links, instead of full url.
 - Fix crash when project is missing a name for the Project. (Thanks to [@NathanWarden](https://github.com/NathanWarden))
 - Added ability to accept Files that are dragged and dropped onto Godot Manager, to add them to the Project List. (Thanks to [@NathanWarden](https://github.com/NathanWarden))
 - Corrected Typo in Use Default.  (Thanks to [@jnetti](https://github.com/jnetti))

## Version 0.2.2
 - Fixed crash on Windows 11 with GodotInstaller. Globalize Installation path in GodotInstaller. (Thanks to [@dogboydog](https://github.com/dogboydog))
 - Added Instructions on how to install on Windows through Scoop. (Thanks to [@sitiom](https://github.com/sitiom))

## Version 0.2.1
 - Fixed bug with Case-Sensitive Directories on certain OSes, that could cause issues.  Windows is Case Insensitive, where as Linux is Case Sensitive, and Mac OS can be either or.  This fixes a bug that prevented a Project Data folder from being displayed. (#39)
 - Cleaned up Signal Handling for specific events that were not properly being dis-connected when finished with a Dialog.  Clears any errors that may occur.
 - Added Option to allow creation of Desktop Shortcuts on Linux in Settings panel for Godot Manager.  This allows users to be able to create the shortcut, even if they missed doing it in the First Time Setup wizard. (#40)
 - Created new Splash Screen yet again.  Instead of using a Static Image, now using a Scene, to allow loading of Version information from the VERSION static class, making it easier to update.
 - Updated New Project Dialog to properly Display GLES2/3 on Godot 3.x projects, and Forward+/Mobile for Godot 4.x projects. (#36)
 - Possible fix to Standard Godot Engine not being properly extracted to their own folder, when installing, as well as it not properly being setup when a user changes the Location of where to install Godot engines during First Time Setup wizard.  (#37 & #38)
 - Added crash prevention check, to check and see if the Godot Engine Executable actually exists, before actually executing the Project.  It will give an Error saying that it can't find the executable, and not just silently fail.  (#41)
 - Modified the way ExecuteEditorProject() and ExecuteProject() handles execution.  Instead now uses the ProjectFile class, instead of just strings passed along.  This gives these functions a chance to check and see if the associated Engine is still installed, if not, it will ask the user to use the current default engine.  (#42)
 - Added organization to the Godot Engine Listings in both the Godot Panel, and Drop Downs for selecting a Version of Godot.  It will now sort by Version, with Mono Editions appearing before Standard Editions, and x.x appearing, before x.x.x releases, making it easier to see which version is which.  (#43)
 - Created a new Dialog to allow users to Select from a drop down to respond to a question, this will be used for feature suggestion (#45, #46)
 - Made RecurseDirectory() an public static function to be reused in other parts of the project. Used for feature suggestion (#45, #46)
 - Implemented the Ability to share settings between versions of Godot.  One install of Godot engine will be set to sharing their settings, while others can link to said install of godot for their settings.  Updates are one direction, updating from the Shared settings to the Linked install on each Editor launch, but will not be copied back to the master one. (#45, #46)
 - Implemented Self-Contained mode in the same fashion that Godot Engine implements Self Contained mode.  Creating a File named "._sc_" or "_sc_" in the directory where the Godot Manager executable resides, will enable it to operate in Self-Contained mode.  This will help with the ability to Create Portable USB Environments for Godot Development.
 - Due to recent changes (As of Nov 29, 2022), updates to the News Page has made the previous method for parsing the news on the Godot Website incompatible.  New parsing has been setup, and is now working with the new formatting.  (#49)


## Version 0.2.0
- Added Channel to Godot Manager, now has Dev for Development version, and Release for Release Version.
- Started work on Translation, now having strings properly translated throughout entire project.
- Added documentation for Assisting with the Translation of the Application.
- Partial Greek Translation provided by GalaxyGamingBoy
- Refactored CentralDataStore to have proper naming
- Implemented Mirror Sites, for official mirrors of Godot Engine, that will be scraped by Godot Mirror Manager.
- Added Proxy Support, to allow HTTP Communication to go through Proxies setup in Settings.
- Added code to Clean up Translations that improperly had a Line-Feed(\r) character in it.  This will allow for cleaner translations.
- Fixed bug for searching for Plugins and Templates, where a null wasn't being returned when not found.  Now using Linq to search central store.
- Fixed bug where Asset Preview was still querying Godot AssetLib for locally imported plugins/templates.
- Fixed error in UpdateAddons/UpdateTemplates where Icons were added to the Download Queue, but the Download Queue was never started.
- Fixed bug where Asset Listing was not accounting for Locally imported Plugins/Templates when trying to get their icons.
- Ensure all Asset Entries in listing have an icon associated with it for local Plugins/Templates, either by using one provided in Zip File, or by using default_project_icon
- Implemented Mirror Selection from mirrors retrieved from Godot Mirror Manager.  Added switch to allow searching for Mono or Standard Edition, and when a Mirror is selected, to search by Stable/Alpha/Beta/Release Candidate.
- Removed Mirror Manager from Settings Panel.
- De-coupled Downloading logic from Godot Panel, and Godot Line Entry into Github and Mirror classes, keeping code more organized.
- Deprecated Downloader class, and switched to using GDCSHTTPClient for all downloading functions.
- Switched from Recreating Project line entries, and Godot Line entries to recycling existing ones, and only adding, or removing as needed, speeding up loading of the entire application, as well as Sorting.
- Added support to Directly Import Godot Projects, by using the Godot Manager executable.  (Thanks Oliveoil)
- Changed way that files are downloaded.  Files are now temporarily stored in a temp file, instead of in memory, to prevent errors in memory.
- Added Update Checks for Mirrors, so Interval Checks can be made against Mirrors
- Fixed bug, where Duplicate Projects were being added to the Central Store, when there was only 1 project to be added.
- Fixed 2 bugs on Linux/BSD where System needs to be prefix, and Executable Name was not named properly in the property.
- Added Capabilities to use Operating System's Titlebar, instead of Faux Titlebar internal to Godot Manager, now toggable as a Setting.
- Fix UI bug where Labels on IconView, to ensure that there is proper spacing, and justification of the labels, so that they don't run over each other.
- Added ability to use Last Engine Mirror Selected between runs.
- Added UI Fix to allow things to be properly aligned to new space when switching between Internal and OS Titlebar.
- Added New Panel for Godot News, so now you can see News from the Godot Engine website directly in the Manager.
- Fixed minor bug where Indexes were not properly aligning to their images in the Download queue.
- Added ability to Pin Categories in Category View, so you can pin specific categories to the Top of the view.  Favorites, and Uncategoriezed will remain at bottom.
- Added ability to create Custom Downloads for Godot Engines.  It will download a Singular release url, useful for Nightly Builds.
- Fixed minor bug where Checking to ensure directories exist, it properly get's the Operating System's Location, and ensures to use the Settings that are saved, in order to get validated location.
- Updated Asset UI, to show the Asset ID for those who want to see the ID of the Asset.  Clicking on the ID, will take you directly to the Asset's page on Godot's Asset Library.
- Fixed bug with Paginated views, not properly updating when lower numbers of results were presented.
- Implemented Version searching in Asset Library.  Will now let you search for Any version of Godot, or a specific version of godot, to find templates, or addons.
- Updated Asset UI to ensure that it is showing the Version of Godot the Addon/Template targets.
- Added Label to show where Godot Engine is installed to.
- Added Popup Menu for Godot Engines, so you can Uninstall, Make Default, Copy Engine Path, or Open Engine Path for Installed versions of Godot Engine, and Install for Available versions to download.
- Updated Godot Manager's downloading of Alpha/Beta naming scheme for Godot 4, as altered from Godot 3.x naming scheme.
- Added check if there is 1 version left of Godot installed after an uninstall of Godot, it will ask user if they want to associate all projects with specified version of Godot, if so, it will update all Projects with that specific version.
- Implemented new First Run Wizard dialog, now gives a wizard interface for when Godot Manager is first run on a system, and nothing has been saved to the Central Store database.  This will give the user a chance to setup Paths for where to install Godot, where to cache files, and where to find Projects, or setup to Save new Projects in said location.
- Deprecated FirstTimeInstall, in favor of new FirstRunWizard.
- Fixed bug on Linux where Installer wasn't properly following Folder structure for Mono Editions of Godot, when decompressing from the Zip File.
- Added new Splash Screen for Godot Manager
- Cleaned up all redundant copies of the Godot Manager Icon, and Updated the Godot Manager Icon to 256x256 sized icon.
- Implemented new method for Updating the Icon of the Executable on Windows, to properly used the correct size definitions, and properly update the icon.
- Implemented integration of Godot Manager with Linux XDG Desktop settings, allowing a user when running First Run Wizard to integrate Godot Manager with the Menu system of their Desktop Environment.

## Version 0.1.3
- Fixed bug where When no update is found, does not hide the BusyDialog. (Thanks to chriba)
- Fixed bug when creating a new Project, the Icon was not properly copied to the new directory.  New method properly reads the data, and writes it to the new project directory, when not using a template.
- Updated Droid Sans fonts.  Grabbed official version of Droid Sans and Droid Sans Bold, that should have Unicode characters for other languages, which should fix Non-English characters from showing in Dialogs.
- Added function to load a ProjectFile from memory buffer (EG: String), there by allowing loading of Project files from Zip Files.
- Added ReadFile() and ReadBuffer() for ZipArchiveEntries, allowing for reading of the file into memory, either as a string, or byte array.
- Fixed Util.LoadImage() to properly read Packed Image files with the executable pack, along with regular images stored in the filesystem.
- Updated UI to properly show a Shade of Grey for files that are disabled, fixing an UI Issue with File Browsing dialogs that are set to look for folders, instead of files, making the text readable.
- Added functions to PaginatedListing control, to allow for loading of Installed Addons / Templates, instead of using Results from an Asset Library query.
- Updated ImportProject, to set required fields for Importing projects, defaulting to the setting for Projects folder.
- Added functionality to allow for Importing of Addons / Templates manually, instead of just from the Asset Library.  This will allow for manually downloaded addons to be installed, along with creating of archives for Addons / Templates from existing projects on the user's system.
- Fixed bug with Cleanup of *.import files, where it was not checking for existance of the folder, before attempting to get a listing of the directory.
- Implemented Sorting Feature.  In ListView, the default sorting is to sort by Favoriates an Last Accessed timestamp.  In CategoryView, it will default to the same sorting of Favoriates, and Last Accessed Timestamp.  Now in ListView, you can now sort by Project Name, and sort by the Version of Godot the project uses.
- Updated Credits to include credit for chriba in source contributions, and our new Sponsor, Quiver.
- Uniform usage of LoadImage(), now uses Util.LoadImage(), instead of a string extension LoadImage(), to reduce code complexity.
- With sorting implemented, switched from always clearing all nodes associated with project listings for all views, to using a Cache system, caching created nodes, with the created projects / categories, to allow for faster sorting.  With the clear and re-create method, it can take anywhere from ~3-5 seconds for a decent quantity of projects to load, now with Cache system, it takes ~500ms to sort things, making things more instantanious and responsive.
- Fixed minor graphical bug with the switch to new Font, that was causing non-displayable characters from showing as default glyphs for Droid Sans.

## Version 0.1.2
- Fixed extension search for Godot executable name in import godot engine dialog, now properly recognizes .64 and .32 on Linux based systems.
- Fixed Add Custom Godot Engine dialog, not closing when adding the engine to the list of available engines.
- Fixed Scan for Projects, now validates that the folders exists, and when no folders exists, prompts user to select new folder.
- Updated Github Workflow to use 3.4.4 for export, instead of 3.4.3
- Added generate_version, so now future executables will be properly version stamped.
- Updated Project store to search by name only, instead of name or location, as location isn't really used.
- Updated Mac with a work around currently to clear extended attributes on the Godot Manager.app package, as it is not Ad-Hoc signed.  (This will be fixed in Godot 3.5)  **NOTE** Mac Users will still need to manually update, as the updater in the previous version did not clear security bits, so will not properly execute the updater.
- Fixed Godot 4 project loading, as format has changed between Godot 3 and 4, for storing Godot specific objects in ConfigFile.  Created special parser to grab only details needed to generate and read proper file format.
- Added links for Itch.io and Discord channels for both Godot Manager, and Github / Itch.io pages.
- Fixed UI bug, where drag and drop would not allow you to drop on a project inside of a category to associate the project with said category.  Now will allow drag and drop to any part of the category to add the project to said category.
- Added scrolling capabilities to the CategoryView, when the category view as more shown above, or below the current view area.  Allowing for easier drag and drop operations between categories.
- Added Management of Godot Asset Plugins, where you can now install, uninstall and update Plugins from the Asset Library, with what is stored in the cache, as well as update any projects that has the asset associated with it.
- Added checks to see if Image exists before attempting to load an image.
- Updated Asset Library Navigation to properly adjust the pages listed, to a limit of 10 pages at a time.
- Added Overlay icons for Assets, to show when you have downloaded the asset, and when there are Updates available for the asset.
- Configured Asset Library panel to no longer automatically configure/search the Asset Library everytime you switch to the tab.  Instead there are timeouts associated with running configure and search updates.  Configure and Search will still run when switching between Addons and Templates
- Fixed bug opening Project and Data folders, not properly launching on Linux or MacOS, now properly works on all three platforms.
- Added similar File Conflict dialog, showing file we're trying to extract from a plugin, and where it is being installed to, giving the options to Overwrite, Overwrite All, Don't Overwrite and Abort.
- Redesigned Settings Panel some, moving All Thanks and Contributions to Contributions tab, and About showing version information, and links to various stuff for Godot Manager.


## Version 0.1.1

- Removed GDScript Addon for Notes as is not needed in the Release of Godot Manager.
- Fixed Listing of projects, so now they are properly sorted by Favorites, as well as last access time.
- Fixed minor bug with View Selector, not properly updating the first icon when it is clicked.
- Special thanks to [Mo8it](https://github.com/Mo8it) for correcting major spelling issues within the documentation, and the code.
- Fixed visibility of category lists, so that they now properly display when Godot Manager re-opens.  (If a category is collapsed, it will remain collapsed)
- Fixed Minor bug in New Project dialog, that didn't properly detect an empty directory as a valid directory in which to create a project in. (Function was still outputting values for '.' and '..' directory markers.)
- Added Changelog to help keep a clean spot for updates, changes, and additions to the code.

## Version 0.1.0

This is the Initial Release of Godot Manager
