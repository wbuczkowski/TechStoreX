using Android.Preference;

namespace TechStoreX
{
    [Activity(Label = "@string/title_activity_settings")]
    public class SettingsActivity : AppCompatPreferenceActivity, Preference.IOnPreferenceChangeListener
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ActionBar actionBar = GetSupportActionBar();
            if (actionBar != null) actionBar.SetDisplayHomeAsUpEnabled(true);
        }

        /**
         * A preference value change listener that updates the preference's summary
         * to reflect its new value.
         */
        public bool OnPreferenceChange(Preference preference, Object value)
        {
            string stringValue = value.toString();

            if (preference is ListPreference)
            {
                // For list preferences, look up the correct display value in
                // the preference's 'entries' list.
                ListPreference listPreference = (ListPreference)preference;
                int index = listPreference.FindIndexOfValue(stringValue);

                // Set the summary to reflect the new value.
                if (index >= 0)
                {
                    preference.SetSummary(listPreference.GetEntries()[index]);
                }
                else
                {
                    preference.SetSummary(Resource.String.pref_not_selected);
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
            else
            {
                // validate values
                switch (preference.GetKey())
                {
                    case "pref_default_plant":
                        if (stringValue.Length != 4)
                        {
                            Toast.MakeText(preference.GetContext(),
                                    Resource.String.plant_length,
                                    Toast.LengthShort).Show();
                            return false;
                        }
                    case "pref_default_storage_location":
                        if (stringValue.Length != 4)
                        {
                            Toast.MakeText(preference.GetContext(),
                                    Resource.String.storage_location_length,
                                    Toast.LengthShort).Show();
                            return false;
                        }
                }
                // For all other preferences, set the summary to the value's
                // simple string representation.
                preference.SetSummary(stringValue);
            }
            return true;
        }

        /**
         * Helper method to determine if the device has an extra-large screen. For
         * example, 10" tablets are extra-large.
         */
        private sealed bool IsXLargeTablet(Context context)
        {
            return (context.GetResources().GetConfiguration().screenLayout
                    & Configuration.SCREENLAYOUT_SIZE_MASK) >= Configuration.SCREENLAYOUT_SIZE_XLARGE;
        }

        public bool onIsMultiPane()
        {
            return isXLargeTablet(this);
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
            preference.OnPreferenceChangeListener = this;

            // Trigger the listener immediately with the preference's
            // current value.
            OnPreferenceChange(preference,
                    String.valueOf(PreferenceManager
                            .getDefaultSharedPreferences(preference.getContext())
                            .getString(preference.getKey(), "")));
        }

        public override void OnBuildHeaders(List<Header> target)
        {
            loadHeadersFromResource(Resource.xml.pref_headers, target);
        }

        /**
         * This method stops fragment injection in malicious applications.
         * Make sure to deny any unknown fragments here.
         */
        protected boolean isValidFragment(string fragmentName)
        {
            return typeof(PreferenceFragment).Name == fragmentName
                || typeof(ScannerPreferenceFragment).Name == fragmentName
                || typeof(DefaultsPreferenceFragment).Name == fragmentName;
        }

        public static class ScannerPreferenceFragment : PreferenceFragment
        {
            public override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(savedInstanceState);
                AddPreferencesFromResource(Resource.xml.pref_scanner);
                SetHasOptionsMenu(true);
                BindPreferenceSummaryToValue(FindPreference("pref_scan_technology"));
            }
            public override bool OnOptionsItemSelected(MenuItem item)
            {
                int id = item.GetItemId();
                if (id == Android.Resource.Id.home)
                {
                    StartActivity(new Intent(GetActivity(), typeof(SettingsActivity)));
                    return true;
                }
                return base.OnOptionsItemSelected(item);
            }
        }

        public static class DefaultsPreferenceFragment : PreferenceFragment
        {
            public override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(savedInstanceState);
                AddPreferencesFromResource(Resource.xml.pref_defaults);
                SetHasOptionsMenu(true);

                // Bind the summaries of EditText/List/Dialog/Ringtone preferences
                // to their values. When their values change, their summaries are
                // updated to reflect the new value, per the Android Design
                // guidelines.
                BindPreferenceSummaryToValue(FindPreference("pref_timeout"));
                BindPreferenceSummaryToValue(FindPreference("pref_default_plant"));
                BindPreferenceSummaryToValue(FindPreference("pref_default_storage_location"));
            }
            public override bool OnOptionsItemSelected(MenuItem item)
            {
                int id = item.GetItemId();
                if (id == Android.Resource.Id.home)
                {
                    StartActivity(new Intent(GetActivity(), typeof(SettingsActivity)));
                    return true;
                }
                return base.OnOptionsItemSelected(item);
            }
        }
        public override bool OnOptionsItemSelected(MenuItem item)
        {
            int id = item.GetItemId();
            if (id == Android.Resource.Id.home)
            {
                OnBackPressed();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}