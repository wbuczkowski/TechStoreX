using Android.App;
using Android.Hardware;
using Android.OS;


namespace TechStoreX
{
    public class CameraSelectorDialogFragment : Android.Support.V4.App.DialogFragment
    {
        public interface ICameraSelectorDialogListener
        {
            void OnCameraSelected(int cameraId);
        }

        private int CameraId;
        private ICameraSelectorDialogListener Listener;

        public override void OnCreate(Bundle state)
        {
            base.OnCreate(state);
            RetainInstance = true;
        }

        public static CameraSelectorDialogFragment NewInstance(ICameraSelectorDialogListener listener, int cameraId)
        {
            CameraSelectorDialogFragment fragment = new CameraSelectorDialogFragment
            {
                CameraId = cameraId,
                Listener = listener
            };
            return fragment;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            if (Listener == null)
            {
                Dismiss();
                return null;
            }

            int numberOfCameras = Camera.NumberOfCameras;
            string[] cameraNames = new string[numberOfCameras];
            int checkedIndex = 0;

            for (int i = 0; i < numberOfCameras; i++)
            {
                Camera.CameraInfo info = new Camera.CameraInfo();
                Camera.GetCameraInfo(i, info);
                if (info.Facing == Camera.CameraInfo.CameraFacingFront)
                {
                    cameraNames[i] = "Front Facing";
                }
                else if (info.Facing == Camera.CameraInfo.CameraFacingBack)
                {
                    cameraNames[i] = "Rear Facing";
                }
                else
                {
                    cameraNames[i] = "Camera ID: " + i;
                }
                if (i == CameraId)
                {
                    checkedIndex = i;
                }
            }

            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
            // Set the dialog title
            builder.SetTitle(Resource.String.select_camera)
                // Specify the list array, the items to be selected by default (null for none),
                // and the listener through which to receive callbacks when items are selected
                .SetSingleChoiceItems(cameraNames, checkedIndex,
                    (sender, args) => { CameraId = (int)args.Which; })
                // Set the action buttons
                .SetPositiveButton(Android.Resource.String.Ok,
                    (sender, args) => { if (Listener != null) Listener.OnCameraSelected(CameraId); })
                .SetNegativeButton(Android.Resource.String.Cancel,
                    (sender, args) => { });
            return builder.Create();
        }
    }
}