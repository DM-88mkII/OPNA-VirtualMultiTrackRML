


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;



namespace FM_VirtualMultiTrackMML
{
	[DataContract]
	public class Packet {
		[DataContract]
		public enum eCommand : byte {
			c,	// 00
			
			r = 0x60,		// 60
			q,		// 61
			t,		// 62
			T,		// 63
			v,		// 64
			V,		// 65
			R,		// 66
			RF,		// 67
			Rm,		// 68
			pM,		// 69
			pR,		// 6a
			pL,		// 6b
			pC,		// 6c
			J,		// 6d
			L,		// 6e
			yRTL,	// 6f
			
			vUp,	// 70
			vDown,	// 71
			
			// Slur
			c_ = 0x80,	// 80
		}
		
		[DataMember]
		byte mCommand;
		
		[DataMember]
		byte mValue;
		
		[DataMember]
		int moText;
		
		[DataMember]
		int moLogger;
		
		
		
		public Packet(eCommand Command, int Value, int oText, int oLogger)
		{
			mCommand = (byte)Command;
			mValue = (byte)Value;
			moText = oText;
			moLogger = oLogger;
		}
		
		
		
		public byte Command
		{
			set { mCommand = value; }
			get { return mCommand; }
		}
		
		
		
		public byte Value
		{
			set { mValue = value; }
			get { return mValue; }
		}
		
		
		
		public int Column
		{
			set { moText = value; }
			get { return moText; }
		}
		
		
		
		public int Logger
		{
			set { moLogger = value; }
			get { return moLogger; }
		}
		
		
		
		public eCommand CommandEnum
		{
			set { mCommand = (byte)value; }
			get { return (eCommand)mCommand; }
		}
		
		
		
		public int ValueInt
		{
			set { mValue = unchecked((byte)value); }
			get { return (int)unchecked((sbyte)mValue); }
		}
		
		
		
		public bool IsScale
		{
			get { return (mCommand == (byte)eCommand.c || mCommand == (byte)eCommand.c_); }
		}
		
		
		
		public bool IsSlur
		{
			get { return (mCommand == (byte)eCommand.c_); }
		}
		
		
		
		public int Scale
		{
			get { return (IsScale)? (mCommand & 0x7f): -1; }
		}
		
		
		
		public bool AddSlur()
		{
			if (IsScale){
				mCommand |= 0x80;
				return true;
			}
			return false;
		}
		
		
		
		public static bool IsFullScale(int FullScale)
		{
			return (FullScale == (int)eCommand.c);
		}
	}
}
