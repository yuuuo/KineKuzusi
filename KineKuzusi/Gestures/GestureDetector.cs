using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Microsoft.Kinect;

namespace Kinect.Toolbox
{
    public abstract class GestureDetector
    {       
        public int MinimalPeriodBetweenGestures { get; set; }

        readonly List<Entry> entries = new List<Entry>();

        public event Action<string> OnGestureDetected;

        DateTime lastGestureDate = DateTime.Now;

        readonly int windowSize; // Number of recorded positions

        protected GestureDetector(int windowSize)
        {
            this.windowSize = windowSize;
        }

        protected List<Entry> Entries
        {
            get { return entries; }
        }

        public int WindowSize
        {
            get { return windowSize; }
        }

        public virtual void Add(SkeletonPoint position, KinectSensor sensor)
        {
            Entry newEntry = new Entry {Position = position.ToVector3(), Time = DateTime.Now};
            Entries.Add(newEntry);

            // Look for gestures
            LookForGesture();
        }

        protected abstract void LookForGesture();

        protected void RaiseGestureDetected(string gesture)
        {
            // Too close?
            if (DateTime.Now.Subtract(lastGestureDate).TotalMilliseconds > MinimalPeriodBetweenGestures)
            {
                if (OnGestureDetected != null)
                    OnGestureDetected(gesture);

                lastGestureDate = DateTime.Now;
            }
        }
    }
}