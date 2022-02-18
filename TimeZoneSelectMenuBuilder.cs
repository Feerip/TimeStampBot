using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _04_interactions_framework
{
    public class TimeZoneSelectMenuBuilder
    {
        public double numPages { get { return Math.Ceiling(_allTimeZones.Count / 25.0d); } }
        private IReadOnlyCollection<TimeZoneInfo> _allTimeZones { get; set; }

        public TimeZoneSelectMenuBuilder()
        {
            _allTimeZones = TimeZoneInfo.GetSystemTimeZones();
        }

        public SelectMenuBuilder MenuBuilder(int page)
        {
            int startAtItem = page * 25;
            SelectMenuBuilder menuBuilder = new SelectMenuBuilder()
                .WithPlaceholder($"Page {page + 1} of {numPages}")
                .WithCustomId("menu-1")
                .WithMinValues(1)
                .WithMaxValues(1)
                ;

            for (int idx = startAtItem; idx < startAtItem + 25 && (_allTimeZones.ElementAtOrDefault(idx)) is not null ; idx++)
            {

                menuBuilder.AddOption(_allTimeZones.ElementAt(idx).Id,
                    $"{_allTimeZones.ElementAt(idx).Id}",
                    //$"set-time-zone:test",
                    _allTimeZones.ElementAt(idx).DisplayName.Substring(0, 11) + (_allTimeZones.ElementAt(idx).SupportsDaylightSavingTime ? " - Supports Daylight Savings" : ""))
                    ;
            }
            return menuBuilder;
        }


    }
}
