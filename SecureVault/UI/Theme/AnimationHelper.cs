// ============================================
// SecureVault - Animation Helper (Redesigned)
// Comprehensive easing, fade, slide, shake,
// scale, stagger, color-lerp, and shimmer
// ============================================

namespace SecureVault.UI.Theme
{
    /// <summary>
    /// Provides smooth, premium animations for WinForms controls.
    /// </summary>
    public static class AnimationHelper
    {
        // ═══════════════════════════════════════════
        //  Easing Functions
        // ═══════════════════════════════════════════

        public static double EaseOutCubic(double t) => 1 - Math.Pow(1 - t, 3);
        public static double EaseOutQuint(double t) => 1 - Math.Pow(1 - t, 5);
        public static double EaseInCubic(double t) => t * t * t;
        public static double EaseInOutCubic(double t) =>
            t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;
        public static double EaseOutBack(double t)
        {
            const double c1 = 1.70158;
            const double c3 = c1 + 1;
            return 1 + c3 * Math.Pow(t - 1, 3) + c1 * Math.Pow(t - 1, 2);
        }

        // ═══════════════════════════════════════════
        //  Form Animations
        // ═══════════════════════════════════════════

        /// <summary>Fades a form from 0 to 1 opacity.</summary>
        public static void FadeIn(Form form, int durationMs = 400)
        {
            if (durationMs <= 0) { form.Opacity = 1; return; }
            form.Opacity = 0;
            var timer = new System.Windows.Forms.Timer { Interval = 16 };
            double step = 1.0 / (durationMs / 16.0);
            int currentStep = 0;
            int totalSteps = durationMs / 16;

            timer.Tick += (s, e) =>
            {
                if (form.IsDisposed) { timer.Stop(); timer.Dispose(); return; }
                currentStep++;
                double progress = EaseOutQuint((double)currentStep / totalSteps);
                form.Opacity = Math.Min(1.0, progress);
                if (currentStep >= totalSteps)
                {
                    form.Opacity = 1;
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        // ═══════════════════════════════════════════
        //  Slide Animations
        // ═══════════════════════════════════════════

        /// <summary>Slides a control in from the left.</summary>
        public static void SlideInFromLeft(Control control, int targetX, int durationMs = 350)
        {
            if (durationMs <= 0) { control.Left = targetX; control.Visible = true; return; }
            int startX = -control.Width;
            control.Left = startX;
            control.Visible = true;
            Animate(durationMs, t =>
            {
                if (control.IsDisposed) return false;
                control.Left = startX + (int)((targetX - startX) * EaseOutQuint(t));
                return true;
            });
        }

        /// <summary>Slides a control in from the right.</summary>
        public static void SlideInFromRight(Control control, int targetX, int containerWidth, int durationMs = 350)
        {
            if (durationMs <= 0) { control.Left = targetX; control.Visible = true; return; }
            int startX = containerWidth;
            control.Left = startX;
            control.Visible = true;
            Animate(durationMs, t =>
            {
                if (control.IsDisposed) return false;
                control.Left = startX + (int)((targetX - startX) * EaseOutQuint(t));
                return true;
            });
        }

        /// <summary>Slides a control in from the bottom with optional fade effect via height.</summary>
        public static void SlideInFromBottom(Control control, int targetY, int durationMs = 350)
        {
            if (durationMs <= 0) { control.Top = targetY; control.Visible = true; return; }
            var parent = control.Parent;
            int startY = parent?.Height ?? control.Top + 100;
            control.Top = startY;
            control.Visible = true;
            Animate(durationMs, t =>
            {
                if (control.IsDisposed) return false;
                control.Top = startY + (int)((targetY - startY) * EaseOutQuint(t));
                return true;
            });
        }

        // ═══════════════════════════════════════════
        //  Counter Animations
        // ═══════════════════════════════════════════

        /// <summary>Animates a label's text from 0 to a target integer.</summary>
        public static void AnimateCounter(Label label, int targetValue, string? prefix = null,
            string? suffix = null, int durationMs = 800)
        {
            if (durationMs <= 0) { label.Text = $"{prefix}{targetValue}{suffix}"; return; }
            Animate(durationMs, t =>
            {
                if (label.IsDisposed) return false;
                int currentVal = (int)(targetValue * EaseOutCubic(t));
                label.Text = $"{prefix}{currentVal}{suffix}";
                return true;
            }, () => { if (!label.IsDisposed) label.Text = $"{prefix}{targetValue}{suffix}"; });
        }

        /// <summary>Animates storage counter (for byte values).</summary>
        public static void AnimateStorageCounter(Label label, long targetBytes, int durationMs = 800)
        {
            if (durationMs <= 0) { label.Text = FormatBytes(targetBytes); return; }
            Animate(durationMs, t =>
            {
                if (label.IsDisposed) return false;
                long currentVal = (long)(targetBytes * EaseOutCubic(t));
                label.Text = FormatBytes(currentVal);
                return true;
            }, () => { if (!label.IsDisposed) label.Text = FormatBytes(targetBytes); });
        }

        // ═══════════════════════════════════════════
        //  Control Animations
        // ═══════════════════════════════════════════

        /// <summary>Fades a control in by growing its height from 0.</summary>
        public static void FadeInControl(Control control, int durationMs = 300)
        {
            control.Visible = true;
            if (durationMs <= 0) return;
            int targetHeight = control.Height;
            if (targetHeight <= 0) return;
            control.Height = 0;
            Animate(durationMs, t =>
            {
                if (control.IsDisposed) return false;
                control.Height = (int)(targetHeight * EaseOutCubic(t));
                return true;
            }, () => { if (!control.IsDisposed) control.Height = targetHeight; });
        }

        /// <summary>Horizontal shake animation for validation errors.</summary>
        public static void ShakeControl(Control control, int intensity = 6, int durationMs = 400)
        {
            int originalX = control.Left;
            int cycles = 3;
            Animate(durationMs, t =>
            {
                if (control.IsDisposed) return false;
                double decay = 1.0 - t;
                double offset = Math.Sin(t * cycles * 2 * Math.PI) * intensity * decay;
                control.Left = originalX + (int)offset;
                return true;
            }, () => { if (!control.IsDisposed) control.Left = originalX; });
        }

        /// <summary>Scale-in animation (simulated via bounds). Good for dialog entrance.</summary>
        public static void ScaleIn(Control control, int durationMs = 250)
        {
            var targetBounds = control.Bounds;
            int dx = targetBounds.Width / 40;   // 2.5% scale
            int dy = targetBounds.Height / 40;
            control.Bounds = new Rectangle(
                targetBounds.X + dx, targetBounds.Y + dy,
                targetBounds.Width - dx * 2, targetBounds.Height - dy * 2);
            control.Visible = true;

            Animate(durationMs, t =>
            {
                if (control.IsDisposed) return false;
                double p = EaseOutBack(t);
                int cx = (int)(dx * (1 - p));
                int cy = (int)(dy * (1 - p));
                control.Bounds = new Rectangle(
                    targetBounds.X + cx, targetBounds.Y + cy,
                    targetBounds.Width - cx * 2, targetBounds.Height - cy * 2);
                return true;
            }, () => { if (!control.IsDisposed) control.Bounds = targetBounds; });
        }

        /// <summary>
        /// Stagger-animates child Controls with a delay between each.
        /// Each child slides up 20px and fades in via height.
        /// </summary>
        public static void StaggerChildren(Control parent, int delayPerChild = 60, int durationPerChild = 300)
        {
            var children = parent.Controls.Cast<Control>().ToList();
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                int targetTop = child.Top;
                child.Top = targetTop + 20;
                child.Visible = false;

                int delay = i * delayPerChild;
                var delayTimer = new System.Windows.Forms.Timer { Interval = Math.Max(1, delay) };
                delayTimer.Tick += (s, e) =>
                {
                    delayTimer.Stop();
                    delayTimer.Dispose();
                    child.Visible = true;
                    int startTop = child.Top;
                    Animate(durationPerChild, t =>
                    {
                        if (child.IsDisposed) return false;
                        child.Top = startTop + (int)((targetTop - startTop) * EaseOutQuint(t));
                        return true;
                    }, () => { if (!child.IsDisposed) child.Top = targetTop; });
                };
                if (delay <= 0)
                {
                    delayTimer.Dispose();
                    child.Visible = true;
                    int startTop2 = child.Top;
                    Animate(durationPerChild, t =>
                    {
                        if (child.IsDisposed) return false;
                        child.Top = startTop2 + (int)((targetTop - startTop2) * EaseOutQuint(t));
                        return true;
                    }, () => { if (!child.IsDisposed) child.Top = targetTop; });
                }
                else
                {
                    delayTimer.Start();
                }
            }
        }

        /// <summary>
        /// Smoothly animates a controls invalidation while transitioning a float
        /// (useful for hover color lerp). Caller stores the animated value.
        /// </summary>
        public static void AnimateValue(float from, float to, int durationMs,
            Action<float> onUpdate, Action? onComplete = null)
        {
            Animate(durationMs, t =>
            {
                float current = from + (to - from) * (float)EaseOutCubic(t);
                onUpdate(current);
                return true;
            }, onComplete);
        }

        // ═══════════════════════════════════════════
        //  Core Animation Engine
        // ═══════════════════════════════════════════

        /// <summary>
        /// Generic timer-based animation loop. Calls onTick with progress 0→1.
        /// Return false from onTick to abort early.
        /// </summary>
        private static void Animate(int durationMs, Func<double, bool> onTick, Action? onComplete = null)
        {
            var timer = new System.Windows.Forms.Timer { Interval = 16 }; // ~60fps
            int totalSteps = Math.Max(1, durationMs / 16);
            int currentStep = 0;

            timer.Tick += (s, e) =>
            {
                currentStep++;
                double progress = Math.Min(1.0, (double)currentStep / totalSteps);
                if (!onTick(progress) || currentStep >= totalSteps)
                {
                    timer.Stop();
                    timer.Dispose();
                    onComplete?.Invoke();
                }
            };
            timer.Start();
        }

        // ═══════════════════════════════════════════
        //  Utilities
        // ═══════════════════════════════════════════

        /// <summary>Formats bytes into human-readable string.</summary>
        public static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }
    }
}
