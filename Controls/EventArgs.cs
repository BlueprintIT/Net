using System;
using System.Windows.Forms;
using System.Drawing;

namespace BlueprintIT.Controls
{
  public delegate void TabMouseEventHandler(object sender, TabMouseEventArgs e);

  public delegate void TabEventHandler(object sender, TabEventArgs e);

  public delegate void TabPaintEventHandler(object sender, TabPaintEventArgs e);

  public delegate void TabClosingEventHandler(object sender, TabClosingEventArgs e);

  public class TabClosingEventArgs : TabEventArgs
  {
    private bool cancel = false;

    public TabClosingEventArgs(BpTabPage page)
      : base(page)
    {
    }

    public bool Cancel
    {
      get
      {
        return cancel;
      }

      set
      {
        cancel = value;
      }
    }
  }

  public class TabEventArgs : EventArgs
  {
    private BpTabPage page;

    public TabEventArgs(BpTabPage page)
      : base()
    {
      this.page = page;
    }

    public BpTabPage TabPage
    {
      get
      {
        return page;
      }
    }
  }

  public class TabMouseEventArgs : MouseEventArgs
  {
    private BpTabPage page;
    private Rectangle bounds;

    public TabMouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta, BpTabPage page, Rectangle bounds)
      : base(button, clicks, x, y, delta)
    {
      this.page = page;
      this.bounds = bounds;
    }

    public TabMouseEventArgs(MouseEventArgs e, BpTabPage page, Rectangle bounds)
      : base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
    {
      this.page = page;
      this.bounds = bounds;
    }

    public BpTabPage TabPage
    {
      get
      {
        return page;
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
    private BpTabPage page;

    public TabPaintEventArgs(Graphics g, Rectangle clip, BpTabPage page)
      : base(g, clip)
    {
      this.page = page;
    }

    public BpTabPage TabPage
    {
      get
      {
        return page;
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
		public TabChangingEventArgs(BpTabPage current, BpTabPage newtab)
		{
			this.cIndex = current;
			this.nIndex = newtab;
		}

		/// <summary>
		/// Gets the value of the <see cref="BpTabControl.SelectedIndex"/> for the
		/// <see cref="BpTabControl"/> from which this event got generated.
		/// </summary>
		public BpTabPage CurrentTab
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
    public BpTabPage NewTab
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
		private BpTabPage cIndex;

		/// <summary>
		/// The index to which the tab control changes.
		/// </summary>
		private BpTabPage nIndex;
	}
}
