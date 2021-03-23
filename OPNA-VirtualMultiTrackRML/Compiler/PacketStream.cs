


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using CallMacroProperty = System.Collections.Generic.KeyValuePair</*oLine*/int, /*nMacro*/int>;



namespace FM_VirtualMultiTrackMML
{
	public class PacketStream
	{
		enum eScale {
			c,
		}
		enum eFunction {
			r,
			C,
			t,
			T,
			v,
			q,
			pM,
			pR,
			pL,
			pC,
			l,
			vUp,		// )
			vDown,		// (
			Slur,		// &
			Tie,		// ^
			RepeatHead,	// [
			RepeatTail,	// ]
			RepeatBreak,// /
			L,
			V,
			R,
			RF,
			Rm,
			yRTL,
			Abort,
			Silence,
			J,
			NOP,
			CallstackPush,
			CallstackPop,
		}
		
		const int cSemibreve = 128;
		const int cDefaultDuration = cSemibreve / 4;
		
		string mvUp;
		string mvDown;
		
		int mSemibreve = cSemibreve;
		int mDefaultDuration = cDefaultDuration;
		bool mbAbort = false;
		bool mbSilence = false;
		
		// SpreadToPacket
		List<Packet> maPacket;
		CallstackLogger mCallstackLogger;
		
		
		
		~PacketStream()
		{
		}
		
		
		
		public PacketStream(Setting @Setting)
		{
			mvUp = (Setting.VolumeSwap)? "(":")";
			mvDown = (Setting.VolumeSwap)? ")":"(";
		}
		
		
		
		public bool SpreadToPacket(out List<Packet> aPacket, out CallstackLogger @CallstackLogger, Spread @Spread)
		{
			var c = new Cursor(Spread.Text);
			
			aPacket = new List<Packet>();
			maPacket = aPacket;
			int oPacketScale = -1;
			int oPacketRepeatHead = -1;
			int oPacketRepeatBreak = -1;
			int oPacketRepeatTail = -1;
			
			CallstackLogger = new CallstackLogger();
			mCallstackLogger = CallstackLogger;
			int oCallstackLogger = -1;
			
			var CallstackFrom = new Callstack(Spread.Channel);
			int Callstack_oPrev = 0;
			int Callstack_oText = 0;
			
			var CallMacro = new Stack<CallMacroProperty>();
			int In_oLine = -1;
			int In_oBase = -1;
			int In_nMacro = -1;
			
			var aoStackRepeatHead = new Stack<int>();
			var aoStackRepeatBreak = new Stack<int>();
			
			bool Result = true;
			while (!c.IsTerm && Result && !mbAbort){
				Callstack_oPrev = Callstack_oText;
				Callstack_oText += (c.Current - c.Preview);
				c.Commit();
				
				eScale Scale = eScale.c;
				if (GetScale(ref Scale, c)){
					c.SkipSpace();
					
					Callstack_oPrev = Callstack_oText;
					Callstack_oText += (c.Current - c.Preview);
					c.Commit();
					
					var oFullScale = (int)Scale;
					if (Packet.IsFullScale(oFullScale)){
						int Clock = 0;
						if (c.GetClock(ref Clock, mSemibreve, mDefaultDuration)){
							oPacketScale = AddPacketScale(((mbSilence)? (int)Packet.eCommand.r: oFullScale), Clock, Callstack_oPrev, oCallstackLogger);
						} else {
							Console.WriteLine($"!! Error !! : Illegal parameter");
							Result = false;
						}
					} else {
						Console.WriteLine($"!! Error !! : Illegal scale");
						Result = false;
					}
				} else {
					eFunction Function = eFunction.r;
					if (GetFunction(ref Function, c)){
						c.SkipSpace();
						
						Callstack_oPrev = Callstack_oText;
						Callstack_oText += (c.Current - c.Preview);
						c.Commit();
						
						switch ((int)Function){
							case (int)eFunction.r:{
								int Clock = 0;
								if (c.GetClock(ref Clock, mSemibreve, mDefaultDuration)){
									AddPacketScale((int)Packet.eCommand.r, Clock, Callstack_oPrev, oCallstackLogger);
									oPacketScale = -1;
									break;
								}
								Console.WriteLine($"!! Error !! : Illegal parameter");
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.C:{
								int Value = 0;
								if (c.GetDecimal(ref Value, true)){
									if (Value > 0){
										mSemibreve = Value;
										break;
									}
								}
								Console.WriteLine($"!! Error !! : Illegal parameter");
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.t:{
								int Value = 0;
								if (c.GetDecimal(ref Value, true)){
									if (Value >= 0x00 && Value <= 0xff){
										maPacket.Add(new Packet(Packet.eCommand.t, Value, Callstack_oPrev, oCallstackLogger));
										break;
									}
								}
								Console.WriteLine($"!! Error !! : Illegal parameter");
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.T:{
								int Value = 0;
								if (c.GetDecimal(ref Value, true)){
									if (Value >= 1 && Value <= 255){
										maPacket.Add(new Packet(Packet.eCommand.T, Value, Callstack_oPrev, oCallstackLogger));
										break;
									}
								}
								Console.WriteLine($"!! Error !! : Illegal parameter");
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.v:{
								int Value = 0;
								if (c.GetDecimal(ref Value, true)){
									if (Value >= 0 && Value <= 31){
										maPacket.Add(new Packet(Packet.eCommand.v, Value, Callstack_oPrev, oCallstackLogger));
										break;
									}
								}
								Console.WriteLine($"!! Error !! : Illegal parameter");
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.q:{
								int Value = 0;
								if (c.GetDecimal(ref Value, true)){
									if (Value >= 0 && Value <= 255){
										maPacket.Add(new Packet(Packet.eCommand.q, Value, Callstack_oPrev, oCallstackLogger));
										break;
									}
								}
								Console.WriteLine($"!! Error !! : Illegal parameter");
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.pM:{
								maPacket.Add(new Packet(Packet.eCommand.pM, 0, Callstack_oPrev, oCallstackLogger));
								break;
							}
							case (int)eFunction.pR:{
								maPacket.Add(new Packet(Packet.eCommand.pR, 0, Callstack_oPrev, oCallstackLogger));
								break;
							}
							case (int)eFunction.pL:{
								maPacket.Add(new Packet(Packet.eCommand.pL, 0, Callstack_oPrev, oCallstackLogger));
								break;
							}
							case (int)eFunction.pC:{
								maPacket.Add(new Packet(Packet.eCommand.pC, 0, Callstack_oPrev, oCallstackLogger));
								break;
							}
							case (int)eFunction.l:{
								int Clock = 0;
								if (c.GetClock(ref Clock, mSemibreve, mDefaultDuration)){
									if (Clock > 0){
										mDefaultDuration = Clock;
										break;
									}
								}
								Console.WriteLine($"!! Error !! : Illegal parameter");
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.vUp:{
								int Value = 1;
								c.GetDecimal(ref Value, true);
								if (Value >= 1 && Value <= 31){
									maPacket.Add(new Packet(Packet.eCommand.vUp, Value, Callstack_oPrev, oCallstackLogger));
									break;
								}
								Console.WriteLine($"!! Error !! : Illegal parameter");
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.vDown:{
								int Value = 1;
								c.GetDecimal(ref Value, true);
								if (Value >= 1 && Value <= 31){
									maPacket.Add(new Packet(Packet.eCommand.vDown, Value, Callstack_oPrev, oCallstackLogger));
									break;
								}
								Console.WriteLine($"!! Error !! : Illegal parameter");
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.Slur:{
								if (oPacketScale >= 0 && maPacket[oPacketScale].IsScale && !maPacket[oPacketScale].IsSlur){
									if (mbSilence){
										break;
									}
									if (maPacket[oPacketScale].AddSlur()){
										break;
									}
								}
								Console.WriteLine($"!! Error !! : Syntax error");
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.Tie:{
								if (oPacketScale >= 0 && maPacket[oPacketScale].IsScale && !maPacket[oPacketScale].IsSlur){
									var Command = maPacket[oPacketScale].Command;
									if (maPacket[oPacketScale].AddSlur()){
										int Clock = 0;
										if (c.GetClock(ref Clock, mSemibreve, mDefaultDuration)){
											oPacketScale = AddPacketScale(((mbSilence)? (int)Packet.eCommand.r: Command), Clock, Callstack_oPrev, oCallstackLogger);
											break;
										}
									}
								}
								Console.WriteLine($"!! Error !! : Syntax error");
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.RepeatHead:{
								aoStackRepeatHead.Push(oPacketRepeatHead);
								oPacketRepeatHead = maPacket.Count;
								oPacketRepeatBreak = -1;
								oPacketRepeatTail = -1;
								break;
							}
							case (int)eFunction.RepeatTail:{
								if (aoStackRepeatHead.Count > 0 && oPacketRepeatHead >= 0){
									int Value = 0;
									if (c.GetDecimal(ref Value, true)){
										if (Value >= 2){
											oPacketRepeatTail = maPacket.Count;
											
											for (int Repeat = Value-1; Repeat > 0; --Repeat){
												for (int oPacket = oPacketRepeatHead; oPacket < oPacketRepeatTail; ++oPacket){
													if (oPacketRepeatBreak >= 0 && oPacketRepeatBreak == oPacket && Repeat == 1) break;
													maPacket.Add(new Packet(maPacket[oPacket].CommandEnum, maPacket[oPacket].Value, maPacket[oPacket].Column, maPacket[oPacket].Logger));
												}
											}
											
											oPacketRepeatHead = aoStackRepeatHead.Pop();
											oPacketRepeatBreak = (oPacketRepeatBreak >= 0)? aoStackRepeatBreak.Pop(): oPacketRepeatBreak;
											oPacketRepeatTail = -1;
											break;
										}
									}
									Console.WriteLine($"!! Error !! : Illegal parameter");
								} else {
									Console.WriteLine($"!! Error !! : Syntax error");
								}
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.RepeatBreak:{
								if (aoStackRepeatHead.Count > 0 && oPacketRepeatHead >= 0){
									aoStackRepeatBreak.Push(oPacketRepeatBreak);
									oPacketRepeatBreak = maPacket.Count;
									break;
								}
								Console.WriteLine($"!! Error !! : Syntax error");
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.L:{
								maPacket.Add(new Packet(Packet.eCommand.L, 0, Callstack_oPrev, oCallstackLogger));
								break;
							}
							case (int)eFunction.V:{
								int Value = 0;
								if (c.GetDecimal(ref Value, true)){
									maPacket.Add(new Packet(Packet.eCommand.V, Value, Callstack_oPrev, oCallstackLogger));
									break;
								}
								Console.WriteLine($"!! Error !! : Illegal parameter");
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.R:{
								int Value = 0;
								if (c.GetDecimal(ref Value, true)){
									maPacket.Add(new Packet(Packet.eCommand.R, Value, Callstack_oPrev, oCallstackLogger));
									break;
								}
								Console.WriteLine($"!! Error !! : Illegal parameter");
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.RF:{
								int Value = 0;
								if (c.GetDecimal(ref Value, true)){
									if (Value >= 0 && Value <= 1){
										maPacket.Add(new Packet(Packet.eCommand.RF, Value, Callstack_oPrev, oCallstackLogger));
										break;
									}
								}
								Console.WriteLine($"!! Error !! : Illegal parameter");
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.Rm:{
								int Value = 0;
								if (c.GetDecimal(ref Value, true)){
									if (Value >= 0 && Value <= 1){
										maPacket.Add(new Packet(Packet.eCommand.Rm, Value, Callstack_oPrev, oCallstackLogger));
										break;
									}
								}
								Console.WriteLine($"!! Error !! : Illegal parameter");
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.yRTL:{
								int Value = 0;
								if (c.IsMark(',', true) > 0){
									if (c.GetValue(ref Value, true)){
										if (Value >= 0 && Value <= 63){
											maPacket.Add(new Packet(Packet.eCommand.yRTL, Value, Callstack_oPrev, oCallstackLogger));
											break;
										}
									}
									Console.WriteLine($"!! Error !! : Illegal parameter");
								} else {
									Console.WriteLine($"!! Error !! : Syntax error");
								}
								Callstack_oText = Callstack_oPrev;
								Result = false;
								break;
							}
							case (int)eFunction.Abort:{
								mbAbort = true;
								break;
							}
							case (int)eFunction.Silence:{
								mbSilence = true;
								break;
							}
							case (int)eFunction.J:{
								maPacket.Add(new Packet(Packet.eCommand.J, 0, Callstack_oPrev, oCallstackLogger));
								break;
							}
							case (int)eFunction.NOP:{
								break;
							}
							case (int)eFunction.CallstackPush:{
								int From_oLine = 0;
								int From_oBase = 0;
								int From_oText = 0;
								if (c.GetDecimal(ref From_oLine, false)){
									if (c.GetDecimal(ref From_oBase, false)){
										if (c.GetDecimal(ref From_oText, false)){
											CallMacro.Push(new CallMacroProperty(In_oLine, In_nMacro));
											
											if (c.GetDecimal(ref In_oLine, false)){
												if (c.GetDecimal(ref In_oBase, false)){
													if (c.GetDecimal(ref In_nMacro, true, false)){
														CallstackFrom.Push(new Callstack.Record(From_oLine, From_oBase, From_oText));
														oCallstackLogger = mCallstackLogger.Add(CallstackFrom, In_oLine, In_nMacro);
														
														if (c.IsMark(',', false) > 0){
															c.Commit();
															
															Callstack_oText = In_oBase;
															break;
														}
													}
												}
											}
										}
									}
								}
								Console.WriteLine($"!! Error !! : Fatal error");
								Result = false;
								break;
							}
							case (int)eFunction.CallstackPop:{
								var Record = CallstackFrom.Pop();
								int From_oBase = Record.Base;
								int From_oText = 0;
								
								var In = CallMacro.Pop();
								In_oLine = In.Key;
								In_nMacro = In.Value;
								oCallstackLogger = mCallstackLogger.Add(CallstackFrom, In_oLine, In_nMacro);
								
								if (CallstackFrom.Count == 0 && CallMacro.Count == 0){
									break;
								}
								if (c.GetDecimal(ref From_oText, true, false)){
									if (c.IsMark(',', false) > 0){
										c.Commit();
										
										Callstack_oText = From_oBase + From_oText;
										break;
									}
								}
								Console.WriteLine($"!! Error !! : Fatal error");
								Result = false;
								break;
							}
						}
					} else {
						Console.WriteLine($"!! Error !! : Unknown function \'{Spread.Text[c.Current]}\'");
						Result = false;
					}
				}
			}
			if (Result && aoStackRepeatHead.Count > 0 && aoStackRepeatBreak.Count > 0){
				Console.WriteLine($"!! Error !! : Repeat not closed");
				Result = false;
			}
			if (!Result){
				CallstackFrom.Log(In_oLine, Callstack_oText, In_nMacro);
			}
			return Result;
		}
		
		
		
		bool GetScale(ref eScale Scale, Cursor c)
		{
			// 1
			if (c.Compare("c")){ Scale = eScale.c; return true; }
			
			return false;
		}
		
		
		
		bool GetFunction(ref eFunction Function, Cursor c)
		{
			// 4
			if (c.Compare("yRTL")){ Function = eFunction.yRTL; return true; }
			
			// 2
			if (c.Compare("pM")){ Function = eFunction.pM; return true; }
			if (c.Compare("pR")){ Function = eFunction.pR; return true; }
			if (c.Compare("pL")){ Function = eFunction.pL; return true; }
			if (c.Compare("pC")){ Function = eFunction.pC; return true; }
			if (c.Compare("RF")){ Function = eFunction.RF; return true; }
			if (c.Compare("Rm")){ Function = eFunction.Rm; return true; }
			
			// 1
			if (c.Compare("r")){ Function = eFunction.r; return true; }
			if (c.Compare("C")){ Function = eFunction.C; return true; }
			if (c.Compare("t")){ Function = eFunction.t; return true; }
			if (c.Compare("T")){ Function = eFunction.T; return true; }
			if (c.Compare("v")){ Function = eFunction.v; return true; }
			if (c.Compare("q")){ Function = eFunction.q; return true; }
			if (c.Compare("l")){ Function = eFunction.l; return true; }
			if (c.Compare(mvUp)){ Function = eFunction.vUp; return true; }
			if (c.Compare(mvDown)){ Function = eFunction.vDown; return true; }
			if (c.Compare("&")){ Function = eFunction.Slur; return true; }
			if (c.Compare("^")){ Function = eFunction.Tie; return true; }
			if (c.Compare("[")){ Function = eFunction.RepeatHead; return true; }
			if (c.Compare("]")){ Function = eFunction.RepeatTail; return true; }
			if (c.Compare("/")){ Function = eFunction.RepeatBreak; return true; }
			if (c.Compare("L")){ Function = eFunction.L; return true; }
			if (c.Compare("V")){ Function = eFunction.V; return true; }
			if (c.Compare("R")){ Function = eFunction.R; return true; }
			if (c.Compare(":")){ Function = eFunction.Abort; return true; }
			if (c.Compare("!")){ Function = eFunction.Silence; return true; }
			if (c.Compare("J")){ Function = eFunction.J; return true; }
			if (c.Compare("|")){ Function = eFunction.NOP; return true; }
			if (c.Compare(" ")){ Function = eFunction.NOP; return true; }
			if (c.Compare("\t")){ Function = eFunction.NOP; return true; }
			if (c.Compare("{")){ Function = eFunction.CallstackPush; return true; }
			if (c.Compare("}")){ Function = eFunction.CallstackPop; return true; }
			
			return false;
		}
		
		
		
		int AddPacketScale(int oFullScale, int Clock, int oText, int oCallstackLogger)
		{
			var Surplus = Clock & 0xff;
			if (Surplus > 0){
				var Packet = new Packet((Packet.eCommand)oFullScale, Surplus, oText, oCallstackLogger);
				Clock -= Surplus;
				if (Clock > 0) Packet.AddSlur();
				maPacket.Add(Packet);
			}
			
			while (Clock > 0){
				var Packet = new Packet((Packet.eCommand)oFullScale, /*0x100*/0, oText, oCallstackLogger);
				Clock -= 0x100;
				if (Clock > 0) Packet.AddSlur();
				maPacket.Add(Packet);
			}
			return maPacket.Count - 1;
		}
		
		
		
		public void Dump(char Channel)
		{
			Console.WriteLine($"{Channel}");
			foreach (var v in maPacket){
				Console.WriteLine($"{v.CommandEnum} {v.Value} {v.Column} {v.Logger}");
				
				if (v.Logger >= 0){
					var Logger = mCallstackLogger[v.Logger];
					Logger.Callstack.Log(Logger.In_oLine, v.Column, Logger.In_nMacro);
				}
			}
		}
	}
}
