﻿using System;
using System.Drawing;
using System.IO;
using Avalonia.Platform;

namespace Avalonia.Win32
{
    internal class IconImpl : IWindowIconImpl, IDisposable
    {
        private readonly Icon _icon;

        public IconImpl(Stream icon)
        {
            _icon = new(icon);
        }

        public IconImpl(IntPtr hIcon)
        {
            _icon = Icon.FromHandle(hIcon);
        }

        // GetSystemMetrics returns values scaled for the primary monitor, as of the time at which the process started.
        // This is no good for a per-monitor DPI aware application. GetSystemMetricsForDpi would solve the problem,
        // but is only available in Windows 10 version 1607 and later. So instead, we just hard-code the 96dpi icon sizes.

        public Icon LoadSmallIcon(double scaleFactor) => new(_icon, GetScaledSize(16, scaleFactor));

        public Icon LoadBigIcon(double scaleFactor) => new(_icon, GetScaledSize(32, scaleFactor));

        private static System.Drawing.Size GetScaledSize(int baseSize, double factor)
        {
            var scaled = (int)Math.Ceiling(baseSize * factor);
            return new(scaled, scaled);
        }

        public void Save(Stream outputStream) => _icon.Save(outputStream);

        public void Dispose() => _icon.Dispose();
    }
}
