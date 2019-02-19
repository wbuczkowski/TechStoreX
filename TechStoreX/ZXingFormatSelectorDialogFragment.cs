using Xamarin.Google.ZXing;
using System.Collections;
using ME.Dm7.Barcodescanner.Zxing.ZXingScannerView;

namespace TechStoreX
{
    public class ZXingFormatSelectorDialogFragment : DialogFragment
    {
        public interface FormatSelectorDialogListener
        {
            void OnFormatsSaved(ArrayList<int> selectedIndices);
        }

        private ArrayList<int> mSelectedIndices;
        private FormatSelectorDialogListener Listener;

        public void OnCreate(Bundle state)
        {
            base.OnCreate(state);
            SetRetainInstance(true);
        }

        public static ZXingFormatSelectorDialogFragment NewInstance(FormatSelectorDialogListener listener, 
            ArrayList<int> selectedIndices)
        {
            ZXingFormatSelectorDialogFragment fragment = new ZXingFormatSelectorDialogFragment();
            if (selectedIndices == null)
            {
                selectedIndices = new ArrayList<>();
            }
            fragment.SelectedIndices = new ArrayList<>(selectedIndices);
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

            String[] formats = new String[ZXingScannerView.ALL_FORMATS.size()];
            boolean[] checkedIndices = new boolean[ZXingScannerView.ALL_FORMATS.size()];
            int i = 0;
            foreach (BarcodeFormat format in ZXingScannerView.ALL_FORMATS)
            {
                formats[i] = format.toString();
                checkedIndices[i] = SelectedIndices.Contains(i);
                i++;
            }

            AlertDialog.Builder builder = new AlertDialog.Builder(GetActivity());
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
                            SelectedIndices.Add((int)args.Which);
                        }
                        else if (SelectedIndices.Contains((int)args.Which))
                        {
                            // Else, if the item is already in the array, remove it
                            SelectedIndices.Remove((int)args.Which);
                        }
                    })
                // Set the action buttons
                .SetPositiveButton(Android.Resource.String.ok,
                    (sender, args) =>
                    {
                        if (Listener != null) Listener.OnFormatsSaved(SelectedIndices);
                    })
                .SetNegativeButton(Android.Resource.String.cancel, (sender, args) => { });
            return builder.Create();
        }
    }
}