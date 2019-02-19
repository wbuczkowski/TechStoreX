namespace TechStoreX
{
    [Activity(Label = "@string/title_activity_zbar")]
    public class ZBarFullScannerActivity : AppActivity, ZBarScannerView.ResultHandler,
        ZBarFormatSelectorDialogFragment.FormatSelectorDialogListener,
        CameraSelectorDialogFragment.CameraSelectorDialogListener
    {
        private const string FLASH_STATE = "FLASH_STATE";
        private const string AUTO_FOCUS_STATE = "AUTO_FOCUS_STATE";
        private const string SELECTED_FORMATS = "SELECTED_FORMATS";
        private const string CAMERA_ID = "CAMERA_ID";
        private ZBarScannerView ScannerView;
        private bool Flash;
        private boolean AutoFocus;
        private ArrayList<int> SelectedIndices;
        private int CameraId = -1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
            {
                Flash = savedInstanceState.GetBoolean(FLASH_STATE, false);
                AutoFocus = savedInstanceState.GetBoolean(AUTO_FOCUS_STATE, true);
                SelectedIndices = savedInstanceState.GetIntegerArrayList(SELECTED_FORMATS);
                CameraId = savedInstanceState.GetInt(CAMERA_ID, -1);
            }
            else
            {
                Flash = false;
                AutoFocus = true;
                SelectedIndices = null;
                CameraId = -1;
            }
            SetContentView(Resource.Layout.activity_zbar);
            //        Toolbar toolbar = findViewById(R.id.toolbar);
            //        setSupportActionBar(toolbar);
            ActionBar actionBar = GetSupportActionBar();
            if (actionBar != null) actionBar.SetDisplayHomeAsUpEnabled(true);
            ViewGroup contentFrame = FindViewById(Resource.Id.content_frame);
            ScannerView = new ZBarScannerView(this);
            SetupFormats();
            contentFrame.AddView(ScannerView);
        }

        public override void OnPause()
        {
            base.OnPause();
            ScannerView.StopCamera();
            CloseFormatsDialog();
        }

        public override void OnResume()
        {
            base.OnResume();
            ScannerView.SetResultHandler(this);
            ScannerView.StartCamera(CameraId);
            ScannerView.SetFlash(Flash);
            ScannerView.SetAutoFocus(AutoFocus);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutBoolean(FLASH_STATE, Flash);
            outState.PutBoolean(AUTO_FOCUS_STATE, AutoFocus);
            outState.PutIntegerArrayList(SELECTED_FORMATS, SelectedIndices);
            outState.PutInt(CAMERA_ID, CameraId);
        }

        public override bool OnCreateOptionsMenu(Menu menu)
        {
            MenuItem menuItem;

            if (Flash)
            {
                menuItem = menu.Add(Menu.NONE, Resource.Id.menu_flash, 0, Resource.String.flash_on);
            }
            else
            {
                menuItem = menu.Add(Menu.NONE, Resource.Id.menu_flash, 0, Resource.String.flash_off);
            }
            menuItem.SetShowAsAction(MenuItem.SHOW_AS_ACTION_NEVER);

            if (AutoFocus)
            {
                menuItem = menu.Add(Menu.NONE, Resource.Id.menu_auto_focus, 0, Resource.String.auto_focus_on);
            }
            else
            {
                menuItem = menu.Add(Menu.NONE, Resource.Id.menu_auto_focus, 0, Resource.String.auto_focus_off);
            }
            menuItem.SetShowAsAction(MenuItem.SHOW_AS_ACTION_NEVER);

            menuItem = menu.Add(Menu.NONE, Resource.Id.menu_formats, 0, Resource.String.formats);
            menuItem.SetShowAsAction(MenuItem.SHOW_AS_ACTION_NEVER);

            menuItem = menu.Add(Menu.NONE, Resource.Id.menu_camera_selector, 0, Resource.String.select_camera);
            menuItem.SetShowAsAction(MenuItem.SHOW_AS_ACTION_NEVER);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(MenuItem item)
        {
            // Handle presses on the action bar items
            switch (item.GetItemId())
            {
                case Resouce.Id.menu_flash:
                    Flash = !Flash;
                    if (Flash)
                    {
                        item.SetTitle(Resource.String.flash_on);
                    }
                    else
                    {
                        item.SetTitle(Resource.String.flash_off);
                    }
                    ScannerView.SetFlash(Flash);
                    return true;
                case Resource.Id.menu_auto_focus:
                    AutoFocus = !AutoFocus;
                    if (AutoFocus)
                    {
                        item.SetTitle(Resource.String.auto_focus_on);
                    }
                    else
                    {
                        item.SetTitle(Resource.String.auto_focus_off);
                    }
                    ScannerView.setAutoFocus(AutoFocus);
                    return true;
                case Resource.Id.menu_formats:
                    DialogFragment fragment = ZBarFormatSelectorDialogFragment.NewInstance(this, SelectedIndices);
                    fragment.Show(GetSupportFragmentManager(), "format_selector");
                    return true;
                case R.id.menu_camera_selector:
                    ScannerView.stopCamera();
                    DialogFragment fragment = CameraSelectorDialogFragment.NewInstance(this, CameraId);
                    fragment.Show(GetSupportFragmentManager(), "camera_selector");
                    return true;
                case Android.Resource.Id.home:
                    OnBackPressed();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override void HandleResult(Result rawResult)
        {
            Intent data = new Intent();
            data.PutExtra("Contents", rawResult.GetContents());
            data.PutExtra("Format", rawResult.GetBarcodeFormat().GetName());
            SetResult(RESULT_OK, data);
            Finish();
        }

        public override void OnFormatsSaved(ArrayList<int> selectedIndices)
        {
            SelectedIndices = selectedIndices;
            SetupFormats();
        }

        public override void OnCameraSelected(int cameraId)
        {
            CameraId = cameraId;
            ScannerView.StartCamera(CameraId);
            ScannerView.SetFlash(Flash);
            ScannerView.SetAutoFocus(AutoFocus);
        }

        private void SetupFormats()
        {
            List<BarcodeFormat> formats = new ArrayList<>();
            if (SelectedIndices == null || SelectedIndices.isEmpty())
            {
                mSelectedIndices = new ArrayList<>();
                for (int i = 0; i < BarcodeFormat.ALL_FORMATS.size(); i++)
                {
                    mSelectedIndices.Add(i);
                }
            }
            foreach (int index in SelectedIndices)
            {
                formats.Add(BarcodeFormat.ALL_FORMATS.Get(index));
            }
            if (ScannerView != null)
            {
                ScannerView.SetFormats(formats);
            }
        }

        private void CloseFormatsDialog()
        {
            CloseDialog("format_selector");
        }

        private void CloseDialog(string dialogName)
        {
            FragmentManager fragmentManager = GetSupportFragmentManager();
            DialogFragment fragment = (DialogFragment)fragmentManager.FindFragmentByTag(dialogName);
            if (fragment != null) fragment.Dismiss();
        }
    }
}