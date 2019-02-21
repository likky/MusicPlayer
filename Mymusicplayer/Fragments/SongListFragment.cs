//using Android.App;
using Android.Support.V4.App;
using Android.OS;
using Android.Widget;
using Android.Views;
using Android.Util;

namespace MyMusicPlayer
{
	public class SongListFragment : ListFragment
	{
        //private ImageView imgPlay;
        //public static FragmentTransaction trans;
        public override void OnActivityCreated (Bundle savedInstanceState)
		{
			base.OnActivityCreated (savedInstanceState);
			ListAdapter = new SongsAdapter (Activity);
		}

        public override void OnListItemClick(ListView l, View v, int position, long id)
		{
                //SongDetailFragment songDetailFragment = new SongDetailFragment ();
				Bundle bundle = new Bundle ();
				bundle.PutInt(MainActivity.SONG_INDEX_KEY, position);
				//songDetailFragment.Arguments = bundle;
    //        //找到代码中的一个潜在bug，为什么写了2次？
    //            var trans = FragmentManager.BeginTransaction();
    //            trans.Replace(Resource.Id.fragment_container, songDetailFragment);
    //            trans.AddToBackStack(null);
				//trans.Commit();
				//Activity.ActionBar.SetDisplayHomeAsUpEnabled(false);

            //imgPlay = (ImageView)view.FindViewById(Resource.Id.imgPlay);
            if (MainActivity.musicService != null)
            {
                MainActivity.musicService.SongPos = position;
                MainActivity.musicService.PlaySong();
                //PAUSE_RESOURCE是全局静态变量
                //imgPlay.SetImageResource(MusicControls.PAUSE_RESOURCE);

                //MusicControlFragment.controller.Show (0);
            }
            else {
				Log.Error (MainActivity.MUSIC_ERR_TAG, "Music service is not started.");
			}
		}
	}
}

