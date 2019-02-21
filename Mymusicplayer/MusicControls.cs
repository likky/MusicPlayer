using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using Android.App;Fragment使用默认的
//using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Content;
using Android.Util;
//using Android.App;
using Android.Support.V4.App;
using System.Threading;
using Android.Content;
using Android.Media;
using Android.Provider;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android.Database;
using Java.Net;

namespace MyMusicPlayer
{
    public class MusicControls:Fragment, SeekBar.IOnSeekBarChangeListener
    {
        public static int PLAY_RESOURCE =Resource.Drawable.ic_play_arrow_black_24dp;
        public static int PAUSE_RESOURCE = Resource.Drawable.ic_pause_black_24dp;

        private ImageView imgPrev;
        public static ImageView imgPlay;
        public static ImageView imgNext;
        //public static ImageView imgHome;
        public static ImageView imgRingtone;
        public static ImageView imgAlarm;
        public static TextView songName;
        public static TextView songDuration;
        private string second;
        private string minutes;
        private string timeString;

        //private TextView navSongPos;
        //private TextView navSongDur;
        //private TextView textview1;
        public static Thread musicThread = new Thread(new ThreadStart(SeekBarHandler));

        //For the seekerbar
        public static SeekBar seekBar;

        //public override void OnResume()
        //{
        //    if (MainActivity.musicService.IsPlay() == true)
        //    {
        //        imgPlay.SetImageResource(PLAY_RESOURCE);
        //        MusicControls.songName.Text = Songleton.Instance[MainActivity.musicService.SongPos].Title;
        //        MusicControls.songDuration.Text = getTime(MainActivity.musicService.GetDur());

        //    }
        //    base.OnResume();
        //}
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.Music_controls, container, false);
            imgPrev = (ImageView)view.FindViewById(Resource.Id.imgPrev);
            imgPlay = (ImageView)view.FindViewById(Resource.Id.imgPlay);
            imgNext = (ImageView)view.FindViewById(Resource.Id.imgNext);
            //imgHome= (ImageView)view.FindViewById(Resource.Id.home);
            songName = (TextView)view.FindViewById(Resource.Id.songname);
            songDuration = (TextView)view.FindViewById(Resource.Id.songduration);
            //imgRingtone= (ImageView)view.FindViewById(Resource.Id.ringtone);
            //imgAlarm = (ImageView)view.FindViewById(Resource.Id.alarm);

            seekBar = (SeekBar)view.FindViewById(Resource.Id.seekBar);
            //navSongPos = (TextView)view.FindViewById(Resource.Id.navSongPos);
            //navSongDur = (TextView)view.FindViewById(Resource.Id.navSongDur);
            //textview1= (TextView)view.FindViewById(Resource.Id.textView1);
            //对这个seekbar进行监听
            seekBar.SetOnSeekBarChangeListener(this);



            //imgHome.Click += delegate
            //{
            //    //Toast.MakeText(this.Context, "回到首页列表", ToastLength.Short).Show();
            //    SongListFragment songListFragment = new SongListFragment();

            //    var trans = FragmentManager.BeginTransaction();
            //    //trans = FragmentManager.BeginTransaction();
            //    trans.Replace(Resource.Id.fragment_container, songListFragment);
            //    trans.AddToBackStack(null);
            //    trans.Commit();
            //    Activity.ActionBar.SetDisplayHomeAsUpEnabled(false);
            //};

            //imgRingtone.Click += delegate
            //{

            //};

            //imgAlarm.Click += delegate
            //{
            //    SetAlarm(MainActivity.musicService.SongPos);
            //    //Toast.MakeText(this.Context, "闹钟设置成功", ToastLength.Short).Show();
            //};

            imgPrev.Click += delegate
            {
                if (MainActivity.musicService.SongPos > 0)
                {
                    MainActivity.musicService.SongPos--;
                    MainActivity.musicService.PlaySong();
                    imgPlay.SetImageResource(PAUSE_RESOURCE);

                    //SongDetailFragment songDetailFragment = new SongDetailFragment();
                    //Bundle bundle = new Bundle();
                    //bundle.PutInt(MainActivity.SONG_INDEX_KEY, MainActivity.musicService.SongPos);
                    //songDetailFragment.Arguments = bundle;

                    //var trans = FragmentManager.BeginTransaction();
                    //trans.Replace(Resource.Id.fragment_container, songDetailFragment);
                    //trans.AddToBackStack(null);
                    //trans.Commit();
                    //Activity.ActionBar.SetDisplayHomeAsUpEnabled(false);

                    //Toast.MakeText(this.Context, "上一首", ToastLength.Short).Show();

                }
                else
                {
                    MainActivity.musicService.SongPos = 0;
                    Toast.MakeText(this.Context, Resource.String.first_song, ToastLength.Short).Show();
                }

            };

            imgPlay.Click += delegate
            {
                if (MainActivity.musicService.IsPlay() == true)
                {
                    MainActivity.musicService.PausePlayer();
                    imgPlay.SetImageResource(PLAY_RESOURCE);
                }
                else
                {
                    //Go()表示继续播放
                    MainActivity.musicService.Go();
                    imgPlay.SetImageResource(PAUSE_RESOURCE);
                    if (musicThread.IsAlive)
                    {
                        ThreadHandler(1);
                    }
                    ThreadHandler(0);
                }
            };

            imgNext.Click += delegate
            {
                if (MainActivity.musicService.SongPos + 1< Songleton.Instance.AllSongs.Count)
                {
                    MainActivity.musicService.SongPos++;
                    MainActivity.musicService.PlaySong();
                    imgPlay.SetImageResource(PAUSE_RESOURCE);

                    //SongDetailFragment songDetailFragment = new SongDetailFragment();
                    //Bundle bundle = new Bundle();
                    //bundle.PutInt(MainActivity.SONG_INDEX_KEY, MainActivity.musicService.SongPos);
                    //songDetailFragment.Arguments = bundle;

                    //var trans = FragmentManager.BeginTransaction();
                    //trans.Replace(Resource.Id.fragment_container, songDetailFragment);
                    //trans.AddToBackStack(null);
                    //trans.Commit();
                    //Activity.ActionBar.SetDisplayHomeAsUpEnabled(false);
                    //Toast.MakeText(this.Context, "下一首", ToastLength.Short).Show();

                }
                else
                {
                    MainActivity.musicService.SongPos = Songleton.Instance.AllSongs.Count - 1;
                    Toast.MakeText(this.Context, Resource.String.last_song, ToastLength.Short).Show();


                }
            };


            return view;
        }

        public string getTime(int t)
        {

            int min = (int)t / (1000 * 60);
            //同上
            int sec = (int)(t - min * 1000 * 60) / 1000;
            //拼接字符串
            if (sec < 10 && min < 10)
            {
                second = "0" + sec.ToString();
                minutes = "0" + min.ToString();
                timeString = minutes + ":" + second;
            }

            if (sec >= 10 && min < 10)
            {
                second = sec.ToString();
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
        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            //navSongDur.Text = MainActivity.musicService.GetDur().ToString();
            //navSongPos.Text = progress.ToString();以下提示会不停出现
            //Toast.MakeText(this.Context, progress.ToString(), ToastLength.Short).Show();
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
            //textview1.Text = seekBar.Progress.ToString();
            //Toast.MakeText(this.Context, "开始拖动", ToastLength.Short).Show();
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            int songposition = Convert.ToInt32(((float)seekBar.Progress / 300) * MainActivity.musicService.GetDur());
            //下面语句已经证明GetDur方法可用，可用获取歌曲长度，单位是毫秒
            //Toast.MakeText(this.Context, MainActivity.musicService.GetDur().ToString(), ToastLength.Short).Show();
            //Toast.MakeText(this.Context, songposition.ToString(), ToastLength.Short).Show();
            MainActivity.musicService.Seek(songposition);
            MainActivity.musicService.Go();
            imgPlay.SetImageResource(PAUSE_RESOURCE);
            //Clears the current seekbar thread first if its running.
            if (musicThread.IsAlive)
            {
                ThreadHandler(1);
            }
            ThreadHandler(0);
            //Toast.MakeText(this.Context, "停止拖动", ToastLength.Short).Show();
        }

        public static void ThreadHandler(byte command)
        {
            if (command == 0)
            {
                //Starts the thread.
                musicThread = new Thread(new ThreadStart(SeekBarHandler));
                musicThread.Start();
            }
            else if (command == 1)
            {
                //Stops the thread.
                musicThread.Abort();
                musicThread.Join();
            }
            //Used for resuming a thread.
            else if (command == 2)
            {
                musicThread.Start();
            }
        }

        private static void SeekBarHandler()
        {
            try
            {
                //if (MusicService.player == null)
                //{
                //    return;
                //}

                do
                {
                    //Handles the seekerbar and the duration display as the mediaplayer plays.
                    int currentPosition = MainActivity.musicService.GetPosn();
                    int totalDuration = MainActivity.musicService.GetDur();
                    //seekBar.Max = totalDuration;
                    //seekBar.Progress = currentPosition;
                    seekBar.Progress = Convert.ToInt32(((float)currentPosition / totalDuration)*300);
         
                } while (MainActivity.musicService.IsPlay());
            }
            catch (Exception p) { }
        }

        public void SetRingtone(string path)
        {
            Java.IO.File sdfile = new Java.IO.File(path);

            ContentValues values = new ContentValues();
            values.Put(MediaStore.MediaColumns.Data, sdfile.AbsolutePath);
            values.Put(MediaStore.MediaColumns.Title, sdfile.Name);
            values.Put(MediaStore.MediaColumns.MimeType, "audio/mp3");
            values.Put(MediaStore.Audio.Media.InterfaceConsts.IsRingtone, true);
            values.Put(MediaStore.Audio.Media.InterfaceConsts.IsAlarm, true);
            values.Put(MediaStore.Audio.Media.InterfaceConsts.IsNotification, false);
            values.Put(MediaStore.Audio.Media.InterfaceConsts.IsMusic, false);

            //Android.Net.Uri uri = MediaStore.Audio.Media.GetContentUriForPath(sdfile.AbsolutePath);
            //ContentResolver resolver = this.context.ContentResolver;
            //Android.Net.Uri newuri = resolver.Insert(uri, values);


            Android.Net.Uri oldRingtoneUri = RingtoneManager.GetActualDefaultRingtoneUri(this.Context, RingtoneType.Ringtone);
            Android.Net.Uri oldAlarmUri = RingtoneManager.GetActualDefaultRingtoneUri(this.Context, RingtoneType.Alarm);

            Android.Net.Uri uri2 = MediaStore.Audio.Media.GetContentUriForPath(sdfile.AbsolutePath);
            Android.Net.Uri newuri2 = null;
            //新增铃声URI ID
            string deleteid = "";
            ICursor cursor = this.Context.ContentResolver.Query(uri2, null, MediaStore.MediaColumns.Data + "=?", new string[] { path }, null);
            if (cursor.MoveToFirst())
            {
                deleteid = cursor.GetString(cursor.GetColumnIndex("_id"));


            }

            string ringtoneid = "";
            string alarmid = "";
            if (null != oldRingtoneUri)
            {
                ringtoneid = oldRingtoneUri.LastPathSegment;

            }

            if (null != oldAlarmUri)
            {
                alarmid = oldAlarmUri.LastPathSegment;

            }

            Android.Net.Uri setRingtoneUri;

            if (ringtoneid.Equals(deleteid))
            {
                //setRingtoneUri = newuri2;
                //如果新添加的铃声ID和已设置的铃声ID相同，不需要重新设置铃声，因为重复URI也是插入不了的。
                //setRingtoneUri = oldRingtoneUri;
                Toast.MakeText(this.Context, "当前已是来电铃声，无需重复设置！", ToastLength.Short).Show();
            }
            else
            {

                if (alarmid.Equals(deleteid))
                {
                    setRingtoneUri = oldAlarmUri;
                    RingtoneManager.SetActualDefaultRingtoneUri(this.Context, RingtoneType.Ringtone, setRingtoneUri);
                    Toast.MakeText(this.Context, "来电设置成功！", ToastLength.Short).Show();

                }
                else
                {

                    Context.ContentResolver.Delete(uri2, MediaStore.MediaColumns.Data + "=\"" + sdfile.AbsolutePath + "\"", null);
                    newuri2 = Context.ContentResolver.Insert(uri2, values);
                    //setRingtoneUri = oldRingtoneUri;
                    //如果新添加的铃声ID和已设置的铃声ID不同，就新增铃声
                    if (newuri2 != null)
                    {
                        setRingtoneUri = newuri2;
                        RingtoneManager.SetActualDefaultRingtoneUri(this.Context, RingtoneType.Ringtone, setRingtoneUri);
                        Toast.MakeText(this.Context, "来电设置成功！", ToastLength.Short).Show();
                    }
                }
            }

        }

        //public void SetAlarm(int pos)
        //{
        //    //Java.IO.File sdfile = new Java.IO.File(path);

        //    ContentValues values = new ContentValues();
        //    var trackUri = ContentUris.WithAppendedId(MediaStore.Audio.Media.ExternalContentUri, Songleton.Instance[pos].Id);
        //    //var songCover = Android.Net.Uri.Parse("content://media/external/audio/albumart");
        //    //var songAlbumArtUri = ContentUris.WithAppendedId(songCover, Songleton.Instance[pos].AlbumId);
        //    Java.IO.File sdfile = new Java.IO.File(trackUri.Path);
        //    Toast.MakeText(this.Context, trackUri.Path, ToastLength.Short).Show();
        //    //Toast.MakeText(this.Context, sdfile.AbsolutePath, ToastLength.Short).Show();
        //    //values.Put(MediaStore.MediaColumns.Data, sdfile.AbsolutePath);
        //    values.Put(MediaStore.MediaColumns.Data, trackUri.Path);
        //    //values.Put(MediaStore.MediaColumns.Title, sdfile.Name);
        //    values.Put(MediaStore.MediaColumns.Title, Songleton.Instance[pos].Title);
        //    values.Put(MediaStore.MediaColumns.MimeType, "audio/mp3");
        //    values.Put(MediaStore.Audio.Media.InterfaceConsts.IsRingtone, true);
        //    values.Put(MediaStore.Audio.Media.InterfaceConsts.IsAlarm, true);
        //    values.Put(MediaStore.Audio.Media.InterfaceConsts.IsNotification, false);
        //    values.Put(MediaStore.Audio.Media.InterfaceConsts.IsMusic, false);

        //    //获取来电和闹钟的默认URI
        //    Android.Net.Uri oldRingtoneUri = RingtoneManager.GetActualDefaultRingtoneUri(this.Context, RingtoneType.Ringtone);
        //    Android.Net.Uri oldAlarmUri = RingtoneManager.GetActualDefaultRingtoneUri(this.Context, RingtoneType.Alarm);

        //    //Android.Net.Uri uri2 = MediaStore.Audio.Media.GetContentUriForPath(sdfile.AbsolutePath);
        //    Android.Net.Uri uri2 = trackUri;
        //    Android.Net.Uri newuri2 = null;
        //    //新增铃声URI ID
        //    string deleteid = "";
        //    //delete之前先进行了一次query操作，如果查到了就把那条记录的_id记录下来
        //    ICursor cursor = this.Context.ContentResolver.Query(uri2, null, MediaStore.MediaColumns.Data + "=?", new string[] { trackUri.Path }, null);
        //    if (cursor.MoveToFirst())
        //    {
        //        deleteid = cursor.GetString(cursor.GetColumnIndex("_id"));


        //    }

        //    string alarmid = "";
        //    string ringtoneid = "";

        //    if (null != oldAlarmUri)
        //    {
        //        //获取用户当前设置的闹钟铃声URI ID
        //        alarmid = oldAlarmUri.LastPathSegment;

        //    }

        //    if (null != oldRingtoneUri)
        //    {
        //        //获取用户当前设置的来电铃声URI ID
        //        ringtoneid = oldRingtoneUri.LastPathSegment;

        //    }

        //    Android.Net.Uri setAlarmUri;

        //    if (alarmid.Equals(deleteid))
        //    {
        //        //setRingtoneUri = newuri2;
        //        //如果新添加的铃声ID和已设置的铃声ID相同，不需要重新设置铃声，因为重复URI也是插入不了的。
        //        //setRingtoneUri = oldRingtoneUri;
        //        Toast.MakeText(this.Context, "当前已是闹钟铃声，无需重复设置！", ToastLength.Short).Show();
        //    }
        //    else
        //    {
        //        if (ringtoneid.Equals(deleteid))
        //        {
        //            setAlarmUri = oldRingtoneUri;
        //            RingtoneManager.SetActualDefaultRingtoneUri(this.Context, RingtoneType.Alarm, setAlarmUri);
        //            Toast.MakeText(this.Context, "闹钟设置成功！", ToastLength.Short).Show();

        //        }
        //        else
        //        {
        //            //需要判断是不是被设置为来电铃声
        //            Context.ContentResolver.Delete(uri2, MediaStore.MediaColumns.Data + "=\"" + sdfile.AbsolutePath + "\"", null);
        //            newuri2 = Context.ContentResolver.Insert(uri2, values);
        //            //setRingtoneUri = oldRingtoneUri;
        //            //如果新添加的铃声ID和已设置的铃声ID不同，就新增铃声
        //            if (newuri2 != null)
        //            {
        //                setAlarmUri = newuri2;
        //                RingtoneManager.SetActualDefaultRingtoneUri(this.Context, RingtoneType.Alarm, setAlarmUri);
        //                Toast.MakeText(this.Context, "闹钟设置成功！", ToastLength.Short).Show();
        //            }
        //        }
        //    }

        //}
    }
}