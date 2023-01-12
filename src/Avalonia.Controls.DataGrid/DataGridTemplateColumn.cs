// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Avalonia.Controls.Templates;
using Avalonia.Controls.Utils;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace Avalonia.Controls
{
    public class DataGridTemplateColumn : DataGridColumn
    {
        public static readonly StyledProperty<IDataTemplate> CellTemplateProperty =
            AvaloniaProperty.Register<DataGridTemplateColumn, IDataTemplate>(nameof(CellTemplate));

        [Content]
        public IDataTemplate CellTemplate
        {
            get => GetValue(CellTemplateProperty);
            set => SetValue(CellTemplateProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="CellEditingTemplate"/> property.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate> CellEditingTemplateProperty =
                AvaloniaProperty.Register<DataGridTemplateColumn, IDataTemplate>(nameof(CellEditingTemplate));

        /// <summary>
        /// Gets or sets the <see cref="IDataTemplate"/> which is used for the editing mode of the current <see cref="DataGridCell"/>
        /// </summary>
        /// <value>
        /// An <see cref="IDataTemplate"/> for the editing mode of the current <see cref="DataGridCell"/>
        /// </value>
        /// <remarks>
        /// If this property is <see langword="null"/> the column is read-only.
        /// </remarks>
        public IDataTemplate CellEditingTemplate
        {
            get => GetValue(CellEditingTemplateProperty);
            set => SetValue(CellEditingTemplateProperty, value);
        }

        private bool _forceGenerateCellFromTemplate;

        protected override void EndCellEdit()
        {
            //the next call to generate element should not resuse the current content as we need to exit edit mode
            _forceGenerateCellFromTemplate = true;
            base.EndCellEdit();
        }

        protected override Control GenerateElement(DataGridCell cell, object dataItem)
        {
            if (CellTemplate != null)
            {
                if (_forceGenerateCellFromTemplate)
                {
                    _forceGenerateCellFromTemplate = false;
                    return CellTemplate.Build(dataItem);
                }
                return (CellTemplate is IRecyclingDataTemplate recyclingDataTemplate)
                    ? recyclingDataTemplate.Build(dataItem, cell.Content as Control)
                    : CellTemplate.Build(dataItem);
            }
            if (Design.IsDesignMode)
            {
                return null;
            }
            else
            {
                throw DataGridError.DataGridTemplateColumn.MissingTemplateForType(typeof(DataGridTemplateColumn));
            }
        }

        protected override Control GenerateEditingElement(DataGridCell cell, object dataItem, out ICellEditBinding binding)
        {
            binding = null;
            if(CellEditingTemplate != null)
            {
                return CellEditingTemplate.Build(dataItem);
            }
            else if (CellTemplate != null)
            {
                return CellTemplate.Build(dataItem);
            }
            if (Design.IsDesignMode)
            {
                return null;
            }
            else
            {
                throw DataGridError.DataGridTemplateColumn.MissingTemplateForType(typeof(DataGridTemplateColumn));
            }
        }

        protected override object PrepareCellForEdit(Control editingElement, RoutedEventArgs editingEventArgs)
        {
            return null;
        }

        protected internal override void RefreshCellContent(Control element, string propertyName)
        {
            var cell = element.Parent as DataGridCell;
            if(propertyName == nameof(CellTemplate) && cell is not null)
            {
                cell.Content = GenerateElement(cell, cell.DataContext);
            }

            base.RefreshCellContent(element, propertyName);
        }
        
        public override bool IsReadOnly
        {
            get
            {
                if (CellEditingTemplate is null)
                {
                    return true;
                }

                return base.IsReadOnly;
            }
            set
            {
                base.IsReadOnly = value;
            }
        }
    }
}
