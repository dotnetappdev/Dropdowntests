# Dropdown tests: full-screen restriction picker

An ASP.NET Web Forms user control for a heavy load route. It shows the restrictions attached to the route in a GridView with one column, Notes.

When a row is in edit mode, its restriction is an ordinary **`asp:DropDownList` in the grid row**, filled in C# in `RowDataBound`. Tapping it opens the list full screen, so long descriptions are readable on a phone or a PC. Tapping a restriction sets the dropdown and goes back to the row. The full-screen list's **Update** and **Cancel** buttons press the row's own Update and Cancel, so `RowUpdating` and `RowCancelingEdit` run as normal.

This is the version that uses a small JavaScript file. The picker works well on phones and on a PC.

## Look and feel

It is styled to match a new Visual Studio **ASP.NET Web Application (.NET Framework) › Web Forms** project, which uses Bootstrap 5's light theme:

- The control uses Bootstrap's own classes: `btn btn-primary` for Add restriction, `table table-striped` on the GridView, and `form-control` on the Notes box. It picks up your site's Bootstrap automatically.
- The picker's Save and Cancel buttons are `btn btn-primary` and `btn btn-outline-secondary`. The rest of the picker uses Bootstrap 5's default colours and spacing in `Content/restriction-picker.css`.
- The demo pages are laid out like the template's `Site.Master`, with the dark navbar and footer.

If your project uses an older template with Bootstrap 3, the control still works. The buttons and table will take Bootstrap 3's look instead.

## Try it without ASP.NET

Open `demo/index.html` in a browser. It loads the real script and CSS from this repository and uses a fake server so you can click through it.

`demo/standalone.html` is the same page with the script and CSS built in, so it runs on its own. Use it to send the demo to someone or open it without the rest of the repository. Both pages have an **AllowMultiple** switch at the top to try choosing one or several.

## Files

| File | What it is |
| --- | --- |
| `Controls/RoadRestrictions.ascx` | The user control: Add restriction button, the Notes grid with a dropdown list in the edit row, and the hidden list box and Save button for the tick list |
| `Controls/RoadRestrictions.ascx.cs` | Code-behind: fills the row's dropdown in `RowDataBound`, adds and updates rows, fills and saves the tick list |
| `Controls/RoadRestrictions.ascx.designer.cs` | Control declarations (Visual Studio regenerates this) |
| `Models/RestrictionData.cs` | Sample in-memory data. Replace `RestrictionTypes` and `RouteRestrictions` with your database calls |
| `Scripts/restriction-picker.js` | The full-screen list for the row dropdown and the tick list, plain ES5 |
| `Content/restriction-picker.css` | Styles for the grid and the picker |
| `demo/index.html` | A page you can open in a browser to try it (uses the script and CSS files above) |
| `demo/standalone.html` | The same demo with the script and CSS built in, so it runs on its own |

## Using it in your app

1. Copy the files into your Web Forms project. Change the namespace `DropdownTests` to your own.
2. Add `Content/restriction-picker.css` to your master page or site stylesheet.
3. Put the control on a page and set the route:
   ```aspx
   <%@ Register Src="~/Controls/RoadRestrictions.ascx" TagPrefix="uc" TagName="RoadRestrictions" %>
   <uc:RoadRestrictions ID="RoadRestrictions1" runat="server" RouteId="42" />
   ```
   The control registers `Scripts/restriction-picker.js` itself.
4. Replace the sample data in `Models/RestrictionData.cs` with your own data access.

### Choose one or several: `AllowMultiple`

```aspx
<uc:RoadRestrictions ID="RoadRestrictions1" runat="server" RouteId="42" AllowMultiple="false" />
```

| `AllowMultiple` | What Add restriction does |
| --- | --- |
| `true` (default) | Opens the full-screen list with tick boxes. Restrictions already on the route are ticked. Save attaches the ticked ones and detaches the unticked ones. The header shows "3 selected · 1 to add, 1 to remove". |
| `false` | Opens the same list, looking the same, but only one restriction can be ticked: ticking another moves the tick. Restrictions already on the route show ticked, greyed out and "Already added". Save adds the ticked one. |

In both modes, **Edit** on a row shows its restriction as a dropdown list. Restrictions already on other rows are disabled in the dropdown and shown as "Already added" in the full-screen list.

### The dropdown in the grid row

```aspx
<EditItemTemplate>
  <span style="position:relative;display:block">
    <asp:DropDownList ID="ddlRestriction" runat="server" CssClass="form-select"
        data-title="Obstruction / restriction"
        onkeydown="return RestrictionPicker.selectKey(this, event)" />
    <%-- invisible button over the dropdown: a tap opens the full-screen list, never the phone's picker --%>
    <button type="button" tabindex="-1" aria-hidden="true"
        style="position:absolute;top:0;left:0;width:100%;height:100%;margin:0;padding:0;border:0;opacity:0;cursor:pointer"
        onclick="RestrictionPicker.fromSelect(this.parentNode.getElementsByTagName('select')[0])"></button>
  </span>
  ...
</EditItemTemplate>
```

- It is filled in `gvRestrictions_RowDataBound` with `FillRestrictions(ddl)` and `ddl.SelectedValue = ...`, like any DropDownList. `RowUpdating` reads `ddl.SelectedValue`.
- An invisible button lies over the dropdown, so clicking or tapping it always opens the full-screen list, never the browser's or iPhone's own picker. With the keyboard, Enter, Space or Alt+Down on the dropdown opens it.
- The list looks and works like the tick list (same square tick boxes, same header), but only one restriction can be ticked: ticking another moves the tick. It stays open until you press Update or Cancel.
- The full-screen list sets its own position and a very high `z-index`, so it fills the screen above menus and headers even if `restriction-picker.css` isn't loaded. Load the CSS anyway for the styling.
- The row's Update and Cancel links have the classes `gv-update` and `gv-cancel`, which is how the full-screen list finds them.

### Removing a restriction

Each row has **Remove** next to **Edit**. It asks first, in a small box: "Remove Low bridge from this route? Its notes will be deleted too.", with Cancel and Remove. That Remove is the GridView's `CommandName="Delete"`, so `gvRestrictions_RowDeleting` runs and calls `RouteRestrictions.Detach(id)`. The box is a popover (no JavaScript), styled by `.rp-confirm`.

### Descriptions and groups

- Each restriction can have a long `Description`. It is written to the list item's `data-desc` attribute and shown under the name in the picker. A blank line starts a new paragraph.
- The code-behind adds a disabled, empty-value item before each group. The picker shows it as a heading.

## Why it won't interfere with the rest of the app

- It does nothing until `RestrictionPicker.open()` is called. Loading the script adds no event listeners and changes nothing on the page.
- It defines one global name, `RestrictionPicker`. Loading it twice is harmless.
- The overlay only exists while it is open. It is removed from the page on Save, Cancel or Esc, along with its event handlers.
- It doesn't touch your form. The overlay is added to `body`, outside the form, and its tick boxes have no `name`, so they never post. The list box only changes when Save is pressed.
- Every CSS rule for the picker starts with `.rp-overlay`, so it can't restyle anything else.
- It stops the page scrolling while open, then restores the page's own setting and puts focus back on the Add restriction button.
- The hidden Save button has `CausesValidation="false"`, so other validators on the page can't block it.

If your app has popups with a `z-index` above 10000, raise `.rp-overlay`'s `z-index` so the picker stays on top.
