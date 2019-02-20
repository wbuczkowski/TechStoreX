using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Preferences;
using Android.Views;
using Android.Widget;

namespace TechStoreX
{
    [Activity(Label = "@string/title_activity_settings")]
    public class SettingsActivity : AppCompatPreferenceActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Android.Support.V7.App.ActionBar actionBar = GetSupportActionBar();
            if (actionBar != null) actionBar.SetDisplayHomeAsUpEnabled(true);
        }

        /**
         * Helper method to determine if the device has an extra-large screen. For
         * example, 10" tablets are extra-large.
         */
        private bool IsXLargeTablet(Context context)
        {
            return (context.Resources.Configuration.ScreenLayout
                    & ScreenLayout.SizeMask) >= ScreenLayout.SizeXlarge;
        }

        public new bool IsMultiPane()
        {
            return IsXLargeTablet(this);
        }

        /**
         * Binds a preference's summary to its value. More specifically, when the
         * preference's value is changed, its summary (line of text below the
         * preference title) is updated to reflect the value. The summary is also
         * immediately updated upon calling this method. The exact display format is
         * dependent on the type of preference.
         *
         * @see #sBindPreferenceSummaryToValueListener
         */
        private static void BindPreferenceSummaryToValue(Preference preference)
        {
            // Set the listener to watch for value changes.
            preference.PreferenceChange += Preference_PreferenceChange;

            //preference.OnPreferenceChangeListener = new BindPreferenceSummaryToValueListener();

            // Trigger the listener immediately with the preference's
            // current value.

            Preference_PreferenceChange(preference, 
                new Preference.PreferenceChangeEventArgs(true, preference, 
                PreferenceManager.GetDefaultSharedPreferences(preference.Context).GetString(preference.Key, "")));
            //preference.OnPreferenceChangeListener.OnPreferenceChange(preference,
            //    PreferenceManager.GetDefaultSharedPreferences(preference.Context).GetString(preference.Key, ""));
        }

        private static void Preference_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            string stringValue = e.NewValue.ToString();

            if (sender is ListPreference listPreference)
            {
                // For list preferences, look up the correct display value in
                // the preference's 'entries' list.
                // ListPreference listPreference = (ListPreference)preference;
                int index = listPreference.FindIndexOfValue(stringValue);

                // Set the summary to reflect the new value.
                if (index >= 0)
                {
                    listPreference.Summary = listPreference.GetEntries()[index];
                }
                else
                {
                    listPreference.SetSummary(Resource.String.pref_not_selected);
                }

            }
            // else if (preference is RingtonePreference)
            // {
            //     // For ringtone preferences, look up the correct display value
            //     // using RingtoneManager.
            //     // if (TextUtils.isEmpty(stringValue))
            //     if (stringValue.Length == 0)
            //     {
            //         // Empty values correspond to 'silent' (no ringtone).
            //         preference.SetSummary(Resource.String.pref_ringtone_silent);

            //     }
            //     else
            //     {
            //         Ringtone ringtone = RingtoneManager.GetRingtone(
            //                 preference.GetContext(), Uri.Parse(stringValue));

            //         if (ringtone == null)
            //         {
            //             // Clear the summary if there was a lookup error.
            //             preference.SetSummary(null);
            //         }
            //         else
            //         {
            //             // Set the summary to reflect the new ringtone display
            //             // name.
            //             string name = ringtone.GetTitle(preference.GetContext());
            //             preference.SetSummary(name);
            //         }
            //     }

            // }
            else if (sender is Preference preference)
            {
                // validate values
                switch (preference.Key)
                {
                    case "pref_default_plant":
                        if (stringValue.Length != 4)
                        {
                            Toast.MakeText(preference.Context,
                                    Resource.String.plant_length,
                                    ToastLength.Short).Show();
                        }
                        break;
                    case "pref_default_storage_location":
                        if (stringValue.Length != 4)
                        {
                            Toast.MakeText(preference.Context,
                                    Resource.String.storage_location_length,
                                    ToastLength.Short).Show();
                        }
                        break;
                }
                // For all other preferences, set the summary to the value's
                // simple string representation.
                preference.Summary = stringValue;
            }
        }

        public override void OnBuildHeaders(IList<Header> target)
        {
            LoadHeadersFromResource(Resource.Xml.pref_headers, target);
        }

        /**
         * This method stops fragment injection in malicious applications.
         * Make sure to deny any unknown fragments here.
         */
        protected override bool IsValidFragment(string fragmentName)
        {
            return typeof(PreferenceFragment).Name == fragmentName
                || typeof(ScannerPreferenceFragment).Name == fragmentName
                || typeof(DefaultsPreferenceFragment).Name == fragmentName;
        }

        public class ScannerPreferenceFragment : PreferenceFragment
        {
            public override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(savedInstanceState);
                AddPreferencesFromResource(Resource.Xml.pref_scanner);
                SetHasOptionsMenu(true);
                BindPreferenceSummaryToValue(FindPreference("pref_scan_technology"));
            }
            public override bool OnOptionsItemSelected(IMenuItem item)
            {
                int id = item.ItemId;
                if (id == Android.Resource.Id.Home)
                {
                    StartActivity(new Intent(Activity, typeof(SettingsActivity)));
                    return true;
                }
                return base.OnOptionsItemSelected(item);
            }
        }

        public class DefaultsPreferenceFragment : PreferenceFragment
        {
            public override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(savedInstanceState);
                AddPreferencesFromResource(Resource.Xml.pref_defaults);
                SetHasOptionsMenu(true);

                // Bind the summaries of EditText/List/Dialog/Ringtone preferences
                // to their values. When their values change, their summaries are
                // updated to reflect the new value, per the Android Design
                // guidelines.
                BindPreferenceSummaryToValue(FindPreference("pref_timeout"));
                BindPreferenceSummaryToValue(FindPreference("pref_default_plant"));
                BindPreferenceSummaryToValue(FindPreference("pref_default_storage_location"));
            }
            public override bool OnOptionsItemSelected(IMenuItem item)
            {
                int id = item.ItemId;
                if (id == Android.Resource.Id.Home)
                {
                    StartActivity(new Intent(Activity, typeof(SettingsActivity)));
                    return true;
                }
                return base.OnOptionsItemSelected(item);
            }
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Android.Resource.Id.Home)
            {
                OnBackPressed();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}