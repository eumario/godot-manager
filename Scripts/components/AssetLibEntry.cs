using Godot;
using GodotSharpExtras;
using Godot.Collections;

public class AssetLibEntry : HBoxContainer
{
#region Node Paths
    [NodePath("Icon")]
    TextureRect _icon = null;

    [NodePath("vc/Title")]
    Label _title = null;

    [NodePath("vc/hc/Category")]
    Label _category = null;

    [NodePath("vc/hc/License")]
    Label _license = null;

    [NodePath("vc/Author")]
    Label _author = null;
#endregion

#region Private Variables
    Texture tIcon;
    string sTitle;
    string sCategory;
    string sLicense;
    string sAuthor;
#endregion

#region Public Accessors
    public Texture Icon {
        get { return (_icon != null ? _icon.Texture : tIcon); }
        set {
            tIcon = value;
            if (_icon != null)
                _icon.Texture = value;
        }
    }

    public string Title {
        get { return (_title != null ? _title.Text : sTitle); }
        set {
            sTitle = value;
            if (_title != null)
                _title.Text = value;
        }
    }

    public string Category {
        get { return (_category != null ? _category.Text : sCategory); }
        set {
            sCategory = value;
            if (_category != null)
                _category.Text = "Category: " + value;
        }
    }

    public string License {
        get { return (_license != null ? _license.Text : sLicense);}
        set {
            sLicense = value;
            if (_license != null)
                _license.Text = "License: " + value;
        }
    }

    public string Author {
        get { return (_author != null ? _author.Text : sAuthor); }
        set {
            sAuthor = value;
            if (_author != null)
                _author.Text = "Author: " + value;
        }
    }
#endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        Icon = tIcon;
        Title = sTitle;
        Category = sCategory;
        License = sLicense;
        Author = sAuthor;
    }
}
