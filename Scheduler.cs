using System;
using System.Collections;
using System.Threading;
using System.Diagnostics;

namespace BlueprintIT.Utils
{
	public delegate object ScheduledEventHandler();

	public class Scheduler
	{
		private class ScheduledEvent
		{
			private DateTime nextcall;
			public ScheduledEventHandler Handler;
			private string id;
			private Timer timer;

			public ScheduledEvent(string id, DateTime time, ScheduledEventHandler handler)
			{
				this.id=id;
				this.Handler=handler;
				timer = new Timer(new TimerCallback(Callback),null,10000,0);
				NextCall=time;
			}

			public DateTime NextCall
			{
				set
				{
					nextcall=value;
					long diff = (nextcall.Ticks-DateTime.Now.Ticks)/10000;
					if (diff<0)
					{
						diff=0;
					}
					timer.Change(diff,0);
				}
			}

			public void Cancel()
			{
				timer.Dispose();
				timer=null;
			}

			private void Callback(object state)
			{
				Debug.WriteLine("Calling scheduled event "+id+" at "+DateTime.Now.ToShortTimeString());
				object repeat = Handler();

				if (repeat is short)
					repeat = (double)((short)repeat);
				if (repeat is int)
					repeat = (double)((int)repeat);
				if (repeat is long)
					repeat = (double)((long)repeat);
				if (repeat is float)
					repeat = (double)((float)repeat);

				if ((repeat is double)&&(((double)repeat)>=0))
				{
					repeat=DateTime.Now.AddSeconds((double)repeat);
				}

				if (repeat is DateTime)
				{
					NextCall=(DateTime)repeat;
					Debug.WriteLine("Repeating at "+nextcall.ToShortTimeString());
				}
				else
				{
					timer.Dispose();
					timer=null;
					lock(map)
					{
						map.Remove(id);
					}
				}
			}
		}

		private static Random random;
		private static IDictionary map;

		static Scheduler()
		{
			random = new Random();
			map = new Hashtable();
		}

		public static void ScheduleEvent(DateTime time, ScheduledEventHandler handler)
		{
			string id;
			lock (map)
			{
				do
				{
					id = random.Next(10000000,100000000).ToString();
				} while (map[id]!=null);
			}
			ScheduleEvent(id,time,handler);
		}

		public static void ScheduleEvent(string id, DateTime time, ScheduledEventHandler handler)
		{
			lock (map)
			{
				ScheduledEvent ev = (ScheduledEvent)map[id];
				if (ev==null)
				{
					Debug.WriteLine("Scheduling event "+id+" for "+time.ToShortTimeString());
					ev = new ScheduledEvent(id,time,handler);
				}
				else
				{
					Debug.WriteLine("Rescheduling event "+id+" for "+time.ToShortTimeString());
					if (handler!=null)
					{
						ev.Handler=handler;
					}
					ev.NextCall=time;
				}
				map[id]=ev;
			}
		}

		public static void CancelEvent(string id)
		{
			lock (map)
			{
				ScheduledEvent ev = (ScheduledEvent)map[id];
				if (ev!=null)
				{
					map.Remove(id);
					ev.Cancel();
				}
			}
		}
	}
}
