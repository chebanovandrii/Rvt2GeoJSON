using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Architexor.GeoJSON.Base;
using Autodesk.Revit.DB;
using Microsoft.Win32;

namespace Architexor.Forms.GeoJSON
{
	public partial class FrmExport : System.Windows.Forms.Form
	{
		private double mLatitude = 0;
		private double mLongitude = 0;
		private double mAngle = 0;
		private List<Level> mLevels = null;
		private string mPath = "D:\\";
		private List<ACategory> mCategories = new List<ACategory>();
		private bool mCurView = false;

		//private System.Windows.Forms.Panel dropdownPanel;
		//private CheckedListBox checkedListBoxCategories;

		public FrmExport()
		{
			InitializeComponent();
			//InitializeComboBoxWithCheckBox();
		}

		public double Latitude { get => mLatitude; set => mLatitude = value; }
		public double Longitude { get => mLongitude; set => mLongitude = value; }
		public double Angle { get => mAngle; set => mAngle = value; }
		public List<Level> Levels { get => mLevels; set => mLevels = value; }
		public string Path { get => mPath; set => mPath = value; }
		public List<ACategory> Categories { get => mCategories; set => mCategories = value; }
		public bool CurView { get { return mCurView; } }

		private void FrmExport_Load(object sender, EventArgs e)
		{
			txtAngle.Text = mAngle.ToString();
			txtLatitude.Text = mLatitude.ToString();
			txtLongitude.Text = mLongitude.ToString();
			txtSavePath.Text = mPath;

			treeViewCategories.Nodes.Clear();
			mCategories.Sort((x, y) => x.Category.Name.CompareTo(y.Category.Name));


			string[] list = [];
			bool curView = false;

			try
			{
				RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run\" + "Architexor" + "\\GeoJSON");
				string s = key.GetValue("Preset").ToString();
				curView = Convert.ToBoolean(key.GetValue("CurrentView"));
				list = s.Split(',');
				key.Close();
			}
			catch (Exception) { }

			mCurView = curView;
			chk_curView.Checked = curView;

			foreach (ACategory category in mCategories)
			{
				TreeNode categoryNode = new TreeNode(category.Category.Name);
				if(list.Contains(category.Category.Name))
				{
					categoryNode.Checked = true;
				}

				category.SubCategories.Sort((x, y) => x.Category.Name.CompareTo(y.Category.Name));
				//	Add subcategories as child nodes
				foreach (ASubCategory subcategory in category.SubCategories)
				{
					TreeNode subcategoryNode = new TreeNode(subcategory.Category.Name);
					if (list.Contains(subcategory.Category.Name))
					{
						subcategoryNode.Checked = true;
					}
					categoryNode.Nodes.Add(subcategoryNode);
				}

				treeViewCategories.Nodes.Add(categoryNode);
			}
		}
		
		//private void PopulateTreeViewWithDisciplines(Document doc)
		//{
		//	// Clear any existing nodes in the TreeView
		//	treeViewCategories.Nodes.Clear();

		//	// Define BuiltInCategory groups for each discipline
		//	var architectureCategories = new List<BuiltInCategory>
		//{
		//		BuiltInCategory.OST_Walls, BuiltInCategory.OST_Doors, BuiltInCategory.OST_Windows,
		//		BuiltInCategory.OST_Floors, BuiltInCategory.OST_Ceilings, BuiltInCategory.OST_Roofs,
		//		BuiltInCategory.OST_Rooms, BuiltInCategory.OST_Furniture, BuiltInCategory.OST_Casework,
		//		BuiltInCategory.OST_GenericModel, BuiltInCategory.OST_Stairs, BuiltInCategory.OST_Railings,
		//		BuiltInCategory.OST_Columns
		//};

		//	var structureCategories = new List<BuiltInCategory>
		//{
		//		BuiltInCategory.OST_StructuralColumns, BuiltInCategory.OST_StructuralFraming,
		//		BuiltInCategory.OST_StructuralFoundation, BuiltInCategory.OST_StructuralTrusses,
		//		BuiltInCategory.OST_StructuralConnections, BuiltInCategory.OST_StructuralStiffeners,
		//		BuiltInCategory.OST_StructuralRebar
		//};

		//	var mechanicalCategories = new List<BuiltInCategory>
		//{
		//		BuiltInCategory.OST_MechanicalEquipment, BuiltInCategory.OST_DuctSystem,
		//		BuiltInCategory.OST_DuctCurves, BuiltInCategory.OST_DuctFitting,
		//		BuiltInCategory.OST_DuctInsulations, BuiltInCategory.OST_DuctAccessory
		//};

		//	var electricalCategories = new List<BuiltInCategory>
		//{
		//		BuiltInCategory.OST_ElectricalEquipment, BuiltInCategory.OST_ElectricalFixtures,
		//		BuiltInCategory.OST_LightingFixtures, BuiltInCategory.OST_LightingDevices,
		//		BuiltInCategory.OST_Conduit, BuiltInCategory.OST_ConduitFitting,
		//		BuiltInCategory.OST_CableTray, BuiltInCategory.OST_CableTrayFitting
		//};

		//	var pipingCategories = new List<BuiltInCategory>
		//{
		//		BuiltInCategory.OST_PipeSegments, BuiltInCategory.OST_PipeCurves,
		//		BuiltInCategory.OST_PipeFitting, BuiltInCategory.OST_PipeAccessory,
		//		BuiltInCategory.OST_PipeInsulations, BuiltInCategory.OST_PlumbingFixtures
		//};

		//	var infrastructureCategories = new List<BuiltInCategory>
		//{
		//		BuiltInCategory.OST_Topography, BuiltInCategory.OST_Roads,
		//		BuiltInCategory.OST_Site, BuiltInCategory.OST_BuildingPad
		//};

		//	// Map each discipline's name to its category list
		//	var disciplineCategories = new Dictionary<string, List<BuiltInCategory>>
		//{
		//		{ "Architecture", architectureCategories },
		//		{ "Structure", structureCategories },
		//		{ "Mechanical", mechanicalCategories },
		//		{ "Electrical", electricalCategories },
		//		{ "Piping", pipingCategories },
		//		{ "Infrastructure", infrastructureCategories }
		//};

		//	// Populate TreeView by discipline
		//	foreach (var disciplineEntry in disciplineCategories)
		//	{
		//		TreeNode disciplineNode = new TreeNode(disciplineEntry.Key);

		//		// Use FilteredElementCollector to get elements for each category in the discipline
		//		foreach (BuiltInCategory bic in disciplineEntry.Value)
		//		{
		//			FilteredElementCollector collector = new FilteredElementCollector(doc)
		//					.OfCategory(bic)
		//					.WhereElementIsNotElementType();

		//			// Add each unique category to the discipline node
		//			foreach (Element element in collector)
		//			{
		//				Category category = element.Category;

		//				if (category != null)
		//				{
		//					// Ensure each category is only added once per discipline
		//					if (disciplineNode.Nodes.ContainsKey(category.Name) == false)
		//					{
		//						TreeNode categoryNode = new TreeNode(category.Name) { Name = category.Name };
		//						disciplineNode.Nodes.Add(categoryNode);

		//						// Add subcategories if they exist
		//						if (category.SubCategories != null)
		//						{
		//							foreach (Category subCategory in category.SubCategories)
		//							{
		//								TreeNode subcategoryNode = new TreeNode(subCategory.Name);
		//								categoryNode.Nodes.Add(subcategoryNode);
		//							}
		//						}
		//					}
		//				}
		//			}
		//		}

		//		treeViewCategories.Nodes.Add(disciplineNode);
		//	}
		//}

		//private void InitializeComboBoxWithCheckbox()
		//{
		//	comboBoxWithCheckBox.DropDownHeight = 1;
		//	comboBoxWithCheckBox.Text = "Select Categories";
		//	comboBoxWithCheckBox.Click += ComboBoxWithCheckBox_Click;

		//	//	Initialize Panel as Dropdown Container
		//	dropdownPanel = new System.Windows.Forms.Panel
		//	{
		//		Height = 200,
		//		Width = comboBoxWithCheckBox.Width,
		//		Visible = false,
		//		BorderStyle = BorderStyle.FixedSingle,
		//	};
		//	Controls.Add(dropdownPanel);
		//	dropdownPanel.BringToFront();

		//	//	Initialize CheckedListBox
		//	checkedListBoxCategories = new CheckedListBox
		//	{
		//		Dock = DockStyle.Fill,
		//		CheckOnClick = true,
		//	};
		//	dropdownPanel.Controls.Add(checkedListBoxCategories);

		//	//	Populate CheckedListBox with categories
		//	PopulateCategories();

		//	//	Handle Checked item changes
		//	checkedListBoxCategories.ItemCheck += CheckedListBoxCategories_ItemCheck;
		//}

		//private void ComboBoxWithCheckBox_Click(object sender, EventArgs e)
		//{
		//	dropdownPanel.Visible = !dropdownPanel.Visible;
		//	dropdownPanel.Location = new System.Drawing.Point(comboBoxWithCheckBox.Left, comboBoxWithCheckBox.Bottom);
		//}

		//private void CheckedListBoxCategories_ItemCheck(object sender, ItemCheckEventArgs e) {
		//	//	Delay the update to allow check change to be reflected
		//	BeginInvoke((Action)UpdateComboBoxText);
		//}

		//private void UpdateComboBoxText()
		//{
		//	List<string> selectedItems = new List<string>();
		//	foreach(var item in checkedListBoxCategories.CheckedItems)
		//	{
		//		selectedItems.Add(item.ToString());
		//	}
		//	comboBoxWithCheckBox.Text = selectedItems.Count > 0 ? string.Join(", ", selectedItems) : "Select Categories";
		//}

		//private void PopulateCategories()
		//{
		//	List<string> categories = new List<string>
		//	{
		//		"Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure"
		//	};

		//	foreach (var category in categories)
		//	{
		//		checkedListBoxCategories.Items.Add(category);
		//	}
		//}

		private void btnOK_Click(object sender, EventArgs e)
		{
			if (!double.TryParse(txtAngle.Text, out mAngle))
			{
				MessageBox.Show("Please input angle correctly.", "Error");
				return;
			}
			if (!double.TryParse(txtLatitude.Text, out mLatitude))
			{
				MessageBox.Show("Please input latitude correctly.", "Error");
				return;
			}
			if (!double.TryParse(txtLongitude.Text, out mLongitude))
			{
				MessageBox.Show("Please input longitude correctly.", "Error");
				return;
			}

			mPath = txtSavePath.Text;

			//	
			List<string> list = GetSelectedCategories();
			mCurView = chk_curView.Checked;

			RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
			RegistryKey subkey = key.CreateSubKey("Architexor");
			key.Close();
			try
			{
				subkey.DeleteSubKey("GeoJSON");
			}
			catch (Exception) { }
			key = subkey.CreateSubKey("GeoJSON");
			subkey.Close();
			key.SetValue("Preset", string.Join(",", list));
			key.SetValue("CurrentView", mCurView);

			DialogResult = DialogResult.OK;
			Close();
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				txtSavePath.Text = folderBrowserDialog1.SelectedPath;
			}
		}

		public List<string> GetSelectedCategories()
		{
			List<string> selectedCategories = new List<string>();

			foreach (TreeNode categoryNode in treeViewCategories.Nodes)
			{
				if (categoryNode.Checked)
				{
					selectedCategories.Add(categoryNode.Text);
				}

				foreach (TreeNode subcategoryNode in categoryNode.Nodes)
				{
					if (subcategoryNode.Checked)
					{
						selectedCategories.Add(subcategoryNode.Text);
					}
				}
			}
			return selectedCategories;
		}

		private void treeViewCategories_AfterCheck(object sender, TreeViewEventArgs e)
		{
			treeViewCategories.AfterCheck -= treeViewCategories_AfterCheck;

			try
			{
				//	Check or uncheck all child nodes if parent node is checked/unchecked
				CheckAllChildNodes(e.Node, e.Node.Checked);

				//	Optionally, we can propagate the check status up to parent nodes if desired
				CheckParentNodes(e.Node);
			}
			finally
			{
				//	Reattach event handler
				treeViewCategories.AfterCheck += treeViewCategories_AfterCheck;
			}
		}

		//	Recursively checks/unchecks all childnodes of the given node
		private void CheckAllChildNodes(TreeNode treeNode, bool nodeChecked)
		{
			foreach (TreeNode childNode in treeNode.Nodes)
			{
				childNode.Checked = nodeChecked;	
				//	Recursively apply to all children of this child node
				CheckAllChildNodes(childNode, nodeChecked);
			}
		}

		//	Optinoally propagate check status up to parent nodes
		private void CheckParentNodes(TreeNode treeNode)
		{
			TreeNode parentNode = treeNode.Parent;
			while (parentNode != null)
			{
				bool allUnChecked = true;
				foreach (TreeNode sibling in parentNode.Nodes)
				{
					if (sibling.Checked)
					{
						allUnChecked = false;
						break;
					}
				}

				parentNode.Checked = !allUnChecked;
				parentNode = parentNode.Parent;
			}
		}
	}
}
