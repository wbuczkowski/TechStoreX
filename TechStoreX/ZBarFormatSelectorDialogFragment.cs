using System.Collections.Generic;
using Android.App;
using Android.OS;
using Java.Lang;
using ME.Dm7.Barcodescanner.Zbar;

namespace TechStoreX
{
    public class ZBarFormatSelectorDialogFragment : Android.Support.V4.App.DialogFragment
    {
        public interface IFormatSelectorDialogListener
        {
            void OnFormatsSaved(IList<Integer> selectedIndices);
        }

        private IList<Integer> SelectedIndices;
        private IFormatSelectorDialogListener Listener;

        public override void OnCreate(Bundle state)
        {
            base.OnCreate(state);
            RetainInstance=true;
        }

        public static ZBarFormatSelectorDialogFragment NewInstance(IFormatSelectorDialogListener listener,
            IList<Integer> selectedIndices)
        {
            ZBarFormatSelectorDialogFragment fragment = new ZBarFormatSelectorDialogFragment();
            if (selectedIndices == null)
            {
                selectedIndices = new List<Integer>();
            }
            fragment.SelectedIndices = new List<Integer>(selectedIndices);
            fragment.Listener = listener;
            return fragment;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            if (SelectedIndices == null || Listener == null)
            {
                Dismiss();
                return null;
            }

            string[] formats = new string[BarcodeFormat.AllFormats.Count ];
            bool[] checkedIndices = new bool[BarcodeFormat.AllFormats.Count];
            int i = 0;
            foreach (BarcodeFormat format in BarcodeFormat.AllFormats)
            {
                formats[i] = format.Name;
                checkedIndices[i] = SelectedIndices.Contains(Integer.ValueOf(i));
                i++;
            }

            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
            // Set the dialog title
            builder.SetTitle(Resource.String.choose_formats)
                // Specify the list array, the items to be selected by default (null for none),
                // and the listener through which to receive callbacks when items are selected
                .SetMultiChoiceItems(formats, checkedIndices,
                    (sender, args) =>
                    {
                        if ((bool)args.IsChecked)
                        {
                            // If the user checked the item, add it to the selected items
                            SelectedIndices.Add(Integer.ValueOf(args.Which));
                        }
                        else if (SelectedIndices.Contains(Integer.ValueOf(args.Which)))
                        {
                            // Else, if the item is already in the array, remove it
                            SelectedIndices.Remove(Integer.ValueOf(args.Which));
                        }
                    })
                // Set the action buttons
                .SetPositiveButton(Android.Resource.String.Ok,
                    (sender, args) =>
                    {
                        if (Listener != null) Listener.OnFormatsSaved(SelectedIndices);
                    })
                .SetNegativeButton(Android.Resource.String.Cancel, (sender, args) => { });
            return builder.Create();
        }
    }
}