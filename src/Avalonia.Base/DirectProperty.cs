using System;
using Avalonia.Data;

namespace Avalonia
{
    /// <summary>
    /// An Avalonia property which stores its own value. Always read-only.
    /// </summary>
    /// <typeparam name="TOwner">The class that registered the property.</typeparam>
    /// <typeparam name="TValue">The type of the property's value.</typeparam>
    /// <remarks>
    /// Direct avalonia properties are backed by a field on the object, but exposed via the
    /// <see cref="AvaloniaProperty"/> system. They hold a getter which
    /// allows the avalonia property system to read and write the current value.
    /// </remarks>
    public class DirectProperty<TOwner, TValue> : DirectPropertyBase<TValue>
        where TOwner : AvaloniaObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectProperty{TOwner, TValue}"/> class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="getter">Gets the current value of the property.</param>
        /// <param name="metadata">The property metadata.</param>
        public DirectProperty(
            string name,
            Func<TOwner, TValue> getter,
            DirectPropertyMetadata<TValue> metadata)
            : base(name, typeof(TOwner), metadata)
        {
            Getter = getter ?? throw new ArgumentNullException(nameof(getter));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AvaloniaProperty"/> class.
        /// </summary>
        /// <param name="source">The property to copy.</param>
        /// <param name="getter">Gets the current value of the property.</param>
        /// <param name="metadata">Optional overridden metadata.</param>
        private DirectProperty(
            DirectPropertyBase<TValue> source,
            Func<TOwner, TValue> getter,
            DirectPropertyMetadata<TValue> metadata)
            : base(source, typeof(TOwner), metadata)
        {
            Getter = getter ?? throw new ArgumentNullException(nameof(getter));
        }

        /// <inheritdoc/>
        public override bool IsDirect => true;

        /// <inheritdoc/>
        public override bool IsReadOnly => true;

        /// <inheritdoc/>
        public override Type Owner => typeof(TOwner);

        /// <summary>
        /// Gets the getter function.
        /// </summary>
        public Func<TOwner, TValue> Getter { get; }

        /// <summary>
        /// Registers the direct property on another type.
        /// </summary>
        /// <typeparam name="TNewOwner">The type of the additional owner.</typeparam>
        /// <param name="getter">Gets the current value of the property.</param>
        /// <param name="unsetValue">
        /// The value to use when the property is set to <see cref="AvaloniaProperty.UnsetValue"/>
        /// </param>
        /// <param name="defaultBindingMode">The default binding mode for the property.</param>
        /// <param name="enableDataValidation">
        /// Whether the property is interested in data validation.
        /// </param>
        /// <returns>The property.</returns>
        public DirectProperty<TNewOwner, TValue> AddOwner<TNewOwner>(
            Func<TNewOwner, TValue> getter,
            TValue unsetValue = default!,
            BindingMode defaultBindingMode = BindingMode.Default,
            bool enableDataValidation = false)
                where TNewOwner : AvaloniaObject
        {
            var metadata = new DirectPropertyMetadata<TValue>(
                unsetValue: unsetValue,
                defaultBindingMode: defaultBindingMode,
                enableDataValidation: enableDataValidation);

            metadata.Merge(GetMetadata<TOwner>(), this);

            var result = new DirectProperty<TNewOwner, TValue>(this, getter, metadata);

            AvaloniaPropertyRegistry.Instance.Register(typeof(TNewOwner), result);
            return result;
        }

        /// <summary>
        /// Registers the direct property on another type.
        /// </summary>
        /// <typeparam name="TNewOwner">The type of the additional owner.</typeparam>
        /// <param name="getter">Gets the current value of the property.</param>
        /// <param name="unsetValue">
        /// The value to use when the property is set to <see cref="AvaloniaProperty.UnsetValue"/>
        /// </param>
        /// <param name="defaultBindingMode">The default binding mode for the property.</param>
        /// <param name="enableDataValidation">
        /// Whether the property is interested in data validation.
        /// </param>
        /// <returns>The property.</returns>
        public DirectProperty<TNewOwner, TValue> AddOwnerWithDataValidation<TNewOwner>(
            Func<TNewOwner, TValue> getter,
            TValue unsetValue = default!,
            BindingMode defaultBindingMode = BindingMode.Default,
            bool enableDataValidation = false)
                where TNewOwner : AvaloniaObject
        {
            var metadata = new DirectPropertyMetadata<TValue>(
                unsetValue: unsetValue,
                defaultBindingMode: defaultBindingMode,
                enableDataValidation: enableDataValidation);

            metadata.Merge(GetMetadata<TOwner>(), this);

            var result = new DirectProperty<TNewOwner, TValue>(this, getter, metadata);

            AvaloniaPropertyRegistry.Instance.Register(typeof(TNewOwner), result);
            return result;
        }

        /// <inheritdoc/>
        internal override TValue InvokeGetter(AvaloniaObject instance)
        {
            return Getter((TOwner)instance);
        }

        /// <inheritdoc/>
        internal override void InvokeSetter(AvaloniaObject instance, BindingValue<TValue> value) => throw new ArgumentException($"The property {Name} is readonly.");
    }
}
