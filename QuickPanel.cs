using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hexagon
{
    public struct QuickPanelStruct
    {
        public string? upper;
        public string? title;
        public string? lower;
        public HorizontalStackLayout? timetable;
    }

    internal static class QuickPanel
    {
        public static QuickPanelStruct EvaluateQuickPanel()
        {
            QuickPanelStruct panelStruct = new QuickPanelStruct();
            //get today
            TimetableDay today = Bakalari.actualTimetable.Days.FirstOrDefault((a) => DateTime.Parse(a.Date) == DateTime.Today);
            
            return panelStruct;
        }
    }
}
