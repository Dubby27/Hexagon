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
            TimetableDay? today = Bakalari.actualTimetable.Days.FirstOrDefault((a) => DateTime.Parse(a.Date) == DateTime.Today, null);

            if (today == null)
            {
                if(DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
                {
                    panelStruct = EvaluateWeekend(WeekendType.Normal, false);
                }
                else
                {
                    panelStruct = NoHours();
                }
            }
            else
            {
                switch (today.DayType)
                {
                    case "WorkDay":
                        panelStruct = EvaluateWorkDay(false); break;
                    case "Weekend":
                        panelStruct = EvaluateWeekend(WeekendType.Normal, false); break;
                    case "Celebration":
                        panelStruct = EvaluateWeekend(WeekendType.Celebration, false); break;
                    case "Holiday":
                        panelStruct = EvaluateWeekend(WeekendType.Holiday, false); break;
                    case "DirectorDay":
                        panelStruct = EvaluateWeekend(WeekendType.DirectorDay, false); break;
                    case "Undefined":
                        Shell.Current.DisplayAlert("Chyba rozvrhu", "Rozvrh pro tento den není definován. Bude použit stálý rozvrh.", "Ok");
                        today = Bakalari.permanentTimetable.Days.FirstOrDefault((a) => DateTime.Parse(a.Date) == DateTime.Today, null);
                        panelStruct = EvaluateWorkDay(true); break;
                }
            }
            
            return panelStruct;
        }

        public enum WeekendType
        { 
            Normal,
            Celebration,
            Holiday,
            DirectorDay
        }

        public static QuickPanelStruct EvaluateWeekend(WeekendType type, bool usePermanent)
        {
            QuickPanelStruct panelStruct = new QuickPanelStruct();

            switch (type)
            {
                case WeekendType.Normal:
                    panelStruct.title = "Je víkend"; break;
                case WeekendType.Celebration:
                    panelStruct.title = "Je svátek"; break;
                case WeekendType.Holiday:
                    panelStruct.title = "Jsou prázdniny"; break;
                case WeekendType.DirectorDay:
                    panelStruct.title = "Je ředitelské volno"; break;
            }

            //find next day
            Timetable nextTimetable = usePermanent ? Bakalari.permanentTimetable : Bakalari.nextTimetable;
            TimetableDay nextDay = nextTimetable.Days[0];

            if(nextDay != null)
            {
                if (nextDay.DayType == "WorkDay")
                {
                    panelStruct.lower = "Rozvrh na " + PrintDayOfWeek(DateTime.Parse(nextDay.Date).DayOfWeek) + ":";
                    panelStruct.timetable = TimetableRenderer.Render(nextTimetable, nextDay);
                }
                else
                {
                    nextDay = nextTimetable.Days.FirstOrDefault((a) => a.DayType == "WorkDay", null);
                    if(nextDay != null)
                    {
                        panelStruct.lower = "Rozvrh na " + PrintDayOfWeek(DateTime.Parse(nextDay.Date).DayOfWeek) + ":";
                        panelStruct.timetable = TimetableRenderer.Render(nextTimetable, nextDay);
                    }
                    else
                    {
                        panelStruct.lower = "Příští týden nejsou žádné hodiny, zkontroluj web vašich Bakalářů";
                    }
                }
            }
            else
            {
                panelStruct.lower = "Příští týden nejsou žádné hodiny, zkontroluj web vašich Bakalářů";
            }

            return panelStruct;
        }

        public static string PrintDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch(dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "pondělí";
                case DayOfWeek.Tuesday:
                    return "úterý";
                case DayOfWeek.Wednesday:
                    return "středa";
                case DayOfWeek.Thursday:
                    return "čtvrtek";
                case DayOfWeek.Friday:
                    return "pátek";
                case DayOfWeek.Saturday:
                    return "sobota";
                case DayOfWeek.Sunday:
                    return "neděle";
                default:
                    return "pondělí";
            }
        }

        public static QuickPanelStruct EvaluateWorkDay(bool usePermanent)
        {
            QuickPanelStruct panelStruct = new QuickPanelStruct();

            return panelStruct;
        }

        public static QuickPanelStruct NoHours()
        {
            QuickPanelStruct panelStruct = new QuickPanelStruct();

            panelStruct.title = "Žádné hodiny";
            panelStruct.lower = "Pro jistotu zkonroluj web vašich Bakalářů";

            return panelStruct;
        }
    }
}
