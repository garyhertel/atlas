﻿using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Atlas.Tabs
{
	public interface IPropertyEditable
	{
		bool Editable { get; }
	}

	public class ListProperty : ListMember, IPropertyEditable
	{
		public PropertyInfo PropertyInfo;
		public bool Cachable;

		private bool _valueCached;
		private object _valueObject = null;

		[HiddenColumn]
		public override bool Editable // rename to IsReadOnly?
		{
			get
			{
				bool propertyReadOnly = (PropertyInfo.GetCustomAttribute<ReadOnlyAttribute>() != null);
				return PropertyInfo.CanWrite && !propertyReadOnly;
			}
		}

		[Hidden]
		public bool IsFormatted => (PropertyInfo.GetCustomAttribute<FormattedAttribute>() != null);

		[Editing, InnerValue, WordWrap]
		public override object Value
		{
			get
			{
				try
				{
					if (Cachable)
					{
						if (!_valueCached)
						{
							_valueCached = true;
							_valueObject = PropertyInfo.GetValue(Object);

							if (IsFormatted)
								_valueObject = _valueObject.Formatted();
						}
						return _valueObject;
					}
					return PropertyInfo.GetValue(Object);
				}
				catch (Exception)
				{
					return null;
				}
			}
			set
			{
				if (PropertyInfo.CanWrite)
				{
					Type type = PropertyInfo.PropertyType;
					if (value != null)
					{
						type = type.GetNonNullableType();
					}
					PropertyInfo.SetValue(Object, Convert.ChangeType(value, type));

					if (Object is INotifyPropertyChanged notifyPropertyChanged)
					{
						//notifyPropertyChanged.PropertyChanged?.Invoke(obj, new PropertyChangedEventArgs(propertyName));
					}
				}
			}
		}

		[Hidden]
		public Type UnderlyingType => PropertyInfo.PropertyType.GetNonNullableType();

		public override string ToString() => Name;

		public ListProperty(object obj, PropertyInfo propertyInfo, bool cachable = true) : 
			base(obj, propertyInfo)
		{
			PropertyInfo = propertyInfo;
			Cachable = cachable;

			var accessors = propertyInfo.GetAccessors(true);
			AutoLoad = !accessors[0].IsStatic;

			NameAttribute attribute = propertyInfo.GetCustomAttribute<NameAttribute>();
			if (attribute != null)
				Name = attribute.Name;
			else
				Name = propertyInfo.Name.WordSpaced();

			if (PropertyInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
				Name = "* " + Name;
		}

		public static new ItemCollection<ListProperty> Create(object obj)
		{
			// this doesn't work for virtual methods (or any method modifier?)
			PropertyInfo[] propertyInfos = obj.GetType().GetProperties().OrderBy(x => x.MetadataToken).ToArray();
			var listProperties = new ItemCollection<ListProperty>();
			var propertyToIndex = new Dictionary<string, int>();
			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				if (!propertyInfo.DeclaringType.IsNotPublic)
				{
					if (propertyInfo.GetCustomAttribute<HiddenAttribute>() != null)
						continue;

					if (propertyInfo.GetCustomAttribute<HiddenRowAttribute>() != null)
						continue;

					if (propertyInfo.DeclaringType.IsNotPublic)
						continue;

					var listProperty = new ListProperty(obj, propertyInfo);

					// move this to later?
					if (propertyInfo.GetCustomAttribute<HideNullAttribute>() != null)
					{
						if (listProperty.Value == null)
							continue;
					}

					var hideAttribute = propertyInfo.GetCustomAttribute<HideAttribute>();
					if (hideAttribute != null && hideAttribute.Values != null)
					{
						if (hideAttribute.Values.Contains(listProperty.Value))
							continue;
					}

					if (propertyToIndex.TryGetValue(propertyInfo.Name, out int index))
					{
						listProperties.RemoveAt(index);
						listProperties.Insert(index, listProperty);
					}
					else
					{
						propertyToIndex[propertyInfo.Name] = listProperties.Count;
						listProperties.Add(listProperty);
					}
				}
			}
			return listProperties;
		}

		// This can be slow due to lazy property loading
		public static ItemCollection<ListProperty> Sort(ItemCollection<ListProperty> listProperties)
		{
			var autoSorted = new ItemCollection<ListProperty>(listProperties.OrderByDescending(i => i.PropertyInfo.GetCustomAttribute<AutoSelectAttribute>() != null).ToList());
			var linkSorted = new ItemCollection<ListProperty>(autoSorted.OrderByDescending(i => TabUtils.ObjectHasLinks(i, true)).ToList());
			return linkSorted;
		}
	}
}

/*
What do we do about child objects?
	can't edit child objects

	DataBinding

This class only works alone, no fields
Just expand these properties only
*/