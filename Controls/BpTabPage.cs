using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Diagnostics;
using System.Drawing;

namespace BlueprintIT.Controls
{
	/// <summary>
	/// Summary description for BpTabPage.
	/// </summary>
	[Designer(typeof(BlueprintIT.Controls.Design.BpTabPageDesigner))]
	public class BpTabPage : ContainerControl
  {
    #region Private Members

    /// <summary>
    /// The index of the image to use for the tab that represents this
    /// <see cref="BpTabPage"/>.
    /// </summary>
    private int imgIndex = -1;

    /// <summary>
    /// Indicates if this tab is disabled and unselectable.
    /// </summary>
    private bool disabled = false;

    /// <summary>
    /// The padding between icon and text.
    /// </summary>
    private int padding = 2;

    #endregion

    #region Constructor

    /// <summary>
		/// Creates an instance of the <see cref="BpTabPage"/> class.
		/// </summary>
		public BpTabPage()
		{
      SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
      base.Dock = DockStyle.Fill;
      base.BackColor = SystemColors.Window;
    }

    #endregion

    #region Public Events

    public event TabMouseEventHandler TabMouseMove;

    public event TabMouseEventHandler TabMouseDown;

    public event TabMouseEventHandler TabMouseUp;

    public event TabMouseEventHandler TabMouseClick;

    public event TabMouseEventHandler TabMouseDoubleClick;

    public event TabEventHandler TabMouseEnter;

    public event TabEventHandler TabMouseLeave;

    public event TabPaintEventHandler TabPaint;

    public event EventHandler TabChanged;

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets or sets the index to the image displayed on this tab.
    /// </summary>
    /// <value>
    /// The zero-based index to the image in the <see cref="BpTabControl.ImageList"/>
    /// that appears on the tab. The default is -1, which signifies no image.
    /// </value>
    /// <exception cref="ArgumentException">
    /// The <see cref="ImageIndex"/> value is less than -1.
    /// </exception>
    [DefaultValue(-1)]
    public virtual int ImageIndex
    {
      get
      {
        return imgIndex;
      }
      set
      {
        if (imgIndex == value)
          return;
        imgIndex = value;
        OnTabChanged(new EventArgs());
      }
    }

    /// <summary>
    /// Gets or sets the disabled state of this tab.
    /// </summary>
    [DefaultValue(false)]
    public virtual bool Disabled
    {
      get
      {
        return disabled;
      }
      set
      {
        if (disabled == value)
          return;
        disabled = value;
        OnTabChanged(new EventArgs());
      }
    }

    /// <summary>
    /// Gets or sets the padding between the tab icon and the text.
    /// </summary>
    [DefaultValue(2)]
    public virtual int ImagePadding
    {
      get
      {
        return padding;
      }
      set
      {
        if (padding == value)
          return;
        padding = value;
        OnTabChanged(new EventArgs());
      }
    }

    /// <summary>
		/// Overridden from <see cref="Panel"/>.
		/// </summary>
		/// <remarks>
		/// Since the <see cref="BpTabPage"/> exists only
		/// in the context of a <see cref="BpTabControl"/>,
		/// it makes sense to always have it fill the
		/// <see cref="BpTabControl"/>. Hence, this property
		/// will always return <see cref="DockStyle.Fill"/>
		/// regardless of how it is set.
		/// </remarks>
    [DefaultValue(DockStyle.Fill)]
		public override DockStyle Dock
		{
			get
			{
				return base.Dock;
			}
			set
			{
				base.Dock = DockStyle.Fill;
			}
		}

		/// <summary>
		/// Label for the tab
		/// </summary>
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
        if (base.Text == value)
          return;
        Debug.WriteLine("TextChange");
				base.Text = value;
        OnTabChanged(new EventArgs());
			}
    }

    #endregion

    #region Public Methods

    public Size GetMinimumTabSize(Graphics g)
    {
      return new Size(0, 0);
    }

    public Size GetPreferredTabSize(Graphics g)
    {
      if (Text == "")
        return new Size(0, 0);
      BpTabControl tabs = (BpTabControl)Parent;
      Size size = GetTabTextExtent(g, Text);
      if ((imgIndex >= 0) && (imgIndex < tabs.ImageList.Images.Count))
      {
        size.Height = Math.Max(size.Height, tabs.ImageList.ImageSize.Height);
        size.Width += padding + tabs.ImageList.ImageSize.Width;
      }
      return size;
    }

    #endregion

    #region Protected Methods

    protected virtual Size GetTabTextExtent(Graphics g, string text)
    {
      BpTabControl tabs = (BpTabControl)Parent;
      VisualStyleElement element = tabs.GetGuessedElement(this);
      /*if (Application.RenderWithVisualStyles && VisualStyleRenderer.IsElementDefined(element))
      {
        VisualStyleRenderer renderer = new VisualStyleRenderer(element);
        Rectangle bounds = renderer.GetTextExtent(g, text, TextFormatFlags.Default);
        return new Size(bounds.Width, bounds.Height);
      }
      else*/
      {
        Font font = SystemFonts.MenuFont;
        return TextRenderer.MeasureText(text, font);
      }
    }

    protected virtual void DrawTabText(Graphics g, Rectangle bounds, string text, TextFormatFlags format)
    {
      BpTabControl tabs = (BpTabControl)Parent;
      VisualStyleElement element = tabs.GetElement(this);
      Font font = SystemFonts.MenuFont;
      Color color;
      if (Disabled)
        color = SystemColors.GrayText;
      else
        color = SystemColors.MenuText;

      TextRenderer.DrawText(g, text, font, bounds, color, format);
    }

    protected virtual void OnTabChanged(EventArgs e)
    {
      if (TabChanged != null)
        TabChanged(this, e);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
      if (Application.RenderWithVisualStyles && VisualStyleRenderer.IsElementDefined(VisualStyleElement.Tab.Body.Normal))
      {
        VisualStyleRenderer renderer = new VisualStyleRenderer(VisualStyleElement.Tab.Body.Normal);
        renderer.DrawBackground(e.Graphics, ClientRectangle);
      }
      else
        base.OnPaintBackground(e);
    }

    /// <summary>
    /// Overriden from <see cref="Control"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="BpTabPage.ControlCollection"/>.
    /// </returns>
    protected override System.Windows.Forms.Control.ControlCollection CreateControlsInstance()
    {
      return new BpTabPage.ControlCollection(this);
    }

    protected virtual void OnTabMouseMove(TabMouseEventArgs e)
    {
      if (TabMouseMove != null)
        TabMouseMove(this, e);
    }

    protected virtual void OnTabMouseDown(TabMouseEventArgs e)
    {
      if (TabMouseDown != null)
        TabMouseDown(this, e);
    }

    protected virtual void OnTabMouseUp(TabMouseEventArgs e)
    {
      if (TabMouseUp != null)
        TabMouseUp(this, e);
    }

    protected virtual void OnTabMouseClick(TabMouseEventArgs e)
    {
      if (TabMouseClick != null)
        TabMouseClick(this, e);
    }

    protected virtual void OnTabMouseDoubleClick(TabMouseEventArgs e)
    {
      if (TabMouseDoubleClick != null)
        TabMouseDoubleClick(this, e);
    }

    protected virtual void OnTabMouseEnter(TabEventArgs e)
    {
      if (TabMouseEnter != null)
        TabMouseEnter(this, e);
    }

    protected virtual void OnTabMouseLeave(TabEventArgs e)
    {
      if (TabMouseLeave != null)
        TabMouseLeave(this, e);
    }

    protected virtual void OnTabPaint(TabPaintEventArgs e)
    {
      BpTabControl tabs = (BpTabControl)Parent;
      Rectangle bounds = e.ClipRectangle;
      if ((imgIndex >= 0) && (imgIndex < tabs.ImageList.Images.Count))
      {
        int y = bounds.Y;
        y += (bounds.Height - tabs.ImageList.ImageSize.Height) / 2;
        tabs.ImageList.Draw(e.Graphics, bounds.X, y, imgIndex);
        bounds = new Rectangle(bounds.X + tabs.ImageList.ImageSize.Width + padding, bounds.Y, bounds.Width - tabs.ImageList.ImageSize.Width - padding, bounds.Height);
      }
      DrawTabText(e.Graphics, bounds, Text,
        TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
      if (TabPaint != null)
        TabPaint(this, e);
    }

    #endregion

    #region Internal Event Hooks

    internal void CallTabMouseMove(TabMouseEventArgs e)
    {
      OnTabMouseMove(e);
    }

    internal void CallTabMouseDown(TabMouseEventArgs e)
    {
      OnTabMouseDown(e);
    }

    internal void CallTabMouseUp(TabMouseEventArgs e)
    {
      OnTabMouseUp(e);
    }

    internal void CallTabMouseClick(TabMouseEventArgs e)
    {
      OnTabMouseClick(e);
    }

    internal void CallTabMouseDoubleClick(TabMouseEventArgs e)
    {
      OnTabMouseDoubleClick(e);
    }

    internal void CallTabMouseEnter(TabEventArgs e)
    {
      OnTabMouseEnter(e);
    }

    internal void CallTabMouseLeave(TabEventArgs e)
    {
      OnTabMouseLeave(e);
    }

    internal void CallTabPaint(TabPaintEventArgs e)
    {
      OnTabPaint(e);
    }

    #endregion

    #region Inner Classes

    /// <summary>
		/// A <see cref="BpTabPage"/>-specific
		/// <see cref="Control.ControlCollection"/>.
		/// </summary>
		public new class ControlCollection : Control.ControlCollection
		{
			/// <summary>
			/// Creates a new instance of the
			/// <see cref="BpTabPage.ControlCollection"/> class with 
			/// the specified <i>owner</i>.
			/// </summary>
			/// <param name="owner">
			/// The <see cref="BpTabPage"/> that owns this collection.
			/// </param>
			/// <exception cref="ArgumentNullException">
			/// Thrown if <i>owner</i> is <b>null</b>.
			/// </exception>
			/// <exception cref="ArgumentException">
			/// Thrown if <i>owner</i> is not a <see cref="BpTabPage"/>.
			/// </exception>
			public ControlCollection( Control owner ) : base( owner )
			{
				if( owner == null )
				{
					throw new ArgumentNullException( "owner", "Tried to create a BpTabPage.ControlCollection with a null owner." );
				}
				BpTabPage c = owner as BpTabPage;
				if( c == null )
				{
					throw new ArgumentException( "Tried to create a BpTabPage.ControlCollection with a non-BpTabPage owner.", "owner" );
				}
			}

			/// <summary>
			/// Overridden. Adds a <see cref="Control"/> to the
			/// <see cref="BpTabPage"/>.
			/// </summary>
			/// <param name="value">
			/// The <see cref="Control"/> to add, which must not be a
			/// <see cref="BpTabPage"/>.
			/// </param>
			/// <exception cref="ArgumentNullException">
			/// Thrown if <i>value</i> is <b>null</b>.
			/// </exception>
			/// <exception cref="ArgumentException">
			/// Thrown if <i>value</i> is a <see cref="BpTabPage"/>.
			/// </exception>
			public override void Add( Control value )
			{
				if( value == null )
				{
					throw new ArgumentNullException( "value", "Tried to add a null value to the BpTabPage.ControlCollection." );
				}
				if (value is BpTabPage)
				{
					throw new ArgumentException( "Tried to add a BpTabPage control to the BpTabPage.ControlCollection.", "value" );
				}
				base.Add( value );
			}
    }

    #endregion
  }
}
