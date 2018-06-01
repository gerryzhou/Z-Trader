#region Using declarations
using System;
using System.ComponentModel;
using System.Drawing;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    /// <summary>
    /// This file holds all user defined indicator methods.
    /// </summary>
		
	public struct ZigZagSwing {
		public int Bar_Start;
		public int Bar_End;
		public double Size;
	}
	
    partial class Indicator
    {
		public int IsLastBarOnChart() {
			try{
				if(Input.Count - CurrentBar <= 2) {
					return Input.Count;
				} else {
					return -1;
				}
			}
			catch(Exception ex){
				Print("IsLastBarOnChart:" + ex.Message);
				return -1;
			}
		}
    }
}
