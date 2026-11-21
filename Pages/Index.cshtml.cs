using System.Collections.Generic;
using System.Linq;
using DropdownTests.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DropdownTests.Pages
{
    public class IndexModel : PageModel
    {
        private readonly RestrictionStore _store;

        public IndexModel(RestrictionStore store)
        {
            _store = store;
        }

        /// <summary>The route whose restrictions are shown (a fixed sample route here).</summary>
        public int RouteId { get; } = 42;

        /// <summary>
        /// Add restriction opens the same full-screen list either way.
        /// true (default): tick several; Save attaches the ticked ones and detaches the unticked ones.
        /// false: only one can be ticked; Save adds it. Ones already on the route show ticked and greyed out.
        /// Read from the query string here (?allowMultiple=false) so the demo can switch; in your app
        /// you would usually fix it in code or configuration.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public bool AllowMultiple { get; set; } = true;

        /// <summary>The row being edited (?editId=3), if any.</summary>
        [BindProperty(SupportsGet = true)]
        public int? EditId { get; set; }

        public IList<RestrictionType> Types { get; private set; }
        public IList<RouteRestriction> Rows { get; private set; }
        public ISet<int> AttachedTypeIds { get; private set; }

        public void OnGet()
        {
            Types = RestrictionTypes.GetAll().OrderBy(t => t.GroupOrder).ThenBy(t => t.Name).ToList();
            Rows = _store.GetForRoute(RouteId);
            AttachedTypeIds = new HashSet<int>(Rows.Select(r => r.RestrictionTypeId));
        }

        // The full-screen list's Save.
        // Several: attach what is ticked, detach what was unticked. One: attach the ticked restriction.
        public IActionResult OnPostSave(List<int> ticked)
        {
            ticked = ticked ?? new List<int>();
            var attached = _store.GetForRoute(RouteId);

            if (!AllowMultiple)
            {
                if (ticked.Count > 0) _store.Attach(RouteId, ticked[0]);
                return BackToList();
            }

            foreach (var typeId in ticked.Except(attached.Select(a => a.RestrictionTypeId)))
                _store.Attach(RouteId, typeId);
            foreach (var a in attached.Where(a => !ticked.Contains(a.RestrictionTypeId)))
                _store.Detach(a.Id);
            return BackToList();
        }

        // A row's Update: its restriction (the dropdown) and notes
        public IActionResult OnPostUpdate(int id, int typeId, string notes)
        {
            _store.Update(id, typeId, notes);
            return BackToList();
        }

        // A row's Remove: unattach the restriction from the route
        public IActionResult OnPostRemove(int id)
        {
            _store.Detach(id);
            return BackToList();
        }

        // Post / Redirect / Get, so refreshing the page doesn't post again
        private IActionResult BackToList()
        {
            return RedirectToPage(new { allowMultiple = AllowMultiple });
        }
    }
}
