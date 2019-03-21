using System;
using System.Collections.Generic;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Preferences;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;

namespace TechStoreX
{
    public abstract class AppActivity : AppCompatActivity, LogoutTimerUtility.ILogOutListener
    {
        protected const string EXTRA_OPTION = "OPTION";
        protected const string EXTRA_WORK_ORDER = "WORK_ORDER";
        protected const string EXTRA_COST_CENTER = "COST_CENTER";
        protected const string EXTRA_MATERIAL = "MATERIAL";
        protected const string EXTRA_PLANT = "PLANT";
        protected const string EXTRA_STORAGE_LOCATION = "STORAGE_LOCATION";
        protected const string EXTRA_BIN = "BIN";
        protected const string EXTRA_QUANTITY = "QUANTITY";
        protected const string EXTRA_DATE = "DATE";
        protected const string EXTRA_INVENTORY = "INVENTORY";
        protected const string EXTRA_VENDOR = "VENDOR";

        protected const string OPTION_GOODS_ISSUE = "1";
        protected const string OPTION_GOODS_RETURN = "2";
        protected const string OPTION_INVENTORY_WITH_DOCUMENT = "4";
        protected const string OPTION_INVENTORY_WO_DOCUMENT = "3";

        private const string ACTION = "com.symbol.datawedge.api.ACTION";
        private const string SOFT_SCAN_TRIGGER = "com.symbol.datawedge.api.SOFT_SCAN_TRIGGER";
        private const string START_SCANNING = "START_SCANNING";

        private const int RC_BARCODE_CAPTURE_ZBAR_LIB = 9001;
        private const int RC_BARCODE_CAPTURE_ZXING_LIB = 9002;

        private const string KEY_PREF_TIMEOUT = "pref_timeout";

        private const string VALUE_PREF_TIMEOUT_DEFAULT = "300000";

        private const string KEY_PREF_SCAN_TECHNOLOGY = "pref_scan_technology";

        private const string VALUE_PREF_SCAN_NONE = "";
        private const string VALUE_PREF_SCAN_DATAWEDGE = "DataWedge";
        private const string VALUE_PREF_SCAN_ZXING = "ZXing";
        private const string VALUE_PREF_SCAN_ZBAR_LIB = "ZBarLib";
        private const string VALUE_PREF_SCAN_ZXING_LIB = "ZXingLib";

        private int logoutTime = 300000;

        private string scanTech;

        protected override void OnResume()
        {
            base.OnResume();
            // read preferences
            ISharedPreferences sharedPref = PreferenceManager.GetDefaultSharedPreferences(this);
            scanTech = sharedPref.GetString(KEY_PREF_SCAN_TECHNOLOGY, VALUE_PREF_SCAN_NONE);
            if (scanTech == VALUE_PREF_SCAN_NONE)
            {
                Snackbar.Make(FindViewById(Resource.Id.fab),
                        "Please select scanning technology!", Snackbar.LengthLong)
                        .SetAction(Resource.String.action_settings, view =>
                        {
                            Intent intent = new Intent(view.Context, typeof(SettingsActivity));
                            intent.PutExtra(PreferenceActivity.ExtraShowFragment,
                                // typeof(SettingsActivity.ScannerPreferenceFragment).Name
                                "TechStoreX.SettingsActivity.ScannerPreferenceFragment");
                            intent.PutExtra(PreferenceActivity.ExtraNoHeaders, true);
                            StartActivity(intent);
                        }).Show();
            }
            //if (useDataWedge) {
            else if (scanTech == VALUE_PREF_SCAN_DATAWEDGE)
            {
                //Register for the intent to receive the scanned data using intent callback.
                //The action and category name used must be same as the names used in the profile creation.
                IntentFilter filter = new IntentFilter();
                filter.AddAction(PackageName + ".SCAN");
                filter.AddCategory(Intent.CategoryDefault);
                RegisterReceiver(receiver, filter);
            }
            // check permissions in runtime
            if ((int)Build.VERSION.SdkInt >= 23) RequestPermissions();
            // set up logout timer
            logoutTime = int.Parse(sharedPref.GetString(KEY_PREF_TIMEOUT, VALUE_PREF_TIMEOUT_DEFAULT));
            LogoutTimerUtility.StartLogoutTimer(this, this, logoutTime);
        }

        protected override void OnPause()
        {
            base.OnPause();
            //Unregister the intent reciever when the app goes to background.
            if (scanTech == VALUE_PREF_SCAN_DATAWEDGE)
            {
                UnregisterReceiver(receiver);
            }
            LogoutTimerUtility.StopLogoutTimer();
        }

        public override void OnUserInteraction()
        {
            base.OnUserInteraction();
            LogoutTimerUtility.StartLogoutTimer(this, this, logoutTime);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle action bar item clicks here. The action bar will
            // automatically handle clicks on the Home/Up button, so long
            // as you specify a parent activity in AndroidManifest.xml.
            if (item.ItemId == Resource.Id.action_settings)
            {
                // launch settings activity
                StartActivity(new Intent(this, typeof(SettingsActivity)));
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        /**
     * Called when an activity you launched exits, giving you the requestCode
     * you started it with, the resultCode it returned, and any additional
     * data from it.  The <var>resultCode</var> will be
     * {@link #RESULT_CANCELED} if the activity explicitly returned that,
     * didn't return any result, or crashed during its operation.
     * <p/>
     * <p>You will receive this call immediately before onResume() when your
     * activity is re-starting.
     * <p/>
     *
     * @param requestCode The integer request code originally supplied to
     *                    startActivityForResult(), allowing you to identify who this
     *                    result came from.
     * @param resultCode  The integer result code returned by the child activity
     *                    through its setResult().
     * @param intent      An Intent, which can return result data to the caller
     *                    (various data can be attached to Intent "extras").
     * @see #startActivityForResult
     * @see #createPendingResult
     * @see #setResult(int)
     */

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            switch (requestCode)
            {
                //            case RC_BARCODE_CAPTURE_GOOGLE_VISION:
                //                //Google Vision
                //                if (resultCode == CommonStatusCodes.SUCCESS) {
                //                    if (intent != null) {
                //                        Barcode barcode = intent.getParcelableExtra(BarcodeCaptureActivity.BarcodeObject);
                ////                        Snackbar.make(findViewById(R.id.fab),
                ////                                R.string.barcode_success + "\n" + barcode.displayValue,
                ////                                Snackbar.LENGTH_LONG).show();
                //                        processBarcode(barcode.displayValue);
                //                    } else {
                //                        Snackbar.make(findViewById(R.id.fab), R.string.barcode_failure,
                //                                Snackbar.LENGTH_LONG).show();
                //                    }
                //                } else {
                //                    Snackbar.make(findViewById(R.id.fab), String.format(getString(R.string.barcode_error),
                //                            CommonStatusCodes.getStatusCodeString(resultCode)),
                //                            Snackbar.LENGTH_LONG).show();
                //                }
                //                break;
                case RC_BARCODE_CAPTURE_ZBAR_LIB:
                case RC_BARCODE_CAPTURE_ZXING_LIB:
                    if (resultCode == Result.Ok)
                    {
                        if (intent != null)
                        {
                            String barcode = intent.GetStringExtra("Contents");
                            //                        Snackbar.make(findViewById(R.id.fab),
                            //                                R.string.barcode_success + "\n" + barcode,
                            //                                Snackbar.LENGTH_LONG).show();
                            ProcessBarcode(barcode);
                        }
                        else
                        {
                            Snackbar.Make(FindViewById(Resource.Id.fab),
                                Resource.String.barcode_failure,
                                Snackbar.LengthLong).Show();
                        }
                    }
                    else
                    {
                        Snackbar.Make(FindViewById(Resource.Id.fab),
                            Resource.String.barcode_error,
                            Snackbar.LengthLong).Show();
                    }
                    break;
 /*               case IntentIntegrator.REQUEST_CODE:
                    //ZXing
                    if (resultCode == Result.Ok)
                    {
                        if (intent != null)
                        {
                            //retrieve scan result
                            IntentResult scanningResult =
                                    IntentIntegrator.parseActivityResult(requestCode, resultCode, intent);
                            if (scanningResult != null)
                            {
                                //we have a result
                                ProcessBarcode(scanningResult.getContents());
                            }
                            else
                            {
                                Snackbar.Make(FindViewById(Resource.Id.fab), Resource.String.barcode_failure,
                                        Snackbar.LengthLong).Show();
                            }
                        }
                        else
                        {
                            Snackbar.Make(FindViewById(Resource.Id.fab), Resource.String.barcode_failure,
                                    Snackbar.LengthLong).Show();
                        }
                    }
                    else
                    {
                        Snackbar.Make(FindViewById(Resource.Id.fab), Resource.String.barcode_error,
                                Snackbar.LengthLong).Show();
                    }
                    break;*/
                default:
                    base.OnActivityResult(requestCode, resultCode, intent);
                    break;
            }
        }

        protected void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            AppActivity appActivity = (AppActivity)view.Context;
            Intent intent;
            switch (appActivity.scanTech)
            {
                case VALUE_PREF_SCAN_DATAWEDGE:
                    // start DataWedge soft-scanning
                    intent = new Intent();
                    intent.SetAction(ACTION);
                    intent.PutExtra(SOFT_SCAN_TRIGGER, START_SCANNING);
                    if (appActivity.FindDataWedgePackage(intent))
                    {
                        appActivity.SendBroadcast(intent);
                    }
                    else
                    {
                        Snackbar.Make(view, "DataWedge is not installed.\n"
                            + "Please select another scanning technology", 
                            Snackbar.LengthLong)
                            .SetAction(Resource.String.action_settings, v =>
                            {
                                Intent intent1 = new Intent(appActivity, typeof(SettingsActivity));
                                intent1.PutExtra(PreferenceActivity.ExtraShowFragment,
                                    // typeof(SettingsActivity.ScannerPreferenceFragment).Name
                                    "TechStoreX.SettingsActivity.ScannerPreferenceFragment");
                                intent1.PutExtra(PreferenceActivity.ExtraNoHeaders, true);
                                appActivity.StartActivity(intent1);
                            })
                            .Show();
                    }
                    break;
                //case VALUE_PREF_SCAN_ZXING:
                //    IntentIntegrator scanIntegrator = new IntentIntegrator(appActivity);
                //    scanIntegrator.initiateScan();
                //    break;
                //                case VALUE_PREF_SCAN_GOOGLEVISION:
                //                    // launch barcode capture activity
                //                    intent = new Intent(AppActivity.this, BarcodeCaptureActivity.class);
                //                    intent.putExtra(BarcodeCaptureActivity.AutoFocus, useAutoFocus);
                //                    intent.putExtra(BarcodeCaptureActivity.UseFlash, useFlash);
                //                    startActivityForResult(intent, RC_BARCODE_CAPTURE_GOOGLE_VISION);
                //                    break;
                case VALUE_PREF_SCAN_ZBAR_LIB:
                    intent = new Intent(appActivity, typeof(ZBarFullScannerActivity));
                    appActivity.StartActivityForResult(intent, RC_BARCODE_CAPTURE_ZBAR_LIB);
                    break;
                case VALUE_PREF_SCAN_ZXING_LIB:
                    intent = new Intent(appActivity, typeof(ZXingFullScannerActivity));
                    appActivity.StartActivityForResult(intent, RC_BARCODE_CAPTURE_ZXING_LIB);
                    break;
                case VALUE_PREF_SCAN_NONE:
                    Snackbar.Make(view, "Please select scanning technology", Snackbar.LengthLong)
                            .SetAction(Resource.String.action_settings, v =>
                            {

                                Intent intent1;
                                intent1 = new Intent(appActivity, typeof(SettingsActivity));
                                intent1.PutExtra(PreferenceActivity.ExtraShowFragment,
                                    typeof(SettingsActivity.ScannerPreferenceFragment).Name);
                                intent1.PutExtra(PreferenceActivity.ExtraNoHeaders, true);
                                appActivity.StartActivity(intent1);
                            })
                            .Show();
                    break;
            }
        }


        private bool FindDataWedgePackage(Intent intent)
        {
            PackageManager pm = PackageManager;
            IList<ResolveInfo> availableApps = pm.QueryIntentActivities(intent, 
                PackageInfoFlags.MatchDefaultOnly);
            if (availableApps != null)
            {
                foreach (ResolveInfo resolveInfo in availableApps)
                {
                    if (resolveInfo.ResolvePackageName == "com.symbol.datawedge")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //Broadcast Receiver for receiving the intents back from DataWedge
        private readonly BroadcastReceiver receiver = new DWBroadcastReceiver();

        class DWBroadcastReceiver : BroadcastReceiver
        {
            const String LABEL_TYPE_TAG = "com.symbol.datawedge.label_type";
            const String DATA_STRING_TAG = "com.symbol.datawedge.data_string";
            //  final String DECODE_DATA_TAG = "com.symbol.datawedge.decode_data";

            public override void OnReceive(Context context, Intent intent)
            {
                String action = intent.Action;
                AppActivity appActivity = (AppActivity)context;
                if (action != null && action == (appActivity.PackageName + ".SCAN"))
                {
                    String labelType = intent.GetStringExtra(LABEL_TYPE_TAG);
                    String decodeString = intent.GetStringExtra(DATA_STRING_TAG);
                    Snackbar.Make(appActivity.FindViewById(Resource.Id.fab),
                                    Resource.String.barcode_success +
                                    "\nType:\t" + labelType +
                                    "\nValue:\t" + decodeString,
                            Snackbar.LengthLong).Show();
                    appActivity.ProcessBarcode(decodeString);
                }
            }
        };

        /**
         * Performing idle time logout
         */

        public void DoLogout()
        {
            StartActivity(new Intent(this, typeof(LoginActivity)));
        }

        protected void ProcessBarcode(String data);

        protected void RequestPermissions();
    }
}