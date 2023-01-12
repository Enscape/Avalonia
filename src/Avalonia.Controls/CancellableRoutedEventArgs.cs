using Avalonia.Interactivity;

namespace Avalonia.Controls
{
    public class CancellableRoutedEventArgs : RoutedEventArgs
    {
        public bool Cancel { get; set; }
    }
}
