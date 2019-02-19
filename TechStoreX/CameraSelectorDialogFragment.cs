namespace TechStoreX
{
    public class CameraSelectorDialogFragment : DialogFragment
    {
        public interface CameraSelectorDialogListener
        {
            void OnCameraSelected(int cameraId);
        }

        private int CameraId;
        private CameraSelectorDialogListener Listener;

        public void OnCreate(Bundle state)
        {
            base.OnCreate(state);
            SetRetainInstance(true);
        }

        public static CameraSelectorDialogFragment NewInstance(CameraSelectorDialogListener listener, int cameraId)
        {
            CameraSelectorDialogFragment fragment = new CameraSelectorDialogFragment();
            fragment.CameraId = cameraId;
            fragment.Listener = listener;
            return fragment;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            if (Listener == null)
            {
                Dismiss();
                return null;
            }

            int numberOfCameras = Camera.GetNumberOfCameras();
            string[] cameraNames = new string[numberOfCameras];
            int checkedIndex = 0;

            for (int i = 0; i < numberOfCameras; i++)
            {
                Camera.CameraInfo info = new Camera.CameraInfo();
                Camera.GetCameraInfo(i, info);
                if (info.facing == Camera.CameraInfo.CAMERA_FACING_FRONT)
                {
                    cameraNames[i] = "Front Facing";
                }
                else if (info.facing == Camera.CameraInfo.CAMERA_FACING_BACK)
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

            AlertDialog.Builder builder = new AlertDialog.Builder(GetActivity());
            // Set the dialog title
            builder.SetTitle(Resource.String.select_camera)
                // Specify the list array, the items to be selected by default (null for none),
                // and the listener through which to receive callbacks when items are selected
                .SetSingleChoiceItems(cameraNames, checkedIndex,
                    (sender, args) => { CameraId = (int)args.Which; })
                // Set the action buttons
                .SetPositiveButton(Android.Resource.String.ok,
                    (sender, args) => { if (Listener != null) Listener.OnCameraSelected(CameraId); })
                .SetNegativeButton(Android.Resource.String.cancel,
                    (sender, args) => { });
            return builder.Create();
        }
    }
}