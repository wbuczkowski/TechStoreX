using System;
using System.Collections.Generic;
using System.Linq;

using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;

namespace TechStoreX
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class LoginActivity : AppActivity
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
            Button buttonLogin = FindViewById<Button>(Resource.Id.buttonLogin);
            buttonLogin.Click += (sender, args) =>
            {
                EditText editText = FindViewById<EditText>(Resource.Id.editUserName);
                string data = editText.Text;
                if (Regex.IsMatch(data, "[a-zA-Z]{3}.*"))
                {
                    // first three characters are letters
                    if (data == EXIT_STRING)
                    {
                        FinishAffinity();
                        System.Environment.Exit(0);
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
            ISharedPreferences sharedPref = GetPreferences(FileCreationMode.Private);
            ISharedPreferencesEditor editor = sharedPref.Edit();
            editor.PutString(EXTRA_WORK_ORDER, "");
            editor.PutString(EXTRA_COST_CENTER, "");
            editor.PutString(EXTRA_INVENTORY, "");
            editor.PutString(EXTRA_PLANT, "");
            editor.PutString(EXTRA_STORAGE_LOCATION, "");
            editor.PutString(EXTRA_VENDOR, "");
            editor.Apply();
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

        protected override void ProcessBarcode(string data)
        {
            if (Regex.IsMatch(data, "[a-zA-Z]{3}.*"))
            {
                if (data == EXIT_STRING)
                {
                    FinishAffinity();
                    System.Environment.Exit(0);
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

        private void InitCamera()
        {
            if (PackageManager.HasSystemFeature(PackageManager.FeatureCamera))
            {
                // this device has a camera
                ISharedPreferences sharedPref = PreferenceManager.GetDefaultSharedPreferences(this);
                ISharedPreferencesEditor editor = sharedPref.Edit();
                editor.PutBoolean(KEY_PREF_ENABLE_CAMERA, true);
                //            editor.putBoolean(KEY_PREF_USE_CAMERA, false);
                editor.Apply();
            }
            else
            {
                // no camera on this device
                ISharedPreferences sharedPref = PreferenceManager.GetDefaultSharedPreferences(this);
                ISharedPreferencesEditor editor = sharedPref.Edit();
                editor.PutBoolean(KEY_PREF_ENABLE_CAMERA, false);
                editor.PutBoolean(KEY_PREF_USE_CAMERA, false);
                editor.Apply();
            }
        }

        private void InitDataWedge()
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
                PackageInfo pInfo = PackageManager.GetPackageInfo(DW_PKG_NAME, PackageInfoFlags.MetaData);
                string versionCurrent = pInfo.VersionName;
                //            Log.i(TAG, "createProfileInDW: versionCurrent=" + versionCurrent);
                if (versionCurrent != null)
                    result = CompareVersion(versionCurrent, DW_INTENT_SUPPORT_VERSION);
                //            Log.i(TAG, "onCreate: result=" + result);
            }
            catch (PackageManager.NameNotFoundException)
            {
                //            Log.e(TAG, "onCreate: NameNotFoundException:", e1);
            }
            if (result >= 0)
            {
                CreateDataWedgeProfile();
                ISharedPreferences sharedPref = PreferenceManager.GetDefaultSharedPreferences(this);
                ISharedPreferencesEditor editor = sharedPref.Edit();
                editor.PutBoolean(KEY_PREF_ENABLE_DATAWEDGE, true);
                //            editor.putBoolean(KEY_PREF_USE_DATAWEDGE, true);
                editor.Apply();
            }
            else
            {
                //            dataTextView.append("DataWedge version is " + versionCurrent + ", " +
                //                    "but the current Sample is only supported with DataWedge version 6.3 or greater.");
                // Disable DataWedge in preferences
                ISharedPreferences sharedPref = PreferenceManager.GetDefaultSharedPreferences(this);
                ISharedPreferencesEditor editor = sharedPref.Edit();
                editor.PutBoolean(KEY_PREF_ENABLE_DATAWEDGE, false);
                editor.PutBoolean(KEY_PREF_USE_DATAWEDGE, false);
                editor.Apply();
            }
        }

        private static int CompareVersion(string v1, string v2)
        {
            List<string> l1 = Regex.Matches(Regex.Replace(v1, "\\s", ""), "\\.")
                .Cast<Match>().Select(match => match.Value).ToList();
            List<string> l2 = Regex.Matches(Regex.Replace(v2, "\\s", ""), "\\.")
                .Cast<Match>().Select(match => match.Value).ToList();
            int i = 0;
            while (i < l1.Count && i < l2.Count && l1[i] == l2[i])
            {
                i++;
            }
            if (i < l1.Count && i < l2.Count)
            {
                int diff = Int32.Parse(l1[i]) - (Int32.Parse(l2[i]));
                return Math.Sign(diff);
            }
            else
            {
                return Math.Sign(l1.Count - l2.Count);
            }
        }

        private void CreateDataWedgeProfile()
        {
            //Create profile if doesn't exit and update the required settings
            const string ACTION = "com.symbol.datawedge.api.ACTION";
            const string SET_CONFIG = "com.symbol.datawedge.api.SET_CONFIG";
            //        final String CREATE_PROFILE = "com.symbol.datawedge.api.CREATE_PROFILE";
            //        final String SWITCH = "com.symbol.datawedge.api.SWITCH_TO_PROFILE";
            string packageName = PackageName;
            string profileName = GetString(Resource.String.app_name) + "App";
            {
                Bundle bConfig = new Bundle();
                Bundle bParams = new Bundle();
                Bundle configBundle = new Bundle();
                Bundle bundleApp1 = new Bundle();

                bParams.PutString("scanner_selection", "auto");
                bParams.PutString("intent_output_enabled", "true");
                bParams.PutString("intent_action", packageName + ".SCAN");
                bParams.PutString("intent_category", Intent.CategoryDefault);
                bParams.PutString("intent_delivery", "2");

                configBundle.PutString("PROFILE_NAME", profileName);
                configBundle.PutString("PROFILE_ENABLED", "true");
                configBundle.PutString("CONFIG_MODE", "CREATE_IF_NOT_EXIST");

                bundleApp1.PutString("PACKAGE_NAME", packageName);
                bundleApp1.PutStringArray("ACTIVITY_LIST", new String[] { packageName + ".SCAN" });

                configBundle.PutParcelableArray("APP_LIST", new Bundle[] { bundleApp1 });

                bConfig.PutString("PLUGIN_NAME", "INTENT");
                bConfig.PutString("RESET_CONFIG", "false");

                bConfig.PutBundle("PARAM_LIST", bParams);
                configBundle.PutBundle("PLUGIN_CONFIG", bConfig);

                Intent i = new Intent();
                i.SetAction(ACTION);
                i.PutExtra(SET_CONFIG, configBundle);
                this.SendBroadcast(i);
            }

            //TO recieve the scanned via intent, the keystroke must disabled.
            {
                Bundle bConfig = new Bundle();
                Bundle bParams = new Bundle();
                Bundle configBundle = new Bundle();

                bParams.PutString("keystroke_output_enabled", "false");

                configBundle.PutString("PROFILE_NAME", profileName);
                configBundle.PutString("PROFILE_ENABLED", "true");
                configBundle.PutString("CONFIG_MODE", "UPDATE");

                bConfig.PutString("PLUGIN_NAME", "KEYSTROKE");
                bConfig.PutString("RESET_CONFIG", "false");

                bConfig.PutBundle("PARAM_LIST", bParams);
                configBundle.PutBundle("PLUGIN_CONFIG", bConfig);

                Intent i = new Intent();
                i.SetAction(ACTION);
                i.PutExtra(SET_CONFIG, configBundle);
                this.SendBroadcast(i);
            }
        }
    }
}