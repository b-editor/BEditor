﻿using BEditor.Drawing;

namespace BEditor.Data.Property
{
    /// <summary>
    /// The metadata of <see cref="ColorProperty"/>.
    /// </summary>
    /// <param name="Name">The string displayed in the property header.</param>
    /// <param name="DefaultColor">The default color.</param>
    /// <param name="UseAlpha">The value of whether to use alpha components or not.</param>
    public record ColorPropertyMetadata(string Name, Color DefaultColor, bool UseAlpha = false)
        : PropertyElementMetadata(Name), IPropertyBuilder<ColorProperty>
    {
        /// <inheritdoc/>
        public ColorProperty Build()
        {
            return new(this);
        }
    }
}
