# Mercurial Source Control Plugin for MS Visual Studio 2022 #

Fork of https://github.com/vitidev/VisualHgMod2

+ update for VS2022
+ project status icon

### Remarks

#### `vsix` generation

I had to create a new empty project and copy the code files into it 

#### context menu

`VSCTCompile ` in `csproj` and `[ProvideMenuResource("Menus.ctmenu", 1)]` attribute as described [there](https://docs.microsoft.com/en-us/visualstudio/extensibility/internals/how-to-create-a-dot-vsct-file?view=vs-2022).

#### status icons

For some unknown reason, the method `IVsSccGlyphs.GetCustomGlyphList` is no longer called (bug???) and the standard method for using custom icons does not work.

And the status icons for git are very terrible. Therefore, I had to use the method `IVsSccGlyphs2.GetCustomGlyphMonikerList`

I don't know how to add custom icons yet, so I used the [existing one.](https://glyphlist.azurewebsites.net/knownmonikers/) which are more or less normally displayed on my monitor.

    /// <summary>
    /// This list of custom monikers will be appended to the standard moniker list
    /// </summary>
    List<ImageMoniker> monikers = new List<ImageMoniker>
    {
        KnownMonikers.OnlineStatusBusy, //???
        KnownMonikers.OnlineStatusBusy, //Modified ++
        KnownMonikers.AddNoColor, //Added +++
        KnownMonikers.Cancel, //Removed
        KnownMonikers.OnlineStatusAvailable, //Clean
        KnownMonikers.OnlineStatusAway, //Missing
        KnownMonikers.Blank, //NotTracked
        KnownMonikers.OnlineStatusUnknown, //Ignored
        KnownMonikers.OnlineStatusOffline, //Renamed
        KnownMonikers.OnlineStatusOffline, //Copied
    };