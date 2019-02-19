using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Support.Design.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Text;
using Java.Util;
using System;

namespace TechStoreX
{
    [Activity(Label = "@string/title_activity_detail")]
    public class DetailActivity : AppActivity
    {
        private const string KEY_PREF_DEFAULT_PLANT = "pref_default_plant";
        private const string KEY_PREF_DEFAULT_STORAGE_LOCATION = "pref_default_storage_location";

        private string Option = "";
        private TextView TitleText;
        private ImageButton Switch;
        private EditText WorkOrder;
        private EditText CostCenter;
        private EditText Material;
        private EditText Plant;
        private EditText StorageLocation;
        private EditText Bin;
        private EditText Quantity;
        private EditText Inventory;
        private EditText Vendor;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_detail);
            //        Toolbar toolbar = FindViewById(Resource.Idtoolbar);
            //        setSupportActionBar(toolbar);
            Android.Support.V7.App.ActionBar actionBar = SupportActionBar;
            if (actionBar != null) actionBar.SetDisplayHomeAsUpEnabled(true);
            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            // get title and switch button
            TitleText = FindViewById<TextView>(Resource.Id.textTitle);
            Switch = FindViewById<ImageButton>(Resource.Id.button_switch);
            Switch.Click += (sender, args) =>
            {
                View v = (View)sender;
                switch (Option)
                {
                    case OPTION_GOODS_ISSUE:
                        Option = OPTION_GOODS_RETURN;
                        TitleText.SetText(Resource.String.activity_goods_return);
                        break;
                    case OPTION_GOODS_RETURN:
                        Option = OPTION_GOODS_ISSUE;
                        TitleText.SetText(Resource.String.activity_goods_issue);
                        break;
                }
            };

            // get edit text controls
            WorkOrder = FindViewById<EditText>(Resource.Id.editWorkOrder);
            CostCenter = FindViewById<EditText>(Resource.Id.editCostCenter);
            Material = FindViewById<EditText>(Resource.Id.editMaterial);
            Plant = FindViewById<EditText>(Resource.Id.editPlant);
            StorageLocation = FindViewById<EditText>(Resource.Id.editStorageLocation);
            Bin = FindViewById<EditText>(Resource.Id.editBin);
            Quantity = FindViewById<EditText>(Resource.Id.editQuantity);
            Inventory = FindViewById<EditText>(Resource.Id.editInventory);
            Vendor = FindViewById<EditText>(Resource.Id.editVendor);

            // set validation watchers
            // WorkOrder.AddTextChangedListener(WorkOrderWatcher);
            // CostCenter.AddTextChangedListener(CostCenterWatcher);
            // Inventory.AddTextChangedListener(InventoryWatcher);
            // Material.AddTextChangedListener(MaterialWatcher);
            // Plant.AddTextChangedListener(PlantWatcher);
            // StorageLocation.AddTextChangedListener(StorageLocationWatcher);
            // Bin.AddTextChangedListener(BinWatcher);
            // Vendor.AddTextChangedListener(VendorWatcher);
            // Quantity.AddTextChangedListener(QuantityWatcher);
            WorkOrder.AfterTextChanged += (sender, args) => { ValidateWorkOrder(args.Editable); };
            CostCenter.AfterTextChanged += (sender, args) => { ValidateCostCenter(args.Editable); };
            Inventory.AfterTextChanged += (sender, args) => { ValidateInventory(args.Editable); };
            Material.AfterTextChanged += (sender, args) => { ValidateMaterial(args.Editable); };
            Plant.AfterTextChanged += (sender, args) => { ValidatePlant(args.Editable); };
            StorageLocation.AfterTextChanged += (sender, args) => { ValidateStorageLocation(args.Editable); };
            Bin.AfterTextChanged += (sender, args) => { ValidateBin(args.Editable); };
            Vendor.AfterTextChanged += (sender, args) => { ValidateVendor(args.Editable); };
            Quantity.AfterTextChanged += (sender, args) => { ValidateQuantity(args.Editable); };

            // get option from intent, if any
            Intent intent = Intent;
            string data = intent.GetStringExtra(EXTRA_OPTION);
            Option = (data.Length != 0) ? data : OPTION_GOODS_ISSUE;

            // load last used values first
            ISharedPreferences sharedPref = GetPreferences(FileCreationMode.Private);
            WorkOrder.Text = sharedPref.GetString(EXTRA_WORK_ORDER, ""); ;
            CostCenter.Text = sharedPref.GetString(EXTRA_COST_CENTER, "");
            Plant.Text = sharedPref.GetString(EXTRA_PLANT, "");
            StorageLocation.Text = sharedPref.GetString(EXTRA_STORAGE_LOCATION, "");
            Inventory.Text = sharedPref.GetString(EXTRA_INVENTORY, "");
            Vendor.Text = sharedPref.GetString(EXTRA_VENDOR, "");

            // overwrite with the values from intent, if provided
            data = intent.GetStringExtra(EXTRA_WORK_ORDER);
            if (data.Length != 0) WorkOrder.Text = data;
            data = intent.GetStringExtra(EXTRA_COST_CENTER);
            if (data.Length != 0) CostCenter.Text = data;
            data = intent.GetStringExtra(EXTRA_MATERIAL);
            if (data.Length != 0) Material.Text = data;
            data = intent.GetStringExtra(EXTRA_PLANT);
            if (data.Length != 0) Plant.Text = data;
            data = intent.GetStringExtra(EXTRA_STORAGE_LOCATION);
            if (data.Length != 0) StorageLocation.Text = data;
            data = intent.GetStringExtra(EXTRA_BIN);
            if (data.Length != 0) Bin.Text = data;
            data = intent.GetStringExtra(EXTRA_INVENTORY);
            if (data.Length != 0) Inventory.Text = data;
            data = intent.GetStringExtra(EXTRA_VENDOR);
            if (data.Length != 0) Vendor.Text = data;

            // get defaults from preferences
            sharedPref = PreferenceManager.GetDefaultSharedPreferences(this);
            if (Plant.Length() == 0)
            {
                Plant.Text = sharedPref.GetString(KEY_PREF_DEFAULT_PLANT, "");
                // if read from defaults: disable field
                Plant.Enabled = (Plant.Length() == 0);
            }
            if (StorageLocation.Length() == 0)
            {
                StorageLocation.Text = sharedPref.GetString(KEY_PREF_DEFAULT_STORAGE_LOCATION, "");
                // if read from defaults: disable field
                StorageLocation.Enabled = (StorageLocation.Length() == 0);
            }

            // initialize visibility and errors
            InitializeVisibility();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            // Inflate the menu; this adds items to the action bar if it is present.
            MenuInflater.Inflate(Resource.Menu.menu_detail, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle action bar item clicks here. The action bar will
            // automatically handle clicks on the Home/Up button, so long
            // as you specify a parent activity in AndroidManifest.xml.
            if (item.ItemId == Resource.Id.action_save)
            {
                if (ValidateData())
                {
                    Intent data = new Intent();
                    data.PutExtra(EXTRA_OPTION, Option);
                    data.PutExtra(EXTRA_WORK_ORDER, WorkOrder.Text);
                    data.PutExtra(EXTRA_COST_CENTER, CostCenter.Text);
                    data.PutExtra(EXTRA_MATERIAL, Material.Text);
                    data.PutExtra(EXTRA_PLANT, Plant.Text);
                    data.PutExtra(EXTRA_STORAGE_LOCATION, StorageLocation.Text);
                    data.PutExtra(EXTRA_BIN, Bin.Text);
                    // TODO: manage format conversion for quantity as string
                    data.PutExtra(EXTRA_QUANTITY, Quantity.Text);
                    SimpleDateFormat ft = new SimpleDateFormat("dd/MM/yy HH:mm:ss", Locale.Us);
                    data.PutExtra(EXTRA_DATE, ft.Format(new Date()));
                    data.PutExtra(EXTRA_INVENTORY, Inventory.Text);
                    data.PutExtra(EXTRA_VENDOR, Vendor.Text);
                    SetResult(Result.Ok, data);
                    // save data for next use
                    ISharedPreferences sharedPref = GetPreferences(FileCreationMode.Private);
                    ISharedPreferencesEditor editor = sharedPref.Edit();
                    editor.PutString(EXTRA_WORK_ORDER, WorkOrder.Text);
                    editor.PutString(EXTRA_COST_CENTER, CostCenter.Text);
                    editor.PutString(EXTRA_INVENTORY, Inventory.Text);
                    editor.PutString(EXTRA_PLANT, Plant.Text);
                    editor.PutString(EXTRA_STORAGE_LOCATION, StorageLocation.Text);
                    editor.PutString(EXTRA_VENDOR, Vendor.Text);
                    editor.Apply();
                    // finish activity
                    Finish();
                }
                else
                {
                    Snackbar.Make(FindViewById(Resource.Id.fab),
                            Resource.String.save_error,
                            Snackbar.LengthLong).Show();
                }
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void InitializeVisibility()
        {
            ViewGroup viewGroup;
            switch (Option)
            {
                case OPTION_GOODS_ISSUE:
                    TitleText.SetText(Resource.String.activity_goods_issue);
                    break;
                case OPTION_GOODS_RETURN:
                    TitleText.SetText(Resource.String.activity_goods_return);
                    break;
                case OPTION_INVENTORY_WO_DOCUMENT:
                case OPTION_INVENTORY_WITH_DOCUMENT:
                    TitleText.SetText(Resource.String.activity_inventory);
                    break;
            }
            if (Option == OPTION_GOODS_ISSUE || Option == OPTION_GOODS_RETURN)
            {
                // hide and clear inventory document number
                Switch.Visibility = ViewStates.Visible;
                viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutInventory);
                if (viewGroup != null) viewGroup.Visibility = ViewStates.Gone;
                Inventory.Text = "";
                if (WorkOrder.Length() != 0)
                {
                    // show work order and disable, hide and clear cost center
                    viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutWorkOrder);
                    if (viewGroup != null) viewGroup.Visibility = ViewStates.Visible;
                    WorkOrder.Error = null;
                    viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutCostCenter);
                    if (viewGroup != null) viewGroup.Visibility = ViewStates.Gone;
                    CostCenter.Text = "";
                }
                else if (CostCenter.Length() != 0)
                {
                    // show cost center and disable, hide and clear work order
                    viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutCostCenter);
                    if (viewGroup != null) viewGroup.Visibility = ViewStates.Visible;
                    CostCenter.Error = null;
                    viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutWorkOrder);
                    if (viewGroup != null) viewGroup.Visibility = ViewStates.Gone;
                    WorkOrder.Text = "";
                }
                else
                {
                    // show and enable both
                    viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutCostCenter);
                    if (viewGroup != null) viewGroup.Visibility = ViewStates.Visible;
                    //                WorkOrder.SetError("Enter either Work Order or Cost Center");
                    viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutWorkOrder);
                    if (viewGroup != null) viewGroup.Visibility = ViewStates.Visible;
                    //                WorkOrder.SetError("Enter either Work Order or Cost Center");
                }

            }
            if (Option == OPTION_INVENTORY_WO_DOCUMENT || Option == OPTION_INVENTORY_WITH_DOCUMENT)
            {
                // show inventory document, hide and clear work order and cost center
                Switch.Visibility = ViewStates.Invisible;
                viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutInventory);
                if (viewGroup != null) viewGroup.Visibility = ViewStates.Visible;
                viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutWorkOrder);
                if (viewGroup != null) viewGroup.Visibility = ViewStates.Gone;
                WorkOrder.Text = "";
                viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutCostCenter);
                if (viewGroup != null) viewGroup.Visibility = ViewStates.Gone;
                CostCenter.Text = "";
            }

            //        mPlant.SetError(mPlant.Length == 0 ? "Enter plant code" : null);
            //        mStorageLocation.SetError(mStorageLocation.Length == 0 ? "Enter storage location code" : null);
            //        mMaterial.SetError(mMaterial.Length == 0 ? "Enter material number" : null);

            if (Bin.Length() != 0)
            {
                // bin provided, show disabled
                viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutBin);
                if (viewGroup != null) viewGroup.Visibility = ViewStates.Visible;
            }
            else
            {
                // bin not provided, hide
                viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutBin);
                if (viewGroup != null) viewGroup.Visibility = ViewStates.Gone;
            }
            if (Vendor.Length() != 0)
            {
                // vendor provided, show
                viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutVendor);
                if (viewGroup != null) viewGroup.Visibility = ViewStates.Visible;
            }
            else
            {
                // vendor not provided, hide
                viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutVendor);
                if (viewGroup != null) viewGroup.Visibility = ViewStates.Gone;
            }
        }

        private bool ValidateData() => ValidateWorkOrder(WorkOrder.EditableText)
                    && ValidateCostCenter(CostCenter.EditableText)
                    && ValidateInventory(Inventory.EditableText)
                    && ValidateMaterial(Material.EditableText)
                    && ValidatePlant(Plant.EditableText)
                    && ValidateStorageLocation(StorageLocation.EditableText)
                    && ValidateBin(Bin.EditableText)
                    && ValidateVendor(Vendor.EditableText)
                    && ValidateQuantity(Quantity.EditableText);

        private bool ValidateWorkOrder(IEditable s)
        {
            if (Option == OPTION_GOODS_ISSUE || Option == OPTION_GOODS_RETURN)
            {
                if (s == null || s.Length() == 0)
                {
                    if (CostCenter.Length() == 0)
                    {
                        // both work order and cost center are empty
                        // show up cost center, if hidden before
                        ViewGroup viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutCostCenter);
                        if (viewGroup != null) viewGroup.Visibility = ViewStates.Visible;
                        WorkOrder.Error = GetString(Resource.String.work_order_empty);
                        CostCenter.Error = GetString(Resource.String.work_order_empty);
                        return false;
                    }
                    else
                    {
                        // work order is empty, but cost center is not
                        // should not be here, as the work order should be hidden
                        ViewGroup viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutWorkOrder);
                        if (viewGroup != null) viewGroup.Visibility = ViewStates.Gone;
                        WorkOrder.Error = null;
                        return true;
                    }
                }
                else
                {
                    // work order entered, clear and hide cost center
                    ViewGroup viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutCostCenter);
                    if (viewGroup != null) viewGroup.Visibility = ViewStates.Gone;
                    if (CostCenter.Length() > 0) CostCenter.Text = "";
                    // check the length
                    if (s.Length() == 10)
                    {
                        WorkOrder.Error = null;
                        return true;
                    }
                    else
                    {
                        WorkOrder.Error = GetString(Resource.String.work_order_length);
                        return false;
                    }
                }
            }
            else
            {
                WorkOrder.Error = null;
                return true;
            }
        }

        private bool ValidateCostCenter(IEditable s)
        {
            if (Option == OPTION_GOODS_ISSUE || Option == OPTION_GOODS_RETURN)
            {
                if (s == null || s.Length() == 0)
                {
                    if (WorkOrder.Length() == 0)
                    {
                        // both work order and cost center are empty
                        // show up work order, if hidden before
                        ViewGroup viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutWorkOrder);
                        if (viewGroup != null) viewGroup.Visibility = ViewStates.Visible;
                        CostCenter.Error = GetString(Resource.String.work_order_empty);
                        WorkOrder.Error = GetString(Resource.String.work_order_empty);
                        return false;
                    }
                    else
                    {
                        // cost center is empty, but work order is not
                        // should not be here, as the cost center should be hidden
                        ViewGroup viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutCostCenter);
                        if (viewGroup != null) viewGroup.Visibility = ViewStates.Gone;
                        CostCenter.Error = null;
                        return true;
                    }
                }
                else
                {
                    // cost center entered, clear and hide work order
                    ViewGroup viewGroup = FindViewById<ViewGroup>(Resource.Id.layoutWorkOrder);
                    if (viewGroup != null) viewGroup.Visibility = ViewStates.Gone;
                    if (WorkOrder.Length() > 0) WorkOrder.Text = "";
                    // check the length
                    if (s.Length() >= 7 && s.Length() <= 10)
                    {
                        CostCenter.Error = null;
                        return true;
                    }
                    else
                    {
                        CostCenter.Error = GetString(Resource.String.cost_center_length);
                        return false;
                    }
                }
            }
            else
            {
                CostCenter.Error = null;
                return true;
            }
        }

        private bool ValidateInventory(IEditable s)
        {
            if (Option == OPTION_INVENTORY_WO_DOCUMENT || Option == OPTION_INVENTORY_WITH_DOCUMENT)
            {
                if (s == null || s.Length() == 0)
                {
                    Inventory.Error = null;
                    if (Option == OPTION_INVENTORY_WITH_DOCUMENT)
                        Option = OPTION_INVENTORY_WO_DOCUMENT;
                    return true;
                }
                else
                {
                    if (s.Length() == 9)
                    {
                        Inventory.Error = null;
                        if (Option == OPTION_INVENTORY_WO_DOCUMENT)
                            Option = OPTION_INVENTORY_WITH_DOCUMENT;
                        return true;
                    }
                    Inventory.Error = GetString(Resource.String.inventory_length);
                    return false;
                }
            }
            else
            {
                Inventory.Error = null;
                return true;
            }
        }

        private bool ValidateMaterial(IEditable s)
        {
            if (s == null || s.Length() == 0)
            {
                Material.Error = GetString(Resource.String.material_empty);
                return false;
            }
            else if (s.Length() == 9)
            {
                Material.Error = null;
                return true;
            }
            else
            {
                Material.Error = GetString(Resource.String.material_length);
                return false;
            }
        }

        private bool ValidatePlant(IEditable s)
        {
            if (s == null || s.Length() == 0)
            {
                Plant.Error = GetString(Resource.String.plant_empty);
                return false;
            }
            else if (s.Length() == 4)
            {
                Plant.Error = null;
                return true;
            }
            else
            {
                Plant.Error = GetString(Resource.String.plant_length);
                return false;
            }
        }

        private bool ValidateStorageLocation(IEditable s)
        {
            if (s == null || s.Length() == 0)
            {
                StorageLocation.Error = GetString(Resource.String.storage_location_empty);
                return false;
            }
            else if (s.Length() == 4)
            {
                StorageLocation.Error = null;
                return true;
            }
            else
            {
                StorageLocation.Error = GetString(Resource.String.storage_location_length);
                return false;
            }
        }

        private bool ValidateBin(IEditable s)
        {
            if (s == null || s.Length() == 0)
            {
                Bin.Error = null;
                return true;
            }
            else
            {
                if (s.Length() <= 10)
                {
                    Bin.Error = null;
                    return true;
                }
                Bin.Error = GetString(Resource.String.bin_length);
                return false;
            }
        }

        private bool ValidateVendor(IEditable s)
        {
            if (s == null || s.Length() == 0)
            {
                Vendor.Error = null;
                return true;
            }
            else
            {
                if (s.Length() == 9)
                {
                    Vendor.Error = null;
                    return true;
                }
                Vendor.Error = GetString(Resource.String.vendor_length);
                return false;
            }
        }

        private bool ValidateQuantity(IEditable s)
        {
            if (s == null || s.Length() == 0)
            {
                Quantity.Error = GetString(Resource.String.quantity_empty);
                return false;
            }
            else
            {
                double q = Double.Parse(s.ToString());
                if (q == 0.0)
                {
                    Quantity.Error = GetString(Resource.String.quantity_zero);
                    return false;
                }
                else if (Math.Floor(q * 1000.0) < q * 1000.0)
                {
                    Quantity.Error = GetString(Resource.String.quantity_decimals_length);
                    return false;
                }
                else if (Math.Floor(q / 10000000000.0) > 0)
                {
                    Quantity.Error = GetString(Resource.String.quantity_integers_length);
                    return false;
                }
                else
                {
                    Quantity.Error = null;
                    return true;
                }
            }
        }

        protected override void ProcessBarcode(string data)
        {
            // TODO 
            throw new System.NotImplementedException();
        }
    }
}