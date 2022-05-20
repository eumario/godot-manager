# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

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