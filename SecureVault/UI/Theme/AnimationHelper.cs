// ============================================
// SecureVault - Animation Helper
// Fade-in, slide-in, and counter animations
// ============================================

namespace SecureVault.UI.Theme
{
    /// <summary>
    /// Provides smooth animations for WinForms controls.
    /// </summary>
    public static class AnimationHelper
    {
        /// <summary>
        /// Fades in a form from 0 to 1 opacity.
        /// </summary>
        public static void FadeIn(Form form, int durationMs = 400)
        {
            if (durationMs <= 0) { form.Opacity = 1; return; }
            form.Opacity = 0;
            var timer = new System.Windows.Forms.Timer { Interval = 15 };
            double step = 1.0 / (durationMs / 15.0);

            timer.Tick += (s, e) =>
            {
                if (form.IsDisposed) { timer.Stop(); timer.Dispose(); return; }
                form.Opacity += step;
                if (form.Opacity >= 1)
                {
                    form.Opacity = 1;
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        /// <summary>
        /// Slides a control in from the left.
        /// </summary>
        public static void SlideInFromLeft(Control control, int targetX, int durationMs = 350)
        {
            if (durationMs <= 0) { control.Left = targetX; control.Visible = true; return; }
            int startX = -control.Width;
            control.Left = startX;
            control.Visible = true;

            var timer = new System.Windows.Forms.Timer { Interval = 15 };
            int totalSteps = durationMs / 15;
            int currentStep = 0;

            timer.Tick += (s, e) =>
            {
                if (control.IsDisposed) { timer.Stop(); timer.Dispose(); return; }
                currentStep++;
                double progress = EaseOutCubic((double)currentStep / totalSteps);
                control.Left = startX + (int)((targetX - startX) * progress);

                if (currentStep >= totalSteps)
                {
                    control.Left = targetX;
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        /// <summary>
        /// Slides a control in from the right.
        /// </summary>
        public static void SlideInFromRight(Control control, int targetX, int containerWidth, int durationMs = 350)
        {
            if (durationMs <= 0) { control.Left = targetX; control.Visible = true; return; }
            int startX = containerWidth;
            control.Left = startX;
            control.Visible = true;

            var timer = new System.Windows.Forms.Timer { Interval = 15 };
            int totalSteps = durationMs / 15;
            int currentStep = 0;

            timer.Tick += (s, e) =>
            {
                if (control.IsDisposed) { timer.Stop(); timer.Dispose(); return; }
                currentStep++;
                double progress = EaseOutCubic((double)currentStep / totalSteps);
                control.Left = startX + (int)((targetX - startX) * progress);

                if (currentStep >= totalSteps)
                {
                    control.Left = targetX;
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        /// <summary>
        /// Animates a label's text from 0 to a target number.
        /// </summary>
        public static void AnimateCounter(Label label, int targetValue, string? prefix = null,
            string? suffix = null, int durationMs = 800)
        {
            if (durationMs <= 0) { label.Text = $"{prefix}{targetValue}{suffix}"; return; }
            var timer = new System.Windows.Forms.Timer { Interval = 20 };
            int totalSteps = durationMs / 20;
            int currentStep = 0;

            timer.Tick += (s, e) =>
            {
                if (label.IsDisposed) { timer.Stop(); timer.Dispose(); return; }
                currentStep++;
                double progress = EaseOutCubic((double)currentStep / totalSteps);
                int currentVal = (int)(targetValue * progress);
                label.Text = $"{prefix}{currentVal}{suffix}";

                if (currentStep >= totalSteps)
                {
                    label.Text = $"{prefix}{targetValue}{suffix}";
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        /// <summary>
        /// Animates storage counter (for byte values).
        /// </summary>
        public static void AnimateStorageCounter(Label label, long targetBytes, int durationMs = 800)
        {
            if (durationMs <= 0) { label.Text = FormatBytes(targetBytes); return; }
            var timer = new System.Windows.Forms.Timer { Interval = 20 };
            int totalSteps = durationMs / 20;
            int currentStep = 0;

            timer.Tick += (s, e) =>
            {
                if (label.IsDisposed) { timer.Stop(); timer.Dispose(); return; }
                currentStep++;
                double progress = EaseOutCubic((double)currentStep / totalSteps);
                long currentVal = (long)(targetBytes * progress);
                label.Text = FormatBytes(currentVal);

                if (currentStep >= totalSteps)
                {
                    label.Text = FormatBytes(targetBytes);
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        /// <summary>
        /// Fades a control in using a height grow animation (WinForms substitute for opacity fade).
        /// </summary>
        public static void FadeInControl(Control control, int durationMs = 300)
        {
            control.Visible = true;
            if (durationMs <= 0) return;

            int targetHeight = control.Height;
            if (targetHeight <= 0) return;
            control.Height = 0;

            var timer = new System.Windows.Forms.Timer { Interval = 15 };
            int totalSteps = durationMs / 15;
            int currentStep = 0;

            timer.Tick += (s, e) =>
            {
                if (control.IsDisposed) { timer.Stop(); timer.Dispose(); return; }
                currentStep++;
                double progress = EaseOutCubic((double)currentStep / totalSteps);
                control.Height = (int)(targetHeight * progress);

                if (currentStep >= totalSteps)
                {
                    control.Height = targetHeight;
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        /// <summary>
        /// Cubic ease-out function for smooth deceleration.
        /// </summary>
        private static double EaseOutCubic(double t)
        {
            return 1 - Math.Pow(1 - t, 3);
        }

        /// <summary>
        /// Formats bytes into human-readable string.
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }
    }
}
