/* Scripts/restriction-picker.js
   Full-screen list for the restriction dropdowns, so long descriptions are readable on any screen.

   1. A dropdown list in a GridView row (an ordinary asp:DropDownList, filled in C#):
        <asp:DropDownList ID="ddlRestriction" runat="server" CssClass="form-select"
            onmousedown="return RestrictionPicker.selectDown(this, event)"
            ontouchend="return RestrictionPicker.selectDown(this, event)"
            onkeydown="return RestrictionPicker.selectKey(this, event)" />
      Clicking or tapping it opens the full-screen list instead of the browser's own popup.
      It looks and works like the tick list, but only one restriction can be ticked; ticking
      another moves the tick. Update sets the dropdown and presses the row's Update.
      The list's Update and Cancel press the row's own Update and Cancel (links with class
      gv-update and gv-cancel), so RowUpdating / RowCancelingEdit run as normal.

   2. The Add restriction list, from a hidden asp:ListBox:
        RestrictionPicker.open(listBoxClientId, saveButtonClientId)
      SelectionMode="Multiple": tick several. SelectionMode="Single": the same list, but only one can
      be ticked; restrictions already on the route (data-attached="true") show ticked and greyed out.
      Save copies the ticks into the list box and clicks the hidden Save button (a normal postback).

   In both lists:
   - An item's data-desc attribute is shown under its name; a blank line starts a new paragraph.
   - A disabled item with an empty value is a group heading.
   - In a one-choice list, an item with data-attached="true" shows ticked, greyed out and "Already added".

   The overlay carries its own layout and styles, with a very high z-index, so it fills the screen,
   sits above menus and headers, and renders correctly even if restriction-picker.css is missing.

   Safe alongside the rest of the app: it does nothing until called, adds no page-wide event
   listeners, defines one global (RestrictionPicker), and the overlay is removed when it closes. */
window.RestrictionPicker = window.RestrictionPicker || (function () {
  function el(tag, cls, text) {
    var e = document.createElement(tag);
    if (cls) e.className = cls;
    if (text != null) e.textContent = text;
    return e;
  }

  // The picker's own styles, scoped to .rp-overlay and removed with it. They make it render
  // correctly even if restriction-picker.css is missing or an older copy; Bootstrap's .btn classes
  // still style the buttons when Bootstrap is on the page.
  var BASE_CSS = [
    '.rp-overlay{background:#fff;color:#212529;font-size:1rem;line-height:1.5;text-align:left}',
    '.rp-overlay *{box-sizing:border-box}',
    '.rp-overlay .rp-head{padding:1rem;border-bottom:1px solid #dee2e6}',
    '.rp-overlay .rp-title{margin:0;font-size:1.25rem;font-weight:500;line-height:1.2}',
    '.rp-overlay .rp-sub{margin-top:.25rem;font-size:.875rem;color:#6c757d}',
    '.rp-overlay .rp-group{padding:.75rem 1rem .375rem;font-size:.75rem;font-weight:600;letter-spacing:.04em;text-transform:uppercase;color:#6c757d;background:#f8f9fa;border-bottom:1px solid #dee2e6}',
    '.rp-overlay .rp-item{display:flex;gap:.75rem;align-items:flex-start;margin:0;padding:.75rem 1rem;border-bottom:1px solid #dee2e6;cursor:pointer;font-weight:400}',
    '.rp-overlay .rp-item:hover{background:#f8f9fa}',
    '.rp-overlay .rp-item.on{background:#e7f1ff}',
    '.rp-overlay .rp-done{cursor:default;opacity:.65}',
    '.rp-overlay .rp-done:hover{background:transparent}',
    '.rp-overlay .rp-item input{flex:none;width:1.125rem;height:1.125rem;margin:.2rem 0 0;-webkit-appearance:none;appearance:none;display:inline-grid;place-content:center;background-color:#fff;border:1px solid #adb5bd;border-radius:.25rem;cursor:pointer;}',
    '.rp-overlay .rp-item input:checked{background-color:#0d6efd;border-color:#0d6efd;}',
    '.rp-overlay .rp-item input:checked::after{content:\"\";width:.35rem;height:.65rem;margin-top:-.15rem;border:solid #fff;border-width:0 2px 2px 0;transform:rotate(45deg);}',
    '.rp-overlay .rp-item input:disabled{background-color:#e9ecef;cursor:default;}',
    '.rp-overlay .rp-text{flex:1;min-width:0;max-width:75ch;overflow-wrap:anywhere}',
    '.rp-overlay .rp-name{display:block}',
    '.rp-overlay .rp-desc{display:block;margin:.25rem 0 0;font-size:.875rem;color:#6c757d}',
    '.rp-overlay .rp-tag{display:inline-block;margin-top:.25rem;padding:.35em .65em;font-size:.75em;font-weight:700;line-height:1;color:#fff;background:#6c757d;border-radius:.375rem}',
    '.rp-overlay .rp-bar{padding:.75rem 1rem;border-top:1px solid #dee2e6;background:#fff}',
    '.rp-overlay .rp-bar button{padding:.625rem .75rem;font:inherit;border-radius:.375rem;cursor:pointer}',
    '.rp-overlay .rp-bar button:disabled{opacity:.65;pointer-events:none}'
  ].join('\n');

  // o: { multi, okText, prompt, requireChoice, closeOnPick, onOk, onCancel, onClose }
  function show(list, o) {
    if (document.querySelector('.rp-overlay')) return;   // already open
    var multi = o.multi, root = document.documentElement, oldOverflow = root.style.overflow;
    var title = list.getAttribute('data-title') || 'Select';

    var ov = el('div', 'rp-overlay'), head = el('div', 'rp-head'), sub = el('div', 'rp-sub');
    var body = el('div', 'rp-body'), bar = el('div', 'rp-bar');
    var cancel = el('button', 'btn btn-outline-secondary', 'Cancel'), ok = el('button', 'btn btn-primary', o.okText || 'Save');
    ov.setAttribute('role', 'dialog');
    ov.setAttribute('aria-modal', 'true');
    ov.setAttribute('aria-label', title);
    // Full screen and on top even if restriction-picker.css is missing, older, or overridden
    ov.style.cssText = 'position:fixed;top:0;right:0;bottom:0;left:0;z-index:2147483000;display:flex;' +
      'flex-direction:column;margin:0;background-color:#fff;';
    body.style.cssText = 'flex:1 1 auto;min-height:0;overflow-y:auto;';
    bar.style.cssText = 'display:flex;gap:.5rem;';
    cancel.style.flex = ok.style.flex = '1';
    cancel.type = ok.type = 'button';
    head.appendChild(el('h5', 'rp-title', title));
    head.appendChild(sub);
    bar.appendChild(cancel);
    bar.appendChild(ok);
    ov.appendChild(el('style', null, BASE_CSS));
    ov.appendChild(head);
    ov.appendChild(body);
    ov.appendChild(bar);

    // One tick box (or radio button) per list item
    var boxes = [];
    for (var i = 0; i < list.options.length; i++) {
      var opt = list.options[i];
      if (!opt.value) {                                  // group heading, or a "-- Select --" placeholder
        if (opt.disabled) body.appendChild(el('div', 'rp-group', opt.text));
        continue;
      }
      var item = el('label', 'rp-item'), cb = el('input'), text = el('div', 'rp-text');
      var already = !multi && opt.getAttribute('data-attached') === 'true';
      // One choice: radios (styled as tick boxes) so only one can be ticked. Restrictions already
      // on the route show ticked and greyed out, like the tick list, but can't be chosen again.
      cb.type = multi || already ? 'checkbox' : 'radio';
      if (cb.type === 'radio') cb.name = 'rp-pick';   // the overlay is outside the form, so it never posts
      cb.checked = opt.selected || already;
      cb.disabled = opt.disabled || already;
      text.appendChild(el('div', 'rp-name', opt.text));
      if (already) { item.className = 'rp-item rp-done'; text.appendChild(el('div', 'rp-tag', 'Already added')); }
      var paras = (opt.getAttribute('data-desc') || '').split(/\n\s*\n/);
      for (var p = 0; p < paras.length; p++) if (paras[p].trim()) text.appendChild(el('p', 'rp-desc', paras[p].trim()));
      item.appendChild(cb);
      item.appendChild(text);
      body.appendChild(item);
      boxes.push({ opt: opt, cb: cb, item: item, was: opt.selected });
    }

    function refresh() {
      var n = 0, add = 0, rem = 0, chosen = null;
      for (var i = 0; i < boxes.length; i++) {
        var b = boxes[i], on = b.cb.checked;
        if (!b.cb.disabled) b.item.className = on ? 'rp-item on' : 'rp-item';
        if (on && (multi || !b.cb.disabled)) { n++; chosen = b; }
        if (on && !b.was) add++;
        if (!on && b.was) rem++;
      }
      if (multi) sub.textContent = n + ' selected' + (add || rem ? ' · ' + add + ' to add, ' + rem + ' to remove' : '');
      else {
        sub.textContent = chosen ? '1 selected \u00b7 ' + chosen.opt.text : (o.prompt || 'Choose one');
        if (o.requireChoice) ok.disabled = !chosen;
      }
      return chosen;
    }
    function apply() {                      // copy the choice back into the real list
      if (multi) { for (var i = 0; i < boxes.length; i++) boxes[i].opt.selected = boxes[i].cb.checked; return; }
      var chosen = refresh();
      if (chosen && list.value !== chosen.opt.value) {
        list.value = chosen.opt.value;
        var ev = document.createEvent('HTMLEvents');   // keeps AutoPostBack and onchange working
        ev.initEvent('change', true, false);
        list.dispatchEvent(ev);
      }
    }
    function close() {
      if (ov.parentNode) ov.parentNode.removeChild(ov);
      root.style.overflow = oldOverflow;
      if (o.onClose) o.onClose();
    }

    body.onchange = function () {
      refresh();
      if (!multi && o.closeOnPick) { apply(); close(); }
    };
    cancel.onclick = function () { close(); if (o.onCancel) o.onCancel(); };
    ok.onclick = function () { apply(); close(); if (o.onOk) o.onOk(); };
    ov.onkeydown = function (e) { if (e.key === 'Escape' || e.keyCode === 27) { e.preventDefault(); close(); } };

    document.body.appendChild(ov);
    root.style.overflow = 'hidden';         // stop the page scrolling behind the overlay
    refresh();
    var first = null;                       // start at the current choice, or the first that can be chosen
    for (var j = 0; j < boxes.length && !first; j++) if (boxes[j].was) first = boxes[j];
    for (var k = 0; k < boxes.length && !first; k++) if (!boxes[k].cb.disabled) first = boxes[k];
    if (first) {
      body.scrollTop = first.item.offsetTop - body.offsetTop - 60;
      first.cb.focus();
    } else ok.focus();
  }

  // Tick list to add several: hidden ListBox + hidden Save button
  function open(listId, saveId) {
    var list = document.getElementById(listId), save = document.getElementById(saveId);
    if (!list || !save) return;
    var opener = document.activeElement;
    show(list, {
      multi: list.multiple, okText: save.value || 'Save', prompt: 'Tick one restriction to add', requireChoice: !list.multiple,
      onOk: function () { save.click(); },
      onClose: function () { if (opener && opener.focus) opener.focus(); }
    });
  }

  // Dropdown list in a GridView row: Update / Cancel press the row's own buttons
  function fromSelect(select) {
    if (!select || select.disabled) return;
    var row = select.parentNode;
    while (row && row.tagName !== 'TR') row = row.parentNode;
    var update = row && row.querySelector('.gv-update'), cancel = row && row.querySelector('.gv-cancel');
    show(select, {
      multi: false, prompt: 'Choose a restriction',
      okText: update ? update.textContent : 'Done',
      onOk: function () { if (update) update.click(); },
      onCancel: function () { if (cancel) cancel.click(); },
      onClose: function () { select.focus(); }
    });
  }

  // Mouse or touch on the dropdown: stop the browser's own popup and open the full-screen list
  function selectDown(select, e) {
    if (e.button > 0) return true;          // only the main button
    if (e.preventDefault) e.preventDefault();
    fromSelect(select);
    return false;
  }

  // Keyboard on the dropdown: Enter, Space, Alt+Down or F4 open the full-screen list
  function selectKey(select, e) {
    var k = e.key || '';
    if (k === 'Enter' || k === ' ' || k === 'F4' || (e.altKey && k === 'ArrowDown') || e.keyCode === 13 || e.keyCode === 32) {
      if (e.preventDefault) e.preventDefault();
      fromSelect(select);
      return false;
    }
    return true;
  }

  return { open: open, fromSelect: fromSelect, selectDown: selectDown, selectKey: selectKey };
})();
