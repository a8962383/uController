﻿using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace System.Reflection
{
    public class ParameterWrapper : ParameterInfo
    {
        private readonly IParameterSymbol _parameter;
        private readonly MetadataLoadContext _metadataLoadContext;

        public ParameterWrapper(IParameterSymbol parameter, MetadataLoadContext metadataLoadContext)
        {
            _parameter = parameter;
            _metadataLoadContext = metadataLoadContext;
        }

        public override Type ParameterType => _parameter.Type.AsType(_metadataLoadContext);
        public override string Name => _parameter.Name;

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            var attributes = new List<CustomAttributeData>();
            foreach (var a in _parameter.GetAttributes())
            {
                attributes.Add(new CustomAttributeDataWrapper(a, _metadataLoadContext));
            }
            return attributes;
        }
    }
}