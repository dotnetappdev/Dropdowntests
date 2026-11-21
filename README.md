# Dropdown tests: full-screen restriction picker

An ASP.NET Web Forms user control for a heavy load route. It shows the restrictions attached to the route in a GridView that has one column, Notes. An **Add restriction** button opens a full-screen tick list of every restriction (the source list), with the ones already attached ticked. Save attaches the new ticks and detaches the unticked ones.

This is the version that uses a small JavaScript file. The picker works well on phones and on a PC.

## Try it without ASP.NET

Open `demo/index.html` in a browser. It loads the real script and CSS from this repository and uses a fake server so you can click through it.

`demo/standalone.html` is the same page with the script and CSS built in, so it runs on its own. Use it to send the demo to someone or open it without the rest of the repository. Both pages have an **AllowMultiple** switch at the top to try choosing one or several.

## Files

| File | What it is |
| --- | --- |
| `Controls/RoadRestrictions.ascx` | The user control: Add restriction button, hidden source list box, hidden Save button, and the Notes grid |
| `Controls/RoadRestrictions.ascx.cs` | Code-behind: fills and pre-selects the source list, saves ticks, edits notes |
| `Controls/RoadRestrictions.ascx.designer.cs` | Control declarations (Visual Studio regenerates this) |
| `Models/RestrictionData.cs` | Sample in-memory data. Replace `RestrictionTypes` and `RouteRestrictions` with your database calls |
| `Scripts/restriction-picker.js` | The full-screen tick list, about 90 lines of plain ES5 |
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

| `AllowMultiple` | What the picker does |
| --- | --- |
| `true` (default) | Tick boxes. Restrictions already on the route are ticked. Save attaches the ticked ones and detaches the unticked ones. The header shows "3 selected · 1 to add, 1 to remove". |
| `false` | Radio buttons, so you choose one restriction. Restrictions already on the route are greyed out and marked "Already added". The button says Add and stays disabled until you choose one. |

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
