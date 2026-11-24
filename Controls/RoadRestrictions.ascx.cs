using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DropdownTests.Controls
{
    public partial class RoadRestrictions : UserControl
    {
        /// <summary>The route whose restrictions are shown. Set by the host page.</summary>
        public int RouteId { get; set; }

        /// <summary>
        /// Add restriction opens the same full-screen list either way.
        /// true (default): tick several; Save attaches the ticked ones and detaches the unticked ones.
        /// false: only one can be ticked; Save adds it. Ones already on the route show ticked and greyed out.
        /// Editing a row always shows its restriction as a dropdown that opens full screen.
        /// Set it in markup, e.g. AllowMultiple="false".
        /// </summary>
        public bool AllowMultiple { get; set; } = true;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            // Set before the posted selection is loaded, so a single list reads one value
            lstRestrictions.SelectionMode = AllowMultiple ? ListSelectionMode.Multiple : ListSelectionMode.Single;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Registered by key, so it loads once even if the control is on the page twice
            ScriptManager.RegisterClientScriptInclude(this, typeof(RoadRestrictions),
                "restriction-picker", ResolveUrl("~/Scripts/restriction-picker.js"));

            if (!IsPostBack) BindGrid();
        }

        private void BindGrid()
        {
            gvRestrictions.DataSource = RouteRestrictions.GetForRoute(RouteId);   // the attached ones
            gvRestrictions.DataBind();
        }

        protected void gvRestrictions_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
                e.Row.TableSection = TableRowSection.TableHeader;   // <thead>, for the sticky header

            // Row in edit mode: fill its dropdown list the usual Web Forms way
            if (e.Row.RowType == DataControlRowType.DataRow && (e.Row.RowState & DataControlRowState.Edit) > 0)
            {
                var row = (RouteRestriction)e.Row.DataItem;
                var ddl = (DropDownList)e.Row.FindControl("ddlRestriction");

                // Restrictions already on another row can't be chosen again
                var taken = new HashSet<int>(RouteRestrictions.GetForRoute(RouteId)
                    .Where(a => a.Id != row.Id).Select(a => a.RestrictionTypeId));

                FillRestrictions(ddl);
                foreach (ListItem item in ddl.Items)
                {
                    int id;
                    if (int.TryParse(item.Value, out id) && taken.Contains(id))
                    {
                        item.Attributes["disabled"] = "disabled";
                        item.Attributes["data-attached"] = "true";      // shown as "Already added"
                    }
                }

                ddl.SelectedValue = row.RestrictionTypeId.ToString();
            }
        }

        protected void gvRestrictions_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvRestrictions.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        protected void gvRestrictions_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            var gridRow = gvRestrictions.Rows[e.RowIndex];
            var id = (int)gvRestrictions.DataKeys[e.RowIndex].Value;
            var ddl = (DropDownList)gridRow.FindControl("ddlRestriction");
            var txtNotes = (TextBox)gridRow.FindControl("txtNotes");

            int typeId;
            if (!int.TryParse(ddl.SelectedValue, out typeId))
            {
                ((Label)gridRow.FindControl("lblRowError")).Text = "Choose a restriction.";
                e.Cancel = true;                // stay in edit mode
                return;
            }

            if (RestrictionTypes.Get(typeId) == null)
            {
                ((Label)gridRow.FindControl("lblRowError")).Text = "Choose a valid restriction.";
                e.Cancel = true;
                return;
            }

            if (HasUnsupportedControlCharacters(txtNotes.Text))
            {
                ((Label)gridRow.FindControl("lblRowError")).Text = "Remove unsupported control characters from the notes.";
                e.Cancel = true;
                return;
            }

            RouteRestrictions.Update(id, typeId, txtNotes.Text);

            gvRestrictions.EditIndex = -1;
            BindGrid();
        }

        // Remove (unattach) a restriction from the route; its notes go with it
        protected void gvRestrictions_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            var id = (int)gvRestrictions.DataKeys[e.RowIndex].Value;
            RouteRestrictions.Detach(id);
            gvRestrictions.EditIndex = -1;
            BindGrid();
        }

        protected void gvRestrictions_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvRestrictions.EditIndex = -1;
            BindGrid();
        }

        // The full-screen list's Save.
        // Several: attach what is ticked, detach what was unticked. One: attach the ticked restriction.
        protected void btnSaveRestrictions_Click(object sender, EventArgs e)
        {
            lblPickerError.Text = "";
            if (!AllowMultiple)
            {
                int typeId;
                if (!int.TryParse(lstRestrictions.SelectedValue, out typeId))
                {
                    lblPickerError.Text = "Choose a restriction before saving.";
                    return;
                }
                if (RestrictionTypes.Get(typeId) == null)
                {
                    lblPickerError.Text = "Choose a valid restriction.";
                    return;
                }
                if (!RouteRestrictions.GetForRoute(RouteId).Any(a => a.RestrictionTypeId == typeId))
                    RouteRestrictions.Attach(RouteId, typeId);
                gvRestrictions.EditIndex = -1;
                BindGrid();
                return;
            }

            var ticked = new List<int>();
            foreach (var item in lstRestrictions.Items.Cast<ListItem>().Where(li => li.Selected && li.Value != ""))
            {
                int typeId;
                if (!int.TryParse(item.Value, out typeId) || RestrictionTypes.Get(typeId) == null)
                {
                    lblPickerError.Text = "Choose valid restrictions.";
                    return;
                }
                ticked.Add(typeId);
            }
            if (ticked.Count == 0)
            {
                lblPickerError.Text = "Choose at least one restriction before saving.";
                return;
            }
            var attached = RouteRestrictions.GetForRoute(RouteId);

            foreach (var typeId in ticked.Except(attached.Select(a => a.RestrictionTypeId)))
                RouteRestrictions.Attach(RouteId, typeId);
            foreach (var a in attached.Where(a => !ticked.Contains(a.RestrictionTypeId)))
                RouteRestrictions.Detach(a.Id);

            gvRestrictions.EditIndex = -1;
            BindGrid();
        }

        private static bool HasUnsupportedControlCharacters(string value)
        {
            return !string.IsNullOrEmpty(value) &&
                value.Any(c => char.IsControl(c) && c != '\r' && c != '\n' && c != '\t');
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            btnPickMany.Attributes["onclick"] = string.Format("RestrictionPicker.open('{0}', '{1}')",
                lstRestrictions.ClientID, btnSaveRestrictions.ClientID);

            // Rebuild the list on every request so the descriptions are always rendered.
            // Several: tick what is attached. One: mark what is attached ("Already added").
            var attachedIds = new HashSet<string>(RouteRestrictions.GetForRoute(RouteId)
                .Select(a => a.RestrictionTypeId.ToString()));
            FillRestrictions(lstRestrictions);
            foreach (ListItem item in lstRestrictions.Items)
            {
                if (item.Value == "" || !attachedIds.Contains(item.Value)) continue;
                if (AllowMultiple) item.Selected = true;
                else item.Attributes["data-attached"] = "true";
            }
        }

        /// <summary>
        /// Fills a DropDownList or ListBox with every restriction: a disabled heading item per group
        /// (empty value) and each restriction's long description in data-desc (blank line = new paragraph).
        /// </summary>
        private static void FillRestrictions(ListControl list)
        {
            list.Items.Clear();
            string group = null;
            foreach (var t in RestrictionTypes.GetAll().OrderBy(t => t.GroupOrder).ThenBy(t => t.Name))
            {
                if (t.GroupName != group)
                {
                    var head = new ListItem(t.GroupName, "");
                    head.Attributes["disabled"] = "disabled";
                    list.Items.Add(head);
                    group = t.GroupName;
                }
                var item = new ListItem(t.Name, t.Id.ToString());
                if (!string.IsNullOrEmpty(t.Description))
                    item.Attributes["data-desc"] = t.Description;
                list.Items.Add(item);
            }
        }
    }
}
