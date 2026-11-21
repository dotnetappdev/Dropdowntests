# Dropdown tests: full-screen restriction picker (Razor Pages, ASP.NET Core 2.1)

**Branch `razor-pages`.** The same restriction picker as the Web Forms versions, as an ASP.NET Core 2.1 Razor Pages app:

| Branch | Version |
| --- | --- |
| `main` | ASP.NET Web Forms, with a small JavaScript file |
| `no-javascript` | ASP.NET Web Forms, no JavaScript of its own |
| `razor-pages` | ASP.NET Core 2.1 Razor Pages, with the same JavaScript file as `main` |

The page shows the restrictions attached to a heavy load route in a table with one column, Notes:

- **Add restriction** opens a full-screen list of every restriction (the source list), grouped, with each restriction's long description. Restrictions already on the route are ticked. Save attaches the ticked ones and detaches the unticked ones.
- **Edit** on a row shows its restriction as a dropdown list and a Notes box. Tapping the dropdown opens the same full-screen list; its Update and Cancel are the row's own Update and Cancel.
- **Remove** on a row asks first ("Remove Low bridge from this route? Its notes will be deleted too.") and then unattaches it.
- It is styled like a new Visual Studio Web Forms project (Bootstrap 5's light theme, dark navbar).

## Run it

You need the .NET Core 2.1 SDK.

```
dotnet run
```

Then open the address it prints (usually http://localhost:5000). The sample data is in memory, so it resets when the app restarts.

ASP.NET Core 2.1 is no longer supported by Microsoft (support ended in August 2021). To move to a supported version, change `TargetFramework` in `RoadRestrictions.csproj` (for example to `net8.0`), remove the two package references, and replace `Program.cs` and `Startup.cs` with the newer hosting setup. The pages, script and CSS don't need to change.

## Choose one or several: `AllowMultiple`

`AllowMultiple` is a property on the page model (`Pages/Index.cshtml.cs`). The demo reads it from the query string so you can switch between them with the link at the top of the page (`/?allowMultiple=false`). In your app you would usually fix it in code or configuration.

| `AllowMultiple` | What Add restriction does |
| --- | --- |
| `true` (default) | Opens the full-screen list with tick boxes. Restrictions already on the route are ticked. Save attaches the ticked ones and detaches the unticked ones. |
| `false` | Opens the same list, looking the same, but only one restriction can be ticked: ticking another moves the tick. Restrictions already on the route show ticked, greyed out and "Already added". Save adds the ticked one. |

## How it fits together

| File | What it is |
| --- | --- |
| `Pages/Index.cshtml` | The page: Add restriction button, the hidden source list and Save button, and the Notes table with the edit row and Remove |
| `Pages/Index.cshtml.cs` | Page model: `OnGet` loads the data; `OnPostSave`, `OnPostUpdate` and `OnPostRemove` handle the forms, then redirect back (Post/Redirect/Get) |
| `Pages/Shared/_Layout.cshtml` | Layout like the Web Forms template's `Site.Master`; loads Bootstrap 5, the CSS and `restriction-picker.js` |
| `Models/RestrictionStore.cs` | Sample data: `RestrictionTypes` (the source list) and `RestrictionStore` (restrictions on each route), registered as a singleton. Replace with your own data access, for example an EF Core `DbContext` |
| `wwwroot/js/restriction-picker.js` | The full-screen list, the same file as on `main` |
| `wwwroot/css/restriction-picker.css` | Styles for the table, the picker and the Remove box |
| `Startup.cs`, `Program.cs`, `RoadRestrictions.csproj` | Standard ASP.NET Core 2.1 setup |

### The Add restriction list

The page writes a hidden `<select>` with one `<option>` per restriction, the same shape the script uses on `main`:

```cshtml
<form method="post" asp-page-handler="Save">
    <input type="hidden" name="AllowMultiple" value="@(Model.AllowMultiple ? "true" : "false")" />
    <select id="lstRestrictions" name="ticked" multiple="@Model.AllowMultiple" data-title="Restrictions on this route" style="display:none">
        ... <option value="" disabled>Height</option>   (group heading)
        ... <option value="3" selected data-desc="Long description...">Low bridge</option>
    </select>
    <button type="submit" id="btnSaveRestrictions" value="Save" style="display:none">Save</button>
</form>
```

`RestrictionPicker.open('lstRestrictions', 'btnSaveRestrictions')` shows it full screen. Save copies the ticks into the `<select>` and clicks the hidden button, which posts to `OnPostSave(List<int> ticked)`.

- With `AllowMultiple` off, the first option is an empty "-- Select a restriction --", because a single-choice `<select>` always selects something.
- The `AllowMultiple` hidden field is written as text. Razor drops a `value="@someBool"` attribute when the value is false, so it would never post "false".

### The dropdown in the edit row

```cshtml
<span style="position:relative;display:block">
    <select name="typeId" class="form-select" data-title="Obstruction / restriction"
            onkeydown="return RestrictionPicker.selectKey(this, event)"> ... </select>
    <button type="button" tabindex="-1" aria-hidden="true" style="position:absolute;top:0;left:0;width:100%;height:100%;opacity:0"
            onclick="RestrictionPicker.fromSelect(this.parentNode.getElementsByTagName('select')[0])"></button>
</span>
```

- The invisible button over the dropdown means a tap always opens the full-screen list, never the phone's own picker.
- The row's Update button (`gv-update`) and Cancel link (`gv-cancel`) are what the full-screen list's Update and Cancel press. Update posts to `OnPostUpdate(int id, int typeId, string notes)`.
- Restrictions already on other rows are disabled and shown as "Already added".

### Remove

Each row's Remove opens a small confirmation popover (no JavaScript). Its Remove button posts to `OnPostRemove(int id)`.

### Forms and anti-forgery

Every form uses `method="post"` with the form tag helper, so Razor Pages adds and checks the anti-forgery token automatically.
