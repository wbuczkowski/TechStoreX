using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Preferences;
using Android.Content.Res;
using Android.Graphics;
using Java.Lang;

namespace TechStoreX
{
    public abstract class AppCompatPreferenceActivity : PreferenceActivity
    {
        private AppCompatDelegate Delegate;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            GetDelegate().InstallViewFactory();
            GetDelegate().OnCreate(savedInstanceState);
            base.OnCreate(savedInstanceState);
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            GetDelegate().OnPostCreate(savedInstanceState);
        }

        ActionBar GetSupportActionBar()
        {
            return GetDelegate().SupportActionBar;
        }

        public MenuInflater GetMenuInflater()
        {
            return GetDelegate().MenuInflater;
        }

        public override void SetContentView(int layoutResID)
        {
            GetDelegate().SetContentView(layoutResID);
        }

        public override void SetContentView(View view)
        {
            GetDelegate().SetContentView(view);
        }

        public override void SetContentView(View view, ViewGroup.LayoutParams layoutParams)
        {
            GetDelegate().SetContentView(view, layoutParams);
        }

        public override void AddContentView(View view, ViewGroup.LayoutParams layoutParams)
        {
            GetDelegate().AddContentView(view, layoutParams);
        }

        protected override void OnPostResume()
        {
            base.OnPostResume();
            GetDelegate().OnPostResume();
        }

        protected override void OnTitleChanged(ICharSequence title, Color color)
        {
            base.OnTitleChanged(title, color);
            GetDelegate().SetTitle(title);
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            GetDelegate().OnConfigurationChanged(newConfig);
        }

        protected override void OnStop()
        {
            base.OnStop();
            GetDelegate().OnStop();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GetDelegate().OnDestroy();
        }

        public override void InvalidateOptionsMenu()
        {
            GetDelegate().InvalidateOptionsMenu();
        }

        private AppCompatDelegate GetDelegate()
        {
            if (Delegate == null)
            {
                Delegate = AppCompatDelegate.Create(this, null);
            }
            return Delegate;
        }
    }
}
