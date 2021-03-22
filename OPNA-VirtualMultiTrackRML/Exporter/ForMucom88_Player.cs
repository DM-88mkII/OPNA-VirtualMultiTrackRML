


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static FM_VirtualMultiTrackMML.OPNA;
using static FM_VirtualMultiTrackMML.Util;



namespace FM_VirtualMultiTrackMML
{
	namespace ForMucom88
	{
		public class Player
		{
			class Track {
				Sequence mSequence;
				
				public bool mb1st = true;
				public bool mbSlur = false;
				public int mClock = 1;
				public int mRelativeVolume = 0;
				public Volume mAbsoluteVolume = new Volume();
				public int mStaccato = 0;
				
				public int mReverv = 0;				// todo
				public bool mbReverv = false;		// todo
				public bool mbRevervMode = false;	// todo
				
				public bool mbLoop = false;
				public bool mbLoopMark = false;
				
				public Register mRegister = new Register();
				
				
				
				public Track(List<Delivery.Part> aPart)
				{
					mSequence = new Sequence(aPart);
				}
				
				
				
				public bool IsTerm
				{
					get { return mSequence.IsTerm; }
				}
				
				
				
				public bool Peek(out Packet @Packet)
				{
					return mSequence.Peek(out Packet);
				}
				
				
				
				public CallstackLogger.Logger Logger(int oLogger)
				{
					return mSequence.Part.CallstackLogger[oLogger];
				}
				
				
				
				public bool Tick(ref StringBuilder Queue, ref int Clock, int oTrack, bool[] abKeyOn, bool[] abKeyOff, out bool bScale, ref Register.Param RTL)
				{
					abKeyOff[oTrack] = false;
					bScale = false;
					
					bool Result = true;
					if (!IsTerm){
						--mClock;
						if ((abKeyOn[oTrack] || mb1st) && mClock <= mStaccato && !mbSlur){
							Duration(ref Queue, ref Clock);
							
							abKeyOn[oTrack] = false;
							abKeyOff[oTrack] = true;
							
							mb1st = false;
						}
						if (mClock == 0){
							bool bBreak = false;
							while (!IsTerm && !bBreak){
								Packet Packet;
								if (mSequence.Pop(out Packet)){
									if (Packet.IsScale){
										Duration(ref Queue, ref Clock);
										if (!mbLoopMark && mbLoop){
											mbLoopMark = true;
											Queue.Append($"L ");
										}
										
										mbSlur = Packet.IsSlur;
										mClock = (Packet.Value > 0)? Packet.Value: 0x100;
										if (!mbSlur){
											abKeyOn[oTrack] = true;
										}
										bScale = true;
										bBreak = true;
									} else {
										switch ((int)Packet.CommandEnum){
											case (int)Packet.eCommand.r:{
												mbSlur = false;
												mClock = (Packet.Value > 0)? Packet.Value: 0x100;
												
												abKeyOn[oTrack] = false;
												abKeyOff[oTrack] = true;
												bBreak = true;
												break;
											}
											case (int)Packet.eCommand.q:{
												mStaccato = Packet.Value;
												break;
											}
											case (int)Packet.eCommand.t:{
												Queue.Append($"t{Packet.Value} ");
												break;
											}
											case (int)Packet.eCommand.T:{
												Queue.Append($"T{Packet.Value} ");
												break;
											}
											case (int)Packet.eCommand.R:{
												mReverv = Packet.Value;
												break;
											}
											case (int)Packet.eCommand.RF:{
												mbReverv = (Packet.Value > 0);
												break;
											}
											case (int)Packet.eCommand.Rm:{
												mbRevervMode = (Packet.Value > 0);
												break;
											}
											case (int)Packet.eCommand.pM:{
												mRegister.LR.Value = 0;
												break;
											}
											case (int)Packet.eCommand.pR:{
												mRegister.LR.Value = 1;
												break;
											}
											case (int)Packet.eCommand.pL:{
												mRegister.LR.Value = 2;
												break;
											}
											case (int)Packet.eCommand.pC:{
												mRegister.LR.Value = 3;
												break;
											}
											case (int)Packet.eCommand.J:{
												Queue.Append($"J ");
												break;
											}
											case (int)Packet.eCommand.L:{
												if (oTrack == 0){
													mbLoop = true;
												}
												break;
											}
											case (int)Packet.eCommand.yRTL:{
												RTL.Value = Packet.Value;
												break;
											}
											
											case (int)Packet.eCommand.v:{
												mAbsoluteVolume.Value = Packet.Value + mRelativeVolume;
												mRegister.IL.Value = mAbsoluteVolume.Clamp;
												break;
											}
											case (int)Packet.eCommand.V:{
												mRelativeVolume = Packet.ValueInt;
												break;
											}
											case (int)Packet.eCommand.vUp:{
												mAbsoluteVolume.Value += Packet.Value;
												mRegister.IL.Value = mAbsoluteVolume.Clamp;
												break;
											}
											case (int)Packet.eCommand.vDown:{
												mAbsoluteVolume.Value -= Packet.Value;
												mRegister.IL.Value = mAbsoluteVolume.Clamp;
												break;
											}
										}
									}
								}
							}
						}
					}
					
					if (Result){
						if (mRegister.LR.IsModified || mRegister.IL.IsModified){
							var Register = OPNA.s_aReg_LR_IL[oTrack];
							var Value = mRegister.IL.Value | (mRegister.LR.Value << 6);
							Append(ref Queue, Register, Value);
						}
					}
					return Result;
				}
			}
			
			
			
			Track[] maTrack = new Track[6];
			
			
			
			~Player()
			{
			}
			
			
			
			public Player(Delivery @Delivery)
			{
				maTrack[0] = new Track(Delivery.PartLibrary.Bake('B'));
				maTrack[1] = new Track(Delivery.PartLibrary.Bake('S'));
				maTrack[2] = new Track(Delivery.PartLibrary.Bake('C'));
				maTrack[3] = new Track(Delivery.PartLibrary.Bake('H'));
				maTrack[4] = new Track(Delivery.PartLibrary.Bake('T'));
				maTrack[5] = new Track(Delivery.PartLibrary.Bake('R'));
			}
			
			
			
			public bool Convert(out string Muc)
			{
				bool Result = true;
				
				var Queue = new StringBuilder(0x4000);
				{	// 
					var Alias = "G";
					
					{	// 
						Queue.Append($"{Alias} ");
					}
					
					bool[] abTerm = new bool[6]{false, false, false, false, false, false};
					bool[] abKeyOn = new bool[6]{false, false, false, false, false, false};
					bool[] abKeyOff = new bool[6]{false, false, false, false, false, false};
					bool[] abScale = new bool[6]{false, false, false, false, false, false};
					int Clock = -1;
					
					var RTL = new Register.Param();
					
					int nOutput = 0;
					
					bool bBreak = false;
					
					while (Result && !bBreak && !(abTerm[0] && abTerm[1] && abTerm[2] && abTerm[3] && abTerm[4] && abTerm[5])){
						++Clock;
						
						{	// 
							int oTrack = 0;
							foreach (var t in maTrack){
								Result = t.Tick(ref Queue, ref Clock, oTrack, abKeyOn, abKeyOff, out abScale[oTrack], ref RTL);
								if (Result){
									abTerm[oTrack] = t.IsTerm;
									++oTrack;
								} else {
									break;
								}
							}
						}
						
						if (Result){
							bool bKeyOff = false;
							bKeyOff = (abKeyOff[0])? abKeyOff[0]: bKeyOff;
							bKeyOff = (abKeyOff[1])? abKeyOff[1]: bKeyOff;
							bKeyOff = (abKeyOff[2])? abKeyOff[2]: bKeyOff;
							bKeyOff = (abKeyOff[3])? abKeyOff[3]: bKeyOff;
							bKeyOff = (abKeyOff[4])? abKeyOff[4]: bKeyOff;
							bKeyOff = (abKeyOff[5])? abKeyOff[5]: bKeyOff;
							
							bool bScale = false;
							bScale = (abScale[0])? abScale[0]: bScale;
							bScale = (abScale[1])? abScale[1]: bScale;
							bScale = (abScale[2])? abScale[2]: bScale;
							bScale = (abScale[3])? abScale[3]: bScale;
							bScale = (abScale[4])? abScale[4]: bScale;
							bScale = (abScale[5])? abScale[5]: bScale;
							
							if (RTL.IsModified){
								var Register = OPNA.s_Reg_RTL;
								var Value = RTL.Value;
								Append(ref Queue, Register, Value);
							}
							
							if (bKeyOff || bScale){
								if (nOutput++ == 0){
									Queue.Append($"\r\n{Alias} ");
								} else {
									nOutput = (nOutput == 6)? 0: nOutput;
								}
								
								if (bKeyOff){
									// Dump
									var Register = OPNA.s_Reg_KeyOn;
									int Value = 0x80;
									for (int oTrack = 0; oTrack < maTrack.Length; ++oTrack){
										Value |= (abKeyOff[oTrack])? s_aValue_Track[oTrack]: 0;
									}
									Append(ref Queue, Register, Value);
								}
								
								if (bScale){
									// KeyOn
									var Register = OPNA.s_Reg_KeyOn;
									int Value = 0;
									for (int oTrack = 0; oTrack < maTrack.Length; ++oTrack){
										Value |= (abScale[oTrack])? s_aValue_Track[oTrack]: 0;
									}
									Append(ref Queue, Register, Value);
								}
							}
						}
						
						{	// 
							int oTrack = 0;
							foreach (var t in maTrack){
								if (t.mbLoop && abTerm[oTrack]){
									bBreak = true;
									Clock = 0;
									break;
								}
							}
						}
					}
					
					if (Result){
						Duration(ref Queue, ref Clock);
					}
				}
				Muc = Queue.ToString();
				return Result;
			}
			
			
			
			static void Append(ref StringBuilder Queue, int Register, int Value)
			{
				Queue.Append($"y${Register:x02},${Value:x02} ");
			}
			
			
			
			static void Duration(ref StringBuilder Queue, ref int Clock)
			{
				if (Clock > 0){
					var Surplus = Clock & 0x7f;
					if (Surplus > 0){
						Clock -= Surplus;
						Queue.Append($"r%{Surplus}");
					}
					while (Clock > 0){
						Clock -= 0x80;
						Queue.Append($"r%128");
					}
					Queue.Append($" ");
				}
			}
		}
	}
}
