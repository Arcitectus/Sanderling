using NUnit.Framework;
using System;

namespace Sanderling.Test.Exe.Parse
{
	public class KeyCodeFromUIText
	{
		[Test]
		public	void Parse_KeyCodeFromUIText_NoCollision()
		{
			foreach (var CollidingKey in Sanderling.Parse.CultureAggregated.KeyCodeFromUITextSetCollidingKey())
			{
				throw new Exception("colliding key: " + CollidingKey);
			}
		}
	}
}
