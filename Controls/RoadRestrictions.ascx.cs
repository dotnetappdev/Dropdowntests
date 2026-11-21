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
        /// true (default): Add restriction opens a full-screen tick list to add or remove several at once.
        /// false: Add restriction adds a new row to the grid with a dropdown to choose one restriction.
        /// Either way, editing a row shows the restriction as a dropdown that opens full screen.
        /// Set it in markup, e.g. AllowMultiple="false".
        /// </summary>
        public bool AllowMultiple { get; set; } = true;

        /// <summary>True while the grid shows a new, unsaved row (Add restriction with AllowMultiple="false").</summary>
        private bool Adding
        {
            get { return (bool?)ViewState["Adding"] ?? false; }
            set { ViewState["Adding"] = value; }
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
            var rows = RouteRestrictions.GetForRoute(RouteId).ToList();   // the attached ones
            if (Adding)
            {
                rows.Add(new RouteRestriction { Id = 0, RouteId = RouteId, Notes = "" });   // the new, unsaved row
                gvRestrictions.EditIndex = rows.Count - 1;
            }
            gvRestrictions.DataSource = rows;
            gvRestrictions.DataBind();
        }

        // AllowMultiple="false": add a new row in edit mode
        protected void btnAddRow_Click(object sender, EventArgs e)
        {
            Adding = true;
            BindGrid();
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

                if (row.Id == 0)
                    ddl.Items.Insert(0, new ListItem("-- Select a restriction --", ""));
                else
                    ddl.SelectedValue = row.RestrictionTypeId.ToString();
            }
        }

        protected void gvRestrictions_RowEditing(object sender, GridViewEditEventArgs e)
        {
            Adding = false;                     // editing another row drops an unsaved new row
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

            if (id == 0)
            {
                RouteRestrictions.Attach(RouteId, typeId, txtNotes.Text);
                Adding = false;
            }
            else
            {
                RouteRestrictions.Update(id, typeId, txtNotes.Text);
            }

            gvRestrictions.EditIndex = -1;
            BindGrid();
        }

        protected void gvRestrictions_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            Adding = false;
            gvRestrictions.EditIndex = -1;
            BindGrid();
        }

        // AllowMultiple="true": the tick list's Save. Attach what is ticked, detach what was unticked.
        protected void btnSaveRestrictions_Click(object sender, EventArgs e)
        {
            var ticked = lstRestrictions.Items.Cast<ListItem>()
                .Where(li => li.Selected && li.Value != "")
                .Select(li => int.Parse(li.Value))
                .ToList();
            var attached = RouteRestrictions.GetForRoute(RouteId);

            foreach (var typeId in ticked.Except(attached.Select(a => a.RestrictionTypeId)))
                RouteRestrictions.Attach(RouteId, typeId);
            foreach (var a in attached.Where(a => !ticked.Contains(a.RestrictionTypeId)))
                RouteRestrictions.Detach(a.Id);

            Adding = false;
            gvRestrictions.EditIndex = -1;
            BindGrid();
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            btnPickMany.Visible = AllowMultiple;
            btnAddRow.Visible = !AllowMultiple;
            lstRestrictions.Visible = btnSaveRestrictions.Visible = AllowMultiple;
            if (!AllowMultiple) return;

            btnPickMany.Attributes["onclick"] = string.Format("RestrictionPicker.open('{0}', '{1}')",
                lstRestrictions.ClientID, btnSaveRestrictions.ClientID);

            // Rebuild the tick list on every request so the descriptions are always rendered,
            // and tick what is attached to this route
            var attachedIds = new HashSet<string>(RouteRestrictions.GetForRoute(RouteId)
                .Select(a => a.RestrictionTypeId.ToString()));
            FillRestrictions(lstRestrictions);
            foreach (ListItem item in lstRestrictions.Items)
                item.Selected = item.Value != "" && attachedIds.Contains(item.Value);
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
