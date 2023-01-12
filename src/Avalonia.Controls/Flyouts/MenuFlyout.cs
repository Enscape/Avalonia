using System.Collections;
using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace Avalonia.Controls
{
    public class MenuFlyout : FlyoutBase
    {
        public MenuFlyout()
        {
            Items = new AvaloniaList<object>();
        }

        /// <summary>
        /// Defines the <see cref="Items"/> property
        /// </summary>
        public static readonly StyledProperty<IEnumerable?> ItemsProperty =
            ItemsControl.ItemsProperty.AddOwner<MenuFlyout>();

        /// <summary>
        /// Defines the <see cref="ItemTemplate"/> property
        /// </summary>
        public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
            AvaloniaProperty.Register<MenuFlyout, IDataTemplate?>(nameof(ItemTemplate));

        public Classes FlyoutPresenterClasses => _classes ??= new Classes();

        /// <summary>
        /// Gets or sets the items of the MenuFlyout
        /// </summary>
        [Content]
        public IEnumerable? Items
        {
            get => GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        /// <summary>
        /// Gets or sets the template used for the items
        /// </summary>
        public IDataTemplate? ItemTemplate
        {
            get => GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        private Classes? _classes;

        protected override Control CreatePresenter()
        {
            return new MenuFlyoutPresenter
            {
                [!ItemsControl.ItemsProperty] = this[!ItemsProperty],
                [!ItemsControl.ItemTemplateProperty] = this[!ItemTemplateProperty]
            };
        }

        protected override void OnOpened()
        {
            if (_classes != null)
            {
                SetPresenterClasses(Popup.Child, FlyoutPresenterClasses);
            }
            base.OnOpened();
        }
    }
}
