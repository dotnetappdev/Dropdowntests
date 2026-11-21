# Dropdown tests: full-screen restriction picker (no JavaScript)

**Branch `no-javascript`.** The `main` branch has the version that uses a small JavaScript file.

An ASP.NET Web Forms user control for a heavy load route. It shows the restrictions attached to the route in a GridView that has one column, Notes. An **Add restriction** button opens a full-screen tick list of every restriction (the source list), with the ones already attached ticked. Save attaches the new ticks and detaches the unticked ones.

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

## Try it without ASP.NET

Open `demo/index.html` in a browser. It uses the real CSS from this repository. The script on that page only pretends to be the server so you can try saving.

## Files

| File | What it is |
| --- | --- |
| `Controls/RoadRestrictions.ascx` | The user control: Add restriction button, the popover picker (an `asp:Repeater` of tick boxes), and the Notes grid |
| `Controls/RoadRestrictions.ascx.cs` | Code-behind: fills the tick list and ticks attached restrictions, saves the ticks, edits notes |
| `Controls/RoadRestrictions.ascx.designer.cs` | Control declarations (Visual Studio regenerates this) |
| `Models/RestrictionData.cs` | Sample in-memory data. Replace `RestrictionTypes` and `RouteRestrictions` with your database calls |
| `Content/restriction-picker.css` | Styles for the grid and the picker. Picker rules are scoped to `.rp-overlay` |
| `demo/index.html` | A page you can open in a browser to try it |

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

| `AllowMultiple` | What the picker does |
| --- | --- |
| `true` (default) | Tick boxes. Restrictions already on the route are ticked. Save attaches the ticked ones and detaches the unticked ones. |
| `false` | Radio buttons, so you choose one restriction. Restrictions already on the route are greyed out and marked "Already added". The button says Add. Pressing Add with nothing chosen does nothing. |

In single mode the radio buttons are plain HTML inputs that share one name, because `asp:RadioButton` inside a Repeater gives each item its own group. The code-behind reads the choice from `Request.Form[PickName]`.

### Descriptions and groups

Each restriction can have a long `Description`. It is shown under the name in the picker, and a blank line starts a new paragraph. Restrictions are grouped under headings by `GroupName`.
