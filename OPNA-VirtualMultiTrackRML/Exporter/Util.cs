


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace FM_VirtualMultiTrackMML
{
	public class Util {
		public class Sequence {
			List<Delivery.Part> maPart;
			int moPart = 0;
			int moPacket = -1;
			
			
			
			public Sequence(List<Delivery.Part> aPart)
			{
				maPart = aPart;
			}
			
			
			
			public bool IsTerm
			{
				get { return (moPart >= maPart.Count); }
			}
			
			
			
			public Delivery.Part Part
			{
				get { return (moPart < maPart.Count)? maPart[moPart]: null; }
			}
			
			
			
			public bool Pop(out Packet @Packet)
			{
				Packet = null;
				if (moPart < maPart.Count){
					if ((moPacket+1) < maPart[moPart].Packet.Count){
						Packet = maPart[moPart].Packet[++moPacket];
						return true;
					} else {
						++moPart;
						moPacket = -1;
						return Pop(out Packet);
					}
				}
				return false;
			}
			
			
			
			public bool Peek(out Packet @Packet)
			{
				Packet = null;
				if (!IsTerm){
					Packet = maPart[moPart].Packet[moPacket];
					return true;
				}
				return false;
			}
		}
		
		
		
		public class Volume {
			int mv = 0;
			
			
			
			public int Value
			{
				set { mv = value; }
				get { return mv; }
			}
			
			
			
			public int Clamp
			{
				get {
					var v = mv;
					v = (v >=  0)? v:  0;
					v = (v <= 31)? v: 31;
					return v;
				}
			}
		}
		
		
		
		public class Register {
			public class Param {
				int mValue = 0;
				bool mbModified = true;
				
				
				
				public int Value
				{
					set { mbModified = (mbModified || mValue != value); mValue = value; }
					get { mbModified = false; return mValue; }
				}
				
				
				
				public bool IsModified
				{
					get { return mbModified; }
				}
			}
			
			public Param LR = new Param();
			public Param IL = new Param();
		}
	}
}
