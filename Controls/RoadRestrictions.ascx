<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RoadRestrictions.ascx.cs"
    Inherits="DropdownTests.Controls.RoadRestrictions" %>

<%-- No JavaScript of its own. The browser's built-in popover opens and closes the full-screen lists:
     popovertarget on a button names the element to show. --%>

<%-- The picker's own styles, scoped to .rp-overlay. They keep the full-screen lists working and
     looking right even if Content/restriction-picker.css is missing or an older copy. --%>
<style>
.rp-overlay { background-color: #fff; color: #212529; font-size: 1rem; line-height: 1.5; text-align: left; }
.rp-overlay * { box-sizing: border-box; }
.rp-overlay .rp-head { padding: 1rem; border-bottom: 1px solid #dee2e6; }
.rp-overlay .rp-title { margin: 0; font-size: 1.25rem; font-weight: 500; line-height: 1.2; }
.rp-overlay .rp-sub { margin-top: .25rem; font-size: .875rem; color: #6c757d; }
.rp-overlay .rp-body { flex: 1; min-height: 0; overflow: auto; }
.rp-overlay .rp-group { padding: .75rem 1rem .375rem; font-size: .75rem; font-weight: 600; letter-spacing: .04em; text-transform: uppercase; color: #6c757d; background-color: #f8f9fa; border-bottom: 1px solid #dee2e6; }
.rp-overlay .rp-item { display: flex; gap: .75rem; align-items: flex-start; margin: 0; padding: .75rem 1rem; border-bottom: 1px solid #dee2e6; cursor: pointer; font-weight: 400; }
.rp-overlay .rp-item:hover { background-color: #f8f9fa; }
.rp-overlay .rp-item input { flex: none; width: 1.125rem; height: 1.125rem; margin: .2rem 0 0; -webkit-appearance: none; appearance: none; display: inline-grid; place-content: center; background-color: #fff; border: 1px solid #adb5bd; border-radius: .25rem; cursor: pointer; }
.rp-overlay .rp-item input:checked { background-color: #0d6efd; border-color: #0d6efd; }
.rp-overlay .rp-item input:checked::after { content: ""; width: .35rem; height: .65rem; margin-top: -.15rem; border: solid #fff; border-width: 0 2px 2px 0; transform: rotate(45deg); }
.rp-overlay .rp-item input:disabled { background-color: #e9ecef; cursor: default; }
.rp-overlay .rp-item input:focus-visible { outline: 0; box-shadow: 0 0 0 .25rem rgba(13, 110, 253, .25); }
.rp-overlay .rp-text { flex: 1; min-width: 0; max-width: 75ch; overflow-wrap: anywhere; }
.rp-overlay .rp-name { display: block; }
.rp-overlay .rp-desc { display: block; margin: .25rem 0 0; font-size: .875rem; color: #6c757d; }
.rp-overlay .rp-tag { display: inline-block; margin-top: .25rem; padding: .35em .65em; font-size: .75em; font-weight: 700; line-height: 1; color: #fff; background-color: #6c757d; border-radius: .375rem; }
.rp-overlay .rp-bar { display: flex; gap: .5rem; padding: .75rem 1rem; border-top: 1px solid #dee2e6; background-color: #fff; }
.rp-overlay .rp-bar .btn { flex: 1; padding-top: .625rem; padding-bottom: .625rem; } .rp-overlay { position: fixed; inset: 0; width: 100%; height: 100%; max-width: none; max-height: none; margin: 0; padding: 0; border: 0; }
.rp-overlay:popover-open { display: flex; flex-direction: column; }
html:has(.rp-overlay:popover-open) { overflow: hidden; }   .rp-overlay .rp-item:has(input:checked) { background-color: #e7f1ff; }
.rp-overlay .rp-item:has(input:disabled) { cursor: default; opacity: .65; }
.rp-overlay .rp-item:has(input:disabled):hover { background-color: transparent; } .rp-dropdown { cursor: pointer; }
.rp-changed { display: none; color: #0d6efd; }
.gv td:has(.rp-overlay input[data-orig]:not(:checked)) .rp-changed,
.gv td:not(:has(.rp-overlay input[data-orig])):has(.rp-overlay input:checked) .rp-changed { display: inline-block; margin-top: .25rem; }
.rp-overlay .btn-close { flex: none; margin-top: .125rem; }
</style>

<div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
  <h3 class="h4 m-0">Route obstructions and restrictions</h3>
  <%-- AllowMultiple="true": opens the full-screen tick list to add several at once --%>
  <asp:PlaceHolder ID="phPickManyButton" runat="server">
    <button type="button" class="btn btn-primary" popovertarget="<%= ClientID %>_picker">Add restriction</button>
  </asp:PlaceHolder>
  <%-- AllowMultiple="false": adds a new row to the grid with a dropdown to choose the restriction --%>
  <asp:Button ID="btnAddRow" runat="server" Text="Add restriction" CssClass="btn btn-primary"
      CausesValidation="false" OnClick="btnAddRow_Click" />
</div>

<%-- AllowMultiple="true" only: the full-screen tick list --%>
<asp:PlaceHolder ID="phPickMany" runat="server">
<div id="<%= ClientID %>_picker" popover class="rp-overlay" role="dialog" aria-label="Restrictions on this route">
  <div class="rp-head">
    <h5 class="rp-title">Restrictions on this route</h5>
    <div class="rp-sub">Tick the restrictions that apply, then Save.</div>
  </div>
  <div class="rp-body">
    <asp:Repeater ID="rptRestrictions" runat="server">
      <ItemTemplate>
        <asp:PlaceHolder runat="server" Visible='<%# Eval("IsFirstInGroup") %>'>
          <div class="rp-group"><%#: Eval("GroupName") %></div>
        </asp:PlaceHolder>
        <label class="rp-item">
          <asp:CheckBox ID="chk" runat="server" Checked='<%# Eval("IsAttached") %>' />
          <asp:HiddenField ID="hfId" runat="server" Value='<%# Eval("Id") %>' />
          <span class="rp-text">
            <span class="rp-name"><%#: Eval("Name") %></span>
            <%# Paragraphs((string)Eval("Description")) %>
          </span>
        </label>
      </ItemTemplate>
    </asp:Repeater>
  </div>
  <div class="rp-bar">
    <button type="button" class="btn btn-outline-secondary" popovertarget="<%= ClientID %>_picker" popovertargetaction="hide">Cancel</button>
    <asp:Button ID="btnSaveRestrictions" runat="server" Text="Save" CssClass="btn btn-primary"
        CausesValidation="false" OnClick="btnSaveRestrictions_Click" />
  </div>
</div>
</asp:PlaceHolder>

<div class="grid-box border rounded">
<asp:GridView ID="gvRestrictions" runat="server" AutoGenerateColumns="false"
    CssClass="table table-striped gv" GridLines="None" DataKeyNames="Id" UseAccessibleHeader="true"
    OnRowDataBound="gvRestrictions_RowDataBound"
    OnRowEditing="gvRestrictions_RowEditing"
    OnRowUpdating="gvRestrictions_RowUpdating"
    OnRowCancelingEdit="gvRestrictions_RowCancelingEdit"
    EmptyDataText="No restrictions attached yet. Click Add restriction.">
  <Columns>
    <asp:TemplateField HeaderText="Notes" ItemStyle-CssClass="notes">
      <ItemTemplate>
        <div class="rname"><%#: Eval("RestrictionName") %></div>
        <div class="ntext"><%#: Eval("Notes") %></div>
      </ItemTemplate>
      <EditItemTemplate>
        <%-- Looks like a dropdown list. It opens this row's full-screen list (a popover, no JavaScript). --%>
        <button type="button" class="form-select text-start rp-dropdown" popovertarget="<%# Container.ClientID %>_pick">
          <%#: (int)Eval("Id") == 0 ? "-- Select a restriction --" : Eval("RestrictionName") %>
        </button>
        <span class="rp-changed small">New restriction chosen. Press Update to save it.</span>
        <asp:Label ID="lblRowError" runat="server" CssClass="text-danger small" EnableViewState="false" />

        <%-- This row's full-screen list: one radio per restriction, filled in RowDataBound.
             The bottom bar holds this row's own Update and Cancel. --%>
        <div id="<%# Container.ClientID %>_pick" popover class="rp-overlay" role="dialog" aria-label="Obstruction / restriction">
          <div class="rp-head d-flex align-items-start gap-2">
            <div class="flex-grow-1">
              <h5 class="rp-title">Obstruction / restriction</h5>
              <div class="rp-sub">Tick one restriction, then press Update. Close (&times;) to go back to the notes.</div>
            </div>
            <button type="button" class="btn-close" aria-label="Back to the notes"
                popovertarget="<%# Container.ClientID %>_pick" popovertargetaction="hide"></button>
          </div>
          <div class="rp-body">
            <asp:Repeater ID="rptPick" runat="server">
              <ItemTemplate>
                <asp:PlaceHolder runat="server" Visible='<%# Eval("IsFirstInGroup") %>'>
                  <div class="rp-group"><%#: Eval("GroupName") %></div>
                </asp:PlaceHolder>
                <label class="rp-item">
                  <%-- Plain HTML radios so they share one name (asp:RadioButton in a Repeater can't).
                       data-orig marks the saved choice, so the page can show when it has changed. --%>
                  <input type="radio" name="<%# Eval("RadioName") %>" value="<%# Eval("Id") %>"
                      <%# (bool)Eval("IsCurrent") ? "checked data-orig" : "" %> <%# (bool)Eval("IsTaken") ? "disabled" : "" %> />
                  <span class="rp-text">
                    <span class="rp-name"><%#: Eval("Name") %></span>
                    <asp:PlaceHolder runat="server" Visible='<%# Eval("IsTaken") %>'>
                      <span class="rp-tag">Already added</span>
                    </asp:PlaceHolder>
                    <%# Paragraphs((string)Eval("Description")) %>
                  </span>
                </label>
              </ItemTemplate>
            </asp:Repeater>
          </div>
          <div class="rp-bar">
            <asp:LinkButton runat="server" CommandName="Cancel" Text="Cancel" CssClass="btn btn-outline-secondary" CausesValidation="false" />
            <asp:LinkButton runat="server" CommandName="Update" Text="Update" CssClass="btn btn-primary" />
          </div>
        </div>

        <asp:TextBox ID="txtNotes" runat="server" TextMode="MultiLine" Rows="4" CssClass="form-control mt-2"
            Text='<%# Bind("Notes") %>' />
      </EditItemTemplate>
    </asp:TemplateField>

    <asp:TemplateField>
      <ItemTemplate>
        <asp:LinkButton runat="server" CommandName="Edit" Text="Edit" />
      </ItemTemplate>
      <EditItemTemplate>
        <asp:LinkButton runat="server" CommandName="Update" Text="Update" CssClass="me-2" />
        <asp:LinkButton runat="server" CommandName="Cancel" Text="Cancel" CausesValidation="false" />
      </EditItemTemplate>
    </asp:TemplateField>
  </Columns>
</asp:GridView>
</div>
