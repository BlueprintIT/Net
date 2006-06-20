using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms.VisualStyles;
using System.Runtime.InteropServices;

namespace BlueprintIT.Controls
{
  [StructLayout(LayoutKind.Sequential)]
  public struct MARGINS
  {
    public int Left;
    public int Right;
    public int Top;
    public int Bottom;
  }

  public enum TabCloseStyle
  {
    None,
    Hover,
    Selected,
    SelectedAndHover,
    All
  }

  /// <summary>
  /// The delegate to use for the <see cref="BpTabControl.TabChanging"/> event.
  /// </summary>
  public delegate void TabChangingEventHandler(object sender, TabChangingEventArgs tcea);
  /// <summary>
	/// Yet Another Tab Control.
	/// </summary>
	[Designer(typeof(BlueprintIT.Controls.Design.BpTabControlDesigner))]
	public class BpTabControl : Control
	{
    #region Private Members

    /// <summary>
    /// When to display close icons on tabs.
    /// </summary>
    private TabCloseStyle closeStyle = TabCloseStyle.None;

    /// <summary>
    /// The event handler that hears about tab changes.
    /// </summary>
    private EventHandler tabEventHandler;

    /// <summary>
    /// Holds the positions of the tabs.
    /// </summary>
    private IDictionary<BpTabPage, TabPosition> positions = new Dictionary<BpTabPage, TabPosition>();

    /// <summary>
    /// The amount of addition size added to the selected tab.
    /// </summary>
    private Padding tabExpand = new Padding(2);

    /// <summary>
    /// An image list used by tabs to draw themselves.
    /// </summary>
    private ImageList images = null;

    /// <summary>
    /// The row holding the selected tab.
    /// </summary>
    private int selectedRow = -1;

    /// <summary>
    /// The index of the selected tab.
    /// </summary>
    private int selectedIndex = -1;

    /// <summary>
    /// The currently selected tab.
    /// </summary>
    private BpTabPage selectedTab = null;

    /// <summary>
    /// Holds whether the user is hovering over a tab close button
    /// </summary>
    private bool hoverClose = false;

    /// <summary>
    /// Remember if the user currently has the mouse down on the close button.
    /// </summary>
    private BpTabPage clickClose = null;

    /// <summary>
    /// The currently hovered tab.
    /// </summary>
    private BpTabPage hoverTab = null;

    /// <summary>
    /// The number of rows of tabs.
    /// </summary>
    private int rows = 0;

    private IList<int> rowCounts = new List<int>();

    /// <summary>
    /// The height of tabs.
    /// </summary>
    private int tabHeight = 0;

    /// <summary>
    /// The rectangle in which the tabs get drawn.
    /// </summary>
    private Rectangle tabsRectangle = Rectangle.Empty;

    /// <summary>
    /// The rectangle for the tab pane.
    /// </summary>
    private Rectangle paneRectangle = Rectangle.Empty;

    /// <summary>
    /// The rectangle transformed for the <see cref="DisplayRectangle"/>
    /// property to return.
    /// </summary>
    private Rectangle displayRectangle = Rectangle.Empty;

    #endregion

    #region UxTheme PInvoke

    [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr OpenThemeData(IntPtr hWnd, String classList);

    [DllImport("uxtheme.dll", ExactSpelling = true)]
    private extern static Int32 CloseThemeData(IntPtr hTheme);

    [DllImport("uxtheme", ExactSpelling = true)]
    private extern static Int32 GetThemeMargins(
      IntPtr hTheme,
      IntPtr hdc,
      int iPartId,
      int iStateId,
      int iPropId,
      IntPtr prc,
      out MARGINS pMargins);

    private Padding GetMargins(IDeviceContext ctxt, MarginProperty type, IntPtr hTheme, int iPartId, int iStateId)
    {
      int iPropId = 0;
      switch (type)
      {
        case MarginProperty.CaptionMargins:
          iPropId = 3603;
          break;
        case MarginProperty.SizingMargins:
          iPropId = 3601;
          break;
        case MarginProperty.ContentMargins:
          iPropId = 3602;
          break;
      }
      MARGINS margins;
      GetThemeMargins(hTheme, ctxt.GetHdc(), iPartId, iStateId, iPropId, IntPtr.Zero, out margins);
      ctxt.ReleaseHdc();

      Padding padding = new Padding();
      padding.Left = margins.Left;
      padding.Right = margins.Right;
      padding.Top = margins.Top;
      padding.Bottom = margins.Bottom;
      return padding;
    }

    private Padding GetMargins(IDeviceContext ctxt, MarginProperty type, VisualStyleElement element)
    {
      IntPtr theme = OpenThemeData(this.Handle, element.ClassName);
      Padding padding = GetMargins(ctxt, type, theme, element.Part, element.State);
      CloseThemeData(theme);
      return padding;
    }

    private Padding GetMargins(IDeviceContext ctxt, MarginProperty type, VisualStyleRenderer renderer)
    {
      return GetMargins(ctxt, type, renderer.Handle, renderer.Part, renderer.State);
    }

    #endregion

    #region Constructor

		/// <summary>
		/// Creates a new instance of the <see cref="BpTabControl"/>
		/// class.
		/// </summary>
		public BpTabControl()
		{
			SetStyle( ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true );
			Height = Width = 300;
			BackColor = SystemColors.Control;
			CalculateRectangles();
      tabEventHandler = new EventHandler(OnTabChanged);
		}

		#endregion

    #region Public Events

    /// <summary>
    /// Occurs when the selected tab is about to change.
    /// </summary>
    public event TabChangingEventHandler TabChanging;

    /// <summary>
    /// Occurs after the selected tab has changed.
    /// </summary>
    public event EventHandler TabChanged;

    public event TabClosingEventHandler TabClosing;

    public event TabEventHandler TabClosed;

    public event TabMouseEventHandler TabMouseMove;

    public event TabMouseEventHandler TabMouseDown;

    public event TabMouseEventHandler TabMouseUp;

    public event TabMouseEventHandler TabMouseClick;

    public event TabMouseEventHandler TabMouseDoubleClick;

    public event TabEventHandler TabMouseEnter;

    public event TabEventHandler TabMouseLeave;

    public event TabPaintEventHandler TabPaint;

    #endregion

    #region Public Properties

    /// <summary>
    /// Determines when to display close buttons on tabs.
    /// </summary>
    [DefaultValue(TabCloseStyle.None)]
    public virtual TabCloseStyle TabCloseStyle
    {
      get
      {
        return closeStyle;
      }

      set
      {
        if (closeStyle == value)
          return;
        this.closeStyle = value;
        CalculateRectangles();
      }
    }

    /// <summary>
    /// Gets and sets the <see cref="ImageList"/> used by this
    /// <see cref="BpTabControl"/>.
    /// </summary>
    /// <remarks>
    /// To display an image on a tab, set the <see cref="ImageIndex"/> property
    /// of that <see cref="BpTabPage"/>. The <see cref="ImageIndex"/> acts as the
    /// index into the <see cref="ImageList"/>.
    /// </remarks>
    public virtual ImageList ImageList
    {
      get
      {
        return images;
      }
      set
      {
        images = value;
        CalculateRectangles();
      }
    }

    /// <summary>
    /// Gets and sets the zero-based index of the selected
    /// <see cref="BpTabPage"/>.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown if this property gets set to a value less than 0 when
    /// <see cref="BpTabPage"/>s exist in the control collection.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if this property gets set to a value greater than
    /// <see cref="Control.ControlCollection.Count"/>.
    /// </exception>
    public virtual int SelectedIndex
    {
      get
      {
        return selectedIndex;
      }
      set
      {
        if (value == selectedIndex)
          return;
        else if (value < 0 && Controls.Count > 0)
          throw new ArgumentException("Tried to set the property SelectedIndex to a negative number.");
        else if (value >= Controls.Count)
          throw new IndexOutOfRangeException("Tried to set the property of the SelectedIndex to a value greater than the number of controls.");

        TabChangingEventArgs tcea = new TabChangingEventArgs(selectedTab, (BpTabPage)Controls[value]);
        OnTabChanging(tcea);
        if (tcea.Cancel)
          return;

        selectedIndex = value;
        if (Controls.Count > 0)
        {
          selectedTab.Visible = false;
          selectedTab = (BpTabPage)Controls[value];
          selectedTab.Visible = true;
          selectedRow = positions[selectedTab].Row;
          CalculateRectangles();
        }
        OnTabChanged(new EventArgs());
      }
    }

    /// <summary>
    /// Gets and sets the currently selected tab.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown if this property gets set to a <see cref="BpTabPage"/>
    /// that has not been added to the <see cref="BpTabControl"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if this property gets set to a <b>null</b> value when
    /// <see cref="BpTabPage"/>s exist in the control.
    /// </exception>
    public virtual BpTabPage SelectedTab
    {
      get
      {
        return selectedTab;
      }
      set
      {
        if (value == selectedTab)
          return;
        if (value == null && Controls.Count > 0)
          throw new ArgumentNullException("value", "Tried to set the SelectedTab property to a null value.");
        else if (value != null && !Controls.Contains(value))
          throw new ArgumentException("Tried to set the SelectedTab property to a BpTabPage that has not been added to this BpTabControl.");
        SelectedIndex = Controls.IndexOf(value);
      }
    }

    /// <summary>
    /// Inherited from <see cref="Control"/>.
    /// </summary>
    public override Rectangle DisplayRectangle
    {
      get
      {
        return displayRectangle;
      }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Determine whether to display a close icon on the given tab.
    /// </summary>
    /// <param name="page">The tab</param>
    /// <returns>True if a close button should be displayed.</returns>
    public bool IncludesCloseButton(BpTabPage page)
    {
      if (closeStyle == TabCloseStyle.All)
        return true;

      if (closeStyle == TabCloseStyle.None)
        return false;

      if (page == selectedTab)
        return closeStyle == TabCloseStyle.Selected || closeStyle == TabCloseStyle.SelectedAndHover;

      if (page == clickClose)
        return true;

      if (page == hoverTab)
      {
        if (clickClose != null)
          return false;
        return closeStyle == TabCloseStyle.Hover || closeStyle == TabCloseStyle.SelectedAndHover;
      }

      return false;
    }

    /// <summary>
    /// Returns the bounding rectangle for a specified tab in this tab control.
    /// </summary>
    /// <param name="index">The 0-based index of the tab you want.</param>
    /// <returns>A <see cref="Rectangle"/> that represents the bounds of the specified tab.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The index is less than zero.<br />-or-<br />The index is greater than or equal to <see cref="Control.ControlCollection.Count" />.
    /// </exception>
    public virtual Rectangle GetTabRect(int index)
    {
      if ((index < 0) || (index > Controls.Count))
      {
        throw new ArgumentNullException("index", "Index outside of allowed range.");
      }
      BpTabPage page = (BpTabPage)Controls[index];
      return GetTabRect(page);
    }

    /// <summary>
    /// Returns the bounding rectangle for a specified tab in this tab control.
    /// </summary>
    /// <param name="page">The Tab Page to retrieve the bounds for.</param>
    /// <returns>A <see cref="Rectangle"/> that represents the bounds of the specified tab.</returns>
    public virtual Rectangle GetTabRect(BpTabPage page)
    {
      TabPosition position = positions[page];
      int row = position.Row;
      if (selectedRow > row)
        row++;
      else if (selectedRow == row)
        row = 0;
      Rectangle result = Rectangle.Empty;
      result.X = tabsRectangle.Left + position.Offset + tabExpand.Left;
      result.Width = position.Size.Width;
      result.Y = tabsRectangle.Bottom - (row * tabHeight) - tabHeight;
      result.Height = tabHeight;
      if (page == selectedTab)
      {
        result.Y -= tabExpand.Top;
        result.Height += tabExpand.Vertical;
        result.X -= tabExpand.Left;
        result.Width += tabExpand.Horizontal;
      }
      return result;
    }

    public virtual Rectangle GetTabContentRect(BpTabPage page)
    {
      return GetTabContentRect(page, GetTabRect(page));
    }

    public virtual Rectangle GetTabContentRect(BpTabPage page, Rectangle tab)
    {
      Graphics g = CreateGraphics();
      Rectangle bounds = new Rectangle(tab.X, tab.Y, tab.Width, tab.Height);
      VisualStyleElement element = GetElement(page);
      VisualStyleRenderer renderer;
      if (Application.RenderWithVisualStyles && VisualStyleRenderer.IsElementDefined(element))
      {
        renderer = new VisualStyleRenderer(element);
        bounds = renderer.GetBackgroundContentRectangle(g, bounds);
      }
      bounds.X += SystemInformation.Border3DSize.Width;
      bounds.Y += SystemInformation.Border3DSize.Height;
      bounds.Width -= SystemInformation.Border3DSize.Width * 2;
      bounds.Height -= SystemInformation.Border3DSize.Height * 2;

      if (IncludesCloseButton(page))
      {
        Size closesize;
        element = VisualStyleElement.ToolTip.Close.Normal;
        if (Application.RenderWithVisualStyles && VisualStyleRenderer.IsElementDefined(element))
        {
          renderer = new VisualStyleRenderer(element);
          closesize = renderer.GetPartSize(g, ThemeSizeType.Draw);
        }
        else
        {
          Font closefont = new Font("Marlett", SystemFonts.MenuFont.Size);
          closesize = TextRenderer.MeasureText("r", closefont);
          closesize.Height += SystemInformation.Border3DSize.Height * 4;
          closesize.Width += SystemInformation.Border3DSize.Width * 4;
        }
        bounds.Width -= closesize.Width;
      }

      return bounds;
    }

    public virtual Rectangle GetTabCloseRect(BpTabPage page)
    {
      if (!IncludesCloseButton(page))
        return Rectangle.Empty;
      return GetTabCloseRect(page, GetTabRect(page));
    }

    public virtual Rectangle GetTabCloseRect(BpTabPage page, Rectangle tab)
    {
      if (!IncludesCloseButton(page))
        return Rectangle.Empty;

      Graphics g = CreateGraphics();
      VisualStyleRenderer renderer;
      Rectangle bounds = new Rectangle(tab.X, tab.Y, tab.Width, tab.Height);
      VisualStyleElement element = GetElement(page);
      if (Application.RenderWithVisualStyles && VisualStyleRenderer.IsElementDefined(element))
      {
        renderer = new VisualStyleRenderer(element);
        bounds = renderer.GetBackgroundContentRectangle(g, bounds);
      }
      bounds.X += SystemInformation.Border3DSize.Width;
      bounds.Y += SystemInformation.Border3DSize.Height;
      bounds.Width -= SystemInformation.Border3DSize.Width * 2;
      bounds.Height -= SystemInformation.Border3DSize.Height * 2;

      Size closesize;
      element = VisualStyleElement.ToolTip.Close.Normal;
      if (Application.RenderWithVisualStyles && VisualStyleRenderer.IsElementDefined(element))
      {
        renderer = new VisualStyleRenderer(element);
        closesize = renderer.GetPartSize(g, ThemeSizeType.Draw);
      }
      else
      {
        Font closefont = new Font("Marlett", SystemFonts.MenuFont.Size);
        closesize = TextRenderer.MeasureText("r", closefont);
        closesize.Height += SystemInformation.Border3DSize.Height * 2;
        closesize.Width += SystemInformation.Border3DSize.Width * 2;
      }
      bounds.X = bounds.Right - closesize.Width;
      bounds.Width = closesize.Width;
      bounds.Y += (bounds.Height - closesize.Height) / 2;
      bounds.Height = closesize.Height;
      return bounds;
    }

    #endregion

		#region Protected Methods

    /// <summary>
		/// Fires the <see cref="TabChanging"/> event.
		/// </summary>
		/// <param name="tcea">
		/// Some <see cref="TabChangingEventArgs"/> for the event.
		/// </param>
		protected virtual void OnTabChanging( TabChangingEventArgs tcea )
		{
			if (TabChanging != null)
			{
				TabChanging(this, tcea);
			}
		}

		/// <summary>
		/// Fires the <see cref="TabChanged"/> event.
		/// </summary>
		/// <param name="ea">
		/// Some <see cref="EventArgs"/> for the event.
		/// </param>
		protected virtual void OnTabChanged( EventArgs ea )
		{
			if (TabChanged != null)
			{
				TabChanged(this, ea);
			}
		}

		/// <summary>
		/// Overridden. Inherited from <see cref="Control"/>.
		/// </summary>
		/// <param name="cea">
		/// See <see cref="Control.OnControlAdded(ControlEventArgs)"/>.
		/// </param>
		protected override void OnControlAdded(ControlEventArgs cea)
		{
			base.OnControlAdded(cea);
			cea.Control.Visible = false;
			if (selectedIndex == -1)
			{
				selectedIndex = 0;
				selectedTab = (BpTabPage)cea.Control;
				selectedTab.Visible = true;
			}
      CalculateRectangles();
      ((BpTabPage)cea.Control).TabChanged += tabEventHandler;
    }

		/// <summary>
		/// Overridden. Inherited from <see cref="Control"/>.
		/// </summary>
		/// <param name="cea">
		/// See <see cref="Control.OnControlRemoved(ControlEventArgs)"/>.
		/// </param>
		protected override void OnControlRemoved(ControlEventArgs cea)
		{
			base.OnControlRemoved(cea);
			if( Controls.Count > 0 )
			{
        if (cea.Control == selectedTab)
        {
          selectedIndex = Math.Min(selectedIndex, Controls.Count - 1);
          selectedTab = (BpTabPage)Controls[selectedIndex];
          selectedTab.Visible = true;
        }
        else
        {
          selectedIndex = Controls.IndexOf(selectedTab);
          selectedRow = positions[selectedTab].Row;
        }
			}
			else
			{
				selectedIndex = -1;
				selectedTab = null;
        selectedRow = -1;
			}
      if (cea.Control == hoverTab)
      {
        hoverTab = null;
        hoverClose = false;
      }
      if (cea.Control == clickClose)
        clickClose = null;
      CalculateRectangles();
      ((BpTabPage)cea.Control).TabChanged -= tabEventHandler;
		}

    /// <summary>
    /// Inherited from <see cref="Control"/>.
    /// </summary>
    /// <param name="disposing">
    /// See <see cref="Control.Dispose(bool)"/>.
    /// </param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        base.Dispose(disposing);
        foreach (Control c in Controls)
        {
          c.Dispose();
        }
      }
    }

    protected virtual void OnTabClosing(TabClosingEventArgs e)
    {
      if (TabClosing != null)
        TabClosing(this, e);

      if (!e.Cancel)
        e.TabPage.CallTabClosing(e);
    }

    protected virtual void OnTabClosed(TabEventArgs e)
    {
      if (TabClosed != null)
        TabClosed(this, e);

      e.TabPage.CallTabClosed(e);
    }

    protected virtual void OnTabMouseMove(TabMouseEventArgs e)
    {
      if (TabMouseMove != null)
        TabMouseMove(this, e);

      Rectangle closerect = GetTabCloseRect(e.TabPage, e.TabBounds);
      if (e.TabPage != hoverTab)
      {
        if (hoverTab != null)
        {
          this.OnTabMouseLeave(new TabEventArgs(hoverTab));
        }
        this.OnTabMouseEnter(new TabEventArgs(e.TabPage));
        if (closerect.Contains(e.Location))
          hoverClose = true;
      }
      else if (closerect.Contains(e.Location))
      {
        if (!hoverClose)
        {
          hoverClose = true;
          Invalidate(closerect);
          Update();
        }
      }
      else if (hoverClose)
      {
        hoverClose = false;
        Invalidate(closerect);
        Update();
      }

      Rectangle bounds = GetTabContentRect(e.TabPage, e.TabBounds);
      e.TabPage.CallTabMouseMove(new TabMouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta, e.TabPage, Bounds));
    }

    protected virtual void OnTabMouseDown(TabMouseEventArgs e)
    {
      if (TabMouseDown != null)
        TabMouseDown(this, e);

      if (IncludesCloseButton(e.TabPage))
      {
        Rectangle closerect = GetTabCloseRect(e.TabPage, e.TabBounds);
        if (closerect.Contains(e.Location))
        {
          clickClose = e.TabPage;
          Invalidate(closerect);
          Update();
          return;
        }
      }
      if (e.Button == MouseButtons.Left)
      {
        if (!e.TabPage.Disabled)
          SelectedTab = e.TabPage;
      }

      Rectangle bounds = GetTabContentRect(e.TabPage, e.TabBounds);
      e.TabPage.CallTabMouseDown(new TabMouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta, e.TabPage, Bounds));
    }

    protected virtual void OnTabMouseUp(TabMouseEventArgs e)
    {
      if (TabMouseUp != null)
        TabMouseUp(this, e);

      if (e.TabPage == clickClose)
      {
        Rectangle closerect = GetTabCloseRect(e.TabPage, e.TabBounds);
        if (closerect.Contains(e.Location))
        {
          TabClosingEventArgs tcea = new TabClosingEventArgs(e.TabPage);
          OnTabClosing(tcea);
          if (!tcea.Cancel)
            OnTabClosed(tcea);
          return;
        }
      }

      Rectangle bounds = GetTabContentRect(e.TabPage, e.TabBounds);
      e.TabPage.CallTabMouseUp(new TabMouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta, e.TabPage, Bounds));
    }

    protected virtual void OnTabMouseClick(TabMouseEventArgs e)
    {
      if (TabMouseClick != null)
        TabMouseClick(this, e);

      Rectangle bounds = GetTabContentRect(e.TabPage, e.TabBounds);
      e.TabPage.CallTabMouseClick(new TabMouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta, e.TabPage, Bounds));
    }

    protected virtual void OnTabMouseDoubleClick(TabMouseEventArgs e)
    {
      if (TabMouseDoubleClick != null)
        TabMouseDoubleClick(this, e);

      Rectangle bounds = GetTabContentRect(e.TabPage, e.TabBounds);
      e.TabPage.CallTabMouseDoubleClick(new TabMouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta, e.TabPage, Bounds));
    }

    protected virtual void OnTabMouseEnter(TabEventArgs e)
    {
      if (TabMouseEnter != null)
        TabMouseEnter(this, e);

      hoverTab = e.TabPage;
      Invalidate(GetTabRect(e.TabPage));

      e.TabPage.CallTabMouseEnter(e);
      Update();
    }

    protected virtual void OnTabMouseLeave(TabEventArgs e)
    {
      if (TabMouseLeave != null)
        TabMouseLeave(this, e);

      if (hoverTab == e.TabPage)
      {
        hoverTab = null;
        hoverClose = false;
      }
      else
        Debug.WriteLine("Mouse left non-hover tab.");

      Invalidate(GetTabRect(e.TabPage));

      e.TabPage.CallTabMouseLeave(e);
      Update();
    }

    /// <summary>
    /// Inherited from <see cref="Control"/>.
    /// </summary>
    /// <param name="e">
    /// See <see cref="Control.OnMouseMove(EventArgs)"/>.
    /// </param>
    protected override void OnMouseMove(MouseEventArgs e)
    {
      base.OnMouseMove(e);
      if (tabsRectangle.Contains(e.Location))
      {
        foreach (BpTabPage page in Controls)
        {
          Rectangle bounds = GetTabRect(page);
          if (bounds.Contains(e.Location))
          {
            this.OnTabMouseMove(new TabMouseEventArgs(e, page, bounds));
            return;
          }
        }
      }
      if (hoverTab != null)
        this.OnTabMouseLeave(new TabEventArgs(hoverTab));
    }

    /// <summary>
    /// Inherited from <see cref="Control"/>.
    /// </summary>
    /// <param name="e">
    /// See <see cref="Control.OnMouseDown(EventArgs)"/>.
    /// </param>
    protected override void OnMouseDown(MouseEventArgs e)
    {
      base.OnMouseDown(e);
      if (tabsRectangle.Contains(e.Location))
      {
        foreach (BpTabPage page in Controls)
        {
          Rectangle bounds = GetTabRect(page);
          if (bounds.Contains(e.Location))
          {
            this.OnTabMouseDown(new TabMouseEventArgs(e, page, bounds));
            break;
          }
        }
      }
    }

    /// <summary>
    /// Inherited from <see cref="Control"/>.
    /// </summary>
    /// <param name="e">
    /// See <see cref="Control.OnMouseUp(EventArgs)"/>.
    /// </param>
    protected override void OnMouseUp(MouseEventArgs e)
    {
      base.OnMouseUp(e);
      if (tabsRectangle.Contains(e.Location))
      {
        foreach (BpTabPage page in Controls)
        {
          Rectangle bounds = GetTabRect(page);
          if (bounds.Contains(e.Location))
          {
            this.OnTabMouseUp(new TabMouseEventArgs(e, page, bounds));
            break;
          }
        }
      }
      if (clickClose != null)
      {
        Invalidate(GetTabCloseRect(clickClose));
        clickClose = null;
        Update();
      }
    }

    /// <summary>
    /// Inherited from <see cref="Control"/>.
    /// </summary>
    /// <param name="e">
    /// See <see cref="Control.OnMouseClick(EventArgs)"/>.
    /// </param>
    protected override void OnMouseClick(MouseEventArgs e)
    {
      base.OnMouseClick(e);
      if (tabsRectangle.Contains(e.Location))
      {
        foreach (BpTabPage page in Controls)
        {
          Rectangle bounds = GetTabRect(page);
          if (bounds.Contains(e.Location))
          {
            this.OnTabMouseClick(new TabMouseEventArgs(e, page, bounds));
            break;
          }
        }
      }
    }

    /// <summary>
    /// Inherited from <see cref="Control"/>.
    /// </summary>
    /// <param name="e">
    /// See <see cref="Control.OnMouseDoubleClick(EventArgs)"/>.
    /// </param>
    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
      base.OnMouseDoubleClick(e);
      if (tabsRectangle.Contains(e.Location))
      {
        foreach (BpTabPage page in Controls)
        {
          Rectangle bounds = GetTabRect(page);
          if (bounds.Contains(e.Location))
          {
            this.OnTabMouseDoubleClick(new TabMouseEventArgs(e, page, bounds));
            break;
          }
        }
      }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
      base.OnMouseLeave(e);
      if (hoverTab != null)
        this.OnTabMouseLeave(new TabEventArgs(hoverTab));
    }

    protected void OnTabPaint(TabPaintEventArgs e)
    {
      if (TabPaint != null)
        TabPaint(this, e);
    }

    protected void PaintTab(BpTabPage page, Graphics g)
    {
      Rectangle bounds = GetTabRect(page);
      VisualStyleElement element = GetElement(page);
      if (Application.RenderWithVisualStyles && VisualStyleRenderer.IsElementDefined(element))
      {
        VisualStyleRenderer renderer = new VisualStyleRenderer(element);
        renderer.DrawBackground(g, bounds);
        bounds = renderer.GetBackgroundContentRectangle(g, bounds);
      }
      else
      {
        ControlPaint.DrawBorder3D(g, bounds, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Middle);
      }

      if (IncludesCloseButton(page))
      {
        Rectangle closerect = GetTabCloseRect(page, bounds);
        element = VisualStyleElement.ToolTip.Close.Normal;
        Border3DStyle borderstyle = Border3DStyle.Flat;
        if (hoverClose)
        {
          if (clickClose == page)
          {
            element = VisualStyleElement.ToolTip.Close.Pressed;
            borderstyle = Border3DStyle.Sunken;
          }
          else if (clickClose == null)
          {
            element = VisualStyleElement.ToolTip.Close.Hot;
            borderstyle = Border3DStyle.Raised;
          }
        }
        if (Application.RenderWithVisualStyles && VisualStyleRenderer.IsElementDefined(element))
        {
          VisualStyleRenderer renderer = new VisualStyleRenderer(element);
          renderer.DrawBackground(g, closerect);
        }
        else
        {
          if (borderstyle != Border3DStyle.Flat)
            ControlPaint.DrawBorder3D(g, closerect, borderstyle);
          Font closefont = new Font("Marlett", SystemFonts.MenuFont.Size);
          Point pos = closerect.Location;
          pos.X += SystemInformation.Border3DSize.Width;
          pos.Y += SystemInformation.Border3DSize.Height;
          TextRenderer.DrawText(g, "r", closefont, pos, Color.Black);
        }
      }
      
      bounds = GetTabContentRect(page, bounds);
      TabPaintEventArgs ea = new TabPaintEventArgs(g, bounds, page);
      OnTabPaint(ea);
      page.CallTabPaint(ea);
    }

    /// <summary>
		/// Inherited from <see cref="Control"/>.
		/// </summary>
		/// <param name="pea">
		/// See <see cref="Control.OnPaint(PaintEventArgs)"/>.
		/// </param>
		protected override void OnPaint(PaintEventArgs pea)
		{
      Graphics g = pea.Graphics;

      VisualStyleRenderer renderer = null;
      if (Application.RenderWithVisualStyles && VisualStyleRenderer.IsElementDefined(VisualStyleElement.Tab.Pane.Normal))
      {
        renderer = new VisualStyleRenderer(VisualStyleElement.Tab.Pane.Normal);
        renderer.DrawBackground(g, paneRectangle);
      }
      else
      {
        ControlPaint.DrawBorder3D(g, paneRectangle, Border3DStyle.Raised, Border3DSide.All);
      }
      if (pea.ClipRectangle.IntersectsWith(tabsRectangle))
      {
        foreach (BpTabPage page in Controls)
        {
          if (page != selectedTab)
            PaintTab(page, g);
        }
        PaintTab(selectedTab, g);
      }
		}

		/// <summary>
		/// Inherited from <see cref="Control"/>.
		/// </summary>
		/// <param name="e">
		/// See <see cref="Control.OnSizeChanged(EventArgs)"/>.
		/// </param>
		protected override void OnSizeChanged( EventArgs e )
		{
			base.OnSizeChanged( e );
			CalculateRectangles();
		}

		/// <summary>
		/// Overriden from <see cref="Control"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="BpTabControl.ControlCollection"/>.
		/// </returns>
		protected override System.Windows.Forms.Control.ControlCollection CreateControlsInstance()
		{
			return new BpTabControl.ControlCollection( this );
		}

		#endregion

		#region Private Methods

    /// <summary>
    /// The event handler that hears about changes to tabs.
    /// </summary>
    /// <param name="sender">The tab page changing</param>
    /// <param name="e">The event arguments</param>
    private void OnTabChanged(object sender, EventArgs e)
    {
      CalculateRectangles();
    }

    /// <summary>
    /// Gives the quick estimation of the <see cref="VisualStyleElement"/>
    /// to use for a give tab.
    /// </summary>
    /// <param name="page">The tab to return the element for.</param>
    /// <returns>The element to use when rendering the tab.</returns>
    internal VisualStyleElement GetGuessedElement(BpTabPage page)
    {
      if (page == selectedTab)
        return VisualStyleElement.Tab.TabItem.Pressed;
      else if (page.Disabled)
        return VisualStyleElement.Tab.TabItem.Disabled;
      else if (page == hoverTab)
        return VisualStyleElement.Tab.TabItem.Hot;
      else
        return VisualStyleElement.Tab.TabItem.Normal;
    }

    /// <summary>
    /// Gives the correct <see cref="VisualStyleElement"/> to use for
    /// a give tab.
    /// </summary>
    /// <param name="page">The tab to return the element for.</param>
    /// <returns>The element to use when rendering the tab.</returns>
    internal VisualStyleElement GetElement(BpTabPage page)
    {
      string className = "TAB";
      int part;
      int state;

      if (page == selectedTab)
        state = 3;
      else if (page.Disabled)
        state = 4;
      else if (page == hoverTab)
        state = 2;
      else
        state = 1;

      TabPosition position = positions[page];
      int toprow = rows - 1;
      if ((rows > 1) && (toprow == selectedRow))
        toprow--;
      
      if (rowCounts[position.Row] == 1)
        part = 1;
      else if (position.Pos == rowCounts[position.Row] - 1)
        part = 2;
      else if (position.Pos == 0)
        part = 3;
      else
        part = 4;

      if (position.Row == toprow)
        part += 4;

      return VisualStyleElement.CreateElement(className, part, state);
    }

		/// <summary>
		/// Calculates the rectangles for the tab area, the client area,
		/// the display area, and the transformed display area.
		/// </summary>
		private void CalculateRectangles()
		{
      Rectangle olddisp = displayRectangle;
      positions.Clear();
      Graphics g = this.CreateGraphics();
      if (Controls.Count > 0)
      {
        int width = Width;

        width -= tabExpand.Horizontal;
        tabHeight = 0;
        rows = 0;
        int used = 0;
        int pos = 0;
        foreach (BpTabPage page in Controls)
        {
          VisualStyleElement element;
          VisualStyleRenderer renderer;

          Size closebutton = new Size(0, 0);
          if (IncludesCloseButton(page))
          {
            element = VisualStyleElement.ToolTip.Close.Normal;
            if (Application.RenderWithVisualStyles && VisualStyleRenderer.IsElementDefined(element))
            {
              renderer = new VisualStyleRenderer(element);
              closebutton = renderer.GetPartSize(g, ThemeSizeType.Draw);
            }
            else
            {
              Font closefont = new Font("Marlett", SystemFonts.MenuFont.Size);
              closebutton = TextRenderer.MeasureText("r", closefont);
              closebutton.Height += SystemInformation.Border3DSize.Height * 2;
              closebutton.Width += SystemInformation.Border3DSize.Width * 2;
            }
          }

          Size size = page.GetPreferredTabSize(g);
          size.Height = Math.Max(size.Height, closebutton.Height);
          size.Width += closebutton.Width;
          element = GetGuessedElement(page);
          if (Application.RenderWithVisualStyles && VisualStyleRenderer.IsElementDefined(element))
          {
            renderer = new VisualStyleRenderer(element);
            Rectangle fake = new Rectangle(0, 0, size.Width, size.Height);
            fake = renderer.GetBackgroundExtent(g, fake);
            size = new Size(fake.Width, fake.Height);
          }
          size.Height += SystemInformation.Border3DSize.Height * 2;
          size.Width += SystemInformation.Border3DSize.Width * 2;

          if (size.Width > width)
            size.Width = width;
          if ((size.Width + used) > width)
          {
            rowCounts.Add(pos);
            rows++;
            used = 0;
            pos = 0;
          }
          TabPosition position = new TabPosition(rows, used, size, pos);
          positions[page] = position;
          tabHeight = Math.Max(tabHeight, size.Height);
          used += size.Width;
          pos++;
          if (page == selectedTab)
            selectedRow = rows;
        }
        rowCounts.Add(pos);
        rows++;
        tabsRectangle = new Rectangle(0, 0, Width, (rows * tabHeight) + tabExpand.Top);
        paneRectangle = new Rectangle(0, tabsRectangle.Bottom , Width, Height - tabsRectangle.Bottom);
      }
      else
      {
        paneRectangle = ClientRectangle;
        tabsRectangle = Rectangle.Empty;
      }
      if (Application.RenderWithVisualStyles && VisualStyleRenderer.IsElementDefined(VisualStyleElement.Tab.Pane.Normal))
      {
        VisualStyleRenderer renderer = new VisualStyleRenderer(VisualStyleElement.Tab.Pane.Normal);
        displayRectangle = renderer.GetBackgroundContentRectangle(g, paneRectangle);
      }
      else
      {
        Padding paneMargin = new Padding(SystemInformation.Border3DSize.Width, SystemInformation.Border3DSize.Height, SystemInformation.Border3DSize.Width, SystemInformation.Border3DSize.Height);
        displayRectangle = Rectangle.Empty;
        displayRectangle.Y = paneRectangle.Y + paneMargin.Top;
        displayRectangle.Height = paneRectangle.Height - paneMargin.Vertical;
        displayRectangle.X = paneRectangle.X + paneMargin.Left;
        displayRectangle.Width = paneRectangle.Width - paneMargin.Horizontal;
      }
      if (olddisp != displayRectangle)
      {
        Invalidate();
        PerformLayout();
        Update();
      }
      else
      {
        Rectangle badrect = tabsRectangle;
        badrect.Height += tabExpand.Bottom;
        Invalidate(badrect);
        Update();
      }
    }

    #endregion

    #region Public Inner Classes

    /// <summary>
    /// Holds details about where if the tab region a given tab lies.
    /// </summary>
    private struct TabPosition
    {
      /// <summary>
      /// The row that this tab appears in.
      /// </summary>
      public int Row;

      /// <summary>
      /// The offset from the start of the row in pixels.
      /// </summary>
      public int Offset;

      /// <summary>
      /// The position in the row.
      /// </summary>
      public int Pos;

      /// <summary>
      /// The size the tab appears at in normal state.
      /// </summary>
      public Size Size;

      public TabPosition(int row, int offset, Size size, int pos)
      {
        this.Row = row;
        this.Offset = offset;
        this.Size = size;
        this.Pos = pos;
      }
    }

		/// <summary>
		/// A <see cref="BpTabControl"/>-specific
		/// <see cref="Control.ControlCollection"/>.
		/// </summary>
		public new class ControlCollection : Control.ControlCollection
		{
			/// <summary>
			/// Creates a new instance of the
			/// <see cref="BpTabControl.ControlCollection"/> class with 
			/// the specified <i>owner</i>.
			/// </summary>
			/// <param name="owner">
			/// The <see cref="BpTabControl"/> that owns this collection.
			/// </param>
			/// <exception cref="ArgumentNullException">
			/// Thrown if <i>owner</i> is <b>null</b>.
			/// </exception>
			/// <exception cref="ArgumentException">
			/// Thrown if <i>owner</i> is not a <see cref="BpTabControl"/>.
			/// </exception>
			public ControlCollection( Control owner ) : base( owner )
			{
				if( owner == null )
				{
					throw new ArgumentNullException( "owner", "Tried to create a BpTabControl.ControlCollection with a null owner." );
				}
				this.owner = owner as BpTabControl;
				if( this.owner == null )
				{
					throw new ArgumentException( "Tried to create a BpTabControl.ControlCollection with a non-BpTabControl owner.", "owner" );
				}
			}

			/// <summary>
			/// Overridden. Adds a <see cref="Control"/> to the
			/// <see cref="BpTabControl"/>.
			/// </summary>
			/// <param name="value">
			/// The <see cref="Control"/> to add, which must be a
			/// <see cref="BpTabPage"/>.
			/// </param>
			/// <exception cref="ArgumentNullException">
			/// Thrown if <i>value</i> is <b>null</b>.
			/// </exception>
			/// <exception cref="ArgumentException">
			/// Thrown if <i>value</i> is not a <see cref="BpTabPage"/>.
			/// </exception>
			public override void Add(Control value)
			{
				if (value == null)
				{
					throw new ArgumentNullException("value", "Tried to add a null value to the BpTabControl.ControlCollection." );
				}
				BpTabPage p = value as BpTabPage;
				if (p == null)
				{
					throw new ArgumentException("Tried to add a non-BpTabPage control to the BpTabControl.ControlCollection.", "value");
				}
				p.SendToBack();
				base.Add(p);
			}

			/// <summary>
			/// Overridden. Inherited from <see cref="Control.ControlCollection.Remove( Control )"/>.
			/// </summary>
			/// <param name="value"></param>
			public override void Remove( Control value )
			{
				base.Remove( value );
			}

			/// <summary>
			/// The owner of this <see cref="BpTabControl.ControlCollection"/>.
			/// </summary>
			private BpTabControl owner;
		}

		#endregion
	}
}
