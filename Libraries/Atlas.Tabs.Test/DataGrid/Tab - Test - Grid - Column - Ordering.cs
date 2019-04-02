﻿using System;
using System.Collections.Generic;
using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestGridColumnOrdering : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			private ItemCollection<TestChild> items;

			public override void Load(Call call)
			{
				items = new ItemCollection<TestChild>();
				for (int i = 0; i < 2; i++)
				{
					TestChild item = new TestChild()
					{
					};

					items.Add(item);
				}

				tabModel.Items = items;
			}
		}

		public abstract class TestParent
		{
			public string Field = "1";
			public string Original { get; set; } = "2";
			public virtual string Virtual { get; set; } = "3";
			public virtual string Overriden { get; set; } = "4";
			public abstract string Abstract { get; }

			public override string ToString()
			{
				return Original;
			}
		}

		public class TestChild : TestParent
		{
			public override string Overriden { get; set; } = "5";
			public override string Abstract { get; } = "6";

			public override string ToString()
			{
				return Original;
			}
		}
	}
}
/*
Tests the column order matches for the different types
*/
