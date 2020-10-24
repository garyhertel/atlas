﻿using Atlas.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Atlas.Serialize
{
	public class TypeRepoEnumerable : TypeRepo
	{
		public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (CanAssign(typeSchema.Type))
					return new TypeRepoEnumerable(serializer, typeSchema);
				return null;
			}
		}

		private Type elementType;
		private TypeRepo listTypeRepo;
		private MethodInfo addMethod;

		public TypeRepoEnumerable(Serializer serializer, TypeSchema typeSchema) : 
			base(serializer, typeSchema)
		{
			Type[] types = LoadableType.GetGenericArguments();
			if (types.Length > 0)
				elementType = types[0];

			addMethod = LoadableType.GetMethods()
				.Where(m => m.Name == "Add" && m.GetParameters().Count() == 1).FirstOrDefault();
		}

		public static bool CanAssign(Type type)
		{
			return type.IsGenericType && typeof(HashSet<>).IsAssignableFrom(type.GetGenericTypeDefinition());
		}

		public override void InitializeLoading(Log log)
		{
			if (elementType != null)
				listTypeRepo = Serializer.GetOrCreateRepo(log, elementType);
		}

		public override void AddChildObjects(object obj)
		{
			IEnumerable iEnumerable = (IEnumerable)obj;
			foreach (var item in iEnumerable)
			{
				Serializer.AddObjectRef(item);
			}
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			PropertyInfo countProp = LoadableType.GetProperty("Count"); // IEnumerable isn't required to implement this
			IEnumerable iEnumerable = (IEnumerable)obj;
			
			int count = (int)countProp.GetValue(iEnumerable, null);
			writer.Write(count);
			foreach (object item in iEnumerable)
			{
				Serializer.WriteObjectRef(elementType, item, writer);
			}
		}

		public override void LoadObjectData(object obj)
		{
			//(IEnumerable<listTypeRepo.type>)objects[i];
			int count = Reader.ReadInt32();

			for (int j = 0; j < count; j++)
			{
				object objectValue = listTypeRepo.LoadObjectRef();
				addMethod.Invoke(obj, new object[] { objectValue });
			}
		}

		protected override object LoadObjectData(byte[] bytes, ref int byteOffset, int objectIndex)
		{
			object obj = Activator.CreateInstance(LoadableType, true);
			Objects[objectIndex] = obj; // must assign before loading any more refs

			//(IEnumerable<listTypeRepo.type>)objects[i];
			int count = BitConverter.ToInt32(bytes, byteOffset);
			byteOffset += sizeof(int);

			for (int j = 0; j < count; j++)
			{
				object objectValue = listTypeRepo.LoadObjectRef(bytes, ref byteOffset);
				addMethod.Invoke(obj, new object[] { objectValue });
			}
			return obj;
		}

		public override void Clone(object source, object dest)
		{
			IEnumerable iSource = (IEnumerable)source;
			foreach (var item in iSource)
			{
				object clone = Serializer.Clone(item);
				addMethod.Invoke(dest, new object[] { clone });
			}
		}
	}
}