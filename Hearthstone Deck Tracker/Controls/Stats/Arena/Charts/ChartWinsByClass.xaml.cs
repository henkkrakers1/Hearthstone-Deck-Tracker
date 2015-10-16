﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Stats;

namespace Hearthstone_Deck_Tracker.Controls.Stats.Arena.Overview
{
	/// <summary>
	/// Interaction logic for ChartWinsByClass.xaml
	/// </summary>
	public partial class ChartWinsByClass : INotifyPropertyChanged
	{
		public ChartWinsByClass()
		{
			InitializeComponent();
			CompiledStats.Instance.PropertyChanged += (sender, args) =>
			{
				if(args.PropertyName == "ArenaWinsByClass")
					OnPropertyChanged("SeriesSourceWins");
			};
		}

		public IEnumerable<WinChartData> SeriesSourceWins
		{
			get
			{
				return
					Enumerable.Range(0, 13)
							  .Select(
									  n =>
									  new WinChartData
				{
					Index = n.ToString(),
					ItemsSource = CompiledStats.Instance.ArenaWinsByClass[n]
				});
			}
		}

		public class WinChartData
		{
			public string Index { get; set; }
			public IEnumerable<ChartStats> ItemsSource { get; set; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}