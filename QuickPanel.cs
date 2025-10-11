using Markdig.Extensions.Tables;
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
            //null check
            if(Bakalari.actualTimetable == null || Bakalari.nextTimetable == null || Bakalari.permanentTimetable == null)
            {
                panelStruct.title = "Chyba";
                panelStruct.lower = "Nejsou dostupná žádná uložená data";
            }

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
                        panelStruct = EvaluateWorkDay(today, false); break;
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
                        panelStruct = EvaluateWorkDay(today, true); break;
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

        public static QuickPanelStruct EvaluateWorkDay(TimetableDay today, bool usePermanent)
        {
            QuickPanelStruct panelStruct = new QuickPanelStruct();

            Timetable current = usePermanent ? Bakalari.permanentTimetable : Bakalari.actualTimetable;

            if(today.Atoms.Count == 0)
            {
                panelStruct = NoHours();
            }
            else
            {
                TimetableAtom? lastClass = today.Atoms.LastOrDefault((a) => a.Change == null || a.Change.ChangeType != "Canceled" || a.Change.ChangeType != "Removed", null);
                TimetableAtom? firstClass = today.Atoms.FirstOrDefault((a) => a.Change == null || a.Change.ChangeType != "Canceled" || a.Change.ChangeType != "Removed", null);

                if(lastClass == null)
                {
                    lastClass = today.Atoms.LastOrDefault((a) => a.Change.ChangeType != "Removed", null);
                }
                if (firstClass == null)
                {
                    firstClass = today.Atoms.FirstOrDefault((a) => a.Change.ChangeType != "Removed", null);
                }

                if(DateTime.Parse(Bakalari.GetTimetableHour(current, lastClass).EndTime) < DateTime.Now)
                {
                    TimetableDay? lastDay = current.Days.LastOrDefault((a) => a.DayType == "WorkDay");
                    if(lastDay == today)
                    {
                        panelStruct = EvaluateWeekend(WeekendType.Normal, usePermanent);
                    }
                    else
                    {
                        TimetableDay? nextWorkDay = current.Days.FirstOrDefault((a) => a.DayType == "WorkDay" && current.Days.IndexOf(a) > current.Days.IndexOf(today));
                        panelStruct.title = "Je po škole";
                        panelStruct.lower = "Rozvrh na zítra:";
                        panelStruct.timetable = TimetableRenderer.Render(current, nextWorkDay);
                    }
                }
                else
                {
                    if (DateTime.Parse(Bakalari.GetTimetableHour(current, firstClass).BeginTime) > DateTime.Now)
                    {
                        panelStruct.upper = "Škola ještě nezačala.\nPrvní hodina je";
                        panelStruct.title = firstClass.Change != null ? firstClass.Change.ChangeType == "Canceled" ? firstClass.Change.Description : 
                            Bakalari.GetTimetableSubject(current, firstClass).Name : Bakalari.GetTimetableSubject(current, firstClass).Name;
                        string? nextRoom = Bakalari.GetTimetableRoom(current, firstClass)?.Abbrev;
                        if (nextRoom != null)
                        {
                            panelStruct.lower = panelStruct.lower + " v " + nextRoom;
                        }
                    }
                    else
                    {
                        TimetableAtom? currentClass = today.Atoms.FirstOrDefault((a) => DateTime.Parse(Bakalari.GetTimetableHour(current, a).BeginTime) < DateTime.Now &&
                            DateTime.Parse(Bakalari.GetTimetableHour(current, a).EndTime) > DateTime.Now &&

                            (a.Change == null || a.Change.Description != null || a.Change.Description != "" || !a.Change.Description.Contains("zruš", comparisonType: StringComparison.InvariantCultureIgnoreCase)), null);

                        TimetableAtom? nextClass = today.Atoms.FirstOrDefault((a) => DateTime.Parse(Bakalari.GetTimetableHour(current, a).BeginTime) > DateTime.Now &&
                            (a.Change == null || a.Change.Description != null || a.Change.Description != "" || !a.Change.Description.Contains("zruš", comparisonType: StringComparison.InvariantCultureIgnoreCase)), null
                            );

                        if(currentClass != null)
                        {
                            //POČAS HODINY
                            string? currentRoom = Bakalari.GetTimetableRoom(current, currentClass)?.Abbrev;
                            if (currentRoom != null)
                            {
                                panelStruct.upper = "Právě je v " + currentRoom;
                            }
                            else
                            {
                                panelStruct.upper = "Právě je";
                            }
                            panelStruct.title = currentClass.Change != null ? currentClass.Change.ChangeType == "Canceled" ? currentClass.Change.Description :
                            Bakalari.GetTimetableSubject(current, currentClass).Name : Bakalari.GetTimetableSubject(current, currentClass).Name;
                            string nextSubject = "";
                            if(nextClass != null)
                            {
                                if (nextClass.Change == null)
                                {
                                    nextSubject = Bakalari.GetTimetableSubject(current, nextClass).Name;
                                }
                                else
                                {
                                    if (nextClass.Change.ChangeType == "Canceled" && !nextClass.Change.Description.Contains("zruš", comparisonType: StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        nextSubject = nextClass.Change.Description;
                                    }
                                }
                                string? nextRoom = Bakalari.GetTimetableRoom(current, nextClass)?.Abbrev;
                                if (nextSubject == panelStruct.title)
                                {
                                    //stejna hodina pokracuje
                                    panelStruct.lower = "Stejný předmět pokračuje příští hodinu";
                                    if(currentRoom != null && nextRoom != null)
                                    {
                                        if(currentRoom != nextRoom)
                                        {
                                            panelStruct.lower = panelStruct.lower + " v " + nextRoom;


                                        }
                                    }
                                }
                                else
                                {
                                    if (nextRoom != null)
                                    {
                                        panelStruct.lower = "Další hodina je " + nextSubject + " v " + nextRoom;
                                    }
                                    else
                                    {
                                        panelStruct.lower = "Další hodina je " + nextSubject;
                                    }
                                }
                            }
                            //
                        }
                        else
                        {
                            //PŘESTÁVKA NEBO VOLNÁ HODINA
                            TimetableAtom? beforeClass = today.Atoms.LastOrDefault((a) => 
                                DateTime.Parse(Bakalari.GetTimetableHour(current, a).EndTime) < DateTime.Now);
                            string? nextRoom = Bakalari.GetTimetableRoom(current, nextClass)?.Abbrev;
                            if (nextRoom != null)
                            {
                                panelStruct.lower = "v " + nextRoom;
                            }
                            if (nextClass.HourId - beforeClass.HourId > 1)
                            {
                                //volná hodina
                                panelStruct.upper = "Je volná hodina\nPříští hodina je";
                                panelStruct.lower = panelStruct.lower + " o " + Bakalari.GetTimetableHour(current, nextClass)
                                    .BeginTime;
                            }
                            else
                            {
                                //prestavka
                                panelStruct.upper = "Je přestávka\nPříští hodina je";
                            }
                            string nextSubject = "";
                            if (nextClass.Change == null)
                            {
                                nextSubject = Bakalari.GetTimetableSubject(current, nextClass).Name;
                            }
                            else
                            {
                                if (nextClass.Change.ChangeType == "Canceled" && !nextClass.Change.Description.Contains("zruš", comparisonType: StringComparison.InvariantCultureIgnoreCase))
                                {
                                    nextSubject = nextClass.Change.Description;
                                }
                            }
                            panelStruct.title = nextSubject;
                            //
                        }
                    }
                }
            }

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
