﻿using System;

namespace BEditor.Data
{
    /// <summary>
    /// Represents the properties of the edited data.
    /// </summary>
    public interface IEditingProperty
    {
        /// <summary>
        /// Get the value of whether to delete with <see cref="EditingObject.ClearDisposable"/>.
        /// </summary>
        public bool IsDisposable { get; }

        /// <summary>
        /// Gets the name of this <see cref="IEditingProperty"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the owner type of this <see cref="IEditingProperty"/>.
        /// </summary>
        public Type OwnerType { get; }

        /// <summary>
        /// Gets the value type of this <see cref="IEditingProperty"/>.
        /// </summary>
        public Type ValueType { get; }

        /// <summary>
        /// Gets the <see cref="IEditingPropertyInitializer"/> that initializes the local value of this <see cref="IEditingProperty"/>.
        /// </summary>
        public IEditingPropertyInitializer? Initializer { get; }

        /// <summary>
        /// Gets the <see cref="IEditingPropertySerializer"/> that serializes the local value of this <see cref="IEditingProperty"/>.
        /// </summary>
        public IEditingPropertySerializer? Serializer { get; init; }

        /// <summary>
        /// Gets the registry key.
        /// </summary>
        public EditingPropertyRegistryKey Key { get; }
    }
}