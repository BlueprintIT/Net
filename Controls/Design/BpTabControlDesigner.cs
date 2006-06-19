using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace BlueprintIT.Controls.Design
{
	/// <summary>
	/// Provides a custom <see cref="ControlDesigner"/> for the
	/// <see cref="BpTabControl"/>.
	/// </summary>
	public class BpTabControlDesigner : ControlDesigner
	{
		/// <summary>
		/// Creates an instance of the <see cref="BpTabControlDesigner"/> class.
		/// </summary>
		public BpTabControlDesigner() {}

		/// <summary>
		/// Overridden. Inherited from <see cref="ControlDesigner"/>.
		/// </summary>
		/// <param name="component">
		/// The <see cref="IComponent"/> to which this designer gets attached.
		/// </param>
		/// <remarks>
		/// This designer exists exclusively for <see cref="BpTabControl"/>s. If
		/// <i>component</i> does not inherit from <see cref="BpTabControl"/>,
		/// then this method throws an <see cref="ArgumentException"/>.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Thrown if this designer gets used with a class that does not
		/// inherit from <see cref="BpTabControl"/>.
		/// </exception>
		public override void Initialize(IComponent component)
		{
			ytc = component as BpTabControl;
			if( ytc == null )
			{
				this.DisplayError(new ArgumentException("Tried to use the BpTabControlDesigner with a class that does not inherit from BpTabControl.", "component"));
				return;
			}
			base.Initialize(component);
		}

		/// <summary>
		/// Overridden. Inherited from <see cref="ControlDesigner"/>.
		/// </summary>
		public override DesignerVerbCollection Verbs
		{
			get
			{
				if( verbs == null )
				{
					verbs = new DesignerVerbCollection();
					verbs.Add( new DesignerVerb( "Add Tab", new EventHandler( AddTab ) ) );
					verbs.Add( new DesignerVerb( "Remove Tab", new EventHandler( RemoveTab ) ) );
				}
				return verbs;
			}
		}

		/// <summary>
		/// Overridden. Inherited from <see cref="ControlDesigner"/>.
		/// </summary>
		/// <param name="m">
		/// The message.
		/// </param>
		/// <remarks>
		/// Checks for WM_LBUTTONDOWN events and uses that to
		/// select the appropriate tab in the <see cref="BpTabControl"/>.
		/// </remarks>
		protected override void WndProc(ref Message m)
		{
			try
			{
				int x = 0;
				int y = 0;
				if (ytc.Created && m.HWnd == ytc.Handle)
				{
					switch (m.Msg)
					{
						case WM_LBUTTONDOWN:
							x = (m.LParam.ToInt32() << 16) >> 16;
							y = m.LParam.ToInt32() >> 16;
							int oi = ytc.SelectedIndex;
							for (int i = 0; i < ytc.Controls.Count; i++)
							{
								Rectangle r = ytc.GetTabRect(i);
								if (r.Contains(x, y))
								{
									ytc.SelectedIndex = i;
									RaiseComponentChanging(TypeDescriptor.GetProperties(Control)["SelectedIndex"]);
									RaiseComponentChanged(TypeDescriptor.GetProperties(Control)["SelectedIndex"], oi, i);
									break;
								}
							}
							break;
					}
				}
			}
			finally
			{
				base.WndProc(ref m);
			}
		}

		/// <summary>
		/// Overridden. Inherited from <see cref="IDesigner.DoDefaultAction()"/>.
		/// </summary>
		public override void DoDefaultAction() {}

		/// <summary>
		/// Id for the WM_LBUTTONDOWN message.
		/// </summary>
		private const int WM_LBUTTONDOWN = 0x0201;

		/// <summary>
		/// Id for the WM_LBUTTONDBLCLICK message.
		/// </summary>
		private const int WM_LBUTTONDBLCLK = 0x0203;
		
		/// <summary>
		/// Event handler for the "Add Tab" verb.
		/// </summary>
		/// <param name="sender">
		/// The sender.
		/// </param>
		/// <param name="ea">
		/// Some <see cref="EventArgs"/>.
		/// </param>
		private void AddTab( object sender, EventArgs ea )
		{
			IDesignerHost dh = ( IDesignerHost ) GetService( typeof( IDesignerHost ) );
			if( dh != null )
			{
				int i = ytc.SelectedIndex;
				string name = GetNewTabName();
				BpTabPage ytp = dh.CreateComponent(typeof(BpTabPage), name) as BpTabPage;
				ytp.Text = name;
				ytc.Controls.Add( ytp );
				ytc.SelectedTab = ytp;
				RaiseComponentChanging( TypeDescriptor.GetProperties( Control )[ "SelectedIndex" ] );
				RaiseComponentChanged( TypeDescriptor.GetProperties( Control )[ "SelectedIndex" ], i, ytc.SelectedIndex );
			}
		}

		/// <summary>
		/// Event handler for the "Remove Tab" verb.
		/// </summary>
		/// <param name="sender">
		/// The sender.
		/// </param>
		/// <param name="ea">
		/// Some <see cref="EventArgs"/>.
		/// </param>
		private void RemoveTab( object sender, EventArgs ea )
		{
			IDesignerHost dh = ( IDesignerHost ) GetService( typeof( IDesignerHost ) );
			if( dh != null )
			{
				int i = ytc.SelectedIndex;
				if( i > -1 )
				{
					BpTabPage ytp = ytc.SelectedTab;
					ytc.Controls.Remove( ytp );
					dh.DestroyComponent( ytp );
					RaiseComponentChanging( TypeDescriptor.GetProperties( Control )[ "SelectedIndex" ] );
					RaiseComponentChanged( TypeDescriptor.GetProperties( Control )[ "SelectedIndex" ], i, 0 );
				}
			}
		}

		/// <summary>
		/// Gets a new tab name for the a tab.
		/// </summary>
		/// <returns></returns>
		private string GetNewTabName()
		{
			int i = 1;
			Hashtable h = new Hashtable( ytc.Controls.Count );
			foreach( Control c in ytc.Controls )
			{
				h[ c.Name ] = null;
			}
			while( h.ContainsKey( "tabPage" + i ) )
			{
				i++;
			}
			return "tabPage" + i;
		}

		/// <summary>
		/// Contains the verbs used to modify the <see cref="BpTabControl"/>.
		/// </summary>
		private DesignerVerbCollection verbs;

		/// <summary>
		/// Contains a cast reference to the <see cref="BpTabControl"/> that
		/// this designer handles.
		/// </summary>
		private BpTabControl ytc;
	}
}
