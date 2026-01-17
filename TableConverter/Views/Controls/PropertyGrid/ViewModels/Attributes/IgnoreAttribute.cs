using System;

namespace TableConverter.Views.Controls.PropertyGrid.ViewModels.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class IgnoreAttribute : Attribute;