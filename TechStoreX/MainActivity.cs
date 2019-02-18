using System;
using Android.App;
using Android.OS;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace TechStoreX
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppActivity, View.IOnClickListener
    {
        private const int RC_GET_DATA = 9101;
        private const string STATE_USERNAME = "userName";

        private string mUserName;
        private TextView mStatus;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.App.ActionBar actionBar = SupportActionBar;
            if (actionBar != null) actionBar.SetDisplayHomeAsUpEnabled(true);
            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.SetOnClickListener(fabListener);
            fab.Click += FabOnClick;
            Button button = FindViewById(Resource.Id.button_goods_issue);
            button.SetOnClickListener(this);
            button = FindViewById(Resource.Id.button_goods_return);
            button.SetOnClickListener(this);
            button = FindViewById(Resource.Id.button_inventory);
            button.SetOnClickListener(this);
            button = FindViewById(Resource.Id.button_display);
            button.SetOnClickListener(this);
            if (savedInstanceState != null)
            {
                mUserName = savedInstanceState.GetString(STATE_USERNAME);
            }
            else
            {
                mUserName = Intent.GetStringExtra(Intent.ExtraText);
            }
            mStatus = FindViewById(Resource.Id.text_status);
            SetStatusText();
        }

        private void SetStatusText()
        {
            // TODO
            throw new NotImplementedException();
        }

        protected override void OnSaveInstanceState(Bundle savedInstanceState)
        {
            // Save the user name
            savedInstanceState.PutString(STATE_USERNAME, mUserName);

            // Always call the superclass so it can save the view hierarchy state
            base.OnSaveInstanceState(savedInstanceState);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }





        public void OnClick(View v)
        {
            // TODO
            throw new NotImplementedException();
        }

        protected override void ProcessBarcode(string data)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}

