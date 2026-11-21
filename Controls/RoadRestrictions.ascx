<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RoadRestrictions.ascx.cs"
    Inherits="DropdownTests.Controls.RoadRestrictions" %>

<%-- No JavaScript of its own. The browser's built-in popover opens and closes the full-screen lists:
     popovertarget on a button names the element to show. --%>

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
              <div class="rp-sub">Choose a restriction, then press Update. Close (&times;) to go back to the notes.</div>
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
