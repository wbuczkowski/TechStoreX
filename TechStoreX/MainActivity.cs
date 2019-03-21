using System;
using Android.App;
using Android.OS;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System.Text.RegularExpressions;
using System.IO;
using Android.Support.V4.Content;
using Android;
using Android.Content.PM;
using Android.Support.V4.App;

namespace TechStoreX
{
    [Activity(Label = "@string/app_name", ParentActivity = typeof(LoginActivity))]
    public class MainActivity : AppActivity, View.IOnClickListener
    {
        private const int RC_GET_DATA = 9101;
        private const int REQUEST_PREMISSION_WRITE = 9102;
        private const string STATE_USERNAME = "UserName";

        private string UserName;
        private TextView Status;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.App.ActionBar actionBar = SupportActionBar;
            if (actionBar != null) actionBar.SetDisplayHomeAsUpEnabled(true);
            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.SetOnClickListener(fabListener);
            fab.Click += FabOnClick;
            Button button = FindViewById<Button>(Resource.Id.button_goods_issue);
            button.SetOnClickListener(this);
            button = FindViewById<Button>(Resource.Id.button_goods_return);
            button.SetOnClickListener(this);
            button = FindViewById<Button>(Resource.Id.button_inventory);
            button.SetOnClickListener(this);
            button = FindViewById<Button>(Resource.Id.button_display);
            button.SetOnClickListener(this);
            if (savedInstanceState != null)
            {
                UserName = savedInstanceState.GetString(STATE_USERNAME);
            }
            else
            {
                UserName = Intent.GetStringExtra(Intent.ExtraText);
            }
            Status = FindViewById<TextView>(Resource.Id.text_status);
            SetStatusText();
        }

        protected override void OnSaveInstanceState(Bundle savedInstanceState)
        {
            // Save the user name
            savedInstanceState.PutString(STATE_USERNAME, UserName);

            // Always call the superclass so it can save the view hierarchy state
            base.OnSaveInstanceState(savedInstanceState);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public void OnClick(View view)
        {
            Intent intent;
            switch (view.Id)
            {
                case Resource.Id.button_goods_issue:
                    intent = new Intent(this, typeof(DetailActivity));
                    intent.PutExtra(EXTRA_OPTION, OPTION_GOODS_ISSUE);
                    StartActivityForResult(intent, RC_GET_DATA);
                    break;
                case Resource.Id.button_goods_return:
                    intent = new Intent(this, typeof(DetailActivity));
                    intent.PutExtra(EXTRA_OPTION, OPTION_GOODS_RETURN);
                    StartActivityForResult(intent, RC_GET_DATA);
                    break;
                case Resource.Id.button_inventory:
                    intent = new Intent(this, typeof(DetailActivity));
                    intent.PutExtra(EXTRA_OPTION, OPTION_INVENTORY_WO_DOCUMENT);
                    StartActivityForResult(intent, RC_GET_DATA);
                    break;
                case Resource.Id.button_display:
                    intent = new Intent(this, typeof(ResultsActivity));
                    StartActivity(intent);
                    break;
            }
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
         * @param data        An Intent, which can return result data to the caller
         *                    (various data can be attached to Intent "extras").
         * @see #startActivityForResult
         * @see #createPendingResult
         * @see #setResult(int)
         */
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            switch (requestCode)
            {
                case RC_GET_DATA:
                    if (resultCode == Result.Ok)
                    {
                        Snackbar.Make(FindViewById(Resource.Id.fab),
                            Resource.String.success,
                            Snackbar.LengthLong).Show();
                        if (WriteFile(intent)) SetStatusText();
                    }
                    else
                    {
                        Snackbar.Make(FindViewById(Resource.Id.fab),
                            Resource.String.cancelled,
                            Snackbar.LengthLong).Show();
                    }
                    break;
                default:
                    base.OnActivityResult(requestCode, resultCode, intent);
                    break;
            }
        }

        protected override void ProcessBarcode(string data)
        {
            string option = "", materialNumber = "",
                    workOrder = "", costCenter = "",
                    plant = "", storageLocation = "", bin = "",
                    inventory = "", vendor = "";
            string[] splitData = data.Split(" ");
            if (Regex.IsMatch(splitData[0], "\\d+"))
            {
                // starts with a digit
                switch (splitData[0].Length)
                {
                    case 10:
                        // this is a work order
                        option = OPTION_GOODS_ISSUE;
                        workOrder = splitData[0];
                        break;
                    case 9:
                        // this is a material
                        option = OPTION_GOODS_ISSUE;
                        materialNumber = splitData[0];
                        if (splitData.Length > 1)
                        {
                            plant = splitData[1];
                            if (splitData.Length > 2)
                            {
                                storageLocation = splitData[2];
                                if (splitData.Length > 3)
                                {
                                    bin = splitData[3];
                                }
                            }
                        }
                        break;
                }
            }
            else if (splitData[0].StartsWith("E")
                  || splitData[0].StartsWith("U"))
            {
                // this is an ERSA or UNBW material
                materialNumber = splitData[0].Substring(1, 10);
                if (Regex.IsMatch(materialNumber, "\\d{9}"))
                {
                    // material number correct
                    option = OPTION_GOODS_ISSUE;
                    if (splitData.Length > 1)
                    {
                        plant = splitData[1];
                        if (splitData.Length > 2)
                        {
                            storageLocation = splitData[2];
                            if (splitData.Length > 3)
                            {
                                bin = splitData[3];
                            }
                        }
                    }
                }
            }
            else if (splitData[0].StartsWith("K"))
            {
                // this is a vendor consignment material
                materialNumber = splitData[0].Substring(1, 10);
                if (Regex.IsMatch(materialNumber, "\\d{9}"))
                {
                    // material number correct
                    option = OPTION_GOODS_ISSUE;
                    if (splitData.Length > 1)
                    {
                        if (Regex.IsMatch(splitData[1], "\\d{9}"))
                        {
                            // this is a vendor code
                            vendor = splitData[1];
                        }
                        else
                        {
                            plant = splitData[1];
                            if (splitData.Length > 2)
                            {
                                storageLocation = splitData[2];
                                if (splitData.Length > 3)
                                {
                                    vendor = splitData[3];
                                    if (splitData.Length > 4)
                                    {
                                        bin = splitData[4];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (splitData[0].StartsWith("C"))
            {
                // this is a cost center
                option = OPTION_GOODS_ISSUE;
                costCenter = splitData[0].Substring(1);
            }
            else if (Regex.IsMatch(splitData[0], "V\\d{9}"))
            {
                // this is a vendor
                option = OPTION_GOODS_ISSUE;
                vendor = splitData[0].Substring(1);
            }
            else if (Regex.IsMatch(splitData[0], "I\\d{9}"))
            {
                // this is an inventory document
                option = OPTION_INVENTORY_WITH_DOCUMENT;
                inventory = splitData[0].Substring(1);
            }
            if (option.Length == 0)
            {
                Snackbar.Make(FindViewById(Resource.Id.fab),
                        Resource.String.barcode_unknown,
                        Snackbar.LengthLong).Show();
            }
            else
            {
                Intent intent = new Intent(this, typeof(DetailActivity));
                intent.PutExtra(EXTRA_OPTION, option);
                intent.PutExtra(EXTRA_WORK_ORDER, workOrder);
                intent.PutExtra(EXTRA_COST_CENTER, costCenter);
                intent.PutExtra(EXTRA_MATERIAL, materialNumber);
                intent.PutExtra(EXTRA_PLANT, plant);
                intent.PutExtra(EXTRA_STORAGE_LOCATION, storageLocation);
                intent.PutExtra(EXTRA_BIN, bin);
                intent.PutExtra(EXTRA_VENDOR, vendor);
                intent.PutExtra(EXTRA_INVENTORY, inventory);
                StartActivityForResult(intent, RC_GET_DATA);
            }
        }

        private void SetStatusText()
        {
            string statusText = GetString(Resource.String.text_status);
            int count = 0;
            if (Android.OS.Environment.ExternalStorageState == Android.OS.Environment.MediaMounted)
            {
                //File file = ((int)Build.VERSION.SdkInt >= 19) ?
                //        new File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments),
                //                GetString(Resource.String.app_name)
                //                        + "/" + GetString(Resource.String.file_name)) :
                //        new File(Android.OS.Environment.ExternalStorageDirectory,
                //                "Documents/" + GetString(Resource.String.app_name)
                //                        + "/" + GetString(Resource.String.file_name));
                //FileInputStream fis = null;
                //BufferedInputStream bis = null;
                //
                //try
                //{
                //    fis = new FileInputStream(file);
                //    bis = new BufferedInputStream(fis);

                //    byte[] c = new byte[1024];

                //    int readChars = bis.Read(c);
                //    if (readChars == -1)
                //    {
                //        // bail out if nothing to read
                //        statusText = statusText + "0";
                //        Status.Text = statusText;
                //        return;
                //    }

                //    // make it easy for the optimizer to tune this loop
                //    while (readChars == 1024)
                //    {
                //        for (int i = 0; i < 1024;)
                //        {
                //            if (c[i++] == '\n')
                //            {
                //                ++count;
                //            }
                //        }
                //        readChars = bis.Read(c);
                //    }

                //    // count remaining characters
                //    while (readChars != -1)
                //    {
                //        // System.out.println(readChars);
                //        for (int i = 0; i < readChars; ++i)
                //        {
                //            if (c[i] == '\n')
                //            {
                //                ++count;
                //            }
                //        }
                //        readChars = bis.Read(c);
                //    }
                //}
                //catch (IOException e)
                //{
                //    Snackbar.Make(FindViewById(Resource.Id.fab),
                //        e.Message, Snackbar.LengthLong).Show();
                //    statusText = statusText + "0";
                //    Status.Text = statusText;
                //    return;
                //}
                //finally
                //{
                //    try
                //    {
                //        if (bis != null) bis.Close();
                //        if (fis != null) fis.Close();
                //    }
                //    catch (IOException e)
                //    {
                //        Snackbar.Make(FindViewById(Resource.Id.fab),
                //            e.Message, Snackbar.LengthLong).Show();
                //    }
                //}
                /*************************************************/
                /* C# code to read file using System.IO         */
                /*************************************************/
                // slower!!!
                string path = (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat) ?
                    Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath :
                    Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/Documents";
                path = Path.Combine(Path.Combine(path, GetString(Resource.String.app_name)), GetString(Resource.String.file_name));
                try
                {
                    using (StreamReader streamReader = new StreamReader(path))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            count++;
                        }
                    }
                }
                catch (Exception e)
                {
                    Snackbar.Make(FindViewById(Resource.Id.fab), e.Message, Snackbar.LengthLong).Show();
                }
            }
            statusText = statusText + count;
            Status.Text = statusText;
        }

        private string PrepareData(Intent intent)
        {
            string data = intent.GetStringExtra(EXTRA_OPTION);
            // if (data == null || data.isEmpty())
            if (data.Length == 0)
            {
                // wrong intent?
                return null;
            }
            // write option
            string dataLine = data;

            switch (data)
            {
                case OPTION_GOODS_ISSUE:
                case OPTION_GOODS_RETURN:
                    data = intent.GetStringExtra(EXTRA_WORK_ORDER);
                    //if (data != null && !data.isEmpty())
                    if (data.Length != 0)
                    {
                        // write work order
                        dataLine = dataLine + "\t" + data;
                    }
                    else
                    {
                        // no work order, take cost center
                        data = intent.GetStringExtra(EXTRA_COST_CENTER);
                        // if (data != null && !data.isEmpty())
                        if (data.Length != 0)
                        {
                            // write cost center
                            dataLine = dataLine + "\t" + data;
                        }
                        else
                        {
                            // no work order and no cost center
                            return null;
                        }
                    }
                    break;
                case OPTION_INVENTORY_WO_DOCUMENT:
                case OPTION_INVENTORY_WITH_DOCUMENT:
                    // write spacer
                    dataLine = dataLine + "\t";
                    break;
                default:
                    // unknown option
                    return null;
            }

            // write material number etc.
            string[][] ii = {
                new string[] {EXTRA_MATERIAL, "true"},
                new string[] {EXTRA_PLANT, "true"},
                new string[] {EXTRA_STORAGE_LOCATION, "true"},
                new string[] {EXTRA_BIN, "false"},
                new string[] {EXTRA_QUANTITY, "true"}
                };
            // for (int i = 0; i<map.length;i++){
            foreach (string[] i in ii)
            {
                data = intent.GetStringExtra(i[0]);
                // if (data != null && !data.isEmpty())
                if (data.Length != 0)
                { // write data
                    dataLine = dataLine + "\t" + data;
                }
                else
                { // no data, quit if mandatory
                    if (bool.Parse(i[1]))
                    { // mandatory: quit
                        return null;
                    }
                    else
                    { // not mandatory: write spacer
                        dataLine = dataLine + "\t";
                    }
                }
            }

            // write user name
            dataLine = dataLine + "\t" + UserName;

            //write date
            data = intent.GetStringExtra(EXTRA_DATE);
            // if (data != null && !data.isEmpty())
            if (data.Length != 0)
            { // write data
                dataLine = dataLine + "\t" + data;
            }
            else
            {
                return null;
            }

            // write inventory number, if provided
            if (data == OPTION_INVENTORY_WITH_DOCUMENT)
            {
                // write inventory document
                data = intent.GetStringExtra(EXTRA_INVENTORY);
                // if (data != null && !data.isEmpty())
                if (data.Length != 0)
                { // write data
                    dataLine = dataLine + "\t" + data;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // write spacer
                dataLine = dataLine + "\t";
            }
            // write vendor
            data = intent.GetStringExtra(EXTRA_VENDOR);
            // if (data != null && !data.isEmpty())
            if (data.Length != 0)
            {
                // write vendor
                dataLine = dataLine + "\t" + data;
            }
            else
            {
                // write spacer
                dataLine = dataLine + "\t";
            }

            // success: write end of line
            // needed in C# ????
            // dataLine = dataLine + "\n";

            return dataLine;
        }

        private bool WriteFile(Intent intent)
        {
            if (Android.OS.Environment.ExternalStorageState == Android.OS.Environment.MediaMounted)
            {
                //    File file = ((int)Build.VERSION.SdkInt >= 19) ?
                //            new File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments),
                //                    GetString(Resource.String.app_name)) :
                //            new File(Android.OS.Environment.ExternalStorageDirectory,
                //                    "Documents/" + GetString(Resource.String.app_name));

                //    if (!file.Exists() && !file.Mkdirs())
                //    {
                //        Snackbar.Make(FindViewById(Resource.Id.fab),
                //            Resource.String.directory_error, Snackbar.LengthLong).Show();
                //        return false;
                //    }
                //    file = new File(file.Path + "/" + GetString(Resource.String.file_name));
                //    if (!file.Exists())
                //    {
                //        try
                //        {
                //            if (!file.CreateNewFile())
                //            {
                //                Snackbar.Make(FindViewById(Resource.Id.fab),
                //                    Resource.String.file_error, Snackbar.LengthLong).Show();
                //                return false;
                //            }
                //        }
                //        catch (IOException e)
                //        {
                //            Snackbar.Make(FindViewById(Resource.Id.fab),
                //                e.Message, Snackbar.LengthLong).Show();
                //            return false;
                //        }
                //    }
                //    BufferedWriter bw = null;
                //    FileWriter fw = null;

                //    string dataLine = PrepareData(intent);

                //    try
                //    {
                //        fw = new FileWriter(file, true);
                //        bw = new BufferedWriter(fw);
                //        if (dataLine != null) bw.Write(dataLine);
                //        return true;
                //    }
                //    catch (IOException e)
                //    {
                //        Snackbar.Make(FindViewById(Resource.Id.fab),
                //            e.Message, Snackbar.LengthLong).Show();
                //        return false;
                //    }
                //    finally
                //    {
                //        try
                //        {
                //            if (bw != null) bw.Close();
                //            if (fw != null) fw.Close();
                //        }
                //        catch (IOException e)
                //        {
                //            Snackbar.Make(FindViewById(Resource.Id.fab),
                //                e.Message, Snackbar.LengthLong).Show();
                //        }
                //    }
                //}

                /*************************************************/
                /* C# code to write file using System.IO         */
                /*************************************************/
                string path = (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat) ?
                    Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath :
                    Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/Documents";
                path = Path.Combine(path, GetString(Resource.String.app_name));
                try
                {
                    if (!Directory.Exists(path) && !Directory.CreateDirectory(path).Exists)
                    {
                        Snackbar.Make(FindViewById(Resource.Id.fab), Resource.String.directory_error, Snackbar.LengthLong).Show();
                        return false;
                    }
                    string file = Path.Combine(path, GetString(Resource.String.file_name));
                    using (StreamWriter streamWriter = new StreamWriter(file, true))
                    {
                        streamWriter.WriteLine(PrepareData(intent));
                    }
                }
                catch (Exception e)
                {
                    Snackbar.Make(FindViewById(Resource.Id.fab), e.Message, Snackbar.LengthLong).Show();
                    return false;
                }
                return true;
            }
            else
            {
                Snackbar.Make(FindViewById(Resource.Id.fab),
                    Resource.String.storage_error, Snackbar.LengthLong).Show();
                return false;
            }
        }

        protected override void RequestPermissions()
        {
            base.RequestPermissions();
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
            {
                // Write external storage permission is not granted. If necessary display rationale & request.
                // if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.WriteExternalStorage))
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
                //                            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.WriteExternalStorage }, REQUEST_PREMISSION_WRITE);
                //                        }
                //             )
                //     ).Show();
                // }
                // else
                // {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.WriteExternalStorage }, REQUEST_PREMISSION_WRITE);
                // }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == REQUEST_PREMISSION_WRITE)
            {
                // If request is cancelled, the result arrays are empty.
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    // permission was granted, yay!
                    // Read the file and continue
                    SetStatusText();
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

