using Android.App;
using Android.OS;
using Android.Views;
using Java.Lang;
using ME.Dm7.Barcodescanner.Zxing;
using System.Collections.Generic;
using Android.Content;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using Google.ZXing;

namespace TechStoreX
{
    [Activity(Label = "@string/title_activity_zxing")]
    public class ZXingFullScannerActivity : AppActivity, ZXingScannerView.IResultHandler,
        ZXingFormatSelectorDialogFragment.IFormatSelectorDialogListener,
        CameraSelectorDialogFragment.ICameraSelectorDialogListener
    {
        private const string FLASH_STATE = "FLASH_STATE";
        private const string AUTO_FOCUS_STATE = "AUTO_FOCUS_STATE";
        private const string SELECTED_FORMATS = "SELECTED_FORMATS";
        private const string CAMERA_ID = "CAMERA_ID";
        private const int REQUEST_PREMISSION_CAMERA = 9101;
        private ZXingScannerView ScannerView;
        private bool Flash;
        private bool AutoFocus;
        private IList<Integer> SelectedIndices;
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
            SetContentView(Resource.Layout.activity_zxing);
            //        Toolbar toolbar = findViewById(R.id.toolbar);
            //        setSupportActionBar(toolbar);
            Android.Support.V7.App.ActionBar actionBar = SupportActionBar;
            if (actionBar != null) actionBar.SetDisplayHomeAsUpEnabled(true);
            ViewGroup contentFrame = FindViewById<ViewGroup>(Resource.Id.content_frame);
            ScannerView = new ZXingScannerView(this);
            SetupFormats();
            contentFrame.AddView(ScannerView);
        }

        protected override void OnPause()
        {
            base.OnPause();
            ScannerView.StopCamera();
            CloseFormatsDialog();
        }

        protected override void OnResume()
        {
            base.OnResume();
            ScannerView.SetResultHandler(this);
            ScannerView.StartCamera(CameraId);
            ScannerView.Flash = Flash;
            ScannerView.SetAutoFocus(AutoFocus);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutBoolean(FLASH_STATE, Flash);
            outState.PutBoolean(AUTO_FOCUS_STATE, AutoFocus);
            outState.PutIntegerArrayList(SELECTED_FORMATS, SelectedIndices);
            outState.PutInt(CAMERA_ID, CameraId);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            IMenuItem menuItem;

            if (Flash)
            {
                menuItem = menu.Add(Menu.None, Resource.Id.menu_flash, 0, Resource.String.flash_on);
            }
            else
            {
                menuItem = menu.Add(Menu.None, Resource.Id.menu_flash, 0, Resource.String.flash_off);
            }
            menuItem.SetShowAsAction(ShowAsAction.Never);

            if (AutoFocus)
            {
                menuItem = menu.Add(Menu.None, Resource.Id.menu_auto_focus, 0, Resource.String.auto_focus_on);
            }
            else
            {
                menuItem = menu.Add(Menu.None, Resource.Id.menu_auto_focus, 0, Resource.String.auto_focus_off);
            }
            menuItem.SetShowAsAction(ShowAsAction.Never);

            menuItem = menu.Add(Menu.None, Resource.Id.menu_formats, 0, Resource.String.formats);
            menuItem.SetShowAsAction(ShowAsAction.Never);

            menuItem = menu.Add(Menu.None, Resource.Id.menu_camera_selector, 0, Resource.String.select_camera);
            menuItem.SetShowAsAction(ShowAsAction.Never);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle presses on the action bar items
            DialogFragment fragment;
            switch (item.ItemId)
            {
                case Resource.Id.menu_flash:
                    Flash = !Flash;
                    if (Flash)
                    {
                        item.SetTitle(Resource.String.flash_on);
                    }
                    else
                    {
                        item.SetTitle(Resource.String.flash_off);
                    }
                    ScannerView.Flash = Flash;
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
                    ScannerView.SetAutoFocus(AutoFocus);
                    return true;
                case Resource.Id.menu_formats:
                    fragment = ZXingFormatSelectorDialogFragment.NewInstance(this, SelectedIndices);
                    fragment.Show(SupportFragmentManager, "format_selector");
                    return true;
                case Resource.Id.menu_camera_selector:
                    ScannerView.StopCamera();
                    fragment = CameraSelectorDialogFragment.NewInstance(this, CameraId);
                    fragment.Show(SupportFragmentManager, "camera_selector");
                    return true;
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public void HandleResult(Google.ZXing.Result rawResult)
        {
            Intent data = new Intent();
            data.PutExtra("Contents", rawResult.Text);
            data.PutExtra("Format", rawResult.BarcodeFormat.ToString());
            SetResult(Android.App.Result.Ok, data);
            Finish();
        }

        private void CloseFormatsDialog()
        {
            CloseDialog("format_selector");
        }

        private void CloseDialog(string dialogName)
        {
            FragmentManager fragmentManager = SupportFragmentManager;
            DialogFragment fragment = (DialogFragment)fragmentManager.FindFragmentByTag(dialogName);
            if (fragment != null) fragment.Dismiss();

        }

        public void OnFormatsSaved(IList<Integer> selectedIndices)
        {
            SelectedIndices = selectedIndices;
            SetupFormats();
        }

        public void OnCameraSelected(int cameraId)
        {
            CameraId = cameraId;
            ScannerView.StartCamera(CameraId);
            ScannerView.Flash = Flash;
            ScannerView.SetAutoFocus(AutoFocus);
        }

        private void SetupFormats()
        {
            List<BarcodeFormat> formats = new List<BarcodeFormat>();
            if (SelectedIndices == null)
            {
                SelectedIndices = new List<Integer>();
                for (int i = 0; i < ZXingScannerView.AllFormats.Count; i++)
                {
                    SelectedIndices.Add(Integer.ValueOf(i));
                }
            }
            foreach (int index in SelectedIndices)
            {
                formats.Add(ZXingScannerView.AllFormats[index] as BarcodeFormat);
            }
            if (ScannerView != null)
            {
                ScannerView.SetFormats(formats);
            }
        }

        protected override void ProcessBarcode(string data)
        {
            throw new System.NotImplementedException();
        }

        protected override void RequestPermissions()
        {
            base.RequestPermissions();
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != (int)Permission.Granted)
            {
                // Camera permission is not granted. If necessary display rationale & request.
                // if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Camera))
                // {
                //     // Provide an additional rationale to the user if the permission was not granted
                //     // and the user would benefit from additional context for the use of the permission.
                //     // For example if the user has previously denied the permission.
                //     Snackbar.Make(FindViewById(Resource.Id.fab),
                //                    Resource.String.permission_write_rationale,
                //                    Snackbar.LengthIndefinite)
                //             .SetAction(Android.Resource.String.Ok,
                //                        new Action<View>(delegate (View obj)
                //                        {
                //                            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.Camera }, REQUEST_PREMISSION_CAMERA);
                //                        }
                //             )
                //     ).Show();
                // }
                // else
                // {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.Camera }, REQUEST_PREMISSION_CAMERA);
                // }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == REQUEST_PREMISSION_CAMERA)
            {
                // If request is cancelled, the result arrays are empty.
                if (grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED)
                {
                    // permission was granted, yay!
                    // Continue
                }
                else
                {
                    // permission denied, boo!
                    // Cannot continue, finish...
                    Finish();
                }
            }
            else
            {
                // other permissions this app might request.
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }
    }
}