﻿namespace Atlas.Core
{
	public class TestBase
	{
		public Call Call { get; set; }

		public virtual void Initialize(string name)
		{
			Call = new Call(name);
			new LogWriterConsole(Call.Log);
		}
	}
}
