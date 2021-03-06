﻿using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Atlas.Tabs
{
	public class ListField : ListMember, IPropertyEditable
	{
		public FieldInfo FieldInfo;
		
		[HiddenColumn]
		public override bool Editable => true;

		[Hidden]
		public bool IsFormatted => (FieldInfo.GetCustomAttribute<FormattedAttribute>() != null);


		[Editing, InnerValue]
		public override object Value
		{
			get
			{
				try
				{
					var value = FieldInfo.GetValue(Object);


					if (IsFormatted)
						value = value.Formatted();

					return value;
				}
				catch (Exception)
				{
					return null;
				}
			}
			set
			{
				FieldInfo.SetValue(Object, Convert.ChangeType(value, FieldInfo.FieldType));
			}
		}

		public ListField(object obj, FieldInfo fieldInfo) : 
			base(obj, fieldInfo)
		{
			FieldInfo = fieldInfo;
			AutoLoad = !fieldInfo.IsStatic;

			Name = fieldInfo.Name.WordSpaced();
			NameAttribute attribute = fieldInfo.GetCustomAttribute<NameAttribute>();
			if (attribute != null)
				Name = attribute.Name;
		}

		public override string ToString() => Name;

		public static new ItemCollection<ListField> Create(object obj)
		{
			FieldInfo[] fieldInfos = obj.GetType().GetFields().OrderBy(x => x.MetadataToken).ToArray();

			var listFields = new ItemCollection<ListField>();
			// replace any overriden/new field & properties
			var fieldToIndex = new Dictionary<string, int>();
			foreach (FieldInfo fieldInfo in fieldInfos)
			{
				// Ignore const, add override?
				if (fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
					continue;

				if (fieldInfo.GetCustomAttribute<HiddenAttribute>() != null)
					continue;

				if (fieldInfo.GetCustomAttribute<HiddenRowAttribute>() != null)
					continue;

				var listField = new ListField(obj, fieldInfo);
				if (fieldToIndex.TryGetValue(fieldInfo.Name, out int index))
				{
					listFields.RemoveAt(index);
					listFields.Insert(index, listField);
				}
				else
				{
					fieldToIndex[fieldInfo.Name] = listFields.Count;
					listFields.Add(listField);
				}
			}
			return listFields;
		}
	}
}
