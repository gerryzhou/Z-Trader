#region Using declarations
using System;
using System.ComponentModel;
using System.IO;
using System.Drawing;
//using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    /// <summary>
    /// This file holds all price action classes.
    /// </summary>
	
	public enum PriceActionType {UpTight, UpWide, DnTight, DnWide, RngTight, RngWide, UnKnown};
	
	/// <summary>
	/// PriceAction include PriceAtionType and the volatility measurement
	/// min and max ticks of up/down expected
	/// shrinking, expanding, or paralleling motion;
	/// </summary>
	public class PriceAction {
		public PriceActionType paType;
		public int minUpTicks;
		public int maxUpTicks;
		public int minDownTicks;
		public int maxDownTicks;
		public PriceAction(PriceActionType pat, int min_UpTicks, int max_UpTicks, int min_DnTicks, int max_DnTicks) {
			this.paType = pat;
			this.minUpTicks = min_UpTicks;
			this.maxUpTicks = max_UpTicks;
			this.minDownTicks = min_DnTicks;
			this.maxDownTicks = max_DnTicks;
		}
	}
}
