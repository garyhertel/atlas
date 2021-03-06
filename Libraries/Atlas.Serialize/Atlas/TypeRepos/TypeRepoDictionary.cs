﻿using Atlas.Core;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Atlas.Serialize
{
	public class TypeRepoDictionary : TypeRepo
	{
		public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (CanAssign(typeSchema.Type))
					return new TypeRepoDictionary(serializer, typeSchema);
				return null;
			}
		}

		private Type typeKey;
		private Type typeValue;
		private TypeRepo list1TypeRepo;
		private TypeRepo list2TypeRepo;
		private MethodInfo addMethod;

		public TypeRepoDictionary(Serializer serializer, TypeSchema typeSchema) : 
			base(serializer, typeSchema)
		{
			Type[] types = LoadableType.GetGenericArguments();
			if (types.Length > 0)
			{
				typeKey = types[0];
				typeValue = types[1];
			}
			addMethod = LoadableType.GetMethods()
				.Where(m => m.Name == "Add" && m.GetParameters().Count() == 2).FirstOrDefault();
		}

		public static bool CanAssign(Type type)
		{
			return typeof(IDictionary).IsAssignableFrom(type);
		}

		public override void InitializeLoading(Log log)
		{
			// these base types might not be serialized
			if (typeKey != null)
				list1TypeRepo = Serializer.GetOrCreateRepo(log, typeKey);
			if (typeValue != null)
				list2TypeRepo = Serializer.GetOrCreateRepo(log, typeValue);
		}

		public override void AddChildObjects(object obj)
		{
			IDictionary dictionary = (IDictionary)obj;
			foreach (DictionaryEntry item in dictionary)
			{
				Serializer.AddObjectRef(item.Key);
				Serializer.AddObjectRef(item.Value);
			}
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			IDictionary dictionary = (IDictionary)obj;
			
			writer.Write(dictionary.Count);
			foreach (DictionaryEntry item in dictionary)
			{
				Serializer.WriteObjectRef(typeKey, item.Key, writer);
				Serializer.WriteObjectRef(typeValue, item.Value, writer);
			}
		}

		public override void LoadObjectData(object obj)
		{
			IDictionary iCollection = (IDictionary)obj;
			int count = Reader.ReadInt32();

			for (int j = 0; j < count; j++)
			{
				object key = list1TypeRepo.LoadObjectRef();
				object value = list2TypeRepo.LoadObjectRef();

				if (key != null)
					addMethod.Invoke(iCollection, new object[] { key, value });
			}
		}

		public override void Clone(object source, object dest)
		{
			IDictionary iSource = (IDictionary)source;
			IDictionary iDest = (IDictionary)dest;
			foreach (DictionaryEntry item in iSource)
			{
				object key = Serializer.Clone(item.Key);
				object value = Serializer.Clone(item.Value);
				addMethod.Invoke(iDest, new object[] { key, value });
			}
		}
	}
}
