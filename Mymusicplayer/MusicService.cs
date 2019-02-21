using System;
using Android.App;
using Android.Media;
using Android.OS;
using Android.Content;
using System.Collections.Generic;
using Android.Provider;
using Android.Util;
using Android.Support.V4.App;

namespace MyMusicPlayer
{
    //其中[Service]负责在AndroidManifest.xml注册服务，自动生成服务
    //以下也会自动注册到AndroidManifest中，不需要再手动写，否则会报错！
    [Service]
	public class MusicService : Service, MediaPlayer.IOnPreparedListener, MediaPlayer.IOnErrorListener, MediaPlayer.IOnCompletionListener
	{
		public int SongPos { get; set; }
        private string second;
        private string minutes;
        private string timeString;

        MusicBinder binder;
        //已定义为全局静态字段
		public static MediaPlayer player;

		public override void OnCreate ()
		{
			base.OnCreate ();

			SongPos = 0;
			player = new MediaPlayer ();
			InitMusicPlayer ();
		}

		void InitMusicPlayer ()
		{
			player.SetWakeMode (Application.Context, WakeLockFlags.Partial);
			player.SetAudioStreamType (Stream.Music);
			player.SetOnPreparedListener (this);
			player.SetOnCompletionListener (this);
			player.SetOnErrorListener (this);
		}

		public override IBinder OnBind (Intent intent)
		{
			binder = new MusicBinder (this);
			return binder;
		}

		public override bool OnUnbind (Intent intent)
		{
			player.Stop ();
			player.Release ();
			return false;
		}

		public void PlaySong ()
		{
            if (MusicControls.musicThread.IsAlive)
            {
                MusicControls.ThreadHandler(1);
            }
            player.Reset ();
            //是因为以下没在主页展示，所以调用时为空指针，报错。先生成再播放
            MusicControls.imgPlay.SetImageResource(MusicControls.PAUSE_RESOURCE);
            var trackUri = ContentUris.WithAppendedId (MediaStore.Audio.Media.ExternalContentUri, Songleton.Instance[SongPos].Id);
			try {
				player.SetDataSource (Application.Context, trackUri);
			} catch (Exception e) {
				Log.Error (MainActivity.MUSIC_ERR_TAG, "Error setting data source", e);
			}
			player.PrepareAsync ();
		}

		public int GetPosn(){
			return player.CurrentPosition;
		}

		public int GetDur(){
			return player.Duration;
            
		}

		public bool IsPlay(){
			return player.IsPlaying;
		}
        //以下语句可能增加了static关键字
		public void PausePlayer(){
			player.Pause();
		}

		public void Seek(int pos){
			player.SeekTo(pos);
		}

		public void Go(){
			player.Start();
		}

		public void OnPrepared (MediaPlayer mp)
		{
			player.Start ();
            MusicControls.songName.Text = Songleton.Instance[SongPos].Title;
            MusicControls.songDuration.Text = getTime(player.Duration);
            MusicControls.ThreadHandler(0);
        }

        public string getTime(int t)
        {

            int min =(int) t / (1000 * 60);
            //同上
            int sec = (int)(t - min * 1000 * 60) / 1000;
            //拼接字符串
            if (sec<10&& min < 10)
            {
                second = "0" + sec.ToString();
                minutes = "0" + min.ToString();
                timeString = minutes + ":" + second;
            }

            if (sec>=10&&min < 10)
            {
                second =sec.ToString();
                minutes = "0" + min.ToString();
                timeString = minutes + ":" + second;
            }
            if (sec < 10 && min >= 10)
            {
                second = "0" + sec.ToString();
                minutes = min.ToString();
                timeString = minutes + ":" + second;
            }
            //string timeString = min.ToString() + ":" + sec.ToString();

            return timeString;
        }
        public bool OnError (MediaPlayer mp, MediaError what, int extra)
		{
			return false;
		}

        //判断应用是否在前台，在返回true否则返回false
        public Boolean IsAppOnForeground()
        {
            ActivityManager activityManager = (ActivityManager)ApplicationContext.GetSystemService(ActivityService);
            String packageName = ApplicationContext.PackageName;
            IList<ActivityManager.RunningAppProcessInfo> appProcesses = activityManager.RunningAppProcesses;

            if (appProcesses == null)
            {
                return false;
            }
            foreach (ActivityManager.RunningAppProcessInfo appProcess in appProcesses)
            {
                // The name of the process that this object is associated with.
                //if (appProcess.ProcessName.Equals(packageName)&& appProcess.Importance == ActivityManager.RunningAppProcessInfo.ImportanceForeground)
                if (appProcess.ProcessName.Equals(packageName) && appProcess.Importance == Android.App.Importance.Foreground)
                {
                    
                    return true;
                }
                
            }

            return false;
        }
        public void OnCompletion (MediaPlayer mp)
		{
            //播放下一首,但是要判断一下是不是最后一首。
            if (Songleton.Instance.AllSongs.Count != SongPos+1)
            {
                MusicControls.imgPlay.SetImageResource(MusicControls.PAUSE_RESOURCE);

                //Activity.ActionBar.SetDisplayHomeAsUpEnabled(true);
                //判断应用是否在前台
                if (IsAppOnForeground()==true)
                {
                    MusicControls.imgNext.PerformClick();

                }
                else
                {
                    SongPos++;
                    PlaySong();

                }

            }
            else
            {


            }

        }
	}

	public class MusicBinder : Binder
	{
		MusicService service;

		public MusicBinder (MusicService service)
		{
			this.service = service;
		}

		public MusicService GetService ()
		{
			return service;
		}



    }
}

