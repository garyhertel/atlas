﻿using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Tabs
{
	// rename to TabInstanceSettings?
	public class TabBookmark
	{
		//public Bookmark bookmark;
		public string Name { get; set; }
		public TabViewSettings tabViewSettings = new TabViewSettings(); // list selections, doesn't know about children
		public Dictionary<string, TabBookmark> tabChildBookmarks { get; set; } = new Dictionary<string, TabBookmark>(); // doesn't know which tabData to use, maps id to child info
		public string Address
		{
			get
			{
				if (tabChildBookmarks.Count > 0)
				{
					string comma = "";
					string address = "";
					if (tabChildBookmarks.Count > 1)
						address += "[";
					//address += Name + "::";
					foreach (var bookmark in tabChildBookmarks)
					{
						address += comma;
						address += bookmark.Key + " / " + bookmark.Value.Address;
						comma = ", ";
					}
					if (tabChildBookmarks.Count > 1)
						address += "]";
					return address;
				}
				else
				{
					//string address = "";
					if (tabViewSettings == null)
						return "";
					//string address = "<" + tabConfiguration.Address + ">";
					string address = tabViewSettings.Address;
					return address;
				}
			}
		}

		// Temporary, Only FindMatches() uses, refactor these out?
		[NonSerialized]
		public HashSet<object> selected = new HashSet<object>(); // does this work with multiple TabDatas?
		[NonSerialized]
		public TabModel tabModel;

		// too complicated, for now
		/*public void MergeNodes(List<Node> nodes)
		{
			foreach (Node node in nodes)
			{
				foreach (var nodeEntry in node.nodes)
				{
					Node existingNode;
					if (this.nodes.TryGetValue(nodeEntry.Key, out existingNode))
					{

					}
					else
					{
						this.nodes.Add(nodeEntry.Key, nodeEntry.Value);
					}
				}
			}
		}*/

		public override string ToString()
		{
			return Name;// string.Join(",", Nodes.Keys);
		}

		public void MergeNode(TabBookmark node)
		{
			foreach (var nodeEntry in node.tabChildBookmarks)
			{
				TabBookmark existingNode;
				if (this.tabChildBookmarks.TryGetValue(nodeEntry.Key, out existingNode))
				{
					existingNode.MergeNode(nodeEntry.Value);
				}
				else
				{
					this.tabChildBookmarks.Add(nodeEntry.Key, nodeEntry.Value);
				}
			}
			if (this.tabViewSettings == null)
			{
				this.tabViewSettings = node.tabViewSettings;
				return;
			}
			Name = " + " + node.Name;
			for (int i = 0; i < this.tabViewSettings.TabDataSettings.Count; i++)
			{
				var currentSelection = this.tabViewSettings.TabDataSettings[i].SelectedRows;
				var otherSelection = node.tabViewSettings.TabDataSettings[i].SelectedRows;

				HashSet<string> labelsUsed = new HashSet<string>();
				HashSet<int> indicesUsed = new HashSet<int>();
				foreach (SelectedRow row in currentSelection)
				{
					if (row.label != null)
						labelsUsed.Add(row.label);
					else
						indicesUsed.Add(row.rowIndex);
				}

				foreach (SelectedRow row in otherSelection)
				{
					if (row.label != null)
					{
						if (!labelsUsed.Contains(row.label))
						{
							currentSelection.Add(row);
						}
					}
					else
					{
						if (!indicesUsed.Contains(row.rowIndex))
						{
							currentSelection.Add(row);
						}
					}
				}
			}
		}
	}
}
