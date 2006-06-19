using System;
using System.Windows.Forms;
using System.Drawing;

namespace BlueprintIT.Controls
{
  public delegate void TabMouseEventHandler(object sender, TabMouseEventArgs e);

  public delegate void TabEventHandler(object sender, TabEventArgs e);

  public delegate void TabPaintEventHandler(object sender, TabPaintEventArgs e);

  public class TabEventArgs : EventArgs
  {
    private int index;

    public TabEventArgs(int index) : base()
    {
      this.index = index;
    }

    public int Index
    {
      get
      {
        return index;
      }
    }
  }

  public class TabMouseEventArgs : MouseEventArgs
  {
    private int index;
    private Rectangle bounds;

    public TabMouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta, int index, Rectangle bounds)
      : base(button, clicks, x, y, delta)
    {
      this.index = index;
      this.bounds = bounds;
    }

    public TabMouseEventArgs(MouseEventArgs e, int index, Rectangle bounds)
      : base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
    {
      this.index = index;
      this.bounds = bounds;
    }

    public int Index
    {
      get
      {
        return index;
      }
    }

    public Rectangle TabBounds
    {
      get
      {
        return bounds;
      }
    }
  }

  public class TabPaintEventArgs : PaintEventArgs
  {
    private int index;

    public TabPaintEventArgs(Graphics g, Rectangle clip, int index)
      : base(g, clip)
    {
      this.index = index;
    }

    public int Index
    {
      get
      {
        return index;
      }
    }
  }

	/// <summary>
	/// A class to contain the information regarding the
	/// <see cref="BpTabControl.TabChanging"/> event.
	/// </summary>
	public class TabChangingEventArgs : EventArgs
	{
		/// <summary>
		/// Creates an instance of the <see cref="TabChangingEventArgs"/>
		/// class with the specified indices.
		/// </summary>
		/// <param name="currentIndex">The current selected index of the <see cref="BpTabControl"/>.</param>
		/// <param name="newIndex">The value to which the selected index changes.</param>
		public TabChangingEventArgs( int currentIndex, int newIndex )
		{
			this.cIndex = currentIndex;
			this.nIndex = newIndex;
		}

		/// <summary>
		/// Gets the value of the <see cref="BpTabControl.SelectedIndex"/> for the
		/// <see cref="BpTabControl"/> from which this event got generated.
		/// </summary>
		public int CurrentIndex
		{
			get
			{
				return cIndex;
			}
		}

		/// <summary>
		/// Gets the value to which the <see cref="BpTabControl.SelectedIndex"/>
		/// will change.
		/// </summary>
		public int NewIndex
		{
			get
			{
				return nIndex;
			}
		}

		/// <summary>
		/// Gets and sets a value indicating whether the <see cref="BpTabControl"/>'s
		/// <see cref="BpTabControl.SelectedIndex"/> should change.
		/// </summary>
		public bool Cancel
		{
			get
			{
				return cancelEvent;
			}
			set
			{
				cancelEvent = value;
			}
		}

		/// <summary>
		/// A boolean value to indicate if the event should get cancelled.
		/// </summary>
		private bool cancelEvent;

		/// <summary>
		/// The current index to report in the event.
		/// </summary>
		private int cIndex;

		/// <summary>
		/// The index to which the tab control changes.
		/// </summary>
		private int nIndex;
	}
}
