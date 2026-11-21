/* Scripts/restriction-picker.js
   Full-screen tick list for a hidden <asp:ListBox SelectionMode="Multiple">.

   Usage (from the user control):
     <button type="button"
         onclick="RestrictionPicker.open('<%= lstRestrictions.ClientID %>', '<%= btnSaveRestrictions.ClientID %>')">
       Add restriction</button>

   - Multiple list box: tick boxes. Items already selected (attached on the server) show ticked.
   - Single list box: radio buttons, choose one. Items with data-attached="true" show as "Already added".
   - Save copies the choice back into the list box and clicks the hidden Save button (a normal postback).
   - Cancel or Esc closes without changing anything.
   - A list item's data-desc attribute is shown under its name; a blank line starts a new paragraph.
   - A disabled item with an empty value is shown as a group heading.

   Safe alongside the rest of the app: it does nothing until open() is called, adds no page-wide
   event listeners, defines one global (RestrictionPicker), and the overlay is removed from the
   page when it closes. Plain ES5. */
window.RestrictionPicker = window.RestrictionPicker || (function () {
  function el(tag, cls, text) {
    var e = document.createElement(tag);
    if (cls) e.className = cls;
    if (text != null) e.textContent = text;
    return e;
  }

  function open(listId, saveId) {
    var list = document.getElementById(listId), save = document.getElementById(saveId);
    if (!list || !save || document.querySelector('.rp-overlay')) return;   // already open
    var opener = document.activeElement, root = document.documentElement, oldOverflow = root.style.overflow;
    var multi = list.multiple;

    var ov = el('div', 'rp-overlay'), head = el('div', 'rp-head'), sub = el('div', 'rp-sub');
    var body = el('div', 'rp-body'), bar = el('div', 'rp-bar');
    var cancel = el('button', 'rp-btn', 'Cancel'), ok = el('button', 'rp-btn rp-primary', save.value || 'Save');
    ov.setAttribute('role', 'dialog');
    ov.setAttribute('aria-modal', 'true');
    ov.setAttribute('aria-label', list.getAttribute('data-title') || 'Select');
    cancel.type = ok.type = 'button';
    head.appendChild(el('div', 'rp-title', list.getAttribute('data-title') || 'Select'));
    head.appendChild(sub);
    bar.appendChild(cancel);
    bar.appendChild(ok);
    ov.appendChild(head);
    ov.appendChild(body);
    ov.appendChild(bar);

    // One tick box (or radio button) per list item. Disabled items with no value are group headings.
    var boxes = [];
    for (var i = 0; i < list.options.length; i++) {
      var o = list.options[i];
      if (o.disabled && !o.value) { body.appendChild(el('div', 'rp-group', o.text)); continue; }
      var item = el('label', 'rp-item'), cb = el('input'), text = el('div', 'rp-text');
      var already = !multi && o.getAttribute('data-attached') === 'true';
      cb.type = multi ? 'checkbox' : 'radio';
      if (!multi) cb.name = 'rp-pick';      // groups the radios; the overlay is outside the form, so it never posts
      cb.checked = multi && o.selected;
      cb.disabled = o.disabled || already;
      text.appendChild(el('div', 'rp-name', o.text));
      if (already) { item.className = 'rp-item rp-done'; text.appendChild(el('div', 'rp-tag', 'Already added')); }
      var paras = (o.getAttribute('data-desc') || '').split(/\n\s*\n/);
      for (var p = 0; p < paras.length; p++) if (paras[p].trim()) text.appendChild(el('p', 'rp-desc', paras[p].trim()));
      item.appendChild(cb);
      item.appendChild(text);
      body.appendChild(item);
      boxes.push({ opt: o, cb: cb, item: item, was: o.selected });
    }

    function refresh() {
      var n = 0, add = 0, rem = 0, chosen = null;
      for (var i = 0; i < boxes.length; i++) {
        var b = boxes[i], on = b.cb.checked;
        if (!b.cb.disabled) b.item.className = on ? 'rp-item on' : 'rp-item';
        if (on) { n++; chosen = b; }
        if (on && !b.was) add++;
        if (!on && b.was) rem++;
      }
      if (multi) sub.textContent = n + ' selected' + (add || rem ? ' · ' + add + ' to add, ' + rem + ' to remove' : '');
      else {
        sub.textContent = chosen ? 'Selected: ' + chosen.opt.text : 'Choose one restriction to add';
        ok.disabled = !chosen;
      }
    }
    function close() {
      if (ov.parentNode) ov.parentNode.removeChild(ov);
      root.style.overflow = oldOverflow;
      if (opener && opener.focus) opener.focus();
    }

    body.onchange = refresh;
    cancel.onclick = close;
    ok.onclick = function () {
      for (var i = 0; i < boxes.length; i++) boxes[i].opt.selected = boxes[i].cb.checked;
      close();
      save.click();                         // normal postback to btnSaveRestrictions_Click
    };
    ov.onkeydown = function (e) { if (e.key === 'Escape' || e.keyCode === 27) { e.preventDefault(); close(); } };

    document.body.appendChild(ov);
    root.style.overflow = 'hidden';         // stop the page scrolling behind the overlay
    refresh();
    var first = null;                       // start at the first ticked item, or the first that can be chosen
    for (var j = 0; j < boxes.length && !first; j++) if (multi && boxes[j].was) first = boxes[j];
    for (var k = 0; k < boxes.length && !first; k++) if (!boxes[k].cb.disabled) first = boxes[k];
    if (first) {
      body.scrollTop = first.item.offsetTop - body.offsetTop - 60;
      first.cb.focus();
    } else ok.focus();
  }

  return { open: open };
})();
