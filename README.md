# Dropdown tests: full-screen restriction picker (no JavaScript)

**Branch `no-javascript`.** The `main` branch has the version that uses a small JavaScript file.

An ASP.NET Web Forms user control for a heavy load route. It shows the restrictions attached to the route in a GridView with one column, Notes.

When a row is in edit mode, its restriction shows as a **dropdown in the grid row** (a button styled like a Bootstrap dropdown list). Tapping it opens that row's list full screen, so long descriptions are readable on a phone or a PC. The full-screen list's **Update** and **Cancel** are the row's own Update and Cancel, so `RowUpdating` and `RowCancelingEdit` run as normal. The close button (&times;) goes back to the row to write notes.

This version has no JavaScript of its own. The only script on the page is ASP.NET's own postback script, which the grid's Edit and Update links already use.

## How it opens without JavaScript

It uses the browser's built-in **popover** feature:

```html
<button type="button" popovertarget="RoadRestrictions1_picker">Add restriction</button>

<div id="RoadRestrictions1_picker" popover class="rp-overlay"> ... </div>
```

- `popovertarget` on a button names the element to show. The button and the picker can be anywhere on the page, in any order.
- Cancel is a button with `popovertargetaction="hide"`.
- Save is an ordinary `asp:Button`. The page comes back from the postback with the picker closed.
- The Esc key closes it.
- A popover shows in the browser's top layer, so it is always above the rest of the page, whatever `z-index` your other elements use. A CSS `transform` on a surrounding element doesn't trap it either.

Browser support: Chrome and Edge 114+, Safari 17+, Firefox 125+. In older browsers the Add restriction button does nothing.

### Compared with the JavaScript version (`main`)

- There's no live "2 to add, 1 to remove" count.
- Cancel closes the picker without unticking your changes. Nothing is saved, and the next postback rebuilds the list from what is attached, but if you reopen before then you see your last ticks.
- The list opens at the top rather than at the first ticked item.

## Look and feel

It is styled to match a new Visual Studio **ASP.NET Web Application (.NET Framework) › Web Forms** project, which uses Bootstrap 5's light theme:

- The control uses Bootstrap's own classes: `btn btn-primary` for Add restriction, `table table-striped` on the GridView, and `form-control` on the Notes box. It picks up your site's Bootstrap automatically.
- The picker's Save and Cancel buttons are `btn btn-primary` and `btn btn-outline-secondary`. The rest of the picker uses Bootstrap 5's default colours and spacing in `Content/restriction-picker.css`.
- The demo pages are laid out like the template's `Site.Master`, with the dark navbar and footer.

If your project uses an older template with Bootstrap 3, the control still works. The buttons and table will take Bootstrap 3's look instead.

## Try it without ASP.NET

Open `demo/index.html` in a browser. It uses the real CSS from this repository. The script on that page only pretends to be the server so you can try saving.

`demo/standalone.html` is the same page with the CSS built in, so it runs on its own. Use it to send the demo to someone or open it without the rest of the repository. Both pages have an **AllowMultiple** switch at the top to try choosing one or several.

## Files

| File | What it is |
| --- | --- |
| `Controls/RoadRestrictions.ascx` | The user control: Add restriction button, the tick list popover, and the Notes grid with a dropdown and full-screen list in the edit row |
| `Controls/RoadRestrictions.ascx.cs` | Code-behind: fills the row's list in `RowDataBound`, adds and updates rows, fills and saves the tick list |
| `Controls/RoadRestrictions.ascx.designer.cs` | Control declarations (Visual Studio regenerates this) |
| `Models/RestrictionData.cs` | Sample in-memory data. Replace `RestrictionTypes` and `RouteRestrictions` with your database calls |
| `Content/restriction-picker.css` | Styles for the grid and the picker. Picker rules are scoped to `.rp-overlay` |
| `demo/index.html` | A page you can open in a browser to try it (uses the CSS file above) |
| `demo/standalone.html` | The same demo with the CSS built in, so it runs on its own |

## Using it in your app

1. Copy the files into your Web Forms project. Change the namespace `DropdownTests` to your own.
2. Add `Content/restriction-picker.css` to your master page or site stylesheet.
3. Put the control on a page and set the route:
   ```aspx
   <%@ Register Src="~/Controls/RoadRestrictions.ascx" TagPrefix="uc" TagName="RoadRestrictions" %>
   <uc:RoadRestrictions ID="RoadRestrictions1" runat="server" RouteId="42" />
   ```
4. Replace the sample data in `Models/RestrictionData.cs` with your own data access.

### Choose one or several: `AllowMultiple`

```aspx
<uc:RoadRestrictions ID="RoadRestrictions1" runat="server" RouteId="42" AllowMultiple="false" />
```

| `AllowMultiple` | What Add restriction does |
| --- | --- |
| `true` (default) | Opens a full-screen tick list. Restrictions already on the route are ticked. Save attaches the ticked ones and detaches the unticked ones. |
| `false` | Adds a new row to the grid in edit mode, with an empty dropdown ("-- Select a restriction --") and a Notes box. Choose the restriction, write the notes, then Update. Update with nothing chosen shows "Choose a restriction." and keeps the row open. |

In both modes, **Edit** on a row shows its restriction as a dropdown. Restrictions already on other rows are greyed out and marked "Already added".

### The dropdown in the grid row

Without JavaScript a real `<select>` can't open full screen, so the row shows a button with Bootstrap's `form-select` look that opens a popover:

- `gvRestrictions_RowDataBound` binds the row's `rptPick` Repeater: one radio per restriction, the saved one checked, ones on other rows disabled.
- The list looks like the tick list (same square tick boxes), but only one restriction can be ticked: ticking another moves the tick. It stays open until you press Update or Cancel, or close it.
- The radios are plain HTML inputs that share one name (the row's `UniqueID + "$pick"`), because `asp:RadioButton` inside a Repeater gives each item its own group. `RowUpdating` reads the choice from `Request.Form`.
- The control includes the picker's own styles (a scoped `<style>` block), so the full-screen list fills the screen and lays out correctly even if `restriction-picker.css` is missing or an older copy. A popover always shows above the rest of the page, whatever `z-index` other elements use.
- The dropdown's text changes after Update (changing it straight away would need JavaScript). Until then the row shows "New restriction chosen. Press Update to save it.", done with CSS.

### Descriptions and groups

Each restriction can have a long `Description`. It is shown under the name in the picker, and a blank line starts a new paragraph. Restrictions are grouped under headings by `GroupName`.
