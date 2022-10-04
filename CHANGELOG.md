# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

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