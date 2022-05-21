# Godot Manager Locale/Translations

This folder will contain the translations for all the Godot Manager strings to allow for translations into other languages.

## Setup

Currently, you need to have the following things installed to properly translate Godot Manager strings.

- **GNU Gettext**  Currently we are using the GNU gettext library for translations, as Godot supports this format, and it is easier to manage.  You can grab it from here: Windows install [64bit Static](https://mlocati.github.io/articles/gettext-iconv-windows.html), macOS: [Homebrew](https://brew.sh) using `brew install gettext` command, or through [MacPorts](https://www.macports.org) with the `sudo port install gettext` command.  Linux, you can use your distro's Pacckage Manager to install the `gettext` package.
- **PyBabel** This is needed to manage the PO Templates.  You will need Python installed, and you can use the command: `pip3 install babel babel-godot babel-thrive` to install the required packages for creating the templates.
- **PoEdit** The easiest way to do translations, is to use the PoEdit editor, which can be found [here](https://poedit.net).

## Updating PO Template

To update the POT file (PO Template), you will need to open a Terminal, and Go into the locale folder, and execute the pybabel extract command.

Example:
```
[eumario@dev ~]: cd Projects/godot-manager/locale
[eumario@dev locale]: pybabel extract -F babelrc -k text -k LineEdit/placeholder_text -k Tr -o godot-l10n.pot ..
extracting messages from ..\Scenes\MainWindow.tscn (encoding="utf-8")
extracting messages from ..\Scenes\SceneManager.tscn (encoding="utf-8")
extracting messages from ..\Scenes\UpdateWindow.tscn (encoding="utf-8")
extracting messages from ..\Scripts\Scenes\MainWindow.cs (encoding="utf-8")
extracting messages from ..\Scripts\Scenes\SceneManager.cs (encoding="utf-8")
extracting messages from ..\Scripts\Scenes\UpdateWindow.cs (encoding="utf-8")
<Bunch of extracting messages>
writing PO template file to godot-l10n.pot
[eumario@dev locale]: 
```

This will ensure all strings are properly updated.  This needs to be run everytime a Label/Button Text is changed, or the Godot Tr() function is used in C#.  If no changes are made to the source code from checkout, the latest pot should be available for users to update.

## Adding Translations

Once you have an up to date Template, you can open the template with PoEdit, and begin adding translations for strings.  You may get an error dialog about the '\n' character being used in strings, you can safely ignore these.  Once open, you can go to the bottom of the window, and select the "Create new translation" button to create a translation for the specified language, and start going through, and updating all strings with the proper translation.  Once done, you can select, File > Save from the menu bar, or click the Floppy Disk Icon marked Save on the Toolbar, to save it as a PO file.

Once the PO file has been generated, you can add it to the project, by going into the Godot Editor, Going to Project > Project Settings... menu, then click on the Localization tab, and click Add... button, navigate to locale, and select the language file, to add it to the project.

## Special Formatting Characters

Godot Manager's C# Code utilizes String Formatting for messages displayed to the user.  As referenced in the POT file, there's Special characters that are used as placeholders for these user input strings or numbers.  These are generally represented as **%s** or **%d**, do not change these characters, as they are needed to properly substitute information in their place holder status.

Another thing used is Escape Characters, such as **\n** to represent a new line.  Do not change these, as they can mess up the formatting of the string.

## Getting Translation Added to Main Repository

To get the translation submitted to the Official repository, it is best to fork the code into your own repository, add your new Translation files to the repository, and then create a Pull Request with the official repository to submit your translations to be added to the main Godot Manager repository.

## Questions, Comments, Suggestions

If you wish to discuss, have questions, or need help with any of these things, feel free to join us on Discord, and we'll be happy to help you out.