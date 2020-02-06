﻿using System;
using System.Collections.Generic;
using Atlas.Core;

namespace Atlas.Tabs.Test
{
	public class TabTestLoading: ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call)
			{
				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Slow Load", new TabTestSlowLoad()),
					new ListItem("Slow Model", new TabTestSlowModel()),
				};
			}
		}
	}
}