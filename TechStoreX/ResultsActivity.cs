using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;

namespace TechStoreX
{
    [Activity(Label = "@string/title_activity_results")]
    public class ResultsActivity : AppActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_results);
            Android.Support.V7.App.ActionBar actionBar = SupportActionBar;
            if (actionBar != null) actionBar.SetDisplayHomeAsUpEnabled(true);

            TableLayout tl = FindViewById<TableLayout>(Resource.Id.tablelayout_contents);
            TableRow tr;
            TextView tv;

            List<string[]> data = ReadData();
            foreach (string[] row in data)
            {
                tr = new TableRow(this);
                foreach (string cell in row)
                {
                    tv = new TextView(this);
                    tv.Background = ContextCompat.GetDrawable(this, Android.Resource.Drawable.EditBoxBackground);
                    tv.Text = cell;
                    tr.AddView(tv);

                }
                tl.AddView(tr);
            }

            string[] header = Resources.GetStringArray(Resource.Array.results_headers);
            tr = FindViewById<TableRow>(Resource.Id.tablerow_header);
            foreach (string cell in header)
            {
                tv = new TextView(this)
                {
                    Background = ContextCompat.GetDrawable(this, Android.Resource.Drawable.EditBoxBackground),
                    Text = cell
                };
                tr.AddView(tv);
            }
            tl.ViewTreeObserver.GlobalLayout += (sender, args) =>
            {
                TableRow trH;
                TextView tvH;
                trH = FindViewById<TableRow>(Resource.Id.tablerow_header);
                // if (tl.ChildCount > 0)
                for (int i = 0; i < tl.ChildCount; i++)
                {
                    // tr = (TableRow)tl.GetChildAt(0);
                    tr = (TableRow)tl.GetChildAt(i);
                    for (int j = 0; j < tr.ChildCount; j++)
                    {
                        tv = (TextView)tr.GetChildAt(j);
                        tvH = (TextView)trH.GetChildAt(j);
                        tvH.SetWidth(tv.Width);
                    }
                }
            };
        }

        private List<string[]> ReadData()
        {
            List<string[]> data = new List<string[]>();
            if (Android.OS.Environment.ExternalStorageState == Android.OS.Environment.MediaMounted)
            {
                string path = ((int)Build.VERSION.SdkInt >= 19) ?
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
                            data.Add(line.Split("\t"));
                        }
                    }
                }
                catch (Exception e)
                {
                    Snackbar.Make(FindViewById(Resource.Id.fab), e.Message, Snackbar.LengthLong).Show();
                }
            }
            //string state = Environment.getExternalStorageState();
            //if (Environment.MEDIA_MOUNTED.equals(state)) {
            //    File file = (Build.VERSION.SDK_INT >= 19) ?
            //            new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOCUMENTS),
            //                    getString(R.string.app_name)
            //                            + "/" + getString(R.string.file_name)) :
            //            new File(Environment.getExternalStorageDirectory(),
            //                    "Documents/" + getString(R.string.app_name)
            //                            + "/" + getString(R.string.file_name));
            //    FileReader fr = null;
            //    BufferedReader br = null;
            //    String line;
            //    try {
            //        fr = new FileReader(file);
            //        br = new BufferedReader(fr);

            //        while ((line = br.readLine()) != null) {
            //            // use tab as separator
            //            String[] row = line.split("\t");
            //            data.add(row);
            //        }

            //    } catch (FileNotFoundException e) {
            //        Snackbar.make(findViewById(R.id.fab), e.getMessage(), Snackbar.LENGTH_LONG).show();
            //    } catch (IOException e) {
            //        Snackbar.make(findViewById(R.id.fab), e.getMessage(), Snackbar.LENGTH_LONG).show();
            //    } finally {
            //        try {
            //            if (br != null) br.close();
            //            if (fr != null) fr.close();
            //        } catch (IOException e) {
            //            Snackbar.make(findViewById(R.id.fab), e.getMessage(), Snackbar.LENGTH_LONG).show();
            //        }
            //    }
            //}
            return data;
        }

        protected override void ProcessBarcode(string data)
        {
            // dummy
        }
    }
}
