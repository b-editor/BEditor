﻿using System;
using System.Collections.Generic;
using System.Globalization;

namespace BEditor.Data.Property
{
    /// <summary>
    /// Represents a property used by <see cref="EffectElement"/>.
    /// </summary>
    public interface IPropertyElement : IChild<EffectElement>, IElementObject, IEditingObject, IJsonObject
    {
        /// <summary>
        /// Gets or sets the metadata for this <see cref="IPropertyElement"/>.
        /// </summary>
        public PropertyElementMetadata? PropertyMetadata { get; set; }
    }
}