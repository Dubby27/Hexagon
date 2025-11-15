using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;

namespace Hexagon
{
    public class AutoFontSizeBehavior : Behavior<Label>
    {
        private double _originalFontSize;
        private bool _isAdjusting;

        public double MinFontSize { get; set; } = 10;

        protected override void OnAttachedTo(Label label)
        {
            base.OnAttachedTo(label);
            _originalFontSize = label.FontSize;
            label.SizeChanged += Label_SizeChanged;
        }

        protected override void OnDetachingFrom(Label label)
        {
            base.OnDetachingFrom(label);
            label.SizeChanged -= Label_SizeChanged;
        }

        private async void Label_SizeChanged(object sender, EventArgs e)
        {
            if (_isAdjusting)
                return;

            var label = (Label)sender;
            if (label.Width <= 0 || label.Height <= 0 || string.IsNullOrWhiteSpace(label.Text))
                return;

            _isAdjusting = true;

            // nech chvíli na layout, aby se velikosti ustálily
            await Task.Delay(50);

            double fontSize = _originalFontSize;
            label.FontSize = fontSize;

            var request = label.Measure(label.Width, double.PositiveInfinity);
            var measuredHeight = request.Height;

            while (measuredHeight > label.Height && fontSize > MinFontSize)
            {
                fontSize -= 0.5;
                label.FontSize = fontSize;
                await Task.Yield(); // dovol UI přepočítat
                request = label.Measure(label.Width, double.PositiveInfinity);
                measuredHeight = request.Height;
            }

            _isAdjusting = false;
        }
    }

}
