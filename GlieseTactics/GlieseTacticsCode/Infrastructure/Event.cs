using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Gliese581g
{
    public abstract class Event
    {
        public TimeSpan EventTriggerTime = TimeSpan.Zero;
        protected TimeSpan m_timePassed = TimeSpan.Zero;
        protected bool m_eventHappened = false;


        public Event(TimeSpan timeUntilEvent)
        {
            EventTriggerTime = timeUntilEvent;
            m_timePassed = TimeSpan.Zero;
            m_eventHappened = false;
        }

        public void Reset()
        {
            m_timePassed = TimeSpan.Zero;
            m_eventHappened = false;
        }

        public abstract void OnEvent(GameScreen parentScreen);

        public virtual bool IsFinished()
        {
            return m_eventHappened;
        }

        public virtual bool Update(GameTime gameTime, GameScreen parentScreen)
        {
            m_timePassed += gameTime.ElapsedGameTime;

            if (m_timePassed > EventTriggerTime && m_eventHappened == false)
            {
                m_eventHappened = true;
                OnEvent(parentScreen);
            }
            return IsFinished();
        }

    }



    public class EventManager
    {
        private List<Event> m_events;

        public EventManager()
        {
            m_events = new List<Event>();
        }

        public void Update(GameTime time, GameScreen screen)
        {
            for (int ii = 0; ii < m_events.Count; ii++)
            {
                if (m_events[ii].Update(time, screen))
                {
                    try
                    {
                        m_events.RemoveAt(ii);
                        ii--;
                    }
                    catch
                    {
                        // One event probably caused some change in the event queue, like clearing it. =P
                        // Just break - you can get any other events on the next pass.
                        break;
                    }
                }
            }
        }

        public void AddEvent(Event newEvent)
        {
            m_events.Add(newEvent);
        }
        public void Clear()
        {
            m_events.Clear();
        }

    }
}
