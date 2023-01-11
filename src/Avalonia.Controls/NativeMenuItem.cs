using System;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using Avalonia.Utilities;

namespace Avalonia.Controls
{
    public class NativeMenuItem : NativeMenuItemBase, INativeMenuItemExporterEventsImplBridge
    {
        private readonly CanExecuteChangedSubscriber _canExecuteChangedSubscriber;

        static NativeMenuItem()
        {
            MenuProperty.Changed.AddClassHandler<NativeMenuItem,NativeMenu?>((s, e) =>
            {
                if (e.NewValue.Value is NativeMenu menu)
                {
                    menu.Parent = s;
                }
            });
            CommandProperty.Changed.AddClassHandler<NativeMenuItem, ICommand?>(OnCommandChanged);
        }

        class CanExecuteChangedSubscriber : IWeakEventSubscriber<EventArgs>
        {
            private readonly NativeMenuItem _parent;

            public CanExecuteChangedSubscriber(NativeMenuItem parent)
            {
                _parent = parent;
            }

            public void OnEvent(object? sender, WeakEvent ev, EventArgs e)
            {
                _parent.CanExecuteChanged();
            }
        }


        public NativeMenuItem()
        {
            _canExecuteChangedSubscriber = new CanExecuteChangedSubscriber(this);
        }

        public NativeMenuItem(string header) : this()
        {
            Header = header;
        }

        public static readonly StyledProperty<NativeMenu?> MenuProperty =
            AvaloniaProperty.Register<NativeMenuItem, NativeMenu?>(nameof(Menu), coerce: CoerceMenu);

        [Content]
        public NativeMenu? Menu
        {
            get => GetValue(MenuProperty);
            set => SetValue(MenuProperty, value);
        }

        private static NativeMenu? CoerceMenu(AvaloniaObject sender, NativeMenu? value)
        {
            if (value != null && value.Parent != null && value.Parent != sender)
                throw new InvalidOperationException("NativeMenu already has a parent");
            return value;
        }

        public static readonly StyledProperty<IBitmap?> IconProperty =
            AvaloniaProperty.Register<NativeMenuItem, IBitmap?>(nameof(Icon));

        public IBitmap? Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }  

        public static readonly StyledProperty<string?> HeaderProperty =
            AvaloniaProperty.Register<NativeMenuItem, string?>(nameof(Header));

        public string? Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly StyledProperty<KeyGesture?> GestureProperty =
            AvaloniaProperty.Register<NativeMenuItem, KeyGesture?>(nameof(Gesture));

        public KeyGesture? Gesture
        {
            get => GetValue(GestureProperty);
            set => SetValue(GestureProperty, value);
        }

        public static readonly StyledProperty<bool> IsCheckedProperty =
            AvaloniaProperty.Register<NativeMenuItem, bool>(nameof(IsChecked));

        public bool IsChecked
        {
            get => GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }
        
        public static readonly StyledProperty<NativeMenuItemToggleType> ToggleTypeProperty =
            AvaloniaProperty.Register<NativeMenuItem, NativeMenuItemToggleType>(nameof(ToggleType));

        public NativeMenuItemToggleType ToggleType
        {
            get => GetValue(ToggleTypeProperty);
            set => SetValue(ToggleTypeProperty, value);
        }

        public static readonly StyledProperty<ICommand?> CommandProperty =
            Button.CommandProperty.AddOwner<NativeMenuItem>();

        /// <summary>
        /// Defines the <see cref="CommandParameter"/> property.
        /// </summary>
        public static readonly StyledProperty<object?> CommandParameterProperty =
            Button.CommandParameterProperty.AddOwner<MenuItem>();

        public static readonly StyledProperty<bool> IsEnabledProperty =
           AvaloniaProperty.Register<NativeMenuItem, bool>(nameof(IsEnabled), true);

        public bool IsEnabled
        {
            get => GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        void CanExecuteChanged()
        {
            IsEnabled = Command?.CanExecute(CommandParameter) ?? true;
        }

        public bool HasClickHandlers => Click != null;

        public ICommand? Command
        {
            get => GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        private static void OnCommandChanged(NativeMenuItem sender, AvaloniaPropertyChangedEventArgs<ICommand?> e)
        {
            if (e.OldValue.Value is ICommand oldCommand)
            {
                WeakEvents.CommandCanExecuteChanged.Unsubscribe(oldCommand, sender._canExecuteChangedSubscriber);
            }
            if (e.NewValue.Value is ICommand newCommand)
            {
                WeakEvents.CommandCanExecuteChanged.Subscribe(newCommand, sender._canExecuteChangedSubscriber);
            }

            sender.CanExecuteChanged();
        }

        /// <summary>
        /// Gets or sets the parameter to pass to the <see cref="Command"/> property of a
        /// <see cref="NativeMenuItem"/>.
        /// </summary>
        public object? CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>
        /// Occurs when a <see cref="NativeMenuItem"/> is clicked.
        /// </summary>
        public event EventHandler? Click;

        void INativeMenuItemExporterEventsImplBridge.RaiseClicked()
        {
            Click?.Invoke(this, new EventArgs());

            if (Command?.CanExecute(CommandParameter) == true)
            {
                Command.Execute(CommandParameter);
            }
        }
    }
    
    public enum NativeMenuItemToggleType
    {
        None,
        CheckBox,
        Radio
    }
}
