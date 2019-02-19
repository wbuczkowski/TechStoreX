using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TechStoreX
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class LoginActivity : Activity
    {
        private const string KEY_PREF_ENABLE_DATAWEDGE = "pref_enable_datawedge";
        private const string KEY_PREF_USE_DATAWEDGE = "pref_use_datawedge";
        private const string KEY_PREF_ENABLE_CAMERA = "pref_enable_camera";
        private const string KEY_PREF_USE_CAMERA = "pref_use_camera";
        private const string EXIT_STRING = "GETMEOUT";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_login);
            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.SetOnClickListener(fabListener);
            fab.Click += FabOnClick;
            Button buttonLogin = FindViewById(Resource.Id.buttonLogin);
            buttonLogin.Click += (sender, args) =>
            {
                EditText editText = findViewById(Resource.Id.editUserName);
                string data = editText.Text;
                if (Regex.IsMatch(data, "[a-zA-Z]{3}.*"))
                {
                    // first three characters are letters
                    if (data == EXIT_STRING)
                    {
                        FinishAffinity();
                        System.Exit(0);
                    }
                    Intent intent = new Intent(editText.Context, typeof(MainActivity));
                    intent.PutExtra(Intent.ExtraText, data);
                    StartActivity(intent);
                }
                else
                {
                    Snackbar.Make(FindViewById(Resource.Id.fab),
                            Resource.String.login_error,
                            Snackbar.LengthLong).Show();
                }
            };
            InitDataWedge();
            InitCamera();
            // clear the saved data
            SharedPreferences sharedPref = getPreferences(MODE_PRIVATE);
            SharedPreferences.Editor editor = sharedPref.edit();
            editor.putString(EXTRA_WORK_ORDER, "");
            editor.putString(EXTRA_COST_CENTER, "");
            editor.putString(EXTRA_INVENTORY, "");
            editor.putString(EXTRA_PLANT, "");
            editor.putString(EXTRA_STORAGE_LOCATION, "");
            editor.putString(EXTRA_VENDOR, "");
            editor.apply();
            // // ATTENTION: This was auto-generated to handle app links.
            // Intent appLinkIntent = getIntent();
            // String appLinkAction = appLinkIntent.getAction();
            // Uri appLinkData = appLinkIntent.getData();
            // //noinspection StatementWithEmptyBody
            // if (Intent.ACTION_VIEW.equals(appLinkAction) && appLinkData != null)
            // {
            //     // TODO: get user id from URI
            //     // String recipeId = appLinkData.getLastPathSegment();
            //     // Uri appData = Uri.parse("content://com.recipe_app/recipe/").buildUpon().appendPath(recipeId).build();
            // }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_login, menu);
            return true;
        }

        public override void ProcessBarcode(string data)
        {
            if (Regex.IsMatch(data, "[a-zA-Z]{3}.*"))
            {
                if (data == EXIT_STRING)
                {
                    FinishAffinity();
                    System.Exit(0);
                }
                // first three characters are letters
                Intent intent = new Intent(this, typeof(MainActivity));
                intent.PutExtra(Intent.ExtraText, data);
                StartActivity(intent);
            }
            else
            {
                Snackbar.Make(FindViewById(Resource.Id.fab),
                        Resource.String.login_error,
                        Snackbar.LengthLong).Show();
            }
        }

        private void initCamera()
        {
            if (getPackageManager().hasSystemFeature(PackageManager.FEATURE_CAMERA))
            {
                // this device has a camera
                SharedPreferences sharedPref = PreferenceManager.getDefaultSharedPreferences(this);
                SharedPreferences.Editor editor = sharedPref.edit();
                editor.putBoolean(KEY_PREF_ENABLE_CAMERA, true);
                //            editor.putBoolean(KEY_PREF_USE_CAMERA, false);
                editor.apply();
            }
            else
            {
                // no camera on this device
                SharedPreferences sharedPref = PreferenceManager.getDefaultSharedPreferences(this);
                SharedPreferences.Editor editor = sharedPref.edit();
                editor.putBoolean(KEY_PREF_ENABLE_CAMERA, false);
                editor.putBoolean(KEY_PREF_USE_CAMERA, false);
                editor.apply();
            }
        }

        private void initDataWedge()
        {
            //All the DataWedge version does not support creating the profile using the DataWedge intent API.
            //To avoid crashes on the device, make sure to check the DtaaWedge version before creating the profile.
            const string DW_PKG_NAME = "com.symbol.datawedge";
            const string DW_INTENT_SUPPORT_VERSION = "6.3";

            int result = -1;
            // Find out current DW version, if the version is 6.3 or higher then we know it support intent config
            // Then we can send CartScan profile via intent
            try
            {
                PackageInfo pInfo = getPackageManager().getPackageInfo(DW_PKG_NAME, PackageManager.GET_META_DATA);
                String versionCurrent = pInfo.versionName;
                //            Log.i(TAG, "createProfileInDW: versionCurrent=" + versionCurrent);
                if (versionCurrent != null)
                    result = compareVersion(versionCurrent, DW_INTENT_SUPPORT_VERSION);
                //            Log.i(TAG, "onCreate: result=" + result);
            }
            catch (PackageManager.NameNotFoundException e1)
            {
                //            Log.e(TAG, "onCreate: NameNotFoundException:", e1);
            }
            if (result >= 0)
            {
                createDataWedgeProfile();
                SharedPreferences sharedPref = PreferenceManager.getDefaultSharedPreferences(this);
                SharedPreferences.Editor editor = sharedPref.edit();
                editor.putBoolean(KEY_PREF_ENABLE_DATAWEDGE, true);
                //            editor.putBoolean(KEY_PREF_USE_DATAWEDGE, true);
                editor.apply();
            }
            else
            {
                //            dataTextView.append("DataWedge version is " + versionCurrent + ", " +
                //                    "but the current Sample is only supported with DataWedge version 6.3 or greater.");
                // Disable DataWedge in preferences
                SharedPreferences sharedPref = PreferenceManager.getDefaultSharedPreferences(this);
                SharedPreferences.Editor editor = sharedPref.edit();
                editor.putBoolean(KEY_PREF_ENABLE_DATAWEDGE, false);
                editor.putBoolean(KEY_PREF_USE_DATAWEDGE, false);
                editor.apply();
            }
        }

        private static int compareVersion(string v1, string v2)
        {
            List<String> l1 = new ArrayList<>(Arrays.asList(
                    v1.replaceAll("\\s", "").split("\\.")));
            List<String> l2 = new ArrayList<>(Arrays.asList(
                    v2.replaceAll("\\s", "").split("\\.")));
            int i = 0;
            while (i < l1.size() && i < l2.size() && l1.get(i).equals(l2.get(i)))
            {
                i++;
            }
            if (i < l1.size() && i < l2.size())
            {
                int diff = Integer.valueOf(l1.get(i)).compareTo(Integer.valueOf(l2.get(i)));
                return Integer.signum(diff);
            }
            else
            {
                return Integer.signum(l1.size() - l2.size());
            }
        }

        private void createDataWedgeProfile()
        {
            //Create profile if doesn't exit and update the required settings
            const string ACTION = "com.symbol.datawedge.api.ACTION";
            const string SET_CONFIG = "com.symbol.datawedge.api.SET_CONFIG";
            //        final String CREATE_PROFILE = "com.symbol.datawedge.api.CREATE_PROFILE";
            //        final String SWITCH = "com.symbol.datawedge.api.SWITCH_TO_PROFILE";
            string packageName = getPackageName();
            string profileName = getString(Resource.String.app_name) + "App";
            {
                Bundle bConfig = new Bundle();
                Bundle bParams = new Bundle();
                Bundle configBundle = new Bundle();
                Bundle bundleApp1 = new Bundle();

                bParams.putString("scanner_selection", "auto");
                bParams.putString("intent_output_enabled", "true");
                bParams.putString("intent_action", packageName + ".SCAN");
                bParams.putString("intent_category", Intent.CATEGORY_DEFAULT);
                bParams.putString("intent_delivery", "2");

                configBundle.putString("PROFILE_NAME", profileName);
                configBundle.putString("PROFILE_ENABLED", "true");
                configBundle.putString("CONFIG_MODE", "CREATE_IF_NOT_EXIST");

                bundleApp1.putString("PACKAGE_NAME", packageName);
                bundleApp1.putStringArray("ACTIVITY_LIST", new String[] { packageName + ".SCAN" });

                configBundle.putParcelableArray("APP_LIST", new Bundle[] { bundleApp1 });

                bConfig.putString("PLUGIN_NAME", "INTENT");
                bConfig.putString("RESET_CONFIG", "false");

                bConfig.putBundle("PARAM_LIST", bParams);
                configBundle.putBundle("PLUGIN_CONFIG", bConfig);

                Intent i = new Intent();
                i.setAction(ACTION);
                i.putExtra(SET_CONFIG, configBundle);
                this.sendBroadcast(i);
            }

            //TO recieve the scanned via intent, the keystroke must disabled.
            {
                Bundle bConfig = new Bundle();
                Bundle bParams = new Bundle();
                Bundle configBundle = new Bundle();

                bParams.putString("keystroke_output_enabled", "false");

                configBundle.putString("PROFILE_NAME", profileName);
                configBundle.putString("PROFILE_ENABLED", "true");
                configBundle.putString("CONFIG_MODE", "UPDATE");

                bConfig.putString("PLUGIN_NAME", "KEYSTROKE");
                bConfig.putString("RESET_CONFIG", "false");

                bConfig.putBundle("PARAM_LIST", bParams);
                configBundle.putBundle("PLUGIN_CONFIG", bConfig);

                Intent i = new Intent();
                i.setAction(ACTION);
                i.putExtra(SET_CONFIG, configBundle);
                this.sendBroadcast(i);
            }
        }
    }
}