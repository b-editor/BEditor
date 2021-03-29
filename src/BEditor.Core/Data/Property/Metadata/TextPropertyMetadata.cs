﻿namespace BEditor.Data.Property
{
    /// <summary>
    /// The metadata of <see cref="TextProperty"/>.
    /// </summary>
    /// <param name="Name">The string displayed in the property header.</param>
    /// <param name="DefaultText">The default value for <see cref="TextProperty.Value"/>.</param>
    public record TextPropertyMetadata(string Name, string DefaultText = "")
        : PropertyElementMetadata(Name), IPropertyBuilder<TextProperty>
    {
        /// <inheritdoc/>
        public TextProperty Build()
        {
            return new(this);
        }
    }
}