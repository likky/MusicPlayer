using Android.Support.V4.App;
using Android.OS;
using Android.Content;
using Android.Views;
using System;
using Android.Util;
using Android.App;
using Android.Widget;
using Android;
using Android.Content.PM;
//using Fragment = Android.Support.V4.App.Fragment;

namespace MyMusicPlayer
{
    //以下也会自动注册到AndroidManifest中，不需要再手动写，否则会报错！
    [Activity (Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
	public class MainActivity : FragmentActivity
    {
        //以下类MusicService已定义全局，所以里面的方法都可以访问
        public static MusicService musicService = null;
		public static bool musicBound = false;

        public static String DISPLAY_OPT_KEY = "MenuDisplayOptions";
		public static String SONG_INDEX_KEY = "SongIndexKey";
		public static String MUSIC_ERR_TAG = "MusicErrorLogTag";
        long currentBackTime = 0;
        long lastBackTime = 0;

        MusicConnection musicConn;
		Intent playIntent;
        int PERMISSION_ReadExternalStorage = 1;

        protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView (Resource.Layout.Main);

            //java.lang.SecurityException: Permission Denial: reading com.android.providers.media.MediaProvider uri content://media/external/audio/media from pid=17271, uid=10085 requires android.permission.READ_EXTERNAL_STORAGE
            //6.0及以上需要动态申请权限
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
            {
                //RequestWriteSettings();
                bool isGrant = ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) == Permission.Granted;
                if (isGrant == false)
                {
                    ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadExternalStorage }, PERMISSION_ReadExternalStorage);
                }
                else
                {
                    Songleton.Instance.AllSongs = MusicRetrieval.LoadSongs(this);
                    if (savedInstanceState == null)
                    {
                        SongListFragment songListFragment = new SongListFragment();
                        //MusicControlFragment musicControlFragment = new MusicControlFragment ();
                        MusicControls musicControls = new MusicControls();
                        //var trans = FragmentManager.BeginTransaction ();以下两条语句极度重要，解决大问题！
                        Android.Support.V4.App.FragmentManager fragmentManager = SupportFragmentManager;
                        Android.Support.V4.App.FragmentTransaction trans = fragmentManager.BeginTransaction();
                        trans.Add(Resource.Id.fragment_container, songListFragment, "songListFragment");
                        trans.Add(Resource.Id.music_controls_container, musicControls, "musicControls");
                        trans.Commit();
                    }
                    //在程序中获取string.xml中字符串和数值
                    Toast.MakeText(this, this.Resources.GetString(Resource.String.scan_songs) + Songleton.Instance.AllSongs.Count.ToString(), ToastLength.Short).Show();
                }
            }

		}

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == PERMISSION_ReadExternalStorage)
            {
                if (grantResults[0] == Permission.Granted)
                {
                    Songleton.Instance.AllSongs = MusicRetrieval.LoadSongs(this);
                        SongListFragment songListFragment = new SongListFragment();
                        //MusicControlFragment musicControlFragment = new MusicControlFragment ();
                        MusicControls musicControls = new MusicControls();
                        //var trans = FragmentManager.BeginTransaction ();以下两条语句极度重要，解决大问题！
                        Android.Support.V4.App.FragmentManager fragmentManager = SupportFragmentManager;
                        Android.Support.V4.App.FragmentTransaction trans = fragmentManager.BeginTransaction();
                        trans.Add(Resource.Id.fragment_container, songListFragment, "songListFragment");
                        trans.Add(Resource.Id.music_controls_container, musicControls, "musicControls");
                        trans.Commit();

                    if (Build.VERSION.SdkInt != Android.OS.BuildVersionCodes.NMr1)
                        Toast.MakeText(this, "读取数据权限已申请", ToastLength.Short).Show();
                }
                else
                {
                    if (Build.VERSION.SdkInt != Android.OS.BuildVersionCodes.NMr1)
                        Toast.MakeText(this, "申请读取数据权限被拒", ToastLength.Short).Show();
                }
            }

        }
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            //捕获返回键按下的事件
            if (keyCode == Keycode.Back)
            {
                //获取当前系统时间的毫秒数
                currentBackTime = Java.Lang.JavaSystem.CurrentTimeMillis();
                //比较上次按下返回键和当前按下返回键的时间差，如果大于2秒，则提示再按一次退出
                if (currentBackTime - lastBackTime > 2000)
                {
                    Toast.MakeText(this, Resource.String.quit, ToastLength.Short).Show();
                    lastBackTime = currentBackTime;
                }
                else
                { //如果两次按下的时间差小于2秒，则退出程序
                  //this.Finish();不仅仅是退出应用
                    StopService(new Intent(this, typeof(MusicService)));
                    this.Finish();

                    //Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                    //Java.Lang.JavaSystem.Exit(0);
                }
                return true;
            }
            return base.OnKeyDown(keyCode, e);

        }
        protected override void OnStart ()
		{
			base.OnStart ();
			if (playIntent == null) {
				playIntent = new Intent(this, typeof(MusicService));
				musicConn = new MusicConnection ();
				BindService(playIntent, musicConn, Bind.AutoCreate);
				StartService(playIntent);
			}
		}
			
		protected override void OnDestroy() 
		{
			StopService(playIntent);
            musicService = null;
			base.OnDestroy();
		}

		public override void OnAttachedToWindow () 
		{
			base.OnAttachedToWindow();
			try {
				//MusicControlFragment.controller.Show (0);
			} catch (Exception e) {
				Log.Error (MUSIC_ERR_TAG, "Error showing media controller", e);
			}
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId) {
			case Android.Resource.Id.Home:
				FragmentManager.PopBackStack ();
				ActionBar.SetDisplayHomeAsUpEnabled (false);
				return true;  
			default:
				return base.OnOptionsItemSelected (item);
			}
		}

		protected override void OnSaveInstanceState(Bundle outState) 
		{
			base.OnSaveInstanceState(outState);
			outState.PutInt(DISPLAY_OPT_KEY, (int)ActionBar.DisplayOptions);
		}

		protected override void OnRestoreInstanceState(Bundle savedInstanceState) 
		{
			base.OnRestoreInstanceState(savedInstanceState);
			int savedDisplayOpt = savedInstanceState.GetInt(DISPLAY_OPT_KEY);
			if (savedDisplayOpt != 0) {
				ActionBar.DisplayOptions = (ActionBarDisplayOptions)savedDisplayOpt;
			}
		}

		class MusicConnection : Java.Lang.Object, IServiceConnection
		{
			public void OnServiceConnected (ComponentName name, IBinder service)
			{
				var binder = service as MusicBinder;
				if (binder != null) {
					MainActivity.musicService = binder.GetService ();
					MainActivity.musicBound = true;
				}
			}

			public void OnServiceDisconnected (ComponentName name)
			{
				MainActivity.musicBound = false;
			}
		}
	}
}