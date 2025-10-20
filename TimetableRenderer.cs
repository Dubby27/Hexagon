using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hexagon
{
    internal static class TimetableRenderer
    {
        public static Timetable? timetable;

        public static View RenderDay(Timetable timetable, TimetableDay day)
        {
            View views = new HorizontalStackLayout();
            TimetableRenderer.timetable = timetable;

            List<TimetableAtom> renderAtoms = new List<TimetableAtom>();
            //NAJIT PRVNI HODINU
            foreach (TimetableHour hour in timetable.Hours)
            {
                TimetableAtom? thisClass = day.Atoms.FirstOrDefault((a) =>
                    a.HourId == hour.Id && a.Change == null || (a.Change.ChangeType != "Canceled" && a.Change.ChangeType != "Removed"
                    && a.Change.Description != "" && !a.Change.Description.Contains("zruš", comparisonType: StringComparison.InvariantCultureIgnoreCase)),
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
                if (hour.Id > renderAtoms[0].HourId)
                {
                    TimetableAtom? thisClass = day.Atoms.FirstOrDefault((a) =>
                    a.HourId == hour.Id, null);
                    if (thisClass != null)
                    {
                        renderAtoms.Add(thisClass);
                        continue;
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

            //ODREZAT PRAZDNE NA KONCI
            int lastIndex = renderAtoms.FindLastIndex(a => a.HourId != -27);
            renderAtoms.RemoveRange(lastIndex + 1, renderAtoms.Count - lastIndex - 1);

            views = CalculateRenderAtoms(renderAtoms);

            return views;
        }

        public static View CalculateRenderAtoms(List<TimetableAtom> atoms)
        {
            Shadow shadow = new Shadow
            {
                Opacity = 0.5f,
                Offset = new Point(7, 7),
                Radius = 30
            };
            FlexLayout views = new FlexLayout
            {
                Background = new SolidColorBrush(HexagonColors.PanelColor()),
                Wrap = FlexWrap.Wrap,
                Direction = FlexDirection.Row,
                HorizontalOptions = LayoutOptions.Center,
                AlignContent = FlexAlignContent.Center,
                Padding = 5
            };
            Border border = new Border
            {
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = 10
                },
                StrokeThickness = 0,
                Background = new SolidColorBrush(HexagonColors.PanelColor()),
                Content = views,
                Shadow = shadow
            };

            //TIME HEADER
            var startHeader = CreateTimeHeader(
                DateTime.Parse(Bakalari.GetTimetableHour(timetable, atoms.First()).BeginTime), false);
            views.Add(startHeader);
            //CELLS
            foreach (TimetableAtom atom in atoms)
            {
                CreateAtomCell(atom);
                views.Add(CreateAtomCell(atom));
            }
            //TIME HEADER
            var endHeader = CreateTimeHeader(
                DateTime.Parse(Bakalari.GetTimetableHour(timetable, atoms.Last()).EndTime), true);
            views.Add(endHeader);

            return border;
        }

        public static View CreateTimeHeader(DateTime time, bool isEnd)
        {
            BindableHorizontalLayout startLayout = new BindableHorizontalLayout
            {
                WidthRequest = 25,
                HeightRequest = 100,
                Margin = 1
            };
            Border border = new Border
            {
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = isEnd ? new CornerRadius(0, 10, 0, 10) :
                                          new CornerRadius(10, 0, 10, 0)
                },
                StrokeThickness = 0,
                Background = new SolidColorBrush(HexagonColors.PanelColor()),
                Content = startLayout
            }; 
            AbsoluteLayout insideLayout = new AbsoluteLayout
            {
                WidthRequest = 25,
                HeightRequest = 100,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            Label timeLabel = new Label
            {
                Text = time.ToString("H:mm"),
                FontFamily = "SpaceGrotesk",
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.NoWrap,
                FontSize = 16,
                Rotation = isEnd ? 90 : -90,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            startLayout.Add(insideLayout);
            insideLayout.SetLayoutBounds(timeLabel, new Rect(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            insideLayout.SetLayoutFlags(timeLabel, AbsoluteLayoutFlags.PositionProportional);
            insideLayout.Add(timeLabel);
            startLayout.SetAppTheme(BindableHorizontalLayout.BackgroundProperty,
                HexagonColors.Light(BoolToFloat(!isEnd), BoolToFloat(isEnd)), HexagonColors.Dark(BoolToFloat(!isEnd), BoolToFloat(isEnd)));

            return border;
        }

        static float BoolToFloat(bool val)
        {
            return val ? 1f : 0f;
        }

        public static BindableHorizontalLayout CreateAtomCell(TimetableAtom atom)
        {
            BindableHorizontalLayout cellLayout = new BindableHorizontalLayout
            {
                WidthRequest = 75,
                HeightRequest = 100,
                Margin = 1
            };
            Grid insideLayout = new Grid
            {
                WidthRequest = 75,
                HeightRequest = 100,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            Label subjectLabel = new Label
            {
                Text = Bakalari.GetTimetableSubject(timetable, atom)?.Abbrev,
                FontFamily = "SpaceGrotesk",
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.NoWrap,
                FontSize = 24,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            Label teacherLabel = new Label
            {
                Text = atom.Theme,
                FontFamily = "SpaceGrotesk",
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.End,
                LineBreakMode = LineBreakMode.NoWrap,
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            cellLayout.Add(insideLayout);
            insideLayout.Add(subjectLabel);
            insideLayout.Add(teacherLabel);
            cellLayout.SetAppTheme(BindableHorizontalLayout.BackgroundProperty,
                HexagonColors.BackgroundColor(), HexagonColors.BackgroundColor());

            return cellLayout;
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
                IsVisible = true,
                Margin = 15,
            };

            views.Add(label);

            return views;
        }
    }

    internal static class HexagonColors
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
                    new GradientStop(Colors.Blue, up),
                    new GradientStop(Colors.BlueViolet, down)
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

        public static Color PanelColor()
        {
            return DeviceInfo.Idiom == DeviceIdiom.Phone ?
                Application.Current.RequestedTheme == AppTheme.Light ?
                    Colors.White : Colors.Black :
                Application.Current.RequestedTheme == AppTheme.Light ?
                    Colors.WhiteSmoke : Colors.Black ;
        }

        public static Color BackgroundColor()
        {
            Application.Current.Resources.TryGetValue("OffBlack", out object value);
            return DeviceInfo.Idiom == DeviceIdiom.Phone ?
                Application.Current.RequestedTheme == AppTheme.Light ?
                    Colors.WhiteSmoke : Colors.Black :
                Application.Current.RequestedTheme == AppTheme.Light ?
                    Colors.White : (Color)value;
        }
    }

    public class BindableHorizontalLayout : HorizontalStackLayout
    {
        public static readonly BindableProperty BackgroundProperty =
            BindableProperty.Create(
                nameof(Background),
                typeof(Brush),
                typeof(BindableHorizontalLayout),
                defaultValue: null,
                propertyChanged: OnBackgroundChanged);

        public new Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        private static void OnBackgroundChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var layout = (BindableHorizontalLayout)bindable;
            layout.SetAppBackground((Brush)newValue);
        }

        private void SetAppBackground(Brush brush)
        {
            // Nastavení pozadí pro MAUI 7 (kde Background není bindable)
#if NET7_0_OR_GREATER
            base.Background = brush;
#else
            this.Background = brush;
#endif
        }
    }

}
