using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hexagon
{
    internal static class TimetableRenderer
    {
        public static Timetable? timetable;

        public static HorizontalStackLayout RenderDay(Timetable timetable, TimetableDay day)
        {
            HorizontalStackLayout views = new HorizontalStackLayout();
            TimetableRenderer.timetable = timetable;

            List<TimetableAtom> renderAtoms = new List<TimetableAtom>();
            //NAJIT PRVNI HODINU
            foreach (TimetableHour hour in timetable.Hours)
            {
                TimetableAtom? thisClass = day.Atoms.FirstOrDefault((a) =>
                    a.HourId == hour.Id || a.Change == null || a.Change.ChangeType != "Canceled" || a.Change.ChangeType != "Removed"
                    || a.Change.Description != "" || !a.Change.Description.Contains("zruš", comparisonType: StringComparison.InvariantCultureIgnoreCase),
                    null);
                if(thisClass != null )
                {
                    renderAtoms.Add(thisClass);
                    break;
                }
            }
            if (renderAtoms.Count == 0)
            {
                views = GenerateError();
                return views;
            }

            //NAJIT VSECHNY OSTATNI
            foreach (TimetableHour hour in timetable.Hours)
            {
                if(hour.Id > renderAtoms[0].HourId)
                {
                    TimetableAtom? thisClass = day.Atoms.FirstOrDefault((a) =>
                    a.HourId == hour.Id, null);
                    if (thisClass != null)
                    {
                        renderAtoms.Add(thisClass);
                        break;
                    }
                    else
                    {
                        TimetableAtom newAtom = new TimetableAtom
                        {
                            HourId = -27
                        };
                        renderAtoms.Add(newAtom);
                    }
                }
            }

            views = CalculateRenderAtoms(renderAtoms);

            return views;
        }

        public static HorizontalStackLayout CalculateRenderAtoms(List<TimetableAtom> atoms)
        {
            HorizontalStackLayout views = new HorizontalStackLayout();

            //TIME HEADER
            var startHeader = CreateTimeHeader(
                DateTime.Parse(Bakalari.GetTimetableHour(timetable, atoms.First()).BeginTime));
            views.Add(startHeader);
            //CELLS
            foreach (TimetableAtom atom in atoms)
            {

            }

            return views;
        }

        public static AbsoluteLayout CreateTimeHeader(DateTime time)
        {
            AbsoluteLayout startLayout = new AbsoluteLayout
            {
                WidthRequest = 25,
                HeightRequest = 100,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            startLayout.SetAppTheme(startLayout.Background, HexagonGradient.Light, HexagonGradient.Dark);

            return startLayout;
        }

        public static HorizontalStackLayout GenerateError()
        {
            HorizontalStackLayout views = new HorizontalStackLayout();

            Label label = new Label
            {
                Text = "Chyba zobrazení rozvrhu",
                FontFamily = "SpaceGrotesk",
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.WordWrap,
                FontSize = 28,
                HorizontalTextAlignment = TextAlignment.Center,
                IsVisible = false,
                Margin = 15,
            };

            views.Add(label);

            return views;
        }
    }

    internal static class HexagonGradient
    {
        public static LinearGradientBrush Light(float up, float down)
        {
            Application.Current.Resources.TryGetValue("GradientStart", out object start);
            Application.Current.Resources.TryGetValue("GradientEnd", out object end);
            return new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop((Color)start, up),
                    new GradientStop((Color)end, down)
                }
            };
        }

        public static LinearGradientBrush Dark(float up, float down)
        {
            Application.Current.Resources.TryGetValue("GradientStartDark", out object start);
            Application.Current.Resources.TryGetValue("GradientEndDark", out object end);
            return new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop((Color)start, up),
                    new GradientStop((Color)end, down)
                }
            };
        }
    }
}
