﻿using System;
using System.ComponentModel;
using System.Globalization;

namespace DuncanApps.DataView.Converters
{
    public class OrderClausesConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string text)
                return ParseHelper.PasreOrderClause(text);

            return base.ConvertFrom(context, culture, value);
        }
    }
}
