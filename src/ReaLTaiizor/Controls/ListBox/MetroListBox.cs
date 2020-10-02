﻿#region Imports

using System;
using System.Drawing;
using ReaLTaiizor.Native;
using ReaLTaiizor.Manager;
using System.Drawing.Text;
using System.Windows.Forms;
using System.ComponentModel;
using ReaLTaiizor.Enum.Metro;
using ReaLTaiizor.Child.Metro;
using ReaLTaiizor.Design.Metro;
using System.Collections.Generic;
using ReaLTaiizor.Extension.Metro;
using ReaLTaiizor.Interface.Metro;
using System.Runtime.InteropServices;

#endregion

namespace ReaLTaiizor.Controls
{
	#region MetroListBox

	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(MetroListBox), "Bitmaps.ListBox.bmp")]
	[Designer(typeof(MetroListBoxDesigner))]
	[DefaultProperty("Items")]
	[DefaultEvent("SelectedIndexChanged")]
	[ComVisible(true)]
	public class MetroListBox : Control, IMetroControl
	{
		#region Interfaces

		[Category("Metro"), Description("Gets or sets the style associated with the control.")]
		public Style Style
		{
			get => StyleManager?.Style ?? _style;
			set
			{
				_style = value;
				switch (value)
				{
					case Style.Light:
						ApplyTheme();
						break;
					case Style.Dark:
						ApplyTheme(Style.Dark);
						break;
					case Style.Custom:
						ApplyTheme(Style.Custom);
						break;
					default:
						ApplyTheme();
						break;
				}
				_svs.Style = value;
				Invalidate();
			}
		}

		[Category("Metro"), Description("Gets or sets the Style Manager associated with the control.")]
		public MetroStyleManager StyleManager
		{
			get => _styleManager;
			set
			{
				_styleManager = value;
				Invalidate();
			}
		}

		[Category("Metro"), Description("Gets or sets the The Author name associated with the theme.")]
		public string ThemeAuthor { get; set; }

		[Category("Metro"), Description("Gets or sets the The Theme name associated with the theme.")]
		public string ThemeName { get; set; }

		#endregion Interfaces

		#region Global Vars

		private readonly Utilites _utl;

		#endregion Global Vars

		#region Internal Vars

		private Style _style;
		private MetroStyleManager _styleManager;
		private MetroItemCollection _items;
		private List<object> _selectedItems;
		private List<object> _indicates;
		private bool _multiSelect;
		private int _selectedIndex;
		private object _selectedItem;
		private string _selectedText;
		private bool _showScrollBar;
		private bool _multiKeyDown;
		private int _hoveredItem;
		private MetroScrollBar _svs;
		private object _selectedValue;

		private bool _isDerivedStyle = true;
		private int _itemHeight;
		private bool _showBorder;
		private Color _selectedItemColor;
		private Color _selectedItemBackColor;
		private Color _hoveredItemColor;
		private Color _hoveredItemBackColor;
		private Color _disabledForeColor;
		private Color _disabledBackColor;
		private Color _borderColor;

		#endregion Internal Vars

		#region Constructors

		public MetroListBox()
		{
			SetStyle
			(
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.Selectable |
				ControlStyles.ResizeRedraw |
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.SupportsTransparentBackColor,
					true
			);
			UpdateStyles();
			base.BackColor = Color.Transparent;
			base.Font = MetroFonts.Light(10);
			_utl = new Utilites();
			ApplyTheme();
			SetDefaults();
		}

		private void SetDefaults()
		{
			SelectedIndex = -1;
			_hoveredItem = -1;
			_showScrollBar = false;
			_items = new MetroItemCollection();
			_items.ItemUpdated += InvalidateScroll;
			_selectedItems = new List<object>();
			_indicates = new List<object>();
			ItemHeight = 30;
			_multiKeyDown = false;
			_svs = new MetroScrollBar()
			{
				Orientation = ScrollOrientate.Vertical,
				Size = new Size(12, Height),
				Maximum = _items.Count * ItemHeight,
				SmallChange = 1,
				LargeChange = 5
			};
			_svs.Scroll += HandleScroll;
			_svs.MouseDown += VS_MouseDown;
			_svs.BackColor = Color.Transparent;
			if (!Controls.Contains(_svs))
			{
				Controls.Add(_svs);
			}
		}

		#endregion Constructors

		#region ApplyTheme

		private void ApplyTheme(Style style = Style.Light)
		{
			if (!IsDerivedStyle)
				return;

			switch (style)
			{
				case Style.Light:
					ForeColor = Color.Black;
					BackColor = Color.White;
					SelectedItemBackColor = Color.FromArgb(65, 177, 225);
					SelectedItemColor = Color.White;
					HoveredItemColor = Color.DimGray;
					HoveredItemBackColor = Color.LightGray;
					DisabledBackColor = Color.FromArgb(204, 204, 204);
					DisabledForeColor = Color.FromArgb(136, 136, 136);
					BorderColor = Color.LightGray;
					ThemeAuthor = "Taiizor";
					ThemeName = "MetroLight";
					UpdateProperties();
					break;
				case Style.Dark:
					ForeColor = Color.FromArgb(170, 170, 170);
					BackColor = Color.FromArgb(30, 30, 30);
					SelectedItemBackColor = Color.FromArgb(65, 177, 225);
					SelectedItemColor = Color.White;
					HoveredItemColor = Color.DimGray;
					HoveredItemBackColor = Color.LightGray;
					DisabledBackColor = Color.FromArgb(80, 80, 80);
					DisabledForeColor = Color.FromArgb(109, 109, 109);
					BorderColor = Color.FromArgb(64, 64, 64);
					ThemeAuthor = "Taiizor";
					ThemeName = "MetroDark";
					UpdateProperties();
					break;
				case Style.Custom:
					if (StyleManager != null)
						foreach (var varkey in StyleManager.ListBoxDictionary)
						{
							switch (varkey.Key)
							{
								case "ForeColor":
									ForeColor = _utl.HexColor((string)varkey.Value);
									break;
								case "BackColor":
									BackColor = _utl.HexColor((string)varkey.Value);
									break;
								case "DisabledBackColor":
									DisabledBackColor = _utl.HexColor((string)varkey.Value);
									break;
								case "DisabledForeColor":
									DisabledForeColor = _utl.HexColor((string)varkey.Value);
									break;
								case "HoveredItemBackColor":
									HoveredItemBackColor = _utl.HexColor((string)varkey.Value);
									break;
								case "HoveredItemColor":
									HoveredItemColor = _utl.HexColor((string)varkey.Value);
									break;
								case "SelectedItemBackColor":
									SelectedItemBackColor = _utl.HexColor((string)varkey.Value);
									break;
								case "SelectedItemColor":
									SelectedItemColor = _utl.HexColor((string)varkey.Value);
									break;
								case "BorderColor":
									BorderColor = _utl.HexColor((string)varkey.Value);
									break;
								default:
									return;
							}
						}
					UpdateProperties();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(style), style, null);
			}
		}

		private void UpdateProperties()
		{
			Invalidate();
		}

		#endregion ApplyTheme

		#region Draw Control

		protected override void OnPaint(PaintEventArgs e)
		{
			var g = e.Graphics;
			g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			var mainRect = new Rectangle(0, 0, Width - (ShowBorder ? 1 : 0), Height - (ShowBorder ? 1 : 0));

			using (var bg = new SolidBrush(Enabled ? BackColor : DisabledBackColor))
			{
				using (var usic = new SolidBrush(Enabled ? ForeColor : DisabledForeColor))
				{
					using (var sic = new SolidBrush(SelectedItemColor))
					{
						using (var sibc = new SolidBrush(SelectedItemBackColor))
						{
							using (var hic = new SolidBrush(HoveredItemColor))
							{
								using (var hibc = new SolidBrush(HoveredItemBackColor))
								{
									using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
									{
										var firstItem = _svs.Value / ItemHeight < 0 ? 0 : _svs.Value / ItemHeight;
										var lastItem = _svs.Value / ItemHeight + Height / ItemHeight + 1 > Items.Count ? Items.Count : _svs.Value / ItemHeight + Height / ItemHeight + 1;

										g.FillRectangle(bg, mainRect);

										for (var i = firstItem; i < lastItem; i++)
										{
											var itemText = (string)Items[i];

											var rect = new Rectangle(5, (i - firstItem) * ItemHeight, Width - 1, ItemHeight);
											g.DrawString(itemText, Font, usic, rect, sf);
											if (MultiSelect && _indicates.Count != 0)
											{
												if (i == _hoveredItem && !_indicates.Contains(i))
												{
													g.FillRectangle(hibc, rect);
													g.DrawString(itemText, Font, hic, rect, sf);
												}
												else if (_indicates.Contains(i))
												{
													g.FillRectangle(sibc, rect);
													g.DrawString(itemText, Font, sic, rect, sf);
												}
											}
											else
											{
												if (i == _hoveredItem && i != SelectedIndex)
												{
													g.FillRectangle(hibc, rect);
													g.DrawString(itemText, Font, hic, rect, sf);
												}
												else if (i == SelectedIndex)
												{
													g.FillRectangle(sibc, rect);
													g.DrawString(itemText, Font, sic, rect, sf);
												}
											}

										}
										if (ShowBorder)
											g.DrawRectangle(Pens.LightGray, mainRect);
									}
								}
							}
						}
					}
				}
			}
		}

		#endregion Draw Control

		#region Properties

		[TypeConverter(typeof(CollectionConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
		[Category("Metro"), Description("Gets the items of the ListBox.")]
		public MetroItemCollection Items => _items;

		[Browsable(false)]
		[Category("Metro"), Description("Gets a collection containing the currently selected items in the ListBox.")]
		public List<object> SelectedItems => _selectedItems;

		[Category("Metro"), Description("Gets or sets the height of an item in the ListBox.")]
		public int ItemHeight
		{
			get { return _itemHeight; }
			set
			{
				_itemHeight = value;
				Refresh();
			}
		}

		[Browsable(false), Category("Metro"), Description("Gets or sets the currently selected item in the ListBox.")]
		public object SelectedItem
		{
			get => _selectedItem;
			set
			{
				_selectedItem = value;
				Invalidate();
			}
		}

		[Browsable(false), Category("Metro"),
		 Description("Gets or sets the currently selected Text in the ListBox.")]
		public string SelectedText
		{
			get => _selectedText;
			set
			{
				_selectedText = value;
				Invalidate();
			}
		}

		[Browsable(false), Category("Metro"), Description("Gets or sets the zero-based index of the currently selected item in a ListBox.")]
		public int SelectedIndex
		{
			get => _selectedIndex;
			set
			{
				_selectedIndex = value;
				Invalidate();
			}
		}

		[Browsable(true), Category("Metro"), Description("Gets or sets the value of the member property specified by the ValueMember property.")]
		public object SelectedValue
		{
			get => _selectedValue;
			set
			{
				_selectedValue = value;
				Invalidate();
			}
		}

		[Category("Metro"), Description("Gets or sets a value indicating whether the ListBox supports multiple rows.")]
		public bool MultiSelect
		{
			get => _multiSelect;
			set
			{
				_multiSelect = value;

				if (_selectedItems.Count > 1)
					_selectedItems.RemoveRange(1, _selectedItems.Count - 1);

				Invalidate();
			}
		}

		[Browsable(false)]
		public int Count => _items.Count;

		[Category("Metro"), Description("Gets or sets a value indicating whether the vertical scroll bar be shown or not.")]
		public bool ShowScrollBar
		{
			get => _showScrollBar;
			set
			{
				_showScrollBar = value;
				_svs.Visible = value;
				Invalidate();
			}
		}

		[Category("Metro"), Description("Gets or sets a value indicating whether the border shown or not.")]
		public bool ShowBorder
		{
			get { return _showBorder; }
			set
			{
				_showBorder = value;
				Refresh();
			}
		}

		[Category("Metro"), Description("Gets or sets backcolor used by the control.")]
		public override Color BackColor { get; set; }

		[Category("Metro"), Description("Gets or sets forecolor used by the control.")]
		public override Color ForeColor { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string Text { get => base.Text; set => base.Text = value; }

		[Category("Metro"), Description("Gets or sets selected item used by the control.")]
		public Color SelectedItemColor
		{
			get { return _selectedItemColor; }
			set
			{
				_selectedItemColor = value;
				Refresh();
			}
		}

		[Category("Metro"), Description("Gets or sets selected item backcolor used by the control.")]
		public Color SelectedItemBackColor
		{
			get { return _selectedItemBackColor; }
			set
			{
				_selectedItemBackColor = value;
				Refresh();
			}
		}

		[Category("Metro"), Description("Gets or sets hovered item used by the control.")]
		public Color HoveredItemColor
		{
			get { return _hoveredItemColor; }
			set
			{
				_hoveredItemColor = value;
				Refresh();
			}
		}

		[Category("Metro"), Description("Gets or sets hovered item backcolor used by the control.")]
		public Color HoveredItemBackColor
		{
			get { return _hoveredItemBackColor; }
			set
			{
				_hoveredItemBackColor = value;
				Refresh();
			}
		}

		[Category("Metro"), Description("Gets or sets disabled forecolor used by the control.")]
		public Color DisabledForeColor
		{
			get { return _disabledForeColor; }
			set
			{
				_disabledForeColor = value;
				Refresh();
			}
		}

		[Category("Metro"), Description("Gets or sets disabled backcolor used by the control.")]
		public Color DisabledBackColor
		{
			get { return _disabledBackColor; }
			set
			{
				_disabledBackColor = value;
				Refresh();
			}
		}

		[Category("Metro"), Description("Gets or sets border color used by the control.")]
		public Color BorderColor
		{
			get { return _borderColor; }
			set
			{
				_borderColor = value;
				Refresh();
			}
		}

		[Category("Metro")]
		[Description("Gets or sets the whether this control reflect to parent(s) style. \n " +
					 "Set it to false if you want the style of this control be independent. ")]
		public bool IsDerivedStyle
		{
			get { return _isDerivedStyle; }
			set
			{
				_isDerivedStyle = value;
				Refresh();
			}
		}

		#endregion Properties

		#region Methods

		public void AddItem(string newItem)
		{
			_items.Add(newItem);
			InvalidateScroll(this, null);
		}

		public void AddItems(string[] newItems)
		{
			foreach (var str in newItems)
				AddItem(str);
			InvalidateScroll(this, null);
		}

		public void RemoveItemAt(int index)
		{
			_items.RemoveAt(index);
			InvalidateScroll(this, null);
		}

		public void RemoveItem(string item)
		{
			_items.Remove(item);
			InvalidateScroll(this, null);
		}

		public int IndexOf(string value)
		{
			return _items.IndexOf(value);
		}

		public bool Contains(object item)
		{
			return _items.Contains(item.ToString());
		}

		public void RemoveItems(string[] itemsToRemove)
		{
			foreach (var item in itemsToRemove)
				_items.Remove(item);
			InvalidateScroll(this, null);
		}

		public void Clear()
		{
			for (var i = _items.Count - 1; i >= 0; i += -1)
				_items.RemoveAt(i);
			InvalidateScroll(this, null);
		}

		#endregion Methods

		#region Events

		public event SelectedIndexChangedEventHandler SelectedIndexChanged;

		public delegate void SelectedIndexChangedEventHandler(object sender);

		public event SelectedValueEventHandler SelectedValueChanged;

		public delegate void SelectedValueEventHandler(object sender);

		protected override void OnSizeChanged(EventArgs e)
		{
			InvalidateScroll(this, e);
			InvalidateLayout();
			base.OnSizeChanged(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			Focus();
			if (e.Button == MouseButtons.Left)
			{
				var index = _svs.Value / ItemHeight + e.Location.Y / ItemHeight;
				if (index >= 0 && index < _items.Count)
				{
					if (MultiSelect && _multiKeyDown)
					{
						_indicates.Add(index);
						_selectedItems.Add(Items[index]);
					}
					else
					{
						_indicates.Clear();
						_selectedItems.Clear();
						_selectedItem = Items[index];
						_selectedIndex = index;
						_selectedValue = Items[index];
						_selectedText = Items[index].ToString();
						SelectedIndexChanged?.Invoke(this);
						SelectedValueChanged?.Invoke(this);
					}
				}
				Invalidate();
			}
			base.OnMouseDown(e);
		}

		private void HandleScroll(object sender)
		{
			Invalidate();
		}

		private void InvalidateScroll(object sender, EventArgs e)
		{
			_svs.Maximum = _items.Count * ItemHeight;
			Invalidate();
		}

		private void VS_MouseDown(object sender, MouseEventArgs e)
		{
			Focus();
		}

		private void InvalidateLayout()
		{
			_svs.Size = new Size(12, Height - (ShowBorder ? 2 : 0));
			_svs.Location = new Point(Width - (_svs.Width + (ShowBorder ? 2 : 0)), ShowBorder ? 1 : 0);
			Invalidate();
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			_svs.Value -= e.Delta / 4;
			base.OnMouseWheel(e);
		}

		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Down:
					try
					{
						_selectedItems.Remove(_items[SelectedIndex]);
						SelectedIndex += 1;
						_selectedItems.Add(_items[SelectedIndex]);
					}
					catch
					{
						//
					}
					break;

				case Keys.Up:
					try
					{
						_selectedItems.Remove(_items[SelectedIndex]);
						SelectedIndex -= 1;
						_selectedItems.Add(_items[SelectedIndex]);
					}
					catch
					{
						//
					}
					break;
			}
			Invalidate();
			return base.IsInputKey(keyData);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			Cursor = Cursors.Hand;
			var index = _svs.Value / ItemHeight + e.Location.Y / ItemHeight;

			if (index >= Items.Count)
				index = -1;

			if (index >= 0 && index < Items.Count)
				_hoveredItem = index;
			Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			_hoveredItem = -1;
			Cursor = Cursors.Default;
			Invalidate();
			base.OnMouseLeave(e);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			_svs.Location = new Point(Width - (_svs.Width + (ShowBorder ? 2 : 0)), ShowBorder ? 1 : 0);
			InvalidateScroll(this, e);
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == User32.WM_SETCURSOR)
			{
				User32.SetCursor(User32.LoadCursor(IntPtr.Zero, User32.IDC_HAND));
				m.Result = IntPtr.Zero;
				return;
			}
			base.WndProc(ref m);
		}

		#endregion Events

	}

	#endregion
}