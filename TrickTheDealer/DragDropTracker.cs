using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace TrickTheDealer
{
    class DragDropTracker
    {
        private Rect currentRectangle;
        public Rect Rect
        {
            get
            {
                return new Rect(
                currentRectangle.X, currentRectangle.Y, currentRectangle.Width, currentRectangle.Height);
            }
        }

        private ICollection<ButtonFrame> allButtons;
        ButtonFrame currentIntersection = null;
        private double kIntersectionAreaThreshold = 0.5;
        public bool StartingCardMiddleChoice { get; set;}
        public bool StartingCardLowerChoice { get; set; }
        public bool StartingCardHigherChoice { get; set; }

        public DragDropTracker(Rect startingRectangle, ICollection<ButtonFrame> allButtons)
        {
            this.currentRectangle = startingRectangle;
            this.allButtons = allButtons;
            this.Invalid = false;
        }

        public void DragDelta(double horizontalChange, double verticalChange)
        {
            currentIntersection = null;
            currentRectangle.X += horizontalChange;
            currentRectangle.Y += verticalChange;
        }

        public ButtonFrame GetIntersectingButton()
        {
            ComputeIntersection();
            return currentIntersection;
        }

        public bool Intersects()
        {
            ComputeIntersection();
            return currentIntersection != null;
        }

        private void ComputeIntersection()
        {
            currentIntersection = allButtons.Where(b => IntersectsMoreThanHalf(b.Frame)).FirstOrDefault();
        }

        private bool IntersectsMoreThanHalf(Rect rectangle)
        {
            Rect intersection = new Rect(
                currentRectangle.X, currentRectangle.Y, currentRectangle.Width, currentRectangle.Height);
            intersection.Intersect(rectangle);

            if (!intersection.IsEmpty)
            {
                double currentRectangleArea = currentRectangle.Height * currentRectangle.Width;
                double otherRectangleArea = rectangle.Height * rectangle.Width;

                double intersectionArea = intersection.Height * intersection.Width;
                return intersectionArea >= kIntersectionAreaThreshold * Math.Min(currentRectangleArea, otherRectangleArea);
            }
            return false;
        }


        public bool Invalid { get; set; }
    }
}
