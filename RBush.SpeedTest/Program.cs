using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RBush.SpeedTest
{
	public class Item : ISpatialData
	{
		private readonly Envelope _envelope;

		public Item(double minX, double minY, double maxX, double maxY)
		{
			_envelope = new Envelope(minX, minY, maxX, maxY);
		}

		public ref readonly Envelope Envelope => ref _envelope;
	}

	public static class ItemListGenerator
	{
		static Random _random = new Random();

		private static Item generateItem(double scale)
		{
			List<double> points = new List<double>() { _random.NextDouble() * scale, _random.NextDouble() * scale, _random.NextDouble() * scale, _random.NextDouble() * scale };
			points.Sort();
			return new Item(points[0], points[2], points[1], points[3]);
		}

		public static IEnumerable<Item> GenerateItems(int numberOfItems, double spaceScale)
		{
			List<Item> result = new List<Item>(numberOfItems);
			for (int i = 0; i < numberOfItems; i++)
			{
				result.Add(generateItem(spaceScale));
			}
			return result.AsEnumerable();
		}
	}

	[CoreJob]
	[RPlotExporter, RankColumn]
	public class RBushBenchmark
	{
		[Params(10000, 50000, 100000, 200000)]
		public int N;

		[Params(5, 16, 32)]
		public int K;

		const double spaceScale = 50;

		private RBush<Item> tree;
		[IterationSetup]
		public void Setup()
		{
			var tree = new RBush<Item>(maxEntries: K);
			tree.BulkLoad(ItemListGenerator.GenerateItems(N, spaceScale));
			this.tree = tree;
		}

		[Benchmark]
		public void OldMethod()
		{
			tree.Search(Envelope.InfiniteBounds);
		}

		[Benchmark]
		public void ShushenMethod()
		{
			tree.Search2(Envelope.InfiniteBounds);
		}

		[Benchmark]
		public void NewMethod()
		{
			tree.Search3(Envelope.InfiniteBounds);
		}

	}

	class Program
	{
		static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<RBushBenchmark>();
		}
	}
}
