namespace TechStoreX
{
    [Activity(Label = "@string/title_activity_detail")]
    public class DetailActivity : AppActivity
    {
        private const string KEY_PREF_DEFAULT_PLANT = "pref_default_plant";
        private const string KEY_PREF_DEFAULT_STORAGE_LOCATION = "pref_default_storage_location";

        private string Option = "";
        private TextView Title;
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
            ActionBar actionBar = SupportActionBar;
            if (actionBar != null) actionBar.SetDisplayHomeAsUpEnabled(true);
            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            // get title and switch button
            Title = FindViewById(Resource.Id.textTitle);
            Switch = FindViewById(Resource.Id.button_switch);
            Switch.Click += (sender, args) =>
            {
                View v = (view)sender;
                switch (Option)
                {
                    case OPTION_GOODS_ISSUE:
                        Option = OPTION_GOODS_RETURN;
                        Title.SetText(Resource.String.activity_goods_return);
                        break;
                    case OPTION_GOODS_RETURN:
                        Option = OPTION_GOODS_ISSUE;
                        Title.SetText(Resource.String.activity_goods_issue);
                        break;
                }
            };

            // get edit text controls
            WorkOrder = FindViewById(Resource.Id.editWorkOrder);
            CostCenter = FindViewById(Resource.Id.editCostCenter);
            Material = FindViewById(Resource.Id.editMaterial);
            Plant = FindViewById(Resource.Id.editPlant);
            StorageLocation = FindViewById(Resource.Id.editStorageLocation);
            Bin = FindViewById(Resource.Id.editBin);
            Quantity = FindViewById(Resource.Id.editQuantity);
            Inventory = FindViewById(Resource.Id.editInventory);
            Vendor = FindViewById(Resource.Id.editVendor);

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
            Intent intent = GetIntent();
            string data = intent.GetStringExtra(EXTRA_OPTION);
            Option = (data != null && !data.isEmpty()) ? data : OPTION_GOODS_ISSUE;

            // load last used values first
            SharedPreferences sharedPref = GetPreferences(MODE_PRIVATE);
            WorkOrder.SetText(sharedPref.GetString(EXTRA_WORK_ORDER, ""));
            CostCenter.SetText(sharedPref.GetString(EXTRA_COST_CENTER, ""));
            Plant.SetText(sharedPref.GetString(EXTRA_PLANT, ""));
            StorageLocation.SetText(sharedPref.GetString(EXTRA_STORAGE_LOCATION, ""));
            Inventory.SetText(sharedPref.GetString(EXTRA_INVENTORY, ""));
            Vendor.SetText(sharedPref.GetString(EXTRA_VENDOR, ""));

            // overwrite with the values from intent, if provided
            data = intent.GetStringExtra(EXTRA_WORK_ORDER);
            if (data != null && !data.isEmpty()) WorkOrder.SetText(data);
            data = intent.GetStringExtra(EXTRA_COST_CENTER);
            if (data != null && !data.isEmpty()) CostCenter.SetText(data);
            data = intent.GetStringExtra(EXTRA_MATERIAL);
            if (data != null && !data.isEmpty()) Material.SetText(data);
            data = intent.GetStringExtra(EXTRA_PLANT);
            if (data != null && !data.isEmpty()) Plant.SetText(data);
            data = intent.GetStringExtra(EXTRA_STORAGE_LOCATION);
            if (data != null && !data.isEmpty()) StorageLocation.SetText(data);
            data = intent.GetStringExtra(EXTRA_BIN);
            if (data != null && !data.isEmpty()) Bin.SetText(data);
            data = intent.GetStringExtra(EXTRA_INVENTORY);
            if (data != null && !data.isEmpty()) Inventory.SetText(data);
            data = intent.GetStringExtra(EXTRA_VENDOR);
            if (data != null && !data.isEmpty()) Vendor.SetText(data);

            // get defaults from preferences
            sharedPref = PreferenceManager.GetDefaultSharedPreferences(this);
            if (Plant.Length == 0)
            {
                Plant.SetText(sharedPref.GetString(KEY_PREF_DEFAULT_PLANT, ""));
                // if read from defaults: disable field
                Plant.SetEnabled(Plant.Length == 0);
            }
            if (StorageLocation.Length == 0)
            {
                StorageLocation.SetText(sharedPref.GetString(KEY_PREF_DEFAULT_STORAGE_LOCATION, ""));
                // if read from defaults: disable field
                StorageLocation.SetEnabled(StorageLocation.Length == 0);
            }

            // initialize visibility and errors
            InitializeVisibility();
        }

        public override bool OnCreateOptionsMenu(Menu menu)
        {
            // Inflate the menu; this adds items to the action bar if it is present.
            MenuInflater.Inflate(Resource.menu.menu_detail, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(MenuItem item)
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
                    data.PutExtra(EXTRA_WORK_ORDER, WorkOrder.GetText().toString());
                    data.PutExtra(EXTRA_COST_CENTER, CostCenter.GetText().toString());
                    data.PutExtra(EXTRA_MATERIAL, mMaterial.GetText().toString());
                    data.PutExtra(EXTRA_PLANT, mPlant.GetText().toString());
                    data.PutExtra(EXTRA_STORAGE_LOCATION, mStorageLocation.GetText().toString());
                    data.PutExtra(EXTRA_BIN, mBin.GetText().toString());
                    // TODO: manage format conversion for quantity as string
                    data.PutExtra(EXTRA_QUANTITY, mQuantity.GetText().toString());
                    SimpleDateFormat ft = new SimpleDateFormat("dd/MM/yy HH:mm:ss", Locale.US);
                    data.PutExtra(EXTRA_DATE, ft.format(new Date()));
                    data.PutExtra(EXTRA_INVENTORY, mInventory.GetText().toString());
                    data.PutExtra(EXTRA_VENDOR, mVendor.GetText().toString());
                    SetResult(RESULT_OK, data);
                    // save data for next use
                    SharedPreferences sharedPref = getPreferences(MODE_PRIVATE);
                    SharedPreferences.Editor editor = sharedPref.edit();
                    editor.PutString(EXTRA_WORK_ORDER, WorkOrder.GetText().toString());
                    editor.PutString(EXTRA_COST_CENTER, CostCenter.GetText().toString());
                    editor.PutString(EXTRA_INVENTORY, mInventory.GetText().toString());
                    editor.PutString(EXTRA_PLANT, mPlant.GetText().toString());
                    editor.PutString(EXTRA_STORAGE_LOCATION, mStorageLocation.GetText().toString());
                    editor.PutString(EXTRA_VENDOR, mVendor.GetText().toString());
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
                    Title.SetText(Resource.String.activity_goods_issue);
                    break;
                case OPTION_GOODS_RETURN:
                    Title.SetText(Resource.String.activity_goods_return);
                    break;
                case OPTION_INVENTORY_WO_DOCUMENT:
                case OPTION_INVENTORY_WITH_DOCUMENT:
                    Title.SetText(Resource.String.activity_inventory);
            }
            if (Option == OPTION_GOODS_ISSUE || Option == OPTION_GOODS_RETURN)
            {
                // hide and clear inventory document number
                Switch.SetVisibility(View.VISIBLE);
                viewGroup = FindViewById(Resource.Id.layoutInventory);
                if (viewGroup != null) viewGroup.SetVisibility(View.GONE);
                Inventory.SetText("");
                if (WorkOrder.Length > 0)
                {
                    // show work order and disable, hide and clear cost center
                    viewGroup = FindViewById(Resource.Id.layoutWorkOrder);
                    if (viewGroup != null) viewGroup.SetVisibility(View.VISIBLE);
                    WorkOrder.SetError(null);
                    viewGroup = FindViewById(Resource.Id.layoutCostCenter);
                    if (viewGroup != null) viewGroup.SetVisibility(View.GONE);
                    CostCenter.SetText("");
                }
                else if (CostCenter.Length > 0)
                {
                    // show cost center and disable, hide and clear work order
                    viewGroup = FindViewById(Resource.Id.layoutCostCenter);
                    if (viewGroup != null) viewGroup.SetVisibility(View.VISIBLE);
                    CostCenter.SetError(null);
                    viewGroup = FindViewById(Resource.Id.layoutWorkOrder);
                    if (viewGroup != null) viewGroup.SetVisibility(View.GONE);
                    WorkOrder.SetText("");
                }
                else
                {
                    // show and enable both
                    viewGroup = FindViewById(Resource.Id.layoutCostCenter);
                    if (viewGroup != null) viewGroup.SetVisibility(View.VISIBLE);
                    //                WorkOrder.SetError("Enter either Work Order or Cost Center");
                    viewGroup = FindViewById(Resource.Id.layoutWorkOrder);
                    if (viewGroup != null) viewGroup.SetVisibility(View.VISIBLE);
                    //                WorkOrder.SetError("Enter either Work Order or Cost Center");
                }

            }
            if (Option == OPTION_INVENTORY_WO_DOCUMENT || Option == OPTION_INVENTORY_WITH_DOCUMENT)
            {
                // show inventory document, hide and clear work order and cost center
                Switch.SetVisibility(View.INVISIBLE);
                viewGroup = FindViewById(Resource.Id.layoutInventory);
                if (viewGroup != null) viewGroup.SetVisibility(View.VISIBLE);
                viewGroup = FindViewById(Resource.Id.layoutWorkOrder);
                if (viewGroup != null) viewGroup.SetVisibility(View.GONE);
                WorkOrder.SetText("");
                viewGroup = FindViewById(Resource.Id.layoutCostCenter);
                if (viewGroup != null) viewGroup.SetVisibility(View.GONE);
                CostCenter.SetText("");
            }

            //        mPlant.SetError(mPlant.Length == 0 ? "Enter plant code" : null);
            //        mStorageLocation.SetError(mStorageLocation.Length == 0 ? "Enter storage location code" : null);
            //        mMaterial.SetError(mMaterial.Length == 0 ? "Enter material number" : null);

            if (Bin.Length > 0)
            {
                // bin provided, show disabled
                viewGroup = FindViewById(Resource.Id.layoutBin);
                if (viewGroup != null) viewGroup.SetVisibility(View.VISIBLE);
            }
            else
            {
                // bin not provided, hide
                viewGroup = FindViewById(Resource.Id.layoutBin);
                if (viewGroup != null) viewGroup.SetVisibility(View.GONE);
            }
            if (Vendor.Length > 0)
            {
                // vendor provided, show
                viewGroup = FindViewById(Resource.Id.layoutVendor);
                if (viewGroup != null) viewGroup.SetVisibility(View.VISIBLE);
            }
            else
            {
                // vendor not provided, hide
                viewGroup = FindViewById(Resource.Id.layoutVendor);
                if (viewGroup != null) viewGroup.SetVisibility(View.GONE);
            }
        }

        private boolean ValidateData()
        {
            return ValidateWorkOrder(WorkOrder.getText())
                    && ValidateCostCenter(CostCenter.getText())
                    && ValidateInventory(mInventory.getText())
                    && ValidateMaterial(mMaterial.getText())
                    && ValidatePlant(mPlant.getText())
                    && ValidateStorageLocation(mStorageLocation.getText())
                    && ValidateBin(mBin.getText())
                    && ValidateVendor(mVendor.getText())
                    && ValidateQuantity(mQuantity.getText());
        }

        private boolean ValidateWorkOrder(Editable s)
        {
            if (Option == OPTION_GOODS_ISSUE || Option == OPTION_GOODS_RETURN)
            {
                if (s == null || s.Length == 0)
                {
                    if (CostCenter.Length == 0)
                    {
                        // both work order and cost center are empty
                        // show up cost center, if hidden before
                        ViewGroup viewGroup = FindViewById(R.id.layoutCostCenter);
                        if (viewGroup != null) viewGroup.SetVisibility(View.VISIBLE);
                        WorkOrder.SetError(GetString(Resource.String.work_order_empty));
                        CostCenter.SetError(GetString(Resource.String.work_order_empty));
                        return false;
                    }
                    else
                    {
                        // work order is empty, but cost center is not
                        // should not be here, as the work order should be hidden
                        ViewGroup viewGroup = FindViewById(R.id.layoutWorkOrder);
                        if (viewGroup != null) viewGroup.SetVisibility(View.GONE);
                        WorkOrder.SetError(null);
                        return true;
                    }
                }
                else
                {
                    // work order entered, clear and hide cost center
                    ViewGroup viewGroup = FindViewById(R.id.layoutCostCenter);
                    if (viewGroup != null) viewGroup.SetVisibility(View.GONE);
                    if (CostCenter.Length > 0) CostCenter.SetText("");
                    // check the length
                    if (s.Length == 10)
                    {
                        WorkOrder.SetError(null);
                        return true;
                    }
                    else
                    {
                        WorkOrder.SetError(GetString(Resource.String.work_order_length));
                        return false;
                    }
                }
            }
            else
            {
                WorkOrder.SetError(null);
                return true;
            }
        }

        private boolean ValidateCostCenter(Editable s)
        {
            if (Option == OPTION_GOODS_ISSUE || Option == OPTION_GOODS_RETURN)
            {
                if (s == null || s.Length == 0)
                {
                    if (WorkOrder.Length == 0)
                    {
                        // both work order and cost center are empty
                        // show up work order, if hidden before
                        ViewGroup viewGroup = FindViewById(R.id.layoutWorkOrder);
                        if (viewGroup != null) viewGroup.SetVisibility(View.VISIBLE);
                        CostCenter.SetError(GetString(Resource.String.work_order_empty));
                        WorkOrder.SetError(GetString(Resource.String.work_order_empty));
                        return false;
                    }
                    else
                    {
                        // cost center is empty, but work order is not
                        // should not be here, as the cost center should be hidden
                        ViewGroup viewGroup = FindViewById(R.id.layoutCostCenter);
                        if (viewGroup != null) viewGroup.SetVisibility(View.GONE);
                        CostCenter.SetError(null);
                        return true;
                    }
                }
                else
                {
                    // cost center entered, clear and hide work order
                    ViewGroup viewGroup = FindViewById(R.id.layoutWorkOrder);
                    if (viewGroup != null) viewGroup.SetVisibility(View.GONE);
                    if (WorkOrder.Length > 0) WorkOrder.SetText("");
                    // check the length
                    if (s.Length >= 7 && s.Length <= 10)
                    {
                        CostCenter.SetError(null);
                        return true;
                    }
                    else
                    {
                        CostCenter.SetError(GetString(Resource.String.cost_center_length));
                        return false;
                    }
                }
            }
            else
            {
                CostCenter.SetError(null);
                return true;
            }
        }

        private boolean ValidateInventory(Editable s)
        {
            if (Option == OPTION_INVENTORY_WO_DOCUMENT || Option == OPTION_INVENTORY_WITH_DOCUMENT)
            {
                if (s == null || s.Length == 0)
                {
                    mInventory.SetError(null);
                    if (Option == OPTION_INVENTORY_WITH_DOCUMENT)
                        Option = OPTION_INVENTORY_WO_DOCUMENT;
                    return true;
                }
                else
                {
                    if (s.Length == 9)
                    {
                        mInventory.SetError(null);
                        if (Option == OPTION_INVENTORY_WO_DOCUMENT)
                            Option = OPTION_INVENTORY_WITH_DOCUMENT;
                        return true;
                    }
                    mInventory.SetError(GetString(Resource.String.inventory_length));
                    return false;
                }
            }
            else
            {
                mInventory.SetError(null);
                return true;
            }
        }

        private boolean ValidateMaterial(Editable s)
        {
            if (s == null || s.Length == 0)
            {
                mMaterial.SetError(GetString(Resource.String.material_empty));
                return false;
            }
            else if (s.Length == 9)
            {
                mMaterial.SetError(null);
                return true;
            }
            else
            {
                mMaterial.SetError(GetString(Resource.String.material_length));
                return false;
            }
        }

        private boolean ValidatePlant(Editable s)
        {
            if (s == null || s.Length == 0)
            {
                mPlant.SetError(GetString(Resource.String.plant_empty));
                return false;
            }
            else if (s.Length == 4)
            {
                mPlant.SetError(null);
                return true;
            }
            else
            {
                mPlant.SetError(GetString(Resource.String.plant_length));
                return false;
            }
        }

        private boolean ValidateStorageLocation(Editable s)
        {
            if (s == null || s.Length == 0)
            {
                mStorageLocation.SetError(GetString(Resource.String.storage_location_empty));
                return false;
            }
            else if (s.Length == 4)
            {
                mStorageLocation.SetError(null);
                return true;
            }
            else
            {
                mStorageLocation.SetError(GetString(Resource.String.storage_location_length));
                return false;
            }
        }

        private boolean ValidateBin(Editable s)
        {
            if (s == null || s.Length == 0)
            {
                mBin.SetError(null);
                return true;
            }
            else
            {
                if (s.Length <= 10)
                {
                    mBin.SetError(null);
                    return true;
                }
                mBin.SetError(GetString(Resource.String.bin_length));
                return false;
            }
        }

        private boolean ValidateVendor(Editable s)
        {
            if (s == null || s.Length == 0)
            {
                mVendor.SetError(null);
                return true;
            }
            else
            {
                if (s.Length == 9)
                {
                    mVendor.SetError(null);
                    return true;
                }
                mVendor.SetError(GetString(Resource.String.vendor_length));
                return false;
            }
        }

        private boolean ValidateQuantity(Editable s)
        {
            if (s == null || s.Length == 0)
            {
                mQuantity.SetError(GetString(Resource.String.quantity_empty));
                return false;
            }
            else
            {
                double q = Double.Parse(s.toString());
                if (q == 0.0)
                {
                    mQuantity.SetError(GetString(Resource.String.quantity_zero));
                    return false;
                }
                else if (Math.Floor(q * 1000.0) < q * 1000.0)
                {
                    mQuantity.SetError(GetString(Resource.String.quantity_decimals_length));
                    return false;
                }
                else if (Math.Floor(q / 10000000000.0) > 0)
                {
                    mQuantity.SetError(GetString(Resource.String.quantity_integers_length));
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}