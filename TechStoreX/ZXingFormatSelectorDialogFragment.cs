using Google.ZXing;
using Java.Lang;
using System.Collections.Generic;
using Android.OS;
using Android.App;

namespace TechStoreX
{
    public class ZXingFormatSelectorDialogFragment : Android.Support.V4.App.DialogFragment
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

        public static ZXingFormatSelectorDialogFragment NewInstance(IFormatSelectorDialogListener listener,
            IList<Integer> selectedIndices)
        {
            ZXingFormatSelectorDialogFragment fragment = new ZXingFormatSelectorDialogFragment();
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

            IList<BarcodeFormat> AllFormats = new List<BarcodeFormat> {
                BarcodeFormat.Aztec, BarcodeFormat.Codabar,
                BarcodeFormat.Code39, BarcodeFormat.Code93,BarcodeFormat.Code128,
                BarcodeFormat.DataMatrix, BarcodeFormat.Ean8,BarcodeFormat.Ean13,
                BarcodeFormat.Itf, BarcodeFormat.Maxicode,BarcodeFormat.Pdf417,
                BarcodeFormat.QrCode, BarcodeFormat.Rss14,BarcodeFormat.RssExpanded,
                BarcodeFormat.UpcA, BarcodeFormat.UpcE,BarcodeFormat.UpcEanExtension
            };

            string[] formats = new string[AllFormats.Count];
            bool[] checkedIndices = new bool[AllFormats.Count];
            int i = 0;
            foreach (BarcodeFormat format in AllFormats)
            {
                formats[i] = format.ToString();
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