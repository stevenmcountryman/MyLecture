using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace MyLecture.Controls
{
    public class InkStrokeMemory
    {
        public InkStroke stroke
        {
            get;
            private set;
        }
        public Point location
        {
            get;
            private set;
        }
        public ActionTaken actionTaken
        {
            get;
            private set;
        }

        public InkStrokeMemory(InkStroke inkStroke, Point point, ActionTaken action)
        {
            this.stroke = inkStroke;
            this.location = point;
            actionTaken = action;
        }

        public enum ActionTaken
        {
            Added,
            Deleted
        }
    }
}
