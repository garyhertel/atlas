﻿using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Atlas.Tabs.Test.Chart
{
	public class TabTestChartOverlay : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			//private ItemCollection<ListItem> items = new ItemCollection<ListItem>();
			//private ItemCollection<int> series = new ItemCollection<int>();
			private ItemCollection<ChartSample> samples = new ItemCollection<ChartSample>();
			private Random random = new Random();
			private bool ChartInitialized = false;
			private DateTime baseDateTime = DateTime.Now.Trim(TimeSpan.FromMinutes(1));

			public class TestItem
			{
				public int Amount { get; set; }
			}

			public class ChartSample
			{
				//public string Group { get; set; } = "Group";
				public string Name { get; set; }
				[XAxis]
				public DateTime TimeStamp { get; set; }
				[Unit("B")]
				public int SeriesAlpha { get; set; }
				[Unit("A")]
				public int SeriesBeta { get; set; }
				[Unit("A")]
				public int SeriesGamma { get; set; }
				[Unit("B")]
				public int SeriesEpsilon { get; set; }  // High Value, small delta
				public TestItem testItem { get; set; } = new TestItem();
				public int InstanceAmount => testItem.Amount;
			}

			public override void Load(Call call, TabModel model)
			{
				//items.Add(new ListItem("Log", series));
				//tabModel.Items = items;

				model.Actions =  new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Add Entry", AddEntry),
					new TaskDelegate("Start: 1 Entry / second", StartTask, true),
				};

				for (int i = 0; i < 10; i++)
				{
					AddSample(i);
				}
				var chartSettings = new ChartSettings(samples);
				model.AddObject(chartSettings, true);
			}

			private void AddEntry(Call call)
			{
				int param1 = 1;
				string param2 = "abc";
				Invoke(call, AddSampleCallback, param1, param2);
			}

			private void StartTask(Call call)
			{
				CancellationToken token = call.TaskInstance.TokenSource.Token;
				for (int i = 0; !token.IsCancellationRequested; i++)
				{
					Invoke(call, AddSampleCallback);
					Thread.Sleep(1000);
				}
			}

			private void AddSample(int i)
			{
				//series.Add(random.Next(1050, 1095));

				var sample = new ChartSample()
				{
					Name = "Name " + i.ToString(),
					TimeStamp = baseDateTime.AddMinutes(i),
					SeriesAlpha = random.Next(50, 100),
					SeriesBeta = random.Next(50, 100),
					SeriesGamma = random.Next(50, 100),
					SeriesEpsilon = random.Next(50, 100),
					testItem = new TestItem()
					{
						Amount = random.Next(0, 100),
					},
				};
				samples.Add(sample);
			}

			private void Initialize()
			{
				if (ChartInitialized)
					return;
				ChartInitialized = true;

				//tabChart.chart.Series.Clear();
				//tabChart.BindListToChart(samples);

				//tabChart.chart.DataBindTable(samples); // databinds class Properties, throws exception when binding non Primitive properties
				//tabChart.chart.DataBind();


				//tabChart.chart.Update();
			}

			// UI context
			private void AddSampleCallback(Call call, object state)
			{
				// data = state;
				Initialize();

				//call.Log.Add("test");
				//call.Log.Add("New Log entry", new Tag("name", "value"));

				//var random = new Random();
				AddSample(samples.Count);
				//tabChart.chart.Series[0].Points.DataBindY(tabModel.Chart); // required to refresh, any alternatives?
				//tabChart.chart.DataBind();
				//tabChart.chart.In
			}
		}
	}
}
