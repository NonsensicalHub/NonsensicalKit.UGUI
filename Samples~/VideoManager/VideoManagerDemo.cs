using UnityEngine;

namespace NonsensicalKit.UGUI.Samples.VideoManager
{
    public class VideoManagerDemo : MonoBehaviour
    {
        [SerializeField] private NonsensicalKit.UGUI.VideoManager.VideoManager m_videoManager;
        [SerializeField] private string m_videoUrl = "http://vjs.zencdn.net/v/oceans.mp4";

        public void PlayTest()
        {
            m_videoManager.PlayVideo(m_videoUrl);
        }
    }
}
