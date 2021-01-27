﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;

using BEditor.Core.Data.Property;

namespace BEditor.Core.Data
{
    /// <summary>
    /// 継承するクラスに対応するUIのデータのキャッシュを表します
    /// </summary>
    [DataContract]
    public class ComponentObject : BasePropertyChanged, IExtensibleDataObject
    {
        private Dictionary<string, dynamic>? _ComponentData;

        /// <summary>
        /// UIなどのキャッシュを入れる配列を取得します
        /// </summary>
        public Dictionary<string, dynamic> ComponentData => _ComponentData ??= new Dictionary<string, dynamic>();
        /// <inheritdoc/>
        public virtual ExtensionDataObject? ExtensionData { get; set; }
    }
}