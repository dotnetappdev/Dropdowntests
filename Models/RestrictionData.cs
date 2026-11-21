using System.Collections.Generic;
using System.Linq;

namespace DropdownTests
{
    // Sample data access so the control runs on its own.
    // Replace the bodies of RestrictionTypes and RouteRestrictions with calls to your database.

    public class RestrictionType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }
        public int GroupOrder { get; set; }
        /// <summary>Long description. A blank line starts a new paragraph.</summary>
        public string Description { get; set; }
    }

    public class RouteRestriction
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public int RestrictionTypeId { get; set; }
        public string RestrictionName { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>The source list of every restriction.</summary>
    public static class RestrictionTypes
    {
        private static readonly List<RestrictionType> All = Build();

        public static IList<RestrictionType> GetAll() { return All; }

        public static RestrictionType Get(int id) { return All.FirstOrDefault(t => t.Id == id); }

        private static List<RestrictionType> Build()
        {
            var groups = new[]
            {
                new { Name = "Height", Items = new[] { "Low bridge", "Overhead power cables", "Overhead telecom cables", "Overhanging trees", "Footbridge", "Gantry / overhead sign", "Height-restricted tunnel" } },
                new { Name = "Width", Items = new[] { "Narrow road", "Width restriction", "Narrow bridge", "Traffic island / central reserve", "Bollards", "Street furniture (signs, lamp posts)", "Parked vehicles", "Road works / barriers" } },
                new { Name = "Weight", Items = new[] { "Weight-restricted bridge", "Weak culvert", "Weight limit on road", "Cellar or void under road" } },
                new { Name = "Length and turning", Items = new[] { "Tight bend", "Hairpin bend", "Small roundabout", "Mini roundabout", "Sharp junction" } },
                new { Name = "Ground clearance", Items = new[] { "Hump-back bridge", "Level crossing", "Speed humps / cushions", "Steep gradient", "Severe camber" } },
                new { Name = "Movement conditions", Items = new[] { "Police escort required", "Time-of-day restriction", "Convoy / low speed only", "Contraflow required", "Toll or barrier" } }
            };
            var descriptions = new Dictionary<string, string>
            {
                ["Low bridge"] = "Any bridge where the headroom is less than the load height plus a safety margin. Check the signed height and the measured height on both carriageways, because headroom often differs between lanes and at the kerb.\n\nIf the load cannot clear the bridge, record the diversion in Notes and confirm it with the highway authority before the movement order is issued.",
                ["Overhead power cables"] = "Overhead lines cross many rural routes and are usually lowest at mid-span, not at the poles. Measure the lowest point over the carriageway, not the verge.\n\nIf the clearance is below the safe distance for the voltage, the network operator must lift the line or switch it off. Book this well ahead and put their reference number in Notes.",
                ["Weight-restricted bridge"] = "A structure with a signed or assessed weight limit below the gross weight or axle loads of the movement.\n\nThe structure owner must be notified. They may require the load to cross alone, at walking pace and on the centre line, or refuse the crossing so that a diversion is needed.",
                ["Small roundabout"] = "The swept path of the vehicle is wider than the roundabout allows.\n\nCommon solutions are overrunning the central island, going the wrong way round under escort, or using rear steer.",
                ["Level crossing"] = "Low loaders and long vehicles can ground on the crossing or take too long to clear it. Where the crossing has a phone, the driver must call the signaller and get permission before crossing."
            };

            var list = new List<RestrictionType>();
            for (var g = 0; g < groups.Length; g++)
                foreach (var name in groups[g].Items)
                {
                    string desc;
                    descriptions.TryGetValue(name, out desc);
                    list.Add(new RestrictionType { Id = list.Count + 1, Name = name, GroupName = groups[g].Name, GroupOrder = g, Description = desc });
                }
            return list;
        }
    }

    /// <summary>Restrictions attached to a route (what the grid shows). In-memory sample only.</summary>
    public static class RouteRestrictions
    {
        private static readonly object Sync = new object();
        private static readonly List<RouteRestriction> Rows = new List<RouteRestriction>();
        private static int _nextId = 1;

        public static IList<RouteRestriction> GetForRoute(int routeId)
        {
            lock (Sync) return Rows.Where(r => r.RouteId == routeId).ToList();
        }

        public static void Attach(int routeId, int restrictionTypeId)
        {
            var type = RestrictionTypes.Get(restrictionTypeId);
            lock (Sync)
                Rows.Add(new RouteRestriction
                {
                    Id = _nextId++, RouteId = routeId, RestrictionTypeId = restrictionTypeId,
                    RestrictionName = type != null ? type.Name : "", Notes = ""
                });
        }

        public static void Detach(int id)
        {
            lock (Sync) Rows.RemoveAll(r => r.Id == id);
        }

        public static void UpdateNotes(int id, string notes)
        {
            lock (Sync)
            {
                var row = Rows.FirstOrDefault(r => r.Id == id);
                if (row != null) row.Notes = notes;
            }
        }
    }
}
